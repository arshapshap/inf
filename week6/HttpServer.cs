using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener listener;
        private bool isWorking;
        private string settingsJsonPath;
        private Settings settings;

        public HttpServer(string settingsPath)
        {
            settingsJsonPath = settingsPath;
            listener = new HttpListener();
            UpdateSettings();
        }

        public void Start()
        {
            if (isWorking)
            {
                Program.PrintMessage("Сервер уже запущен.");
                return;
            }

            UpdateSettings();
            
            listener.Start();
            isWorking = true;

            Program.PrintMessage("Сервер запущен.");

            Thread thread = new Thread(Processing);
            thread.Start();
        }

        public void Stop()
        {
            if (!isWorking)
            {
                Program.PrintMessage("Сервер уже остановлен.");
                return;
            }
            listener.Stop();
            isWorking = false;

            Program.PrintMessage("Сервер остановлен.");
        }

        private void Processing()
        {
            HttpListenerContext context;
            try
            {
                context = listener.GetContext();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                (byte[] buffer, string contentType) serverResponse;
                if (!MethodHandler(request, response, out serverResponse))
                {
                    string filePath = settings.Path + request.RawUrl.Replace("%20", " ");
                    bool notFoundError;
                    serverResponse = FileLoader.GetResponse(filePath, out notFoundError);
                    if (notFoundError)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        Program.PrintMessage($"Ресурс не найден по следующему пути: {filePath}.");
                    }
                }

                response.Headers.Set("Content-Type", serverResponse.contentType);
                response.ContentLength64 = serverResponse.buffer.Length;
                Stream output = response.OutputStream;
                output.Write(serverResponse.buffer, 0, serverResponse.buffer.Length);
                output.Close();

                Processing();
            }
            catch (Exception ex)
            {
                if (isWorking)
                {
                    Program.PrintMessage("Произошла ошибка: " + ex.Message);
                    Stop();
                }
                return;
            }
        }

        private bool MethodHandler(HttpListenerRequest request, HttpListenerResponse response, out (byte[] buffer, string contentType) serverResponse)
        {
            if (request.Url.Segments.Length < 2)
            {
                serverResponse = (new byte[0], "");
                return false;
            }

            string controllerName = request.Url.Segments[1].Replace("/", "");

            string[] strParams = request.Url
                                    .Segments
                                    .Skip(2)
                                    .Select(s => s.Replace("/", ""))
                                    .ToArray();

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(ApiController)) &&
                    (t.GetCustomAttribute(typeof(ApiController)) as ApiController)?.ClassURI == controllerName)
                .FirstOrDefault();

            if (controller == null)
            {
                serverResponse = (new byte[0], "");
                return false;
            }

            var methodURI = (strParams.Length > 0) ? strParams[0] : "";
            var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}" && Regex.IsMatch(methodURI, ((HttpRequest)attr).MethodURI)))
                .FirstOrDefault();

            if (method == null)
            {
                serverResponse = (new byte[0], "");
                return false;
            }

            if (request.HttpMethod == "POST")
            {
                var postData = GetRequestPostData(request);
                strParams = postData.Split('&').Select(p => p.Split('=')[1]).ToArray();
            }

            object[] queryParams = method.GetParameters()
                                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                .ToArray();

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            if (request.HttpMethod == "POST")
            {
                response.Redirect(@"http://steampowered.com");
                serverResponse = (new byte[0], "");
                return true;
            }

            serverResponse = (Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret)), "application/json");
            return true;
        }

        private void UpdateSettings()
        {
            settings = new Settings();
            if (File.Exists(settingsJsonPath))
                settings = SettingsDeserializer.GetSettings(settingsJsonPath);
            else
                Program.PrintMessage($"Файл настроек не найден по следующему пути: {settingsJsonPath}.");

            listener.Prefixes.Clear();
            listener.Prefixes.Add($"http://localhost:{settings.Port}/");
        }

        private static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (Stream body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
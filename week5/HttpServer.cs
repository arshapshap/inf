using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.Json;

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

                string filePath = settings.Path + request.RawUrl.Replace("%20", " ");
                bool notFoundError;
                var serverResponse = FileLoader.GetResponse(filePath, out notFoundError);
                if (notFoundError)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    Program.PrintMessage($"Ресурс не найден по следующему пути: {filePath}.");
                }

                response.Headers.Set("Content-Type", $"text/{serverResponse.contentType}");
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpServer
{
    internal class ResponseProvider
    {
        public static HttpListenerResponse GetResponse(HttpListenerContext context, string path, out byte[] buffer)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            (byte[] buffer, string contentType) serverResponse;
            if (!TryHandleMethod(request, response, out serverResponse))
            {
                string filePath = path + request.RawUrl.Replace("%20", " ");
                if (!FileLoader.TryGetResponse(filePath, out serverResponse))
                {
                    serverResponse = GetErrorServerResponse(HttpStatusCode.NotFound);
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    Program.PrintMessage($"Ресурс не найден по следующему пути: {filePath}.");
                }
            }

            response.Headers.Set("Content-Type", serverResponse.contentType);
            response.ContentLength64 = serverResponse.buffer.Length;

            buffer = serverResponse.buffer;

            return response;
        }

        private static bool TryHandleMethod(HttpListenerRequest request, HttpListenerResponse response, out (byte[] buffer, string contentType) serverResponse)
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
                    ((ApiController?)t.GetCustomAttribute(typeof(ApiController)))?.ClassURI == controllerName)
                .FirstOrDefault();

            if (controller == null)
            {
                serverResponse = (new byte[0], "");
                return false;
            }

            var methodURI = (strParams.Length > 0) ? strParams[0] : "";

            var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}"
                    && Regex.IsMatch(methodURI, (((HttpRequest)attr).MethodURI == "") ? t.Name.ToLower() : ((HttpRequest)attr).MethodURI)))
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

            object[] queryParams = null;

            var httpGetAttribute = (HttpGET)method.GetCustomAttribute(typeof(HttpGET));
            if (httpGetAttribute?.OnlyForAuthorized == true)
            {
                var sessionCookie = request.Cookies.Where(cookie => cookie.Name == "SessionId").FirstOrDefault();

                if (sessionCookie == null ||
                    !SessionManager.Instance.CheckSession(Guid.Parse(sessionCookie.Value)))
                {
                    serverResponse = GetErrorServerResponse(HttpStatusCode.Unauthorized);
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return true;
                }

                if (httpGetAttribute?.NeedSessionId == true)
                {
                    queryParams = method.GetParameters()
                            .Skip(1)
                            .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                            .Append(Guid.Parse(sessionCookie.Value))
                            .ToArray();
                }
            }

            if (queryParams == null)
                 queryParams = method.GetParameters()
                                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                .ToArray();

            var methodResponse = (ControllerResponse)method.Invoke(Activator.CreateInstance(controller), queryParams);

            methodResponse.action.Invoke(response);
            if (methodResponse.statusCode != HttpStatusCode.OK)
            {
                serverResponse = GetErrorServerResponse(methodResponse.statusCode);
                response.StatusCode = (int)methodResponse.statusCode;
                return true;
            }

            serverResponse = (Encoding.ASCII.GetBytes(JsonSerializer.Serialize(methodResponse.response)), "application/json");
            return true;
        }
        private static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return null;
            using (Stream body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static (byte[] buffer, string contentType) GetErrorServerResponse(HttpStatusCode statusCode)
            => (Encoding.UTF8.GetBytes($"ERROR {((int)statusCode)}: {statusCode}."), "text/plain");
    }

}
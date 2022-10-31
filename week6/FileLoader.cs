using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    static class FileLoader
    {
        public static (byte[] buffer, string contentType) GetResponse(string filePath, out bool error)
        {
            byte[] buffer = null;
            var contentType = "plain";
            error = false;

            if (Directory.Exists(filePath))
                filePath += "/index.html";

            if (File.Exists(filePath))
            {
                buffer = File.ReadAllBytes(filePath);
                contentType = filePath.Split('.').Last();
            }
            else
            {
                buffer = Encoding.UTF8.GetBytes("ERROR 404: Resource not found.");
                error = true;
            }

            return (buffer, $"text/{contentType}");
        }
    }
}

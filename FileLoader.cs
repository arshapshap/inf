﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    static class FileLoader
    {
        public static bool TryGetResponse(string filePath, out (byte[] buffer, string contentType) response)
        {
            byte[] buffer;
            string contentType;

            if (Directory.Exists(filePath))
                filePath += "/index.html";

            if (!File.Exists(filePath))
            {
                response = (Encoding.UTF8.GetBytes("ERROR 404: Resource not found."), "text/plain");
                return false;
            }

            buffer = File.ReadAllBytes(filePath);
            contentType = filePath.Split('.').Last();

            response = (buffer, $"text/{contentType}");
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    internal struct ControllerResponse
    {
        public readonly object? response;
        public readonly HttpStatusCode statusCode;
        public readonly Action<HttpListenerResponse> action;

        public ControllerResponse(object? response, 
            Action<HttpListenerResponse> action = null, 
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            this.response = response;
            this.statusCode = statusCode;
            this.action = (action == null) ? (HttpListenerResponse) => { } : action;
        }
    }
}

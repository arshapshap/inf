using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    class HttpGET : HttpRequest
    {
        public HttpGET(string methodURI) : base(methodURI) { }
        public HttpGET() : base() { }
    }
}

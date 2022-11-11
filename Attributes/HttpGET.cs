using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    class HttpGET : HttpRequest
    {
        public readonly bool OnlyForAuthorized;
        public readonly bool NeedSessionId;
        public HttpGET(string methodURI = "", bool onlyForAuthorized = false, bool needSessionId = false) : base(methodURI) 
        { 
            OnlyForAuthorized = onlyForAuthorized;
            NeedSessionId = needSessionId;
        }
    }
}

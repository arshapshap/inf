using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    class ApiController : Attribute
    {
        public string ClassURI;
        public ApiController(string classURI)
        {
            ClassURI = classURI;
        }
    }
}

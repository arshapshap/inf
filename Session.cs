using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    internal struct Session
    {
        public readonly Guid Guid;
        public readonly int AccountId;
        public readonly string Login;
        public readonly DateTime Created;

        public Session(Guid guid, int accountId, string login, DateTime created)
        {
            Guid = guid;
            AccountId = accountId;
            Login = login;
            Created = created;
        }
    }
}

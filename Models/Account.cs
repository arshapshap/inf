using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    class Account
    {
        public int Id { get; set; }
        [FieldDB("login")]
        public string Login { get; set; }
        [FieldDB("password")]
        public string Password { get; set; }

        public Account(int id, string login, string password) : this(login, password)
        {
            Id = id;
        }

        public Account(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}

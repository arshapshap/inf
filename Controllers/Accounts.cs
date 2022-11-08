using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    [ApiController("accounts")]
    class Accounts
    {
        public static HttpListenerRequest Request;
        public static HttpListenerResponse Response;

        static AccountDAO accountDAO 
            = new AccountDAO(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True", "Table");

        [HttpPOST]
        public static bool Login(string login, string password)
        {
            var account = accountDAO.Select(login, password);
            if (account == null)
                return false;

            var session = SessionManager.Instance.CreateSession(account.Id, account.Login);
            var cookie = new Cookie("SessionId", session.Guid.ToString());
            Response.Cookies.Add(cookie);

            return true;
        }

        [HttpPOST("save")]
        public static void SaveAccount(string login, string password)
        {
            accountDAO.Insert(new Account(login, password));

            Response.Redirect(@"http://steampowered.com");
        }

        [HttpGET(@"\d")]
        public static Account? GetAccountById(int id)
        {
            return accountDAO.Select(id);
        }

        [HttpGET("info", onlyForAuthorized: true)]
        public static Account GetAccountInfo()
        {
            var sessionId = Guid.Parse(Request.Cookies.Where(cookie => cookie.Name == "SessionId").First().Value);
            var session = SessionManager.Instance.GetSession(sessionId);
            return GetAccountById(session.AccountId);
        }

        [HttpGET("", onlyForAuthorized: true)]
        public static Account[] GetAccounts()
        {
            return accountDAO.Select();
        }
    }
}

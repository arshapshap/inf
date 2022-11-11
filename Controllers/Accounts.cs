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
        HttpListenerRequest request;
        HttpListenerResponse response;

        AccountDAO accountDAO 
            = new AccountDAO(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True", "Table");

        public Accounts(HttpListenerRequest request, HttpListenerResponse response)
        {
            this.request = request;
            this.response = response;
        }

        [HttpPOST]
        public bool Login(string login, string password)
        {
            var account = accountDAO.Select(login, password);
            if (account == null)
                return false;

            var session = SessionManager.Instance.CreateSession(account.Id, account.Login);
            var cookie = new Cookie("SessionId", session.Guid.ToString());
            response.Cookies.Add(cookie);

            return true;
        }

        [HttpPOST("save")]
        public void SaveAccount(string login, string password)
        {
            accountDAO.Insert(new Account(login, password));

            response.Redirect(@"http://steampowered.com");
        }

        [HttpGET(@"\d")]
        public Account? GetAccountById(int id)
        {
            return accountDAO.Select(id);
        }

        [HttpGET("info", onlyForAuthorized: true)]
        public Account GetAccountInfo()
        {
            var sessionId = Guid.Parse(request.Cookies.Where(cookie => cookie.Name == "SessionId").First().Value);
            var session = SessionManager.Instance.GetSession(sessionId);
            return GetAccountById(session.AccountId);
        }

        [HttpGET("", onlyForAuthorized: true)]
        public Account[] GetAccounts()
        {
            return accountDAO.Select();
        }
    }
}

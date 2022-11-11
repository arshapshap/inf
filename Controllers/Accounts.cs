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
        static AccountDAO accountDAO 
            = new AccountDAO(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True", "Table");


        [HttpPOST("^$")]
        public static ControllerResponse Login(string login, string password)
        {
            var account = accountDAO.Select(login, password);
            if (account == null)
                return new ControllerResponse(false);

            var session = SessionManager.Instance.CreateSession(account.Id, account.Login);

            var addCookieAction =
                (HttpListenerResponse response) => {
                    var cookie = new Cookie("SessionId", session.Guid.ToString());
                    response.Cookies.Add(cookie);
                };

            return new ControllerResponse(true, action: addCookieAction);
        }

        [HttpPOST("save")]
        public static ControllerResponse SaveAccount(string login, string password)
        {
            accountDAO.Insert(new Account(login, password));

            var redirectAction = (HttpListenerResponse response) => {
                    response.Redirect(@"http://steampowered.com");
                };

            return new ControllerResponse(null, redirectAction);
        }

        [HttpGET(@"\d")]
        public ControllerResponse GetAccountById(int id)
        {
            var foundAccount = accountDAO.Select(id);
            if (foundAccount == null)
                return new ControllerResponse(null, statusCode: HttpStatusCode.NotFound);
            return new ControllerResponse(foundAccount);
        }

        [HttpGET("info", onlyForAuthorized: true, needSessionId: true)]
        public ControllerResponse GetAccountInfo(Guid sessionId)
        {
            var session = SessionManager.Instance.GetSession(sessionId);
            return GetAccountById(session.AccountId);
        }

        [HttpGET("^$", onlyForAuthorized: true)]
        public ControllerResponse GetAccounts()
        {
            return new ControllerResponse(accountDAO.Select());
        }
    }
}
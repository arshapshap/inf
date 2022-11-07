using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    [ApiController("accounts")]
    class Accounts
    {
        static AccountDAO accountDAO;
        static Accounts()
        {
            accountDAO = new AccountDAO(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True", "Table");
        }

        [HttpPOST]
        public static int Login(string login, string password)
        {
            var account = accountDAO.Select(login, password);
            if (account != null)
                return account.Id;

            return -1;
        }

        [HttpPOST("save")]
        public static void SaveAccount(string login, string password)
        {
            accountDAO.Insert(new Account(login, password));
        }

        [HttpGET(@"\d")]
        public static Account? GetAccountById(int id)
        {
            return accountDAO.Select(id);
        }

        [HttpGET]
        public static Account[] GetAccounts()
        {
            return accountDAO.Select();
        }
    }
}

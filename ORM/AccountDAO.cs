using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    internal class AccountDAO
    {
        readonly MyORM orm;

        public AccountDAO(string connectionString, string tableName)
        {
            orm = new MyORM(connectionString, tableName);
        }

        public Account[] Select() => orm.Select<Account>();

        public Account[] SelectWhere(Dictionary<string, object> conditions) => orm.SelectWhere<Account>(conditions);

        public Account? Select(string login, string password) => orm.SelectWhere<Account>(new Dictionary<string, object>() { { "login", login }, { "password", password } }).FirstOrDefault();

        public Account? Select(int id) => orm.Select<Account>(id);

        public void Insert(Account account) => orm.Insert(account);

        public void Update(int id, Account account) => orm.Update(id, account);

        public void Delete(int id) => orm.Delete(id);
    }
}

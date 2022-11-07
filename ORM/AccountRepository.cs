using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    internal class AccountRepository
    {
        public string TableName;
        readonly string connectionString;
        readonly Dictionary<int, Account> repository;

        public AccountRepository(string connectionString, string tableName)
        {
            this.connectionString = connectionString;
            TableName = tableName;

            var orm = new MyORM(connectionString, tableName);
            repository = orm.Select<Account>().ToDictionary(account => account.Id, account => account);
        }

        public Account[] Select() => repository.Values.ToArray();

        public Account Select(int id) => repository[id];

        public void Insert(Account account)
        {
            repository[account.Id] = account;
            var orm = new MyORM(connectionString, TableName);
            orm.Insert(account);
        }

        public void Update(int id, Account account)
        {
            repository[id] = account;
            var orm = new MyORM(connectionString, TableName);
            orm.Update(id, account);
        }

        public void Delete (int id)
        {
            repository.Remove(id);
            var orm = new MyORM(connectionString, TableName);
            orm.Delete(id);
        }
    }
}

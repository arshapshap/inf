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
        [HttpPOST]
        public static void SaveAccount(string login, string password)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = string.Format("INSERT INTO [dbo].[Table] (login, password) VALUES ('{0}', '{1}')", login, password);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        [HttpGET(@"\d")]
        public static Account GetAccountById(int id)
        {
            var result = new Account();

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";

            string sqlExpression = string.Format("SELECT * FROM [dbo].[Table] WHERE id={0}", id);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows && reader.Read())
                {
                    result = new Account { Id = reader.GetInt32(0), Login = reader.GetString(1), Password = reader.GetString(2) };
                }

                reader.Close();
            }

            return result;
        }

        [HttpGET]
        public static Account[] GetAccounts()
        {
            var accounts = new List<Account>();

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";

            string sqlExpression = "SELECT * FROM [dbo].[Table]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accounts.Add(new Account { Id = reader.GetInt32(0), Login = reader.GetString(1), Password = reader.GetString(2) });
                    }
                }

                reader.Close();
            }
            return accounts.ToArray();
        }
    }
}

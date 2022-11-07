using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    internal class MyORM
    {
        public string TableName;
        readonly string connectionString;

        public MyORM(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public MyORM(string connectionString, string tableName) : this(connectionString)
        {
            TableName = tableName;
        }

        public T[] Select<T>()
        {
            var result = new List<T>();

            string sqlExpression = $"SELECT * FROM [dbo].[{TableName}]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var newItem = Activator.CreateInstance(typeof(T), GetValues(reader));
                        if (newItem is T item)
                            result.Add(item);
                    }
                }

                reader.Close();
            }
            return result.ToArray();
        }

        public T[] SelectWhere<T>(Dictionary<string, object> conditions)
        {
            var result = new List<T>();

            var stringConditions = conditions.Select(c => $"{c.Key}='{c.Value}'");
            string sqlExpression = $"SELECT * FROM [dbo].[{TableName}] WHERE {string.Join(" AND ", stringConditions)}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var newItem = Activator.CreateInstance(typeof(T), GetValues(reader));
                        if (newItem is T item)
                            result.Add(item);
                    }
                }

                reader.Close();
            }
            return result.ToArray();
        }

        public T? Select<T>(int id) => SelectWhere<T>(new Dictionary<string, object>() { { "id", id } }).FirstOrDefault();

        public void Insert<T>(T item)
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute(typeof(FieldDB)) != null)
                .ToDictionary(p => ((FieldDB)p.GetCustomAttribute(typeof(FieldDB))).ColumnName, p => $"'{p.GetValue(item)}'");

            string sqlExpression = $"INSERT INTO [dbo].[{TableName}]({string.Join(',', properties.Keys)}) VALUES ({string.Join(',', properties.Values)})";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public void Update<T>(int id, T item)
        {
            var changes = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute(typeof(FieldDB)) != null)
                .Select(p =>
                    $"{((FieldDB)p.GetCustomAttribute(typeof(FieldDB))).ColumnName} = '{p.GetValue(item)}'");

            string sqlExpression = $"UPDATE [dbo].[{TableName}] SET {string.Join(',', changes)} WHERE id={id}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string sqlExpression = $"DELETE FROM [dbo].[{TableName}] WHERE id={id}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        private object[] GetValues(SqlDataReader reader)
        {
            var values = new List<object>();

            for (int i = 0; i < reader.FieldCount; i++)
                values.Add(reader.GetValue(i));

            return values.ToArray();
        }
    }
}

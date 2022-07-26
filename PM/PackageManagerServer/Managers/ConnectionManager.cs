using Microsoft.Data.Sqlite;
using System.Text;

namespace PackageManagerServer.Managers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIncrementAttribute : System.Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class IdentityAttribute : System.Attribute { }

    public class ConnectionManager
    {
        private string _dbFileName = "data.db";
        private SqliteConnection _connection { get; set; }

        public ConnectionManager(string filename = "")
        {
            if (!string.IsNullOrWhiteSpace(filename)) _dbFileName = filename;
            _connection = new SqliteConnection($"Data Source={_dbFileName};");
            _connection.Open();
        }

        public SqliteConnection GetSQLConnection()
        {
            if (_connection != null)
            {
                return _connection;
            }
            throw new Exception("Fatal! db was unitialized when requested.");
        }

        public int GetLastInsertRowId()
        {
            var conn = GetSQLConnection();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT last_insert_rowid();";

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int Update<Ty>(Ty data)
        {
            var cmd = _connection.CreateCommand();
            var identities = typeof(Ty).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(IdentityAttribute))).ToList();
            var properties = typeof(Ty).GetProperties().ToList();

            StringBuilder sbUpdate = new StringBuilder();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, typeof(IdentityAttribute)))
                    sbUpdate.AppendLine($"{prop.Name} = @{prop.Name} {(prop == properties.Last() ? "" : ",")}");
                cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(data));
            }

            StringBuilder sbWhere = new StringBuilder();
            foreach (var identity in identities)
            {
                sbWhere.AppendLine($"{identity.Name} = @{identity.Name} {(identity == identities.Last() ? "" : " AND ")}");
            }
            cmd.CommandText = $"UPDATE {typeof(Ty).Name} SET {sbUpdate} WHERE {sbWhere}";

            return cmd.ExecuteNonQuery();
        }

        public int Insert<Ty>(Ty data) where Ty : class
        {
            var cmd = _connection.CreateCommand();
            var properties = typeof(Ty).GetProperties()
                .Where(prop => !Attribute.IsDefined(prop, typeof(AutoIncrementAttribute))).ToList();

            StringBuilder sbFields = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            foreach (var prop in properties)
            {
                sbFields.AppendLine($"{prop.Name}{(prop == properties.Last() ? "" : ",")}");
                cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(data));
                sbValues.AppendLine($"@{prop.Name}{(prop == properties.Last() ? "" : ",")}");
            }
            cmd.CommandText = $"INSERT INTO {typeof(Ty).Name} ({sbFields}) VALUES ({sbValues})";

            return cmd.ExecuteNonQuery();
        }

        public IEnumerable<Ty> Select<Ty>() where Ty : class, new()
        {
            var reader = Select($"SELECT * FROM {typeof(Ty).Name}");
            var properties = typeof(Ty).GetProperties();
            while (reader.Read())
            {
                Ty data = new Ty();
                foreach (var prop in properties)
                {
                    prop.SetValue(data, reader.GetValue(reader.GetOrdinal(prop.Name)));
                }
                yield return data;
            }
            reader.Close();
        }

        public void Register<Ty>() where Ty : class
        {
            StringBuilder sbColumns = new StringBuilder();
            var properties = typeof(Ty).GetProperties().ToList();
            var identities = typeof(Ty).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(IdentityAttribute)))
                .Select(prop => $"PRIMARY KEY({prop.Name}{(Attribute.IsDefined(prop, typeof(AutoIncrementAttribute)) ? " AUTOINCREMENT" : "")})");
            if (!identities.Any())
            {
                throw new ArgumentException($"type {typeof(Ty).FullName} must have at least one property marked as Identity");
            }
            foreach (var prop in properties)
            {
                sbColumns.AppendLine($"{prop.Name} {GetSqliteType(prop.PropertyType)},");
            }

            string? script = $"CREATE TABLE IF NOT EXISTS '{typeof(Ty).Name}' ({sbColumns}\n{string.Join(",", identities)});";
            if (!string.IsNullOrWhiteSpace(script)) CreateTable(script);
        }

        private SqliteDataReader Select(string query)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = query;

            return cmd.ExecuteReader();
        }

        private void CreateTable(string script)
        {
            var createTblCmd = _connection.CreateCommand();
            createTblCmd.CommandText = script;

            createTblCmd.ExecuteNonQuery();
        }

        private string GetSqliteType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type == typeof(int) || type == typeof(long)) return "INTEGER";
            if (type == typeof(string)) return "TEXT";
            if (type == typeof(bool)) return "BOOLEAN";
            return "TEXT";
        }
    }
}

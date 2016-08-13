using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerCompactSample
{
    class SuperSimpleDb
    {
        private SqlCeConnection connection;

        public SuperSimpleDb()
        {
            const string connectionString = "Data Source=|DataDirectory|\\MyProjectDb.sdf";

            this.connection = new SqlCeConnection(connectionString);

            // If database does not exists, create it.
            string databasePath = GetDatabasePath();
            if (!File.Exists(databasePath))
            {
                SqlCeEngine engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
            }

            this.connection.Open();
        }

        public int Execute(string query)
        {
            using (SqlCeCommand command = new SqlCeCommand(query, this.connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Execute a set of commands within a transaction that commits changes to disk immediately.
        /// </summary>
        /// <param name="queries"></param>
        public void ExecuteTransaction(IEnumerable<string> queries)
        {
            using (SqlCeTransaction transaction = this.connection.BeginTransaction())
            {
                foreach (string query in queries)
                {
                    using (SqlCeCommand command = new SqlCeCommand(query, this.connection))
                    {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit(CommitMode.Immediate);
            }
        }

        public DataTable Query(string query)
        {
            DataTable table = new DataTable();
            using (SqlCeCommand command = new SqlCeCommand(query, this.connection))
            {
                using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        public string GetDatabasePath()
        {
            const string dataDirectoryKey = "|DataDirectory|";
            if (!this.connection.DataSource.Contains(dataDirectoryKey))
            {
                return this.connection.DataSource;
            }

            string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(dataDirectory))
            {
                dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            return Path.Combine(
                dataDirectory,
                this.connection.DataSource.Replace(dataDirectoryKey, "."));
        }

        public bool TableExists(string tableName)
        {
            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            using (SqlCeCommand command = new SqlCeCommand(query, this.connection))
            {
                command.Parameters.AddWithValue("@tableName", tableName);
                var value = command.ExecuteScalar();
                if (value != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// In SQL Server Compact it is important to close the connection to flush changes to the store.
        /// A power loss or abnormal termination after a write and before a flush will result in data lost.
        /// Alternatively, use a transaction with CommitMode.Immediate.
        /// </summary>
        public void Dispose()
        {
            var localConnection = this.connection;
            if (localConnection != null)
            {
                localConnection.Dispose();
            }
        }
    }
}

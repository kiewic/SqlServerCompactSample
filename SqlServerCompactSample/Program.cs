using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerCompactSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new SuperSimpleDb();

            // CREATE TABLE example
            if (!db.TableExists("foo11"))
            {
                db.Execute("CREATE TABLE foo11 (Id INTEGER UNIQUE, Name NVARCHAR(256))");
            }

            // INSERT example
            int rowsAffected = db.Execute("INSERT INTO foo11 VALUES (1, 'hello world')");
            Console.WriteLine(rowsAffected);

            // BEGIN TRANSACTION and COMMIT TRANSACTION example
            db.ExecuteTransaction(new[] 
            {
                "INSERT INTO foo11 VALUES (2, 'NameA')",
                "INSERT INTO foo11 VALUES (3, 'NameB')"
            });

            var table1 = db.Query("SELECT * FROM foo11");
            ShowResults(table1);

            var table2 = db.Query("SELECT * FROM INFORMATION_SCHEMA.TABLES");
            ShowResults(table2);
        }

        private static void ShowResults(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                Console.Write(column.ColumnName + "\t");
            }
            Console.WriteLine();
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.Write(row[column]);
                    Console.Write("\t");
                }
                Console.WriteLine();
            }
        }
    }
}

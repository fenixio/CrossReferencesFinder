using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace FindCrossRefs
{
    public class DBSet : IDisposable
    {
        public SQLiteConnection Connection { get; private set; }

        public int functId;
        public int refId;
        public object functObj = new object();
        public object refObj  = new object();

        public DBSet(string dbName)
        {
            Connection = new SQLiteConnection("Data Source=" + dbName + ";version=3;");
            Connection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS Methods (Id INTEGER PRIMARY KEY, NamespaceName VARCHAR(200), TypeName VARCHAR(200), Name VARCHAR(400))";

            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            sql = "CREATE TABLE IF NOT EXISTS Refs (Id INTEGER PRIMARY KEY, CallerID INTEGER, NamespaceName VARCHAR(200), TypeName VARCHAR(200), Name VARCHAR(400))";
            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            sql = "DELETE FROM Methods";
            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
                functId = 0;

            }
            sql = "DELETE FROM Refs";
            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
                refId = 0;
            }

        }

        public int WriteFunction(string namespaceName, string typeName, string function)
        {

            lock (functObj)
            {
                functId++;
            }
            string sql = string.Format("INSERT INTO Methods( Id, NamespaceName, TypeName, Name) VALUES ({0},'{1}','{2}','{3}')", functId, namespaceName, typeName, function);
            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            return functId;
        }
        public int WriteReference(int callerId, string namespaceName, string typeName, string functionName)
        {

            lock (refObj)
            {
                refId++;
            }
            string sql = string.Format("INSERT INTO Refs( Id, CallerId, NamespaceName, TypeName,  Name) VALUES ({0},{1},'{2}','{3}','{4}')", refId, callerId, namespaceName, typeName, functionName);
            using (SQLiteCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            return functId;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}

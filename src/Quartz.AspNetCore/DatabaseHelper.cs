using System;
using System.IO;

namespace Quartz.AspNetCore
{
    public static class DatabaseHelper
    {
        private static string _mySql;
        private static string _sqlServer;

        public static string GetMysqlScript()
        {
            if (_mySql == null)
            {
                var stream = typeof(DatabaseHelper).Assembly.GetManifestResourceStream("Quartz.AspNetCore.DDL.MySql.sql");
                using var reader = new StreamReader(stream ?? throw new Exception("Resource not found"));
                _mySql = reader.ReadToEnd();
            }

            return _mySql;
        }

        public static string GetSqlServerScript()
        {
            if (_sqlServer == null)
            {
                var stream = typeof(DatabaseHelper).Assembly.GetManifestResourceStream("Quartz.AspNetCore.DDL.SqlServer.sql");
                using var reader = new StreamReader(stream ?? throw new Exception("Resource not found"));
                _sqlServer = reader.ReadToEnd();
            }

            return _sqlServer;
        }
    }
}
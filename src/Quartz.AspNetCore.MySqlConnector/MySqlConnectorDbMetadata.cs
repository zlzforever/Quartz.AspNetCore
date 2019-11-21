using System;
using MySql.Data.MySqlClient;
using Quartz.Impl.AdoJobStore.Common;

namespace Quartz.AspNetCore.MySqlConnector
{
    public class MySqlConnectorDbMetadata : DbMetadata
    {
        private readonly Type _parameterDbType;
        private readonly Type _connectionType;
        private readonly Type _commandType;
        private readonly Type _parameterType;
        private readonly Type _exceptionType;

        public MySqlConnectorDbMetadata()
        {
            _parameterDbType = typeof(MySqlDbType);
            _connectionType = typeof(MySqlConnection);
            _commandType = typeof(MySqlCommand);
            _parameterType = typeof(MySqlParameter);
            _exceptionType = typeof(MySqlException);
        }

        public override string ProductName => "MySQL, MySQL provider";
        public override string ParameterNamePrefix => "?";
        public override Type ConnectionType => _connectionType;
        public override Type CommandType => _commandType;
        public override Type ParameterDbType => _parameterDbType;
        public override Type ParameterType => _parameterType;
        public override Type ExceptionType => _exceptionType;
        public override bool BindByName => true;
    }
}
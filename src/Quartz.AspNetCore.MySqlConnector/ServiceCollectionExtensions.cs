using Quartz.Impl.AdoJobStore.Common;

namespace Quartz.AspNetCore.MySqlConnector
{
    public static class ServiceCollectionExtensions
    {
        public static QuartzOptionsBuilder UseMySqlConnector(this QuartzOptionsBuilder builder,
            string connectString, string serializerType = "binary", string tablePrefix = "QRTZ_")
        {
            builder.UseMySql(connectString, serializerType, tablePrefix);
            builder.Properties.Set("quartz.dataSource.myDs.provider", "MySqlConnector");
            var metadata = new MySqlConnectorDbMetadata
            {
                ParameterDbTypePropertyName = "MySqlDbType", DbBinaryTypeName = "Blob"
            };
            metadata.Init();
            DbProvider.RegisterDbMetadata("MySqlConnector", metadata);
            return builder;
        }
    }
}
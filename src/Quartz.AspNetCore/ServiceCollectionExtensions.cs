using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Util;

namespace Quartz.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartz(this IServiceCollection services,
            Action<QuartzOptionsBuilder> optionsAction = null)
        {
            var builder = new QuartzOptionsBuilder
            {
                Services = services,
                Properties = new NameValueCollection {{"quartz.jobStore.useProperties", "true"}}
            };
            optionsAction?.Invoke(builder);

            var serverSched = new StdSchedulerFactory(builder.Properties).GetScheduler().Result;
            builder.Services.AddSingleton(serverSched);
            builder.Services.AddTransient<ISchedulerFactory, StdSchedulerFactory>();
            builder.Services.AddTransient<LoggingProvider>();
            return services;
        }

        public static QuartzOptionsBuilder UseSchedulerName(this QuartzOptionsBuilder builder, string name)
        {
            builder.Properties.Set("schedName", name);
            return builder;
        }

        public static QuartzOptionsBuilder UseMemoryStore(this QuartzOptionsBuilder builder)
        {
            builder.Properties.Set("quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz");
            builder.Properties.Remove("quartz.jobStore.useProperties");
            builder.Properties.Remove("quartz.jobStore.driverDelegateType");
            builder.Properties.Remove("quartz.jobStore.dataSource");
            builder.Properties.Remove("quartz.jobStore.tablePrefix");
            builder.Properties.Remove("quartz.dataSource.myDs.provider");
            builder.Properties.Remove("quartz.dataSource.myDs.connectionString");
            return builder;
        }

        public static QuartzOptionsBuilder UseSqlServer(this QuartzOptionsBuilder builder, string connectString,
            string serializerType = "binary", string tablePrefix = "QRTZ_")
        {
            builder.Properties.Set("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
            builder.Properties.Set("quartz.jobStore.driverDelegateType",
                "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
            builder.Properties.Set("quartz.jobStore.dataSource", "myDs");
            builder.Properties.Set("quartz.dataSource.myDs.provider", "SqlServer");
            builder.Properties.Set("quartz.jobStore.tablePrefix", tablePrefix);
            builder.Properties.Set("quartz.serializer.type", serializerType);
            builder.Properties.Set("quartz.dataSource.myDs.connectionString", connectString);
            return builder;
        }

        public static QuartzOptionsBuilder UseMySql(this QuartzOptionsBuilder builder, string connectString,
            string serializerType = "binary", string tablePrefix = "QRTZ_")
        {
            builder.Properties.Set("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
            builder.Properties.Set("quartz.jobStore.driverDelegateType",
                "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
            builder.Properties.Set("quartz.jobStore.dataSource", "myDs");
            builder.Properties.Set("quartz.dataSource.myDs.provider", "MySql");
            builder.Properties.Set("quartz.jobStore.tablePrefix", tablePrefix);
            builder.Properties.Set("quartz.serializer.type", serializerType);
            builder.Properties.Set("quartz.dataSource.myDs.connectionString", connectString);
            return builder;
        }

        public static IServiceProvider UseQuartz(this IServiceProvider provider)
        {
            LogProvider.SetCurrentLogProvider(provider.GetRequiredService<LoggingProvider>());

            var sched = provider.GetRequiredService<IScheduler>();
            sched.Start();

            return provider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ensureDatabaseCreated"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IApplicationBuilder UseQuartz(this IApplicationBuilder builder,
            bool ensureDatabaseCreated = false)
        {
            LogProvider.SetCurrentLogProvider(builder.ApplicationServices.GetRequiredService<LoggingProvider>());

            if (ensureDatabaseCreated)
            {
                EnsureDatabaseCreated();
            }

            var sched = builder.ApplicationServices.GetRequiredService<IScheduler>();
            sched.Start();


            return builder;
        }

        private static void EnsureDatabaseCreated()
        {
            using var conn = DBConnectionManager.Instance.GetConnection("myDs");
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = $"SELECT count(*) FROM QRTZ_LOCKS";
                command.ExecuteScalar();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("doesn't exist"))
                {
                    var command = conn.CreateCommand();
                    var connName = conn.GetType().FullName;

                    if (connName == "MySql.Data.MySqlClient.MySqlConnection")
                    {
                        command.CommandText = DatabaseHelper.GetMysqlScript();
                    }
                    else if (connName == "System.Data.SqlClient.SqlConnection")
                    {
                        command.CommandText = DatabaseHelper.GetSqlServerScript();
                    }
                    else
                    {
                        throw new ArgumentException($"Can't handle this DBConnection: {connName}");
                    }

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
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

			builder.Services.AddSingleton(provider =>
			{
				LogProvider.SetCurrentLogProvider(provider.GetRequiredService<LoggingProvider>());
				return provider.GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult();
			});
			builder.Services.AddSingleton<ISchedulerFactory>(provider => new StdSchedulerFactory(builder.Properties));
			builder.Services.AddSingleton<LoggingProvider>();
			return services;
		}

		public static QuartzOptionsBuilder UseSchedulerName(this QuartzOptionsBuilder builder, string name)
		{
			builder.Properties.Set("schedName", name);
			return builder;
		}

		public static QuartzOptionsBuilder UseMemoryStore(this QuartzOptionsBuilder builder)
		{
			builder.Services.AddSingleton(new UsedMemoryStore());
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

		public static IServiceProvider UseQuartz(this IServiceProvider provider, bool ensureDatabaseCreated = false)
		{
			var sched = provider.GetRequiredService<IScheduler>();

			var usedMemoryStore = provider.GetServices<UsedMemoryStore>().Any();
			if (!usedMemoryStore && ensureDatabaseCreated)
			{
				EnsureDatabaseCreated();
			}

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
			builder.ApplicationServices.UseQuartz(ensureDatabaseCreated);
			return builder;
		}

		private static void EnsureDatabaseCreated()
		{
			using var conn = DBConnectionManager.Instance.GetConnection("myDs");
			conn.Open();

			try
			{
				conn.Execute("SELECT count(*) FROM QRTZ_LOCKS");
			}
			catch (Exception e)
			{
				if (e.Message.Contains("doesn't exist"))
				{
					var connName = conn.GetType().FullName;

					switch (connName)
					{
						case "MySql.Data.MySqlClient.MySqlConnection":
						{
							conn.Execute(DatabaseHelper.GetMysqlScript());
							break;
						}
						case "System.Data.SqlClient.SqlConnection":
						{
							conn.Execute(DatabaseHelper.GetSqlServerScript());
							break;
						}
						default:
						{
							throw new ArgumentException($"Can't handle this DBConnection: {connName}");
						}
					}
				}
			}
		}

		private static void Execute(this IDbConnection conn, string sql)
		{
			var command = conn.CreateCommand();
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}
	}
}

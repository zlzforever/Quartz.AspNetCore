using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz.AspNetCore.Logging;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Quartz.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddQuartz(this IServiceCollection services,
			Action<QuartzOtpionsBuilder> optionsAction = null)
		{
			var builder = new QuartzOtpionsBuilder { Services = services };
			builder.Properties = new NameValueCollection
			{
				{ "quartz.jobStore.useProperties", "true" }
			};
			optionsAction?.Invoke(builder);

			var serverSched = new StdSchedulerFactory(builder.Properties).GetScheduler().Result;
			builder.Services.AddSingleton(serverSched);
			builder.Services.AddTransient<ISchedulerFactory, StdSchedulerFactory>();
			builder.Services.AddTransient<ISchedulerFactory, StdSchedulerFactory>();
			builder.Services.AddTransient<LoggingProvider>();
			return services;
		}

		public static QuartzOtpionsBuilder UseSchedulerName(this QuartzOtpionsBuilder builder, string name)
		{
			builder.Properties.Set("schedName", name);
			return builder;
		}

		public static QuartzOtpionsBuilder UseMemoryStore(this QuartzOtpionsBuilder builder)
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

		public static QuartzOtpionsBuilder UseSqlServer(this QuartzOtpionsBuilder builder, string connectString, string serializerType = "binary", string tablePrefix = "QRTZ_")
		{
			builder.Properties.Set("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
			builder.Properties.Set("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
			builder.Properties.Set("quartz.jobStore.dataSource", "myDs");
			builder.Properties.Set("quartz.dataSource.myDs.provider", "SqlServer");
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

		public static IApplicationBuilder UseQuartz(this IApplicationBuilder builder)
		{
			LogProvider.SetCurrentLogProvider(builder.ApplicationServices.GetRequiredService<LoggingProvider>());
			var sched = builder.ApplicationServices.GetRequiredService<IScheduler>();
			sched.Start();
			return builder;
		}
	}
}

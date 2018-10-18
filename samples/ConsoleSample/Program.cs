using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using Quartz.AspNetCore;
using System.Collections.Specialized;
using Quartz;
using System.Threading.Tasks;

namespace ConsoleSample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("[1] UseMemoryStore");
			Console.WriteLine("[2] UseMemoryStore");
			var store = Console.ReadLine();
			if (store.Trim() == "1")
			{
				UseMemoryStore().Wait();
			}
			else
			{
				// Make sure you database is prepared.
				UseSqlServer().Wait();

			}
			Console.Read();
		}

		static async Task UseMemoryStore()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddLogging(options =>
			{
				options.AddConsole();
				options.AddSerilog();
			});
			services.AddQuartz(options =>
			{
				options.UseMemoryStore();
			});

			var provider = services.BuildServiceProvider();
			provider.UseQuartz();

			var sched = provider.GetRequiredService<IScheduler>();
			var job1 = JobBuilder.Create<MemorySimpleJob>().WithIdentity("job1").Build();
			var trigger1 = TriggerBuilder.Create().WithIdentity("trigger1").WithCronSchedule("*/5 * * * * ?").Build();
			await sched.ScheduleJob(job1, trigger1);
		}

		static async Task UseSqlServer()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddLogging(options =>
			{
				options.AddConsole();
				options.AddSerilog();
			});
			services.AddQuartz(options =>
			{
				options.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=QUARTZ;Integrated Security=True");
			});

			var provider = services.BuildServiceProvider();
			provider.UseQuartz();

			var sched = provider.GetRequiredService<IScheduler>();
			if (!await sched.CheckExists(new JobKey("job2")))
			{
				var job1 = JobBuilder.Create<SqlServerSimpleJob>().WithIdentity("job2").Build();
				var trigger1 = TriggerBuilder.Create().WithIdentity("trigger2").WithCronSchedule("*/5 * * * * ?").Build();
				await sched.ScheduleJob(job1, trigger1);
			}
		}

		class MemorySimpleJob : IJob
		{
			public virtual Task Execute(IJobExecutionContext context)
			{
				JobKey jobKey = context.JobDetail.Key;
				Console.WriteLine(string.Format("MemorySimpleJob says: {0} executing at {1}", jobKey, DateTime.Now.ToString("r")));
				return Task.CompletedTask;
			}
		}

		class SqlServerSimpleJob : IJob
		{
			public virtual Task Execute(IJobExecutionContext context)
			{
				JobKey jobKey = context.JobDetail.Key;
				Console.WriteLine(string.Format("SqlServerSimpleJob says: {0} executing at {1}", jobKey, DateTime.Now.ToString("r")));
				return Task.CompletedTask;
			}
		}
	}
}

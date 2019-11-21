using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.AspNetCore;
using Quartz.AspNetCore.MySqlConnector;

namespace AspNetCoreSample
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddQuartz(options =>
			{
				options.UseMySqlConnector(
					"Database='quartz';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306");
				// options.UseMemoryStore();
			});
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseQuartz(true);

			using (var scope = app.ApplicationServices.CreateScope())
			{
				var provider = scope.ServiceProvider;
				var sched = provider.GetRequiredService<IScheduler>();

				var job1 = JobBuilder.Create<SimpleJob>().WithIdentity("job1").Build();
				if (!sched.CheckExists(job1.Key).Result)
				{
					var trigger1 = TriggerBuilder.Create().WithIdentity("trigger1").WithCronSchedule("*/5 * * * * ?")
						.Build();
					sched.ScheduleJob(job1, trigger1).GetAwaiter().GetResult();
				}
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSample.Controllers;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddQuartz(options =>
            {
                options.UseMySqlConnector(
                    "Database='quartz';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306");
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            loggerFactory.AddConsole(LogLevel.Debug);

//            using (var conn =
//                new MySqlConnection("Database='quartz';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
//            {
//                try
//                {
//                    conn.Execute("select * from QRTZ_LOCKS");
//                }
//                catch (Exception e)
//                {
//                    if (e.Message.Contains("Table 'quartz.QRTZ_LOCKS' doesn't exist"))
//                    {
//                        conn.Execute(DatabaseHelper.GetMysqlScript());
//                    }
//                }
//            }

            app.UseQuartz();

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
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreSample.Models;
using Quartz;

namespace AspNetCoreSample.Controllers
{
	class MemorySimpleJob : IJob
	{
		public virtual Task Execute(IJobExecutionContext context)
		{
			JobKey jobKey = context.JobDetail.Key;
			Console.WriteLine(string.Format("MemorySimpleJob says: {0} executing at {1}", jobKey, DateTime.Now.ToString("r")));
			return Task.CompletedTask;
		}
	}


	public class HomeController : Controller
	{
		private readonly IScheduler _sched;

		public HomeController(IScheduler sched)
		{
			_sched = sched;
		}

		public IActionResult Index()
		{

			var job1 = JobBuilder.Create<MemorySimpleJob>().WithIdentity("job1").Build();
			var trigger1 = TriggerBuilder.Create().WithIdentity("trigger1").WithCronSchedule("*/5 * * * * ?").Build();

			_sched.ScheduleJob(job1, trigger1).Wait();

			return View();
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}

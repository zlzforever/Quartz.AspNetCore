using System;
using System.Threading.Tasks;
using Quartz;

namespace AspNetCoreSample
{
    public class SimpleJob : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            Console.WriteLine($"SimpleJob says: {jobKey} executing at {DateTime.Now:r}");
            return Task.CompletedTask;
        }
    }
}
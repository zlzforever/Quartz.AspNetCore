using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace Quartz.AspNetCore
{
	public class QuartzOptionsBuilder
	{
		public IServiceCollection Services { get; set; }

		public NameValueCollection Properties { get; set; }
	}
}

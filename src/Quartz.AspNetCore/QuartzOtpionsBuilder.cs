using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Quartz.AspNetCore
{
	public class QuartzOptionsBuilder
	{
		public IServiceCollection Services { get; set; }

		public NameValueCollection Properties { get; set; }
	}
}

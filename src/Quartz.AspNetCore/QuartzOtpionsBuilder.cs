using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Quartz.AspNetCore
{
	public class QuartzOtpionsBuilder
	{
		public IServiceCollection Services { get; set; }

		internal NameValueCollection Properties { get; set; }

	}
}

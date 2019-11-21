using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;

namespace Quartz.AspNetCore
{
	public class LoggingProvider : ILogProvider
	{
		private readonly ILoggerFactory _loggerFactory;

		public LoggingProvider(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public Logger GetLogger(string name)
		{
			return (level, func, exception, parameters) =>
			{
				if (func != null)
				{
					var message = func();
					switch (level)
					{
						case Logging.LogLevel.Info:
							{
								_loggerFactory.CreateLogger(name).LogInformation(exception, message, parameters);
								break;
							}
						case Logging.LogLevel.Debug:
							{
								_loggerFactory.CreateLogger(name).LogDebug(exception, message, parameters);
								break;
							}
						case Logging.LogLevel.Error:
						case Logging.LogLevel.Fatal:
							{
								_loggerFactory.CreateLogger(name).LogError(exception, message, parameters);
								break;
							}
						case Logging.LogLevel.Trace:
							{
								_loggerFactory.CreateLogger(name).LogTrace(exception, message, parameters);
								break;
							}
						case Logging.LogLevel.Warn:
							{
								_loggerFactory.CreateLogger(name).LogWarning(exception, message, parameters);
								break;
							}
					}
				}

				return true;
			};
		}

		public IDisposable OpenNestedContext(string message)
		{
			return null;
		}

		public IDisposable OpenMappedContext(string key, string value)
		{
			return null;
		}
	}
}

using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private ILoggerManager _logger;
		public WeatherForecastController(ILoggerManager logger)
		{
			_logger = logger;
		}
		[HttpGet]
		public IEnumerable<string> Get()
		{
			_logger.LogInfo("Вот информационное сообщение от нашего контроллера значений.");
			_logger.LogDebug("Вот отладочное сообщение от нашего контроллера значений.");
			_logger.LogWarn("Вот сообщение предупреждения от нашего контроллера значений.");
			_logger.LogError("Вот сообщение об ошибке от нашего контроллера значений.");
			return new string[] { "value1", "value2" };
		}
	}
}
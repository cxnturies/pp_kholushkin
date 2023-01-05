using Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace pp_kholushkin.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class WeatherForecastController : ControllerBase
	{
		private ILoggerManager _logger;
		private readonly IRepositoryManager _repository;
		public WeatherForecastController(ILoggerManager logger, IRepositoryManager repository)
		{
			_logger = logger;
			_repository = repository;
		}

		/// <summary>
		/// Логирование API
		/// </summary>
		/// <returns>Логи</returns>.
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			_logger.LogInfo("Вот информационное сообщение от нашего контроллера значений.");
			_logger.LogDebug("Вот отладочное сообщение от нашего контроллера значений.");
			_logger.LogWarn("Вот сообщение предупреждения от нашего контроллера значений.");
			_logger.LogError("Вот сообщение об ошибке от нашего контроллера значений.");
			//_repository.Company.AnyMethodFromCompanyRepository();
			//_repository.Employee.AnyMethodFromEmployeeRepository();
			return new string[] { "value1", "value2" };
		}
	}
}
using Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/companies")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "v2")]
	public class CompaniesV2Controller : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		public CompaniesV2Controller(IRepositoryManager repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Получает список всех компаний
		/// </summary>
		/// <returns>Список компаний</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetCompanies()
		{
			var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
			return Ok(companies);
		}
	}
}
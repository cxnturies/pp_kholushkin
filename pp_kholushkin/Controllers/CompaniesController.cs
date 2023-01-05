using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;
using pp_kholushkin.ModelBinders;
using System.Threading.Tasks;
using pp_kholushkin.ActionFilters;
using Microsoft.AspNetCore.Authorization;

namespace pp_kholushkin.Controllers
{
	[Route("api/companies")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "v1")]
	public class CompaniesController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;

		public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}


		/// <summary>
		/// Получает список всех компаний
		/// </summary>
		/// <returns>Список компаний</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="401"> Не авторизован</response>.
		/// <response code="403"> Доступ запрещён</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet(Name = "GetCompanies"), Authorize]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(403)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetCompanies()
		{
			var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
			var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
			return Ok(companiesDto);
		}

		/// <summary>
		/// Получает данные о компании
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Компания</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet("{id}", Name = "CompanyById")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetCompany(Guid id)
		{
			var company = await _repository.Company.GetCompanyAsync(id, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			else
			{
				var companyDto = _mapper.Map<CompanyDto>(company);
				return Ok(companyDto);
			}
		}

		/// <summary>
		/// Создает вновь созданную компанию
		/// </summary>
		/// <returns>Вновь созданная компания</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpPost(Name = "CreateCompany")]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
		{
			var companyEntity = _mapper.Map<Company>(company);
			_repository.Company.CreateCompany(companyEntity);
			await _repository.SaveAsync();
			var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
			return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
		}

		/// <summary>
		/// Получает коллекцию компании
		/// </summary>
		/// <param name="ids"></param>.
		/// <returns> Коллекция компании</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpGet("collection/({ids})", Name = "CompanyCollection")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				_logger.LogError("Parameter ids is null");
				return BadRequest("Parameter ids is null");
			}
			var companyEntities = await _repository.Company.GetByIdsAsync(ids, trackChanges: false);
			if (ids.Count() != companyEntities.Count())
			{
				_logger.LogError("Some ids are not valid in a collection");
				return NotFound();
			}
			var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
			return Ok(companiesToReturn);
		}

		/// <summary>
		/// Создает коллекцию для компании
		/// </summary>
		/// <returns> Коллекция компании</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpPost("collection")]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
		{
			if (companyCollection == null)
			{
				_logger.LogError("Company collection sent from client is null.");
				return BadRequest("Company collection is null");
			}
			var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
			foreach (var company in companyEntities)
			{
				_repository.Company.CreateCompany(company);
			}
			await _repository.SaveAsync();
			var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
			var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));
			return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
		}

		/// <summary>
		/// Удаляет определённую компанию
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Компания</returns>.
		/// <response code="204"> Элемент удалён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpDelete("{id}")]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
		public async Task<IActionResult> DeleteCompany(Guid id)
		{
			var company = HttpContext.Items["company"] as Company;
			_repository.Company.DeleteCompany(company);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные компании
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Компания</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPut("{id}")]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
		public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
		{
			var companyEntity = HttpContext.Items["company"] as Company;
			_mapper.Map(company, companyEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Получение заголовков запроса
		/// </summary>
		/// <returns>Заголовки запроса</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpOptions]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public IActionResult GetCompaniesOptions()
		{
			Response.Headers.Add("Allow", "GET, OPTIONS, POST");
			return Ok();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using pp_kholushkin.ActionFilters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace pp_kholushkin.Controllers
{
	[Route("api/companies/{companyId}/employees")]
	[ApiController]
	public class EmployeesController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		private readonly IDataShaper<EmployeeDto> _dataShaper;

		public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
			_dataShaper = dataShaper;
		}

		/// <summary>
		/// Получает работников компании
		/// </summary>
		/// <returns>Работники компании</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet]
		[HttpHead]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
		{
			if (!employeeParameters.ValidAgeRange)
				return BadRequest("Max age can't be less than min age.");

			var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
			Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));
			var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
			return Ok(_dataShaper.ShapeData(employeesDto, employeeParameters.Fields));
		}

		/// <summary>
		/// Получает работника компании
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Работник компании</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}
			var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
			if (employeeDb == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			var employee = _mapper.Map<EmployeeDto>(employeeDb);
			return Ok(employee);
		}

		/// <summary>
		/// Создает сотрудника для компании
		/// </summary>
		/// <returns> Сотрудник для компании</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpPost]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
		{
			var employeeEntity = _mapper.Map<Employee>(employee);
			_repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
			await _repository.SaveAsync();
			var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
			return CreatedAtRoute("GetEmployeeForCompany", new
			{
				companyId,
				id = employeeToReturn.Id
			}, employeeToReturn);
		}

		/// <summary>
		/// Удаляет сотрудника в компании
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Сотрудник</returns>.
		/// <response code="204"> Элемент удалён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
		{
			var employeeForCompany = HttpContext.Items["employee"] as Employee;
			_repository.Employee.DeleteEmployee(employeeForCompany);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные о сотруднике
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Сотрудник</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
		{
			var employeeEntity = HttpContext.Items["employee"] as Employee;
			_mapper.Map(employee, employeeEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные о сотруднике
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Сотрудник</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPatch("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null.");
				return BadRequest("patchDoc object is null");
			}
			var employeeEntity = HttpContext.Items["employee"] as Employee;
			var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
			patchDoc.ApplyTo(employeeToPatch, ModelState);
			TryValidateModel(employeeToPatch);
			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the patch document");
				return UnprocessableEntity(ModelState);
			}
			_mapper.Map(employeeToPatch, employeeEntity);
			await _repository.SaveAsync();
			return NoContent();
		}
	}
}
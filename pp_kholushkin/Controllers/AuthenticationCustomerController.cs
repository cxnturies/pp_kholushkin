using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using pp_kholushkin.ActionFilters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthenticationCustomerController : ControllerBase
	{
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		private readonly UserManager<Customer> _customerManager;
		private readonly IAuthenticationManager _authManager;

		public AuthenticationCustomerController(ILoggerManager logger, IMapper mapper, UserManager<Customer> customerManager, IAuthenticationManager authManager)
		{
			_logger = logger;
			_mapper = mapper;
			_customerManager = customerManager;
			_authManager = authManager;
		}

		/// <summary>
		/// Регистрация заказчика
		/// </summary>
		/// <returns>Зарегистрированный заказчик</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPost]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> RegisterCustomer([FromBody] CustomerForRegistrationDto customerForRegistration)
		{
			var customer = _mapper.Map<Customer>(customerForRegistration);
			var result = await _customerManager.CreateAsync(customer, customerForRegistration.Password);
			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.TryAddModelError(error.Code, error.Description);
				}
				return BadRequest(ModelState);
			}
			await _customerManager.AddToRolesAsync(customer, customerForRegistration.Roles);
			return StatusCode(201);
		}

		/// <summary>
		/// Авторизация заказчика
		/// </summary>
		/// <returns>Авторизованный заказчик</returns>.
		/// <response code="200"> Заказчик авторизовался</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPost("login")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> AuthenticateCustomer([FromBody] CustomerForAuthenticationDto customer)
		{
			if (!await _authManager.ValidateCustomer(customer))
			{
				_logger.LogWarn($"{nameof(AuthenticateCustomer)}: Authentication failed. Wrong user name or password.");
				return Unauthorized();
			}
			return Ok(new { Token = await _authManager.CreateTokenCustomer() });
		}
	}
}
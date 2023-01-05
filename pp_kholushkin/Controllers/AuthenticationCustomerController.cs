using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pp_kholushkin.ActionFilters;
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

		[HttpPost]
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

		[HttpPost("login")]
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
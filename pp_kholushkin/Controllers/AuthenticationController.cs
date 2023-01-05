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
	[Route("api/authentication")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		private readonly UserManager<User> _userManager;
		private readonly IAuthenticationManager _authManager;

		public AuthenticationController(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IAuthenticationManager authManager)
		{
			_logger = logger;
			_mapper = mapper;
			_userManager = userManager;
			_authManager = authManager;
		}

		/// <summary>
		/// Регистрация пользователя
		/// </summary>
		/// <returns>Зарегистрированный пользователь</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPost]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistration)
		{
			var user = _mapper.Map<User>(userForRegistration);
			var result = await _userManager.CreateAsync(user, userForRegistration.Password);
			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.TryAddModelError(error.Code, error.Description);
				}
				return BadRequest(ModelState);
			}
			await _userManager.AddToRolesAsync(user, userForRegistration.Roles);
			return StatusCode(201);
		}

		/// <summary>
		/// Авторизация пользователя
		/// </summary>
		/// <returns>Авторизованный пользователь</returns>.
		/// <response code="200"> пользователь авторизовался</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPost("login")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto user)
		{
			if (!await _authManager.ValidateUser(user))
			{
				_logger.LogWarn($"{nameof(Authenticate)}: Authentication failed. Wrong user name or password.");
				return Unauthorized();
			}
			return Ok(new { Token = await _authManager.CreateToken() });
		}
	}
}
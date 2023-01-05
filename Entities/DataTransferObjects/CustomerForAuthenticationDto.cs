using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
	public class CustomerForAuthenticationDto
	{
		[Required(ErrorMessage = "Поле имя пользователя является обязательным")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Поле пароль является обязательным")]
		public string Password { get; set; }
	}
}
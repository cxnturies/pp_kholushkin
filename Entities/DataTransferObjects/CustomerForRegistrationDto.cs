using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
	public class CustomerForRegistrationDto
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		[Required(ErrorMessage = "Поле имя пользователя является обязательным")]
		public string UserName { get; set; }
		[Required(ErrorMessage = "Поле пароль является обязательным")]
		public string Password { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public ICollection<string> Roles { get; set; }
	}
}
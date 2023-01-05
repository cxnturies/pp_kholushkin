using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
	public class Customer : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
using Entities.DataTransferObjects;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IAuthenticationManager
	{
		Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);
		Task<bool> ValidateCustomer(CustomerForAuthenticationDto customerForAuth);
		Task<string> CreateToken();
		Task<string> CreateTokenCustomer();
	}
}
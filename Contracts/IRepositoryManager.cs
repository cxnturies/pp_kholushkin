using System.Threading.Tasks;

namespace Contracts
{
	public interface IRepositoryManager
	{
        ICompanyRepository Company { get; }
        IEmployeeRepository Employee { get; }
        IOrderRepository Order { get; }
        IProductRepository Product { get; }
        Task SaveAsync();
    }
}
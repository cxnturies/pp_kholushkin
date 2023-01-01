namespace Contracts
{
	public interface IRepositoryManager
	{
		ICompanyRepository Company { get; }
		IEmployeeRepository Employee { get; }
		IOrderRepository Order { get; }
		IProductRepository Product { get; }
		void Save();
	}
}
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
	public class OrderRepository : RepositoryBase<Order>, IOrderRepository
	{
		public OrderRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{
		}

		public IEnumerable<Order> GetAllOrders(bool trackChanges) => FindAll(trackChanges)
			.OrderBy(c => c.Id)
			.ToList();
	}
}
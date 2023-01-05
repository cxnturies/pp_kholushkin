using Contracts;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
	public class OrderRepository : RepositoryBase<Order>, IOrderRepository
	{
		public OrderRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{
		}

		public async Task<IEnumerable<Order>> GetAllOrdersAsync(bool trackChanges) => await FindAll(trackChanges)
			.OrderBy(c => c.Id)
			.ToListAsync();

		public async Task<Order> GetOrderAsync(Guid orderId, bool trackChanges) => await FindByCondition(c => c.Id.Equals(orderId), trackChanges)
			.SingleOrDefaultAsync();

		public void CreateOrder(Order order) => Create(order);

		public async Task<IEnumerable<Order>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) => await FindByCondition(x => ids.Contains(x.Id), trackChanges)
			.ToListAsync();

		public void DeleteOrder(Order order)
		{
			Delete(order);
		}
	}
}
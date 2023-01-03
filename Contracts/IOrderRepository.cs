using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
	public interface IOrderRepository
	{
		IEnumerable<Order> GetAllOrders(bool trackChanges);
		Order GetOrder(Guid orderId, bool trackChanges);
		void CreateOrder(Order order);
		IEnumerable<Order> GetByIds(IEnumerable<Guid> ids, bool trackChanges);
	}
}
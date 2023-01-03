using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
	public interface IOrderRepository
	{
		IEnumerable<Order> GetAllOrders(bool trackChanges);
		Order GetOrder(Guid orderId, bool trackChanges);
	}
}
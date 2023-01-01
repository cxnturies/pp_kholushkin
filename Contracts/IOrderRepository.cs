using Entities.Models;
using System.Collections.Generic;

namespace Contracts
{
	public interface IOrderRepository
	{
		IEnumerable<Order> GetAllOrders(bool trackChanges);
	}
}
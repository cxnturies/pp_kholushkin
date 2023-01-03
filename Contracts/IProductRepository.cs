using Entities.Models;
using System.Collections.Generic;
using System;

namespace Contracts
{
	public interface IProductRepository
	{
		IEnumerable<Product> GetProducts(Guid orderId, bool trackChanges);
		Product GetProduct(Guid orderId, Guid Id, bool trackChanges);
	}
}
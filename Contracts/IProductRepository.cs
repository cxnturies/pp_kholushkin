using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
	public interface IProductRepository
	{
		IEnumerable<Product> GetProducts(Guid orderId, bool trackChanges);
		Product GetProduct(Guid orderId, Guid Id, bool trackChanges);
		void CreateProductForOrder(Guid orderId, Product product);
		void DeleteProduct(Product product);
	}
}
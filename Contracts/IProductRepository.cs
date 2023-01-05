using Entities.Models;
using Entities.RequestFeatures.MetaData;
using Entities.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IProductRepository
	{
		Task<PagedList<Product>> GetProductsAsync(Guid orderId, ProductParameters productParameters, bool trackChanges);
		Task<Product> GetProductAsync(Guid orderId, Guid id, bool trackChanges);
		void CreateProductForOrder(Guid orderId, Product product);
		void DeleteProduct(Product product);
	}
}
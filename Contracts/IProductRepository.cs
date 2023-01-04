using Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IProductRepository
	{
        Task<IEnumerable<Product>> GetProductsAsync(Guid orderId, bool trackChanges);
        Task<Product> GetProductAsync(Guid orderId, Guid id, bool trackChanges);
        void CreateProductForOrder(Guid orderId, Product product);
        void DeleteProduct(Product product);
    }
}
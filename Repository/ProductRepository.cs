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
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        public ProductRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(Guid orderId, bool trackChanges) => await FindByCondition(e => e.OrderId.Equals(orderId), trackChanges)
            .OrderBy(e => e.Name)
            .ToListAsync();

        public async Task<Product> GetProductAsync(Guid orderId, Guid id, bool trackChanges) => await FindByCondition(e => e.OrderId.Equals(orderId) && e.Id.Equals(id), trackChanges)
            .SingleOrDefaultAsync();
        public void CreateProductForOrder(Guid orderId, Product product)
        {
            product.OrderId = orderId;
            Create(product);
        }

        public void DeleteProduct(Product product)
        {
            Delete(product);
        }
    }
}
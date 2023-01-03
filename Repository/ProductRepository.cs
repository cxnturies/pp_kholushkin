using Contracts;
using Entities;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
	public class ProductRepository : RepositoryBase<Product>, IProductRepository
	{
		public ProductRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{
		}

		public IEnumerable<Product> GetProducts(Guid orderId, bool trackChanges) => FindByCondition(e => e.OrderId.Equals(orderId), trackChanges).OrderBy(e => e.Name);

		public Product GetProduct(Guid orderId, Guid Id, bool trackChanges) => FindByCondition(e => e.OrderId.Equals(orderId) && e.Id.Equals(Id), trackChanges).SingleOrDefault();
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
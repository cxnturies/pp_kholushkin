using Contracts;
using Entities;
using Entities.Models;
using Entities.RequestFeatures.MetaData;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Repository.Extensions;


namespace Repository
{
	public class ProductRepository : RepositoryBase<Product>, IProductRepository
	{
		public ProductRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{
		}

		public async Task<PagedList<Product>> GetProductsAsync(Guid orderId, ProductParameters productParameters, bool trackChanges)
		{
			var products = await FindByCondition(e => e.OrderId.Equals(orderId), trackChanges)
				.FilterProducts(productParameters.MinPrice, productParameters.MaxPrice).Search(productParameters.SearchTerm)
				.Search(productParameters.SearchTerm)
				.Sort(productParameters.OrderBy)
				.OrderBy(e => e.Name).ToListAsync();
			return PagedList<Product>
				.ToPagedList(products, productParameters.PageNumber, productParameters.PageSize);
		}

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
using System;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Configuration
{
	public class ProductConfiguration : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.HasData
			(
				new Product
				{
					Id = new Guid("f1496536-d896-4d73-88e6-d59143a5ab59"),
					Name = "Чай",
					Price = 100,
					OrderId = new Guid("5fb26e60-970d-4da7-9f74-247dcff3684a")
				},
				new Product
				{
					Id = new Guid("1420cd11-4440-406c-9525-ba14f8fde77f"),
					Name = "Наушники",
					Price = 250,
					OrderId = new Guid("5fb26e60-970d-4da7-9f74-247dcff3684a")
				},
				new Product
				{
					Id = new Guid("20f975e8-31ba-453f-ade1-32c975b50601"),
					Name = "Жесткий диск",
					Price = 2500,
					OrderId = new Guid("5fb26e60-970d-4da7-9f74-247dcff3684a")
				});
		}
	}
}
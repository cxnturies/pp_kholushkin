using System;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Configuration
{
	public class OrderConfiguration : IEntityTypeConfiguration<Order>
	{
		public void Configure(EntityTypeBuilder<Order> builder)
		{
			builder.HasData
			(
				new Order
				{
					Id = new Guid("5fb26e60-970d-4da7-9f74-247dcff3684a"),
					IdUser = new Guid("7a8077aa-7ba9-4025-bc2a-b60366179ffc"),
					Date = "13.09.2022",
					Time = new TimeSpan(13, 37, 0),
					NameDistrict = "Пролетарский район",
					Status = "Доставлено"
				}
			);
		}
	}
}
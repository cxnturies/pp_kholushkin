using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
	public class Product
	{
		[Column("Идентификационный номер товара")]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Дата заказа является обязательным полем")]
		[Column("Название товара")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Дата заказа является обязательным полем")]
		[Column("Цена товара")]
		public double Price { get; set; }

		[Column("Идентификационный номер заказа")]
		[ForeignKey(nameof(Order))]
		public Guid OrderId { get; set; }
		public Order Order { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
	public class Order
	{
		[Column("Идентификационный номер заказа")]
		public Guid Id { get; set; }

		[Column("Идентификационный номер пользователя")]
		[Required(ErrorMessage = "Идентификационный номер пользователя является обязательным полем")]
		public Guid IdUser { get; set; }

		[Column("Дата заказа")]
		[Required(ErrorMessage = "Дата заказа является обязательным полем")]
		[MaxLength(10, ErrorMessage = "Максимальная длина дата заказа - 10 символов.")]
		public string Date { get; set; }

		[Column("Время заказа")]
		[Required(ErrorMessage = "Время заказа является обязательным полем")]
		[MaxLength(5, ErrorMessage = "Максимальная длина время заказа - 5 символов")]
		public TimeSpan Time { get; set; }

		[Column("Район доставки")]
		[Required(ErrorMessage = "Район доставки является обязательным полем")]
		public string NameDistrict { get; set; }

		[Column("Статус заказа")]
		[Required(ErrorMessage = "Статус заказ является обязательным полем")]
		[MaxLength(30, ErrorMessage = "Максимальная длина статуса заказа - 30 символов.")]
		public string Status { get; set; }

		public ICollection<Product> Products { get; set; }
	}
}
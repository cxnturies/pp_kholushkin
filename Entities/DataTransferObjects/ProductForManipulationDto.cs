using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
	public abstract class ProductForManipulationDto
	{
		[Required(ErrorMessage = "Название является обязательным полем")]
		public string Name { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Поле цена не может быть меньше нуля")]
		[Required(ErrorMessage = "Цена является обязательным полем")]
		public double Price { get; set; }
	}
}
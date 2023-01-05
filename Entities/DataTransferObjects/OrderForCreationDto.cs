using System;
using System.Collections.Generic;

namespace Entities.DataTransferObjects
{
	public class OrderForCreationDto
	{
		public Guid Id { get; set; }
		public Guid IdUser { get; set; }
		public string Date { get; set; }
		public string Time { get; set; }
		public string NameDistrict { get; set; }
		public string Status { get; set; }
		public IEnumerable<ProductForCreationDto> Products { get; set; }
	}
}
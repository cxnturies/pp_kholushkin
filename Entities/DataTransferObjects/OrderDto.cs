using System;

namespace Entities.DataTransferObjects
{
	public class OrderDto
	{
		public Guid Id { get; set; }
		public string DateAndTime { get; set; }
		public string Status { get; set; }
	}
}
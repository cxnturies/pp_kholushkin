using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.RequestFeatures
{
	public class ProductParameters : RequestParameters
	{
		public ProductParameters()
		{
			OrderBy = "name";
		}

		public uint MinPrice { get; set; }
		public uint MaxPrice { get; set; } = int.MaxValue;
		public bool ValidPriceRange => MaxPrice > MinPrice;
		public string SearchTerm { get; set; }
	}
}
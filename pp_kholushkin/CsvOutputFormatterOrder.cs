using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pp_kholushkin
{
	public class CsvOutputFormatterOrder : TextOutputFormatter
	{
		public CsvOutputFormatterOrder()
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);
		}

		protected override bool CanWriteType(Type type)
		{
			if (typeof(OrderDto).IsAssignableFrom(type) || typeof(IEnumerable<OrderDto>).IsAssignableFrom(type))
			{
				return base.CanWriteType(type);
			}

			return false;
		}

		public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
		{
			var response = context.HttpContext.Response;
			var buffer = new StringBuilder();
			if (context.Object is IEnumerable<OrderDto>)
			{
				foreach (var order in (IEnumerable<OrderDto>)context.Object)
				{
					FormatCsv(buffer, order);
				}
			}
			else
			{
				FormatCsv(buffer, (OrderDto)context.Object);
			}

			await response.WriteAsync(buffer.ToString());
		}

		private static void FormatCsv(StringBuilder buffer, OrderDto order)
		{
			buffer.AppendLine($"{order.Id},\"{order.DateAndTime},\"{order.Status}");
		}
	}
}
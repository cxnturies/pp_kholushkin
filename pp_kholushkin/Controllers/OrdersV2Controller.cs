using Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/orders")]
	[ApiController]
	public class OrdersV2Controller : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		public OrdersV2Controller(IRepositoryManager repository)
		{
			_repository = repository;
		}

		[HttpGet]
		public async Task<IActionResult> GetOrders()
		{
			var orders = await _repository.Order.GetAllOrdersAsync(trackChanges: false);
			return Ok(orders);
		}
	}
}
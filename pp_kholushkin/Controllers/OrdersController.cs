using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using pp_kholushkin.ModelBinders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pp_kholushkin.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/orders")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;

		public OrdersController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> GetOrders()
		{
			var orders = await _repository.Order.GetAllOrdersAsync(trackChanges: false);
			var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
			return Ok(ordersDto);
		}

		[HttpGet("{id}", Name = "OrderById")]
		public async Task<IActionResult> GetOrder(Guid id)
		{
			var order = await _repository.Order.GetOrderAsync(id, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			else
			{
				var orderDto = _mapper.Map<OrderDto>(order);
				return Ok(orderDto);
			}
		}

		[HttpPost]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateOrder([FromBody] OrderForCreationDto order)
		{
			var orderEntity = _mapper.Map<Order>(order);
			_repository.Order.CreateOrder(orderEntity);
			await _repository.SaveAsync();
			var orderToReturn = _mapper.Map<OrderDto>(orderEntity);
			return CreatedAtRoute("OrderById", new { id = orderToReturn.Id }, orderToReturn);
		}

		[HttpGet("collection/({ids})", Name = "OrderCollection")]
		public async Task<IActionResult> GetOrderCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				_logger.LogError("Parameter ids is null");
				return BadRequest("Parameter ids is null");
			}

			var orderEntities = await _repository.Order.GetByIdsAsync(ids, trackChanges: false);

			if (ids.Count() != orderEntities.Count())
			{
				_logger.LogError("Some ids are not valid in a collection");
				return NotFound();
			}
			var ordersToReturn = _mapper.Map<IEnumerable<OrderDto>>(orderEntities);
			return Ok(ordersToReturn);
		}

		[HttpPost("collection")]
		public async Task<IActionResult> CreateOrderCollection([FromBody] IEnumerable<OrderForCreationDto> orderCollection)
		{
			if (orderCollection == null)
			{
				_logger.LogError("Order collection sent from client is null.");
				return BadRequest("Order collection is null");
			}
			var orderEntities = _mapper.Map<IEnumerable<Order>>(orderCollection);
			foreach (var order in orderEntities)
			{
				_repository.Order.CreateOrder(order);
			}
			await _repository.SaveAsync();
			var orderCollectionToReturn = _mapper.Map<IEnumerable<OrderDto>>(orderEntities);
			var ids = string.Join(",", orderCollectionToReturn.Select(c => c.Id));
			return CreatedAtRoute("OrderCollection", new { ids }, orderCollectionToReturn);
		}

		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateOrderExistsAttribute))]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			var order = HttpContext.Items["order"] as Order;
			_repository.Order.DeleteOrder(order);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidateOrderExistsAttribute))]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderForUpdateDto order)
		{
			var orderEntity = HttpContext.Items["order"] as Order;
			_mapper.Map(order, orderEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpOptions]
		public IActionResult GetOrdersOptions()
		{
			Response.Headers.Add("Allow", "GET, OPTIONS, POST");
			return Ok();
		}
	}
}
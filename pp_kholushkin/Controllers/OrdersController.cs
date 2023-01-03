using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using pp_kholushkin.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
		public IActionResult GetOrders()
		{
			var orders = _repository.Order.GetAllOrders(trackChanges: false);
			var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
			return Ok(ordersDto);
		}

		[HttpGet("{id}", Name = "OrderById")]
		public IActionResult GetOrder(Guid id)
		{
			var order = _repository.Order.GetOrder(id, trackChanges: false);
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
		public IActionResult CreateOrder([FromBody] OrderForCreationDto order)
		{
			if (order == null)
			{
				_logger.LogError("OrderForCreationDto object sent from client is null");
				return BadRequest("OrderForCreationDto object is null");
			}

			var orderEntity = _mapper.Map<Order>(order);
			_repository.Order.CreateOrder(orderEntity);
			_repository.Save();
			var orderToReturn = _mapper.Map<OrderDto>(orderEntity);
			return CreatedAtRoute("OrderById", new { id = orderToReturn.Id }, orderToReturn);
		}

		[HttpGet("collection/({ids})", Name = "OrderCollection")]
		public IActionResult GetOrderCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				_logger.LogError("Parameter ids is null");
				return BadRequest("Parameter ids is null");
			}

			var orderEntities = _repository.Order.GetByIds(ids, trackChanges: false);

			if (ids.Count() != orderEntities.Count())
			{
				_logger.LogError("Some ids are not valid in a collection");
				return NotFound();
			}
			var ordersToReturn = _mapper.Map<IEnumerable<OrderDto>>(orderEntities);
			return Ok(ordersToReturn);
		}

		[HttpPost("collection")]
		public IActionResult CreateOrderCollection([FromBody] IEnumerable<OrderForCreationDto> orderCollection)
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
			_repository.Save();
			var orderCollectionToReturn = _mapper.Map<IEnumerable<OrderDto>>(orderEntities);
			var ids = string.Join(",", orderCollectionToReturn.Select(c => c.Id));
			return CreatedAtRoute("OrderCollection", new { ids }, orderCollectionToReturn);
		}
	}
}
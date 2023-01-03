using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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

		[HttpGet("{id}")]
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
	}
}
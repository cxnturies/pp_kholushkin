using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace pp_kholushkin.Controllers
{
	[Route("api/orders/{orderId}/products")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;

		public ProductsController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetProductsForOrder(Guid orderId)
		{
			var order = _repository.Order.GetOrder(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productsFromDb = _repository.Product.GetProducts(orderId, trackChanges: false);
			var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromDb);
			return Ok(productsDto);
		}

		[HttpGet("{id}")]
		public IActionResult GetProductForOrders(Guid orderId, Guid Id)
		{
			var order = _repository.Order.GetOrder(Id, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {Id} doesn't exist in the database.");
				return NotFound();
			}

			var productDb = _repository.Product.GetProduct(orderId, Id, trackChanges: false);
			if (productDb == null)
			{
				_logger.LogInfo($"Product with id: {Id} doesn't exist in the database.");
				return NotFound();
			}

			var product = _mapper.Map<ProductDto>(productDb);
			return Ok(product);
		}
	}
}
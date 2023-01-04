using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public async Task<IActionResult> GetProductsForOrder(Guid orderId)
		{
			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productsFromDb = await _repository.Product.GetProductsAsync(orderId, trackChanges: false);
			var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromDb);
			return Ok(productsDto);
		}

		[HttpGet("{id}", Name = "GetProductForOrder")]
		public async Task<IActionResult> GetProductForOrder(Guid orderId, Guid id)
		{
			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}

			var productDb = await _repository.Product.GetProductAsync(orderId, id, trackChanges: false);
			if (productDb == null)
			{
				_logger.LogInfo($"Product with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			var product = _mapper.Map<ProductDto>(productDb);
			return Ok(product);
		}

		[HttpPost]
		public async Task<IActionResult> CreateProductForOrder(Guid orderId, [FromBody] ProductForCreationDto product)
		{
			if (product == null)
			{
				_logger.LogError("ProductForCreationDto object sent from client is null.");
				return BadRequest("ProductForCreationDto object is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the ProductForCreationDto object");
				return UnprocessableEntity(ModelState);
			}

			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}

			var productEntity = _mapper.Map<Product>(product);
			_repository.Product.CreateProductForOrder(orderId, productEntity);
			await _repository.SaveAsync();
			var productToReturn = _mapper.Map<ProductDto>(productEntity);
			return CreatedAtRoute("GetProductForOrder", new
			{
				orderId,
				id = productToReturn.Id
			}, productToReturn);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProductForOrder(Guid orderId, Guid id)
		{
			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productForOrder = await _repository.Product.GetProductAsync(orderId, id, trackChanges: false);
			if (productForOrder == null)
			{
				_logger.LogInfo($"Product with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			_repository.Product.DeleteProduct(productForOrder);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateProductForOrder(Guid orderId, Guid id, [FromBody] ProductForUpdateDto product)
		{
			if (product == null)
			{
				_logger.LogError("ProductForUpdateDto object sent from client is null.");
				return BadRequest("ProductForUpdateDto object is null");
			}
			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the ProductForUpdateDto object");
				return UnprocessableEntity(ModelState);
			}
			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productEntity = await _repository.Product.GetProductAsync(orderId, id, trackChanges: true);
			if (productEntity == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			_mapper.Map(product, productEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpPatch("{id}")]
		public async Task<IActionResult> PartiallyUpdateProductForOrder(Guid orderId, Guid id, [FromBody] JsonPatchDocument<ProductForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null.");
				return BadRequest("patchDoc object is null");
			}
			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productEntity = await _repository.Product.GetProductAsync(orderId, id, trackChanges: true);
			if (productEntity == null)
			{
				_logger.LogInfo($"Product with id: {id} doesn't exist in the database.");
				return NotFound();
			}
			var productToPatch = _mapper.Map<ProductForUpdateDto>(productEntity);
			patchDoc.ApplyTo(productToPatch, ModelState);
			TryValidateModel(productToPatch);
			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the patch document");
				return UnprocessableEntity(ModelState);
			}
			_mapper.Map(productToPatch, productEntity);
			await _repository.SaveAsync();
			return NoContent();
		}
	}
}
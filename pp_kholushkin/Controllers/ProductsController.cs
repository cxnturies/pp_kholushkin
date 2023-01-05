using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using pp_kholushkin.ActionFilters;
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
		private readonly IDataShaper<ProductDto> _dataShaper;

		public ProductsController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<ProductDto> dataShaper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
			_dataShaper = dataShaper;
		}

		[HttpGet]
		[HttpHead]
		public async Task<IActionResult> GetProductsForOrder(Guid orderId, [FromQuery] ProductParameters productParameters)
		{
			if (!productParameters.ValidPriceRange)
				return BadRequest("Max price can't be less than min price.");

			var order = await _repository.Order.GetOrderAsync(orderId, trackChanges: false);
			if (order == null)
			{
				_logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
				return NotFound();
			}
			var productsFromDb = await _repository.Product.GetProductsAsync(orderId, productParameters, trackChanges: false);
			Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(productsFromDb.MetaData));
			var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromDb);
			return Ok(_dataShaper.ShapeData(productsDto, productParameters.Fields));
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
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateProductForOrder(Guid orderId, [FromBody] ProductForCreationDto product)
		{

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
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		public async Task<IActionResult> DeleteProductForOrder(Guid orderId, Guid id)
		{
			var productForOrder = HttpContext.Items["product"] as Product;
			_repository.Product.DeleteProduct(productForOrder);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> UpdateProductForOrder(Guid orderId, Guid id, [FromBody] ProductForUpdateDto product)
		{
			var productEntity = HttpContext.Items["product"] as Product;
			_mapper.Map(product, productEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		[HttpPatch("{id}")]
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		public async Task<IActionResult> PartiallyUpdateProductForOrder(Guid orderId, Guid id, [FromBody] JsonPatchDocument<ProductForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null.");
				return BadRequest("patchDoc object is null");
			}
			var productEntity = HttpContext.Items["product"] as Product;
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
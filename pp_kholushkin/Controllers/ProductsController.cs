using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using pp_kholushkin.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

		/// <summary>
		/// Получает продукты заказа
		/// </summary>
		/// <returns>Продукты заказа</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet]
		[HttpHead]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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

		/// <summary>
		/// Получает данные о продукте в заказе
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Продукт заказа</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
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

		/// <summary>
		/// Создает продукт для заказа
		/// </summary>
		/// <returns> Продукт для заказа</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpPost]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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

		/// <summary>
		/// Удаляет продукт в заказе
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Продукт</returns>.
		/// <response code="204"> Элемент удалён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> DeleteProductForOrder(Guid orderId, Guid id)
		{
			var productForOrder = HttpContext.Items["product"] as Product;
			_repository.Product.DeleteProduct(productForOrder);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные о продукте
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Продукт</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> UpdateProductForOrder(Guid orderId, Guid id, [FromBody] ProductForUpdateDto product)
		{
			var productEntity = HttpContext.Items["product"] as Product;
			_mapper.Map(product, productEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные о продукте
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Продукт</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPatch("{id}")]
		[ServiceFilter(typeof(ValidateProductForOrderExistsAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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
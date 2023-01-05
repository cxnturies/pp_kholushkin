using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using pp_kholushkin.ActionFilters;
using pp_kholushkin.ModelBinders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/orders")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "v1Customer")]
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


		/// <summary>
		/// Получает список всех заказов
		/// </summary>
		/// <returns>Список заказов</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="401"> Не авторизован</response>.
		/// <response code="403"> Доступ запрещён</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet(Name = "GetOrders"), Authorize]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(403)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> GetOrders()
		{
			var orders = await _repository.Order.GetAllOrdersAsync(trackChanges: false);
			var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
			return Ok(ordersDto);
		}

		/// <summary>
		/// Получает данные о заказе
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Заказ</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpGet("{id}", Name = "OrderById")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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

		/// <summary>
		/// Создает вновь созданный заказ
		/// </summary>
		/// <param name="order"></param>.
		/// <returns>Вновь созданный заказ</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPost(Name = "CreateOrder")]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateOrder([FromBody] OrderForCreationDto order)
		{
			var orderEntity = _mapper.Map<Order>(order);
			_repository.Order.CreateOrder(orderEntity);
			await _repository.SaveAsync();
			var orderToReturn = _mapper.Map<OrderDto>(orderEntity);
			return CreatedAtRoute("OrderById", new { id = orderToReturn.Id }, orderToReturn);
		}

		/// <summary>
		/// Получает коллекцию заказов
		/// </summary>
		/// <param name="ids"></param>.
		/// <returns> Коллекция заказов</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpGet("collection/({ids})", Name = "OrderCollection")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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

		/// <summary>
		/// Создает коллекцию для заказа
		/// </summary>
		/// <returns> Коллекция заказа</returns>.
		/// <response code="201"> Возвращает только что созданный элемент</response>.
		/// <response code="400"> Элемент равен null</response>.
		/// <response code="422"> Модель недействительна</response>.
		[HttpPost("collection")]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
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

		/// <summary>
		/// Удаляет определённый заказ
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Заказ</returns>.
		/// <response code="204"> Элемент удалён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateOrderExistsAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			var order = HttpContext.Items["order"] as Order;
			_repository.Order.DeleteOrder(order);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Обновляет данные заказа
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Заказ</returns>.
		/// <response code="204"> Элемент обновлён</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidateOrderExistsAttribute))]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ProducesResponseType(204)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderForUpdateDto order)
		{
			var orderEntity = HttpContext.Items["order"] as Order;
			_mapper.Map(order, orderEntity);
			await _repository.SaveAsync();
			return NoContent();
		}

		/// <summary>
		/// Получение заголовков запроса
		/// </summary>
		/// <returns>Заголовки запроса</returns>.
		/// <response code="200"> Запрос выполнен успешно</response>.
		/// <response code="400"> Если элемент равен null</response>.
		/// <response code="422"> Если модель недействительна</response>.
		[HttpOptions]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(422)]
		public IActionResult GetOrdersOptions()
		{
			Response.Headers.Add("Allow", "GET, OPTIONS, POST");
			return Ok();
		}
	}
}
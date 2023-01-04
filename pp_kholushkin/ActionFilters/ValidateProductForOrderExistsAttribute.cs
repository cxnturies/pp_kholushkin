using Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace pp_kholushkin.ActionFilters
{
    public class ValidateProductForOrderExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;

        public ValidateProductForOrderExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;
            var orderId = (Guid)context.ActionArguments["orderId"];
            var order = await _repository.Order.GetOrderAsync(orderId, false);

            if (order == null)
            {
                _logger.LogInfo($"Order with id: {orderId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }

            var id = (Guid)context.ActionArguments["id"];
            var product = await _repository.Product.GetProductAsync(orderId, id, trackChanges);

            if (product == null)
            {
                _logger.LogInfo($"Product with id: {id} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("product", product);
                await next();
            }
        }
    }
}
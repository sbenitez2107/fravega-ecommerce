using FravegaEcommerceAPI.Exceptions;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FravegaEcommerceAPI.Controllers
{
    [ApiController]
    [Route("v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var result = await _orderService.CreateOrder(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{orderId}/events")]
        public async Task<IActionResult> AddOrderEvent(int orderId, [FromBody] AddEventRequest request)
        {
            try
            {
                var result = await _orderService.AddOrderEvent(orderId, request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var result = await _orderService.GetOrder(orderId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchOrders([FromQuery] OrderFilter filters)
        {
            var result = await _orderService.FindOrdersByFilter(filters);
            return Ok(result);
        }
    }
}

using AutoMapper;
using FluentValidation;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Exceptions;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.DTOs.Responses;
using FravegaEcommerceAPI.Models.Entities;
using FravegaEcommerceAPI.Repositories;
using FravegaEcommerceAPI.Transitions;
using ValidationException = FravegaEcommerceAPI.Exceptions.ValidationException;

namespace FravegaEcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrderRequest> _orderValidator;
        private readonly IValidator<AddEventRequest> _eventValidator;

        public OrderService(
            IOrderRepository orderRepository,
            IMapper mapper,
            IValidator<CreateOrderRequest> orderValidator,
            IValidator<AddEventRequest> eventValidator
        )
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _orderValidator = orderValidator;
            _eventValidator = eventValidator;
        }

        public async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request)
        {
            await ValidateCreateOrderRequest(request);

            if (await _orderRepository.ExistsExternalReference(
                request.ExternalReferenceId, request.Channel))
                return CreateIdempotentCreateOrderResponse(request);

            var order = _mapper.Map<Order>(request);
            order.Status = OrderStatus.Created;
            order.UpdatedOn =  DateTime.UtcNow;
            order.Events = new List<Event> { CreateInitialEvent() };

            var createdOrder = await _orderRepository.CreateOrder(order);
            return _mapper.Map<CreateOrderResponse>(createdOrder);
        }

        public async Task<AddEventResponse> AddOrderEvent(int orderId, AddEventRequest request)
        {
            await ValidateEventRequest(request);

            var order = await _orderRepository.GetOrderById(orderId)
                ?? throw new NotFoundException("Order not found");

            if (order.Events.Any(e => e.Id == request.Id))
                return CreateIdempotentEventResponse(order);

            ValidateStateTransition(order.Status, OrderStatusHelper.FromString(request.Type));

            var orderEvent = _mapper.Map<Event>(request);
            var updatedOrder = await _orderRepository.AddEvent(orderId, orderEvent);

            return new AddEventResponse(
                orderId,
                order.Status.ToString(),
                updatedOrder.Status.ToString(),
                orderEvent.Date);
        }

        public async Task<GetOrderResponse> GetOrder(int orderId)
        {
            var order = await _orderRepository.GetOrderById(orderId)
            ?? throw new NotFoundException("Order not found");

            return _mapper.Map<GetOrderResponse>(order);
        }

        public async Task<IEnumerable<GetOrderResponse>> FindOrdersByFilter(OrderFilter filters)
        {
            var orders = await _orderRepository.FindOrdersByFilter(filters);
            return _mapper.Map<IEnumerable<GetOrderResponse>>(orders);
        }

        private async Task ValidateCreateOrderRequest(CreateOrderRequest request)
        {
            var validationResult = await _orderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var totalProductsValue = request.Products.Sum(p => p.Price * p.Quantity);
            if (totalProductsValue != request.TotalValue)
                throw new ValidationException($"TotalValue {request.TotalValue} doesn't match products sum {totalProductsValue}");
        }

        private async Task ValidateEventRequest(AddEventRequest request)
        {
            var validationResult = await _eventValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.Errors);
        }

        private void ValidateStateTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            if (!StateTransitions.IsValidTransition(currentStatus, newStatus))
                throw new BusinessRuleException($"Invalid state transition from {currentStatus} to {newStatus}");
        }

        private static Event CreateInitialEvent() => new()
        {
            Id = Guid.NewGuid().ToString(),
            Type = OrderStatus.Created.ToString(),
            Date = DateTime.UtcNow
        };

        private static AddEventResponse CreateIdempotentEventResponse(Order order) => new(
            order.OrderId,
            order.Status.ToString(),
            order.Status.ToString(),
            DateTime.UtcNow);

        private static CreateOrderResponse CreateIdempotentCreateOrderResponse(CreateOrderRequest order) => new(
            1,
            OrderStatus.Created.ToString(),
            DateTime.UtcNow);

    }
}

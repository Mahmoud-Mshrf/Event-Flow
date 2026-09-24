using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Commands.CancelOrder;
using EventFlow.Application.Features.Orders.Commands.CreateOrder;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Queries.GetEventsOrders;
using EventFlow.Application.Features.Orders.Queries.GetMyOrders;
using EventFlow.Application.Features.Orders.Queries.GetOrderById;
using EventFlow.Domain.Orders.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/orders")]
[Authorize]
public sealed class OrdersController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Place an order for event tickets.")]
    [EndpointName("CreateOrder")]
    public async Task<ActionResult> Create(
        [FromBody] CreateOrderCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel a pending order.")]
    [EndpointName("CancelOrder")]
    public async Task<ActionResult> Cancel(
        Guid orderId,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelOrderCommand(orderId), ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(PaginatedList<OrderSummaryDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all orders placed by the current attendee.")]
    [EndpointName("GetMyOrders")]
    public async Task<ActionResult> GetMyOrders(
        [FromQuery] PageRequest page,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetMyOrdersQuery
        {
            AttendeeId = currentUser.UserId,
            Page = page.Page,
            PageSize = page.PageSize
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get order details.")]
    [EndpointName("GetOrderById")]
    public async Task<ActionResult> GetById(
        Guid orderId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetOrderByIdQuery(orderId, currentUser.UserId), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [Route("api/events/{eventId:guid}/orders")]
    [Authorize]
    public sealed class EventOrdersController(
        ISender sender,
        ICurrentTenant currentTenant) : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<OrderSummaryDto>), StatusCodes.Status200OK)]
        [EndpointSummary("Get all orders for an event (organizer view).")]
        [EndpointName("GetEventOrders")]
        public async Task<ActionResult> GetAll(
            Guid eventId,
            [FromQuery] PageRequest page,
            [FromQuery] OrderStatus? status,
            CancellationToken ct)
        {
            var result = await sender.Send(new GetEventOrdersQuery
            {
                EventId = eventId,
                TenantId = currentTenant.TenantGuid,
                Status = status,
                Page = page.Page,
                PageSize = page.PageSize
            }, ct);

            return result.Match(response => Ok(response), Problem);
        }
    }
}
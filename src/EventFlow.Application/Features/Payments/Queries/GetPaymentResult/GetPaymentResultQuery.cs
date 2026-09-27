using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders.Enums;
using MediatR;

namespace EventFlow.Application.Features.Payments.Queries.GetPaymentResult;

public sealed record GetPaymentResultQuery(
    string OrderNumber,
    string? Success,
    string? Hmac,
    IReadOnlyDictionary<string, string> AllQueryParams)
    : IRequest<Result<PaymentResultDto>>;

public sealed record PaymentResultDto(
    string OrderNumber,
    bool IsSuccessful,
    OrderStatus OrderStatus,
    string Message);

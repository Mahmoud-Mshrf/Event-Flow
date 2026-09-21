using System.Reflection;
using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using FluentValidation;
using MediatR;

namespace EventFlow.Application.Common.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        return CreateValidationResult<TResponse>(errors);
    }

    private static TResponse CreateValidationResult<TResponse>(List<Error> errors)
    {
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var implicitOperator = typeof(TResponse)
                .GetMethod(
                    "op_Implicit",
                    BindingFlags.Public | BindingFlags.Static,
                    [typeof(List<Error>)])!;

            return (TResponse)implicitOperator.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException(
            $"Cannot create a validation result for {typeof(TResponse).Name}. " +
            $"Return type must be Result<T>.");
    }
}

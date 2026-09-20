using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EventFlow.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            // Marker structs return NoContent — no meaningful body to return
            if (result.Value is Success or Updated or Deleted)
                return Results.NoContent();

            if (result.Value is Created)
                return Results.StatusCode(201);

            return Results.Ok(result.Value);
        }

        return MapErrors(result.Errors!);
    }

    private static IResult MapErrors(List<Error> errors)
    {
        // If multiple errors exist, all are validation errors —
        // return them together in one UnprocessableEntity response
        if (errors.Count > 1 || errors[0].Type == ErrorType.Validation)
            return Results.UnprocessableEntity(ToValidationProblemDetails(errors));

        var error = errors[0];

        return error.Type switch
        {
            ErrorType.Conflict     => Results.Conflict(ToProblemDetails(error, 409)),
            ErrorType.NotFound     => Results.NotFound(ToProblemDetails(error, 404)),
            ErrorType.Unauthorized => Results.Unauthorized(),
            _                      => Results.Problem(ToProblemDetails(error, 500))
        };
    }

    private static ProblemDetails ToProblemDetails(Error error, int statusCode) => new()
    {
        Title = error.Code,
        Detail = error.Description,
        Status = statusCode,
    };

    private static HttpValidationProblemDetails ToValidationProblemDetails(List<Error> errors)
    {
        // Groups errors by property name — matches RFC 9457 validation problem shape
        // Multiple errors on the same field are grouped under the same key
        var errorDictionary = errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        return new HttpValidationProblemDetails(errorDictionary)
        {
            Status = 422,
            Title = "One or more validation errors occurred.",
        };
    }
}
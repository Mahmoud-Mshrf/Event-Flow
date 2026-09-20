using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EventFlow.Api.Extensions;
public static class ProblemExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
            return Results.Problem();

        if (errors.All(e => e.Type == ErrorType.Validation))
            return ValidationProblem(errors);

        return Problem(errors[0]);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation   => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
            ErrorType.Failure      => StatusCodes.Status400BadRequest,
            ErrorType.Unexpected   => StatusCodes.Status500InternalServerError,
            _                      => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description);
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var errorsDict = errors.ToDictionary(
            e => e.Code,
            e => new[] { e.Description });

        return Results.ValidationProblem(
            errorsDict,
            statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}
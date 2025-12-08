namespace BaseDock.Api.Extensions;

using BaseDock.Domain.Primitives;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : Results.Problem(
                statusCode: GetStatusCode(result.Error),
                title: result.Error.Code,
                detail: result.Error.Message);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(
                statusCode: GetStatusCode(result.Error),
                title: result.Error.Code,
                detail: result.Error.Message);
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, string location)
    {
        return result.IsSuccess
            ? Results.Created(location, result.Value)
            : Results.Problem(
                statusCode: GetStatusCode(result.Error),
                title: result.Error.Code,
                detail: result.Error.Message);
    }

    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            _ when error.Code.Contains("NotFound") => StatusCodes.Status404NotFound,
            _ when error.Code.Contains("Validation") => StatusCodes.Status400BadRequest,
            _ when error.Code.Contains("Conflict") => StatusCodes.Status409Conflict,
            _ when error.Code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            _ when error.Code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

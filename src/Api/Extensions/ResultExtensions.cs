using Api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.Success)
        {
            if (result.Data is null)
                return new NoContentResult();

            return new OkObjectResult(result.Data);
        }

        return result.Message switch
        {
            "unauthorized"   => new UnauthorizedResult(),
            "forbidden"      => new ForbidResult(),
            "not_found"      => new NotFoundResult(),
            "already_exists" => new ConflictResult(),
            "validation_error" => new UnprocessableEntityObjectResult(result.Message),
            _ => new BadRequestObjectResult(result.Message)
        };
    }
}
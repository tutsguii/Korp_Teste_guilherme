using BuildingBlocks.SharedKernel.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.SharedKernel;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

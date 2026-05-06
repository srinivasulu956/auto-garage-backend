using Auto_Garage.Data.AutoGarageAuthDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Middlewares
{
    public class TokenBlacklistMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, AutoGarageAuthDbContext _dbContext)
        {
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "").Trim();

                var isBlacklisted = await _dbContext.BlacklistedTokens
                    .AnyAsync(t => t.Token == token);

                if (isBlacklisted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"Token has been invalidated. Please login again.\"}");
                    return;
                }
            }

            await next(context);
        }
    }
}

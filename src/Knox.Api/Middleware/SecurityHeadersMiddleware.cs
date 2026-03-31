namespace Knox.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment)
{
    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Use relaxed CSP for Swagger UI in development, strict CSP otherwise
        var isSwaggerPath = context.Request.Path.StartsWithSegments("/swagger");
        if (environment.IsDevelopment() && isSwaggerPath)
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;";
        }
        else
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'none'; frame-ancestors 'none'; base-uri 'self';";
        }

        await next(context);
    }
}

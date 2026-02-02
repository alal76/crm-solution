using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CRM.Api.Middleware
{
    /// <summary>
    /// Middleware to add security headers to HTTP responses.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // X-Content-Type-Options: Prevents MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // X-Frame-Options: Prevents clickjacking attacks
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // X-XSS-Protection: Enables XSS filter in older browsers
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Referrer-Policy: Controls how much referrer information is included
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content-Security-Policy: Helps prevent XSS attacks
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                     "style-src 'self' 'unsafe-inline'; " +
                     "img-src 'self' data: https:; " +
                     "font-src 'self' data:; " +
                     "connect-src 'self'; " +
                     "frame-ancestors 'none';";
            context.Response.Headers.Add("Content-Security-Policy", csp);

            // Permissions-Policy: Controls browser features
            var permissionsPolicy = "geolocation=(), microphone=(), camera=(), payment=()";
            context.Response.Headers.Add("Permissions-Policy", permissionsPolicy);

            // Strict-Transport-Security: Forces HTTPS (only add in production with HTTPS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            // Remove Server header to avoid information disclosure
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");

            await _next(context);
        }
    }
}

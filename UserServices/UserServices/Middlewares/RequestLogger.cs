using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace UserServices.Middlewares {
    public class RequestLogger {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLogger(RequestDelegate next, ILoggerFactory loggerFactory) {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestLogger>();
        }

        public async Task Invoke(HttpContext httpContext) {
            _logger.LogInformation("Handling request: " + httpContext.Request.Path);
            var startTime = DateTime.UtcNow;
            var watch = Stopwatch.StartNew();
            await _next.Invoke(httpContext);
            watch.Stop();

            _logger.LogInformation($"Client IP: {httpContext.Connection.RemoteIpAddress.ToString()} \n" +
                $"Request path: {httpContext.Request.Path} \n" +
                $"Request content type: {httpContext.Request.ContentType} \n" +
                $"Request content length: {httpContext.Request.ContentLength} \n" +
                $"Start time: {startTime} \n" +
                $"Duration: {watch.ElapsedMilliseconds} \n");

            _logger.LogInformation("Finished handling request.");
        }
    }

    public static class RequestLoggerExtensions {
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder) {
            return builder.UseMiddleware<RequestLogger>();
        }
    }
}

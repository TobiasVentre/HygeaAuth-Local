using Application.Dtos.Response;
using Application.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace AuthMS.Api.Middleware
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteErrorAsync(context, exception);
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                InvalidValueException => (int)HttpStatusCode.BadRequest,
                InvalidEmailException => (int)HttpStatusCode.BadRequest,
                InvalidRefreshTokenException => (int)HttpStatusCode.BadRequest,
                InactiveUserException => (int)HttpStatusCode.BadRequest,
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var payload = new ApiError
            {
                Message = exception is ValidationException validationException
                    ? string.Join("; ", validationException.Errors.Select(error => error.ErrorMessage))
                    : exception.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}

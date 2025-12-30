using Raffle.Api.Exceptions;
using System.Net;
using System.Text.Json;

namespace Raffle.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (RaffleNotFoundException ex)
            {
                await WriteError(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (RaffleClosedException ex)
            {
                await WriteError(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (NoRaffleMembersException ex)
            {
                await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (RaffleDrawAlreadyExistsException ex)
            {
                await WriteError(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (NoClosedRaffleDrawsException ex)
            {
                await WriteError(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (NoMembersProvidedException ex)
            {
                await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain exception");
                await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteError(context, HttpStatusCode.InternalServerError, "Unexpected error occurred");
            }
        }

        private static async Task WriteError(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                error = message
            });

            await context.Response.WriteAsync(payload);
        }
    }
}

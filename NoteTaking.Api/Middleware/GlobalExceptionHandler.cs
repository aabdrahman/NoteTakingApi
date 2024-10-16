using System;
using Microsoft.AspNetCore.Diagnostics;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
                                          Exception exception, 
                                          CancellationToken cancellationToken)
    {   
        _logger.LogError($"An error occurred processing request: {exception.Message}");

        var errorResponse = new ErrorResponse
        {
            ErrorMessage = exception.Message,
        };

        switch (exception)
        {
            case BadHttpRequestException:
                errorResponse.Title = exception.GetType().Name;
                break;
            
            default:
                errorResponse.ErrorMessage = "Internal Server Error";
                break;
        }


        await httpContext.Response.WriteAsJsonAsync<ErrorResponse>(errorResponse, cancellationToken);

        return true;
       
    }
}

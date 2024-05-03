using FlowWing.API.Controllers;
using FlowWing.Business.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var scope = context.RequestServices.CreateScope())
        {
            var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();

            // İstek bilgilerini al
            var request = context.Request;
            var requestBody = await FormatRequest(request);
            var userEmail = context.Items["UserEmail"]?.ToString() ?? "Anonymous";
            var ip = context.Connection.RemoteIpAddress.ToString();
            
            //clear the null bytes from the request body
            requestBody = requestBody.Replace("\0", "");
            var logMessage = $"Request Method: {request.Method}, Path: {request.Path}, Body: {requestBody}, Email: {userEmail}, IP Address: {ip}";
            await loggingService.CreateLogAsync(logMessage);

            // Continue to the next middleware
            await _next(context);

            var response = context.Response;

            // Log response
            logMessage = $"Response Status: {response.StatusCode}";
            await loggingService.CreateLogAsync(logMessage);
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var buffer = new byte[request.ContentLength ?? 0];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);

        request.Body.Seek(0, SeekOrigin.Begin);

        return bodyAsText;
    }
}
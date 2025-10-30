using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeSignManagement.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private const string GENERAL_ERROR_CODE = "0";
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IJson _json;
        private readonly IDater _dater;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger, IJson json, IDater dater)
        {
            _next = next;
            _logger = logger;
            _json = json;
            _dater = dater;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string requestBody = null;
            try
            {

                requestBody = await FormatRequest(httpContext.Request);
                requestBody = HideInfoInLog(requestBody);
                await _next(httpContext);
            }
            catch (InvalidOperationException ex)
            {
                var generalError = new GeneralError(ex.Message);
                if (!requestBody.Contains("/refresh"))
                {
                   _logger.Error(ex, "Invalid Operation Error. Message: {Errors}.{NewLine} Request: {Request}",
                         generalError.errors["error"], Environment.NewLine, requestBody);
                }
                await HandleExceptionAsync(httpContext, generalError, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "General Error. HResult : {HResult}.  Request: {Request}",
                     ex.HResult, requestBody);
                var error = new GeneralError(GENERAL_ERROR_CODE, ex.HResult.ToString(), _dater.UtcNow());
                await HandleExceptionAsync(httpContext, error, HttpStatusCode.InternalServerError);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, GeneralError error, HttpStatusCode httpStatusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)httpStatusCode;
            var errorJson = _json.Serialize(error);

            return context.Response.WriteAsync(errorJson);
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;
            return $"{request.Method} - {request.Scheme}://{request.Host}{request.Path} {request.QueryString} {Environment.NewLine}{bodyAsText}";
        }
        
        private string HideInfoInLog(string requestBody)
        {
            if (requestBody.Contains("/login"))
            {
                int startIndexPassword = requestBody.LastIndexOf(':') + 3;
                int endIndexPassword = requestBody.LastIndexOf('"');
                int passwordLength = endIndexPassword - startIndexPassword;
                if (passwordLength > 0)
                {
                    string originalPassword = requestBody.Substring(startIndexPassword, passwordLength);
                    requestBody = requestBody.Replace(originalPassword, "****");
                }
            }
            if (requestBody.ToLower().Contains("base64file"))
            {
                var array = requestBody.Split('"');
                int count = array.Count(x => x.ToLower().Contains("base64file"));
                for (int i = 0; i < count; i++)
                {
                    int base64contentIndex = Array.FindIndex(array, x => x.ToLower().Contains("base64file")) + 2;
                    if (base64contentIndex < array.Length)
                    {
                        var parts = array[base64contentIndex].Split(";base64");
                        if (parts.Length == 1)
                        {
                            requestBody = requestBody.Replace(parts[0], string.Empty);
                        }
                        if (parts.Length == 2)
                        {
                            requestBody = requestBody.Replace(parts[1], string.Empty);
                        }
                    }
                    array[base64contentIndex] = string.Empty;
                }
            }
            return requestBody;
        }
    }
}

namespace WeSign.Middlewares
{
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Settings;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Serilog;
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using WeSign.Models.Templates;

    public class ErrorHandlingMiddleware
    {
        private const string GENERAL_ERROR_CODE = "0";
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IJson _json;
        private readonly IDater _dater;
        private readonly GeneralSettings _generalSettings;
       

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger, IJson json, 
            IDater dater, IOptions<GeneralSettings> generalSettings)
        {
            _next = next;
            _logger = logger;
            _json = json;
            _dater = dater;
            _generalSettings = generalSettings.Value;
            
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
                bool setToIgnure = false;
                var generalError = new GeneralError(ex.Message, extraInfo: ex.InnerException?.ToString());
                if(_generalSettings.InvalidOperationExceptionToIgnoreInLog.Count> 0)
                {
                   if(int.TryParse(ex.Message, out int errorId))
                    {
                        if(_generalSettings.InvalidOperationExceptionToIgnoreInLog.Contains(errorId))
                        {
                            setToIgnure = true;
                            
                            _logger.Warning("Skipping invalid operation id {ErrorId} for request body {RequestBody}", errorId, requestBody);

                        }
                    }
                }
                
                if (!requestBody.Contains("/refresh") && !setToIgnure)
                {

                    _logger.Error("{NewLine}{NewLine}--------------{NewLine} Invalid Operation Error. Message: {Errors}. Request: {Request}{NewLine}{Exception}",
                        Environment.NewLine, generalError.errors["error"], requestBody, ex.ToString());
                }
                await HandleExceptionAsync(httpContext, generalError, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error("{NewLine}{NewLine}--------------{NewLine} General Error. HResult : {HResult}.  Request: {Request}{NewLine}{Eception}",
                    Environment.NewLine, ex.HResult, requestBody, ex.ToString());
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
            string bodyAsText = string.Empty;
            if (request.ContentLength > _generalSettings.MaxContentLengthBodyRequest)
            {
                bodyAsText = "Too big content";
            }
            else
            {
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                bodyAsText = Encoding.UTF8.GetString(buffer);
            }
            request.Body.Position = 0;
            return $"{request.Method} - {request.Scheme}://{request.Host}{request.Path} {request.QueryString} {Environment.NewLine}{bodyAsText}";
        }

        private string HideInfoInLog(string requestBody)
        {
            if (requestBody.Contains("/login")|| requestBody.Contains("/sign/verify"))
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
            else if (requestBody.Contains("/password"))
            {
                //TODO UpdatePassword hide NewPassword
            }
            else if (requestBody.ToLower().Contains("base64file"))
            {
                var array = requestBody.Split('"');
                int base64contentIndex = Array.FindIndex(array, x => x.ToLower() == "base64file") + 2;
                if(base64contentIndex < array.Length)
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
            }
            else if(requestBody.Contains("/templates/merge"))
            {
                var array = requestBody.Split('"');
                var base64content = array.FirstOrDefault(x => x.ToLower().Contains(";base64,"));
                if(base64content != null)
                        {
                    requestBody = requestBody.Replace(base64content, "Content Replace was Too big ]");
                }
                
            }
            return requestBody;
        }
    }
    public static class ErrorHandlingMiddlewareExtentions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
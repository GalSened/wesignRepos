using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeSignSigner.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private const string GENERAL_ERROR_CODE = "0";
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IJson _json;
        private readonly IDater _dater;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger log, IJson json, IDater dater)
        {
            _next = next;
            _logger = log;
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
                _logger.Error("{NewLine}{NewLine}--------------{NewLine} Invalid Operation Error. Message: {Errors}. Request: {Request}{NewLine}{Exception}",
                        Environment.NewLine, generalError.errors["error"], requestBody, ex.ToString());
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

        private string HideInfoInLog(string requestBody)
        {
            if (requestBody.ToLower().Contains("\"password\""))
            {
                int startIndexPassword = requestBody.ToLower().LastIndexOf("\"password\"") + "\"password\"".Length +  3;
                int endIndexPassword = requestBody.LastIndexOf('"');
                var done = false;
                for (int i = startIndexPassword; i < requestBody.Length && !done; ++i)
                {
                    if(requestBody[i] == '"')
                    {
                        done = true;
                        endIndexPassword = i;
                    }
                }
                int passwordLength = endIndexPassword - startIndexPassword;
                if (passwordLength > 0)
                {
                    string originalPassword = requestBody.Substring(startIndexPassword, passwordLength);
                    requestBody = requestBody.Replace(originalPassword, "****");
                }
            }
          
            return requestBody;
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

    }
}

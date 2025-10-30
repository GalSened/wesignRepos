using Ganss.Xss;
using System.Text;

namespace PdfExternalService.Middlewares
{
    public class InputHTMLSanitationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public InputHTMLSanitationMiddleware(RequestDelegate next, IHtmlSanitizer htmlSanitizer)
        {
            _next = next;
            _htmlSanitizer = htmlSanitizer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await ReadRequestBody(context);

            await _next(context);
        }

        private async Task ReadRequestBody(HttpContext context)
        {

            using (var buffer = new MemoryStream())
            {
                await context.Request.Body.CopyToAsync(buffer);
                context.Request.Body = buffer;
                buffer.Position = 0;

                var encoding = Encoding.UTF8;


                var requestContent = await new StreamReader(buffer, encoding).ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(requestContent))
                {


                    requestContent = _htmlSanitizer.Sanitize(requestContent);
                    await context.Request.Body.DisposeAsync();
                    context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestContent));
                }
                context.Request.Body.Position = 0;

            }



        }
    }
}

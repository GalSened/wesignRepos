using AspNetCoreRateLimit;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models.Settings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PdfExternalService.Handlers;
using PdfExternalService.Interfaces;
using PdfExternalService.Middlewares;
using PdfExternalService.Models;
using PdfExternalService.Models.DTO;
using PdfExternalService.Validators;
using PdfHandler;
using Serilog;
using Serilog.Exceptions;
using System.IO.Abstractions;

namespace PdfExternalService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddControllers().AddFluentValidation().SetCompatibilityVersion(CompatibilityVersion.Latest);

            builder.Services.AddOptions();
            builder.Services.AddMemoryCache();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

         
            var configuration = new ConfigurationBuilder()
                                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                         .Build();

            var generalSettings = new PDFExternalGeneralSettings();
            builder.Configuration.GetSection("GeneralSettings").Bind(generalSettings);
            builder.Services.Configure<PDFExternalGeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FreeForAllCorsPolicy",
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            });

            builder.Services.AddTransient<IPBKDF2, PBKDF2Handler>();
           
                      
            builder.Services.AddSingleton<Serilog.ILogger>(
                           new LoggerConfiguration()
                           .Enrich.WithExceptionDetails()
                           .ReadFrom.Configuration(configuration)
                           .CreateLogger());
            builder.Services.AddTransient<IJson, JsonHandler>();
      
            builder.Services.AddTransient<IEncryptor, EncryptorHandler>();
            builder.Services.AddTransient<IFileSystem, FileSystem>();
            builder.Services.AddTransient<IPdfOperations , PdfOperationsHandler>();
            builder.Services.AddTransient<IDebenuPdfLibrary, DebenuPDFLibrary>();
            builder.Services.AddTransient<IHtmlSanitizer, HtmlSanitizer>();
            builder.Services.AddTransient<IDataUriScheme, DataUriSchemeHandler>();
            


            builder.Services.AddTransient<IValidator<FileMergeDTO>, FileMergeValidator>();
            builder.Services.AddTransient<IDater, DaterHandler>();
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = generalSettings.MaxRequestBodySize;
            });

            var app = builder.Build();

         
                app.UseSwagger();
                app.UseSwaggerUI();
            

            app.UseHttpsRedirection();

            app.UseAuthorization();
            

            app.UseCors("FreeForAllCorsPolicy");
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("x-total-count", "token-expired", "x-file-name"));
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<InputHTMLSanitationMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}
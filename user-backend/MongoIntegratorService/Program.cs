using HistoryIntegratorService.BackgroundServices;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Extensions;
using HistoryIntegratorService.Middlewares;
using HistoryIntegratorService.Providers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var generalSettings = new GeneralSettings();
builder.Configuration.GetSection(nameof(GeneralSettings)).Bind(generalSettings);

// Add services to the container.
builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
    builder => builder.WithOrigins(generalSettings.UserBackendRoute, generalSettings.ManagementBackendRoute)
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FreeForAllCorsPolicy",
    builder => builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

// Register the background service for specific type
if (generalSettings.UseRabbitMQ)
{
    builder.Services.AddHostedService<DocumentsCollectionConsumeService>();
}

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GuidListModelBinderProvider());
    options.ModelBinderProviders.Insert(1, new EnumListModelBinderProvider());
}).AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new OpenApiInfo { Title = "HistoryIntegrator", Version = "v3" });
});
builder.Services.AddHandlers(builder.Environment, generalSettings);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v3/swagger.json", "WSE History Integrator V3");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("CorsPolicy");

app.Run();
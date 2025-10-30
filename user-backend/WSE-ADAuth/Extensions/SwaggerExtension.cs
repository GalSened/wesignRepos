using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace WSE_ADAuth.Extensions
{

    public static class SwaggerExtension
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo { Title = "WeSign Auth", Version = "v3" });
               
            });
        }
    }
}

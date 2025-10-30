namespace WeSign.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Swagger;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class SwaggerExtension
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // ✅ Define both swagger groups
                //c.SwaggerDoc("api", new OpenApiInfo { Title = "WeSign API", Version = "v3" });
                //c.SwaggerDoc("ui", new OpenApiInfo { Title = "WeSign UI", Version = "v3" });
                c.SwaggerDoc("v3", new OpenApiInfo { Title = "WeSign API", Version = "v3" });

                // ✅ Only include endpoints matching the swagger group
                c.DocInclusionPredicate((docName, apiDesc) => true);
                //c.DocInclusionPredicate((docName, apiDesc) =>
                //{
                //    //if (string.IsNullOrWhiteSpace(apiDesc.GroupName))
                //    //    return false;

                //    return apiDesc.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase) == true;
                //});

                // ✅ JWT Bearer authentication setup
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                // ✅ Optional: Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
        }
    }
}

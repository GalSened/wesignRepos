
using Common.Hubs;
using Common.Models.Settings;
using FluentValidation.AspNetCore;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignerBL.Hubs;
using System;
using WeSignSigner.Extensions;
using WeSignSigner.Middlewares;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.IO;
using Common.Interfaces.RabbitMQ;

namespace WeSignSigner
{
    public class Startup
    {
        private GeneralSettings _generalSettings ;

        // todo: gzip compression settings

        //private StaticFileOptions StaticFileOptions
        //{
        //    get
        //    {
        //        return new StaticFileOptions
        //        {
        //            OnPrepareResponse = OnPrepareResponse
        //        };
        //    }
        //}

        //private void OnPrepareResponse(StaticFileResponseContext context)
        //{
        //    var file = context.File;
        //    var request = context.Context.Request;
        //    var response = context.Context.Response;

        //    if (file.Name.EndsWith(".gz"))
        //    {
        //        response.Headers[HeaderNames.ContentEncoding] = "gzip";
        //        return;
        //    }

        //    if (file.Name.IndexOf(".min.", StringComparison.OrdinalIgnoreCase) != -1)
        //    {
        //        var requestPath = request.Path.Value;
        //        var filePath = file.PhysicalPath;

        //        //if (IsDevelopment)
        //        //{
        //            if (File.Exists(filePath.Replace(".min.", ".")))
        //            {
        //                response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
        //                response.Headers[HeaderNames.Location] = requestPath.Replace(".min.", ".");
        //            }
        //        //}
        //        else
        //        {
        //            var acceptEncoding = (string)request.Headers[HeaderNames.AcceptEncoding];
        //            if (acceptEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) != -1)
        //            {
        //                if (File.Exists(filePath + ".gz"))
        //                {
        //                    response.StatusCode = (int)HttpStatusCode.MovedPermanently;
        //                    response.Headers[HeaderNames.Location] = requestPath + ".gz";
        //                }
        //            }
        //        }
        //    }
        //}

        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
            _generalSettings = new GeneralSettings();
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddFluentValidation().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddConfiguration(Configuration);

            Configuration.GetSection("GeneralSettings").Bind(_generalSettings);
            services.AddCors(options =>
            {
                    options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(_generalSettings.SignerFronendApplicationRoute,
                                                   _generalSettings.UserFronendApplicationRoute)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddDetection();
           services.AddSignalR(hubOptions => {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(3);
                hubOptions.MaximumReceiveMessageSize = 65536; // bytes ~ 64KB
                //hubOptions.MaximumReceiveMessageSize = null;
            });
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("CanCreateSingleLink", policy =>
            //        policy.Requirements.Add(new CanCreateSingleLinkAuth()));
            //}); // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
            services.AddHandlers(_generalSettings);
            services.AddValidation();
            if (_generalSettings.ShowSwaggerUI)
            {
                services.AddSwagger();
            }
            if (_generalSettings.MaxUploadFileSize > 0)
            {
                services.Configure<IISServerOptions>(options =>
                {
                    options.MaxRequestBodySize = _generalSettings.MaxUploadFileSize;
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (_generalSettings.ShowSwaggerUI)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v3/swagger.json", "WeSign Signer V3");
                });
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseDetection();

            app.UseIpRateLimiting();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("x-total-count", "x-file-name", "x-file-content"));
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseInputHTMLSanititionMiddleware();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                // prod path will look like this https://devtest.comda.co.il/signerapi/v3/smartcardsocket
                // debug path will look like this https://localhost:44357/v3/smartcardsocket
                endpoints.MapHub<SmartCardSigningHub>("v3/smartcardsocket");
                // prod path will look like this https://devtest.comda.co.il/signerapi/v3/livesocket
                // debug path will look like this https://localhost:44357/v3/livesocket                
                endpoints.MapHub<LiveHub>("v3/livesocket");
                // prod path will look like this https://devtest.comda.co.il/signerapi/v3/agentsocket
                // debug path will look like this https://localhost:44357/v3/agentsocket
                endpoints.MapHub<AgentHub>("v3/agentsocket");
                // prod path will look like this https://devtest.comda.co.il/signerapi/v3/identitsocket
                // debug path will look like this https://localhost:44357/v3/identitsocket
                endpoints.MapHub<IdentityHub>("v3/identitsocket");


            });
            LoadMQConnectors(app);

            //app.UseDefaultFiles()
            //    .UseStaticFiles(StaticFileOptions);
        }

        private void LoadMQConnectors(IApplicationBuilder app)
        {
            var rabbitMQSettings = new RabbitMQSettings();
            Configuration.GetSection("RabbitMQSettings").Bind(rabbitMQSettings);
            if (rabbitMQSettings != null && rabbitMQSettings.UseRabbitSync)
            {
                app.ApplicationServices.GetService<IMessageQSmartCardConnector>();
                app.ApplicationServices.GetService<IMessageMQLiveConnector>();
                app.ApplicationServices.GetService<IMessageMQAgentConnector>();
                app.ApplicationServices.GetService<IMessageMQSignerIdentityConnector>();
            }
        }
    }
}

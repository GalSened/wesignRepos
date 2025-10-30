using AspNetCoreRateLimit;
using Common.Interfaces.ManagementApp;
using Common.Models.Settings;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WeSignManagement.Binders;
using WeSignManagement.Extensions;
using WeSignManagement.Helpers;
using WeSignManagement.Middlewares;
using WeSignManagement.Providers;

namespace WeSignManagement
{
    public class Startup
    {
        private GeneralSettings _generalSettings;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddFluentValidation().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new GuidListModelBinderProvider());
                options.ModelBinderProviders.Insert(1, new EnumListModelBinderProvider());
            })
                .AddNewtonsoftJson();
            services.AddRazorPages();
             _generalSettings = new GeneralSettings();
            Configuration.GetSection("GeneralSettings").Bind(_generalSettings);
            services.AddConfiguration(Configuration);
            services.AddBearerAuthentication(Configuration);
            services.AddHttpContextAccessor();
            services.AddHandlers(_generalSettings);
            services.AddValidation();
            if (_generalSettings.ShowSwaggerUI)
            {
                services.AddSwagger();
            }
            services.AddHangfireJobs();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {

            if (_generalSettings.ShowSwaggerUI)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v3/swagger.json", "WeSign Management V3");
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

            app.UseIpRateLimiting();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("x-total-count", "token-expired", "x-file-name"));
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region hangfire
            if (!_generalSettings.DisableHangfire)
            {
                if (_generalSettings.AuthorizeHangfire)
                {
                    app.UseHangfireDashboard("/jobs", new DashboardOptions
                    {
                        Authorization = new[] { new HangfireAuthorizationHelper() }
                    });
                }
                else
                {
                    app.UseHangfireDashboard("/jobs");
                }
                app.UseHangfireServer();



                //backgroundJobClient.Enqueue(() => serviceProvider.GetService<IJobs>().DeleteLogsFromDB());
                //backgroundJobClient.Enqueue(() => serviceProvider.GetService<IJobs>().CreateDataFolders());
                //backgroundJobClient.Enqueue(() => serviceProvider.GetService<IJobs>().SendProgramExpiredNotification());

                AddJobs(recurringJobManager, serviceProvider);


                RecurringJob.Trigger("Create AD Users and Contacts");
            }
            #endregion
        }

        private static void AddJobs(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {
            var _jobService = serviceProvider.GetService<IJobs>();

            //recurringJobManager.AddOrUpdate("Delete Documents",
            //                                () => _jobService.DeleteDocumentsFromServer(),
            //                                Cron.Daily);


            // every 3 hours
            recurringJobManager.AddOrUpdate("Clean DB",
                                            () => _jobService.CleanDb(),
                                            "0 */3 * * *");


            recurringJobManager.AddOrUpdate("Create AD Users and Contacts",
                                            () => _jobService.CreateActiveDirectoryUsersAndContacts(),
                                           "0 22 * * *");

            recurringJobManager.AddOrUpdate("Send Program Expired Notification",
                                            () => _jobService.SendProgramExpiredNotification(),
                                            Cron.Daily);
            // every 3 hours
            recurringJobManager.AddOrUpdate("Reset Programs Utilizations",
                                            () => _jobService.ResetProgramsUtilization(),
                                            "0 */3 * * *");

            // every 5 minutes - "*/5 * * * *"
            recurringJobManager.AddOrUpdate("Send Program Capacity Is About To Expired Notification",
                                            () => _jobService.SendProgramCapacityIsAboutToExpiredNotification(), Cron.Daily(6));


            // every 5 minutes - "*/5 * * * *"
            recurringJobManager.AddOrUpdate("Clear Logs DB jobs",
                                            () => _jobService.DeleteLogsFromDB(), "*/20 * * * *");

            // once a day - at 6 UTC (8:00 am Israel time)
            recurringJobManager.AddOrUpdate("Send Document Is About To Be Deleted Notification",
                                            () => _jobService.SendDocumentIsAboutToBeDeletedNotification(),
                                            Cron.Daily(6));


            recurringJobManager.AddOrUpdate("Delete Unused template and contacts",
                                            () => _jobService.CleanUnusedTemplatesAndContacts(),
                                                 Cron.Daily(5));


            recurringJobManager.AddOrUpdate("Send sign reminder notification",
                () => _jobService.SendSignReminders(),
                Cron.Daily(8));

            recurringJobManager.AddOrUpdate("Send user periodic reports",
                () => _jobService.SendUserPeriodicReports(),
                Cron.Daily(8));

            recurringJobManager.AddOrUpdate("Send management periodic reports",
                () => _jobService.SendManagementPeriodicReports(),
                Cron.Daily(9));

            recurringJobManager.AddOrUpdate("Delete expired periodic report files",
                            () => _jobService.DeleteExpiredPeriodicReportFiles(),
                            Cron.Daily(10));

            recurringJobManager.AddOrUpdate("Update expired signer tokens",
                                            () => _jobService.UpdateExpiredSignerTokens(),
                                            Cron.Daily());
        }
    }
}

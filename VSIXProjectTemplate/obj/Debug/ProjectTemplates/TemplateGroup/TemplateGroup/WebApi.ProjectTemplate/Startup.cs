using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
$if$ ("$ext_useSerilog$" == "True")using Serilog;
using Serilog.Enrichers;$endif$
$if$ ("$ext_useSwagger$" == "True")using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;$endif$
using System.IO;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
$if$ ("$ext_useHealth$" == "True")using $safeprojectname$.Health.Models;$endif$
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using $safeprojectname$.DAL;
using $safeprojectname$.DAL.DataProvider;
using $safeprojectname$.DAL.Repositories;
using $safeprojectname$.BL;

namespace $safeprojectname$
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			$if$ ("$ext_useSerilog$" == "True")
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.With(new ThreadIdEnricher())
                .CreateLogger();$endif$
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
			$if$ ("$ext_useSwagger$" == "True")
            // Подключение Swagger
            services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Name = ""
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ProjectTemplate",
                    Description = "",
                    Contact = new OpenApiContact
                    {
                        Name = "",
                        Email = ""
                    },
                    License = new OpenApiLicense
                    {
                        Name = ""
                    },
                    Version = "v1"
                });
                var files = Directory.GetFiles(AppContext.BaseDirectory, $"*.xml");
                foreach (var xmlFile in files)
                    options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            });
			$endif$
			$if$ ("$ext_useHealth$" == "True")
            services.AddHealthChecks();
			$endif$
			$if$ ("$ext_useEntity$" == "True")
            //Добавляем провайдер к фабрике
            services.AddDataProviderFactory(DatabaseProviderEnum.test, typeof(DataProvider));
			$endif$
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<ITestLogic, TestLogic>();
			$if$ ("$ext_useSerilog$" == "True")
            services.AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(dispose: true);
            });$endif$
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
			$if$ ("$ext_useSwagger$" == "True")
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectTemplate");
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.RoutePrefix = String.Empty;
            });
			$endif$
            app.UseHttpsRedirection();
            app.UseRouting();
            $if$ ("$ext_useSerilog$" == "True")
            app.UseSerilogRequestLogging();$endif$

            app.UseAuthorization();
			$if$ ("$ext_useHealth$" == "True")
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new HealthCheckResponse
                    {
                        Status = report.Status.ToString(),
                        HealthChecks = report.Entries.Select(s => new IndividualHealthResponse
                        {
                            Component = s.Key,
                            Status = s.Value.Status.ToString(),
                            Description = s.Value.Description
                        }),
                        HealthCheckDuration = report.TotalDuration
                    };
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });
			$endif$
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

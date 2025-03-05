using Api.Configuration.Correlation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Serilog;
using SResult;

namespace Api.Configuration
{
    public static class ConfigureExtensions
    {
        public static IServiceCollection ConfigureApi(this IServiceCollection services)
        {
            services.AddProblemDetails(options => {

                options.CustomizeProblemDetails = context => {
                    context.ProblemDetails.Extensions["TraceId"] = context.HttpContext.TraceIdentifier;                    
                    
                    ICorrelationService? cs = context.HttpContext.RequestServices.GetService<ICorrelationService>();

                    if (cs is not null)
                    {
                        context.ProblemDetails.Extensions[nameof(CorrelationId)] = cs.GetCorrelationId().Value;
                    }
                };
            });

            services.AddSerilog();
            services.AddCorrelationLogging();
            services.AddEndpointsApiExplorer();
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        public static void ConfigureApi(this WebApplication app)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(app.Configuration).CreateLogger();
            app.UseSerilogRequestLogging();
            app.UseCorrelationHeaderMiddleware();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseGlobalErrorHandling();
        }

        public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
        {
            return services.AddScoped<ICorrelationService, CorrelationService>();
        }

        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler();
        }
    }
}    
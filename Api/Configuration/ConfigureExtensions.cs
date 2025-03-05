using Api.Configuration.Correlation;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Api.Configuration
{
    public static class ConfigureExtensions
    {
        public static IServiceCollection ConfigureApi(this IServiceCollection services)
        {
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
            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }            
        }
    }
}

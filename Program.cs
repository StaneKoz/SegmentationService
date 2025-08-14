using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SegmentationService.Data;
using SegmentationService.Services;
using System.Reflection;

namespace SegmentationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var isDocker = builder.Configuration["APP_ENV"] == "docker";

            string dbConnection = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(dbConnection));

            builder.Services.AddScoped<SegmentService>();
            builder.Services.AddScoped<SegmentDeletionService>();
            builder.Services.AddHangfire(config =>
                config.UsePostgreSqlStorage(dbConnection));
            builder.Services.AddHangfireServer();

            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Segmentation API", Version = "v1" });
            });

            var app = builder.Build();

            ApplyMigrations(app);

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "Segmentation Jobs",
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            app.MapControllers();

            app.Run();
        }

        private static void ApplyMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                db.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }
    }

    // Фильтр авторизации для Hangfire Dashboard
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true; // Разрешаем всем в development
        }
    }
}
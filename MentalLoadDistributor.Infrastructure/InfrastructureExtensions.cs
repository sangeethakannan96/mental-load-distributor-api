using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Repositories;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MentalLoadDistributor.Infrastructure.Services;

namespace MentalLoadDistributor.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Use SQL Server provider. Provide a connection string via configuration (DefaultConnection).
            // Example connection string in Web project's appsettings.json:
            // "ConnectionStrings": { "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MentalLoadDistributorDb;Trusted_Connection=True;MultipleActiveResultSets=true" }
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
            {
                // Fallback to a localdb connection when no configuration is present.
                conn = "Server=(localdb)\\mssqllocaldb;Database=MentalLoadDistributorDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            }

            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

            // Register EF implementations
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IFamilyRepository, EfFamilyRepository>();
            services.AddScoped<ITaskRepository, EfTaskRepository>();
            services.AddHttpClient<IAiService, OpenAiService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IFamilyProfileRepository, EfFamilyProfileRepository>();
            services.AddScoped<ITaskSuggestionService, FakeTaskSuggestionService>();

            return services;
        }
    }
}

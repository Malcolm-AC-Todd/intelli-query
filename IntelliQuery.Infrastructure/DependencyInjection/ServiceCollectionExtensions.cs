using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IntelliQuery.Application.Abstractions;
using IntelliQuery.Infrastructure.Storage;

namespace IntelliQuery.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<ICsvImportService, CsvImportService>();

            return services;
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.DataAccess;
using Otus.Teaching.PromoCodeFactory.DataAccess.Data;
using Otus.Teaching.PromoCodeFactory.DataAccess.Repositories;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.Services
{
    public static class Configuration
    {
        public static IServiceCollection GetServiceCollection(IConfigurationRoot configuration, string serviceName, IServiceCollection serviceCollection = null)
        {
            if (serviceCollection == null)
            {
                serviceCollection = new ServiceCollection();
            }
            serviceCollection
                .AddSingleton(configuration)
                .AddSingleton((IConfiguration)configuration)
                .ConfigureAllRepositories()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning);
                })
                .AddMemoryCache();
            return serviceCollection;
        }

        public static IServiceCollection ConfigureInMemoryContext(this IServiceCollection services)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddEntityFrameworkProxies()
                .BuildServiceProvider();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDb", builder => { });
                options.UseInternalServiceProvider(serviceProvider);
                options.UseLazyLoadingProxies();
            });
            services.AddTransient<DbContext, DataContext>();
            return services;
        }

        private static IServiceCollection ConfigureAllRepositories(this IServiceCollection services) => services
            .AddTransient(typeof(IRepository<>), typeof(EfRepository<>))
            .AddTransient<IDbInitializer, EfDbInitializer>();

 }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.DataAccess;
using Otus.Teaching.PromoCodeFactory.DataAccess.Data;
using System;
using System.Collections.Generic;


namespace Otus.Teaching.PromoCodeFactory.UnitTests.Services
{
    public class TestFixture_InMemory : IDisposable
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IServiceCollection ServiceCollection { get; set; }

        /// <summary>
        /// Выполняется перед запуском тестов
        /// </summary>
        public TestFixture_InMemory()
        {
            var builder = new ConfigurationBuilder();
            var configuration = builder.Build();
            ServiceCollection = Configuration.GetServiceCollection(configuration, "Tests");
            var serviceProvider = GetServiceProvider();
            ServiceProvider = serviceProvider;
            SeedData();
        }

        private IServiceProvider GetServiceProvider()
        {
            var serviceProvider = ServiceCollection
                .ConfigureInMemoryContext()
                .BuildServiceProvider();
            return serviceProvider;
        }

        private async void SeedData()
        {
            var partner1 = new Partner()
            {
                Id = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"),
                Name = "Суперигрушки",
                NumberIssuedPromoCodes=50,
                IsActive = true,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                        CreateDate = new DateTime(2020, 07, 9),
                        EndDate = new DateTime(2020, 10, 9),
                        Limit = 100
                    }
                }
            };
            var partner2 = new Partner()
            {
                Id = Guid.Parse("894b6e9b-eb5f-406c-aefa-8ccb35d39319"),
                Name = "Каждому кота",
                NumberIssuedPromoCodes = 50,
                IsActive = true,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.Parse("c9bef066-3c5a-4e5d-9cff-bd54479f075e"),
                        CreateDate = new DateTime(2020, 05, 3),
                        EndDate = new DateTime(2020, 10, 15),
                        CancelDate = new DateTime(2020, 06, 16),
                        Limit = 1000
                    }
                }
            };

            using (var scope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // Альтернативный вариант инициализации всех сущностей базы данных из FakeDataFactory 
                //var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                //dbInitializer.InitializeDb();
                var partnersRepository = scope.ServiceProvider.GetRequiredService<IRepository<Partner>>();
                await partnersRepository.AddAsync(partner1);
                await partnersRepository.AddAsync(partner2);
            }
        }

        public void Dispose()
        {
        }
    }
}

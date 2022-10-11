using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Otus.Teaching.PromoCodeFactory.UnitTests.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncInMemoryTests: IClassFixture<TestFixture_InMemory>
    {
        private IRepository<Partner> partnersRepository;

        public SetPartnerPromoCodeLimitAsyncInMemoryTests(TestFixture_InMemory testFixture)
        {
            var serviceProvider = testFixture.ServiceProvider;
            partnersRepository = serviceProvider.GetRequiredService<IRepository<Partner>>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_DoLimitActivePartner_ZeroNumberIssuedPromoCodes()
        {
            // Arrange
            var partner = await partnersRepository.GetByIdAsync(Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"));

            var controller = new PartnersController(partnersRepository);
            var partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(partnerPromoCodeLimit.EndDate).With_Limit(partnerPromoCodeLimit.Limit).Build();

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            partner = await partnersRepository.GetByIdAsync(Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"));

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_DoLimitNotActivePartner_NotChangeNumberIssuedPromoCodes()
        {
            // Arrange
            var partner = await partnersRepository.GetByIdAsync(Guid.Parse("894b6e9b-eb5f-406c-aefa-8ccb35d39319"));

            var controller = new PartnersController(partnersRepository);
            var partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(partnerPromoCodeLimit.EndDate).With_Limit(partnerPromoCodeLimit.Limit).Build();
            var numberIssuedPromoCodes = partner.NumberIssuedPromoCodes;

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            partner = await partnersRepository.GetByIdAsync(Guid.Parse("894b6e9b-eb5f-406c-aefa-8ccb35d39319"));

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(numberIssuedPromoCodes);
        }
    }
}
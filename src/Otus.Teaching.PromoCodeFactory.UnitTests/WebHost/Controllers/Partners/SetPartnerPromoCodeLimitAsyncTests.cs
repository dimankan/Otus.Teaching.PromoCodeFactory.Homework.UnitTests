using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq.Language;
using Namotion.Reflection;
using Xunit;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using Otus.Teaching.PromoCodeFactory.DataAccess.Data;
using AutoFixture.AutoMoq;
using AutoFixture;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Otus.Teaching.PromoCodeFactory.UnitTests.Services;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly AutoFixture.IFixture _fixture;
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = _fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = _fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }
 
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new SetPartnerPromoCodeLimitRequestBuilder().Build();
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(null as Partner);
            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(id, request);
            var statusCode = (result as NotFoundResult)?.StatusCode;

            // Assert
            //Assert.IsType<NotFoundResult>(result);
            //Assert.Equal(404, statusCode);
            result.Should().BeOfType<NotFoundResult>();
            statusCode.Should().Be(404);
        }

 
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnBadRequest()
        {
            //Arrange
            var partner = _fixture.Build<Partner>()
                .With(e => e.IsActive, false).Without(e=>e.PartnerLimits)
                .Create();
            var id = partner.Id;

            var request = new SetPartnerPromoCodeLimitRequestBuilder().Build();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(partner);
            var controller = new PartnersController(_partnersRepositoryMock.Object);

            //Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(id, request);
            var message = (result as ObjectResult)?.Value;

            //Assert
            //Assert.IsType<BadRequestObjectResult>(result);
            result.Should().BeOfType<BadRequestObjectResult>();
            message.Should().Be("Данный партнер не активен");
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_DoLimitActivePartner_ZeroNumberIssuedPromoCodes()
        {
            // Arrange
            Partner partner = CreateBasePartner();

            var partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(partnerPromoCodeLimit.EndDate).With_Limit(partnerPromoCodeLimit.Limit).Build();
            var numberIssuedPromoCodes = partner.NumberIssuedPromoCodes;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner));

            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_DoLimitNotActivePartner_NotChangeNumberIssuedPromoCodes()
        {
            // Arrange
            Partner partner = CreateBasePartner();

            var partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            partnerPromoCodeLimit.CancelDate= DateTime.Now;
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(partnerPromoCodeLimit.EndDate).With_Limit(partnerPromoCodeLimit.Limit).Build();

            var numberIssuedPromoCodes = partner.NumberIssuedPromoCodes;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner));

            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            partner.NumberIssuedPromoCodes.Should().Be(numberIssuedPromoCodes);
        }

        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_PartnerDisableLastLimit_CancelDateNotNull()
        {
            // Arrange
            Partner partner = CreateBasePartner();
            PartnerPromoCodeLimit partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(DateTime.Now.AddMonths(1)).With_Limit(100).Build();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner));
            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            partnerPromoCodeLimit.CancelDate.Should().NotBeNull();
        }

        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_LimitZero_ReturnBadRequest()
        {
            // Arrange
            Partner partner = CreateBasePartner();
            PartnerPromoCodeLimit partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(DateTime.Now.AddMonths(1)).With_Limit(0).Build();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner));
            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            var message = (result as ObjectResult)?.Value;

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            message.Should().Be("Лимит должен быть больше 0");
        }

        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_PartnerAddLimit_DeltaCountPartnerPromoCodeLimit()
        {
            // Arrange
            Partner partner = CreateBasePartner();
            PartnerPromoCodeLimit partnerPromoCodeLimit = partner.PartnerLimits.FirstOrDefault();
            var CountPartnerPromoCodeLimitBeFore = partner.PartnerLimits.Count;
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithEndDate(DateTime.Now.AddMonths(1)).With_Limit(100).Build();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner));
            var controller = new PartnersController(_partnersRepositoryMock.Object);

            // Act
            var result = await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            var DeltaCountPartnerPromoCodeLimit = partner.PartnerLimits.Count-CountPartnerPromoCodeLimitBeFore;

            // Assert
            partnerPromoCodeLimit.CancelDate.Should().NotBeNull();
            DeltaCountPartnerPromoCodeLimit.Should().Be(1);
        }

        public Partner CreateBasePartner()
        {
            var partner = new Partner()
            {
                Id = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"),
                Name = "Суперигрушки",
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

            return partner;
        }
    }
}
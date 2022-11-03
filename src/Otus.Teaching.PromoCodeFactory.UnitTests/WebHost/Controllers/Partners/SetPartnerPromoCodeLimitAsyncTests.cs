using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using Xunit;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly PartnersController _partnersController;

        private readonly Mock<IRepository<Partner>> _partnerRepositoryMock;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnerRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }

        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnNotFound()
        {
            //Arrange
            var guid = new Guid();
            Partner partner = null;
            SetPartnerPromoCodeLimitRequest request = null;
            
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            //Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnFalse()
        {
            //Arrange
            var guid = new Guid();
            var partner = new PartnerBuilder().WithIsActive(false).Build();
            SetPartnerPromoCodeLimitRequest request = null;
            
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            //Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_LimitEnd_NumberIssuedPromoCodesDoNotChange()
        {
            //Arrange
            var guid = new Guid();
            var fixture = new Fixture();
            var request = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                              .With(e => e.Limit, 10).Create();
            var partner = new PartnerBuilder()
                .WithId(guid)
                .WithIsActive(true)
                .WithName("test")
                .WithNumberIssuedPromocodes(100).Build();
            
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            var result = await _partnerRepositoryMock.Object.GetByIdAsync(guid);
            
            //Assert
            result.NumberIssuedPromoCodes.Should().Be(100);
        }
        
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_LimitActive_NumberIssuedPromoCodesisZero()
        {
            //Arrange
            var guid = new Guid();
            var fixture = new Fixture();
            var request = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(e => e.Limit, 10).Create();
            var limit = fixture.Build<PartnerPromoCodeLimit>()
                .With(el => el.Limit, 100)
                .Without(el => el.CancelDate)
                .Without(el => el.Partner).Create();
            var partner = new PartnerBuilder()
                .WithId(guid)
                .WithIsActive(true)
                .WithName("test")
                .WithNumberIssuedPromocodes(100)
                .WithPartnerLimits(limit).Build();
            
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            var result = await _partnerRepositoryMock.Object.GetByIdAsync(guid);
            
            //Assert
            result.NumberIssuedPromoCodes.Should().Be(0);
        }
        
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_SetLimit_LastLimitDisable()
        {
            //Arrange
            var guid = new Guid();
            var fixture = new Fixture();
            var request = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(e => e.Limit, 10).Create();
            var limit = fixture.Build<PartnerPromoCodeLimit>()
                .With(el => el.Limit, 100)
                .Without(el => el.CancelDate)
                .Without(el => el.Partner).Create();
            var partner = new PartnerBuilder()
                .WithId(guid)
                .WithIsActive(true)
                .WithName("test")
                .WithNumberIssuedPromocodes(100)
                .WithPartnerLimits(limit).Build();
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);

            //Assert
            limit.CancelDate.HasValue.Should().BeTrue();
        }
        
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_SetNegativeLimit_RetutnBadRequest()
        {
            //Arrange
            var guid = new Guid();
            var fixture = new Fixture();
            var request = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(e => e.Limit, -10).Create();
            var partner = new PartnerBuilder().Build();
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            
            //Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }
        
        
        [Fact]
        public async void SetPartnerPromoCodeLimitAsync_LimitActive_AddLimitInDb()
        {
            //Arrange
            var guid = new Guid();
            var fixture = new Fixture();
            var request = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(e => e.Limit, 10).Create();
            var limit = fixture.Build<PartnerPromoCodeLimit>()
                .With(el => el.Limit, 100)
                .Without(el => el.CancelDate)
                .Without(el => el.Partner).Create();
            var partner = new PartnerBuilder()
                .WithId(guid)
                .WithIsActive(true)
                .WithName("test")
                .WithNumberIssuedPromocodes(100)
                .WithPartnerLimits(limit).Build();
            
            _partnerRepositoryMock
                .Setup(m => m.GetByIdAsync(guid)).ReturnsAsync(partner);
            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(guid, request);
            var result = await _partnerRepositoryMock.Object.GetByIdAsync(guid);
            
            //Assert
            result.PartnerLimits.FirstOrDefault(el => !el.CancelDate.HasValue).Should().NotBeNull();
        }
    }
}
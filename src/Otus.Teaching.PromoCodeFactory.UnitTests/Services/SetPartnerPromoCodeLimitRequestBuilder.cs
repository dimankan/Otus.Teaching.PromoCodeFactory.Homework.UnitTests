using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.Services
{
    public class SetPartnerPromoCodeLimitRequestBuilder
    {
        private DateTime _EndDate;
        private int _Limit;
        public SetPartnerPromoCodeLimitRequestBuilder()
        {

        }

        public SetPartnerPromoCodeLimitRequestBuilder WithEndDate(DateTime EndDate)
        {
            _EndDate = EndDate;
            return this;
        }

        public SetPartnerPromoCodeLimitRequestBuilder With_Limit(int Limit)
        {
            _Limit = Limit;
            return this;
        }

        public SetPartnerPromoCodeLimitRequest Build()
        {
            return new SetPartnerPromoCodeLimitRequest()
            {
                EndDate= _EndDate,
                Limit= _Limit
            };
        }
    }
}

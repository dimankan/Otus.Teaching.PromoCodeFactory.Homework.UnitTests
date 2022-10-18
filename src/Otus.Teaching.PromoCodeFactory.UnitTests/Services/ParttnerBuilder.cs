using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.Services
{
    public class PartnerBuilder
    {
        private Guid _partnerid;
        private string _name;
        private int _numberIssuedPromoCodes;
        private bool _isActive;
        private List<PartnerPromoCodeLimit> _partnerLimits;
        public PartnerBuilder()
        {

        }
        public PartnerBuilder WithPartnerId(Guid partnerId)
        {
            _partnerid = partnerId;
            return this;
        }

        public PartnerBuilder WithPartnerName(string Name)
        {
            _name = Name;
            return this;
        }

        public PartnerBuilder WithPartnerNumberIssuedPromoCodes(int NumberIssuedPromoCodes)
        {
            _numberIssuedPromoCodes = NumberIssuedPromoCodes;
            return this;
        }

        public PartnerBuilder WithPartnerIsActive(bool IsActive)
        {
            _isActive = IsActive;
            return this;
        }

        public PartnerBuilder WithPartnerLimitsEndDate(DateTime EndDate)
        {
            if(_partnerLimits.Count==0)
                _partnerLimits.Add(new PartnerPromoCodeLimit());
            _partnerLimits[0].EndDate = EndDate;
            return this;
        }

        public PartnerBuilder WithPartnerLimitsLimit(int Limit)
        {
            if (_partnerLimits.Count == 0)
                _partnerLimits.Add(new PartnerPromoCodeLimit());
            _partnerLimits[0].Limit = Limit;
            return this;
        }

        public Partner Build()
        {
            return new Partner()
            {
                Id= _partnerid,
                Name= _name,
                NumberIssuedPromoCodes=_numberIssuedPromoCodes,
                IsActive = _isActive,
                PartnerLimits = _partnerLimits
            };
        }
    }
}

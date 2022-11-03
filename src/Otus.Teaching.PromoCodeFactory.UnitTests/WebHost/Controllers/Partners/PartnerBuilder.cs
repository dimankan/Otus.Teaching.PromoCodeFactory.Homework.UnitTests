using System;
using System.Collections.Generic;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class PartnerBuilder
{
    private Guid _id;
    
    private string _name;

    private int _numberIssuedPromocodes;

    private bool _isActive;

    private ICollection<PartnerPromoCodeLimit> _partnerLimits = new List<PartnerPromoCodeLimit>();

    public PartnerBuilder WithId(Guid partnerId)
    {
        _id = partnerId;
        return this;
    }

    public PartnerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PartnerBuilder WithNumberIssuedPromocodes(int numberIssuedPromocodes)
    {
        _numberIssuedPromocodes = numberIssuedPromocodes;
        return this;
    }

    public PartnerBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public PartnerBuilder WithPartnerLimits(PartnerPromoCodeLimit limit)
    {
        _partnerLimits.Add(limit);
        return this;
    }

    public Partner Build()
    { 
        Partner partner = new Partner()
        {
            Id = _id,
            Name = _name,
            NumberIssuedPromoCodes = _numberIssuedPromocodes,
            IsActive = _isActive,
            PartnerLimits = _partnerLimits
        };
        return partner;
    }
}
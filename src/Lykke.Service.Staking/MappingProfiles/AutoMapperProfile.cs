using AutoMapper;
using Lykke.Service.Staking.Client.Models;
using Lykke.Service.Staking.Domain.Models;

namespace Lykke.Service.Staking.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ReferralStakeRequest, ReferralStakeModel>()
                .ForMember(x => x.Timestamp, opt => opt.Ignore())
                .ForMember(x => x.IsWarningSent, opt => opt.Ignore())
                .ForMember(x => x.ExpirationBurnRatio, opt => opt.MapFrom(r => r.BurnRatio))
                .ForMember(x => x.Status, opt => opt.Ignore());

            CreateMap<ReferralStakeModel, ReferralStakeResponseModel>();
        }
    }
}

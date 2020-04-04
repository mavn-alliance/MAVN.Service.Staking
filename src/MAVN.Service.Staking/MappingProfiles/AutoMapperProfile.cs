using AutoMapper;
using MAVN.Service.Staking.Client.Models;
using MAVN.Service.Staking.Domain.Models;

namespace MAVN.Service.Staking.MappingProfiles
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

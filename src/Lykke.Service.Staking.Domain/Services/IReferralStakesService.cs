using System.Collections.Generic;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Service.Staking.Domain.Enums;
using Lykke.Service.Staking.Domain.Models;

namespace Lykke.Service.Staking.Domain.Services
{
    public interface IReferralStakesService
    {
        Task<ReferralStakeRequestErrorCode> ReferralStakeAsync(ReferralStakeModel model);

        Task ProcessExpiredReferralStakes();

        Task ProcessWarningsForReferralStakes();

        Task<Money18> GetNumberOfStakedTokensForCustomer(string customerId);

        Task<IEnumerable<ReferralStakeModel>> GetReferralStakesByCustomerAndCampaignIds(string customerId,
            string campaignId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MAVN.Numerics;
using Lykke.Common.MsSql;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;

namespace MAVN.Service.Staking.Domain.Repositories
{
    public interface IReferralStakesRepository
    {
        Task AddAsync(ReferralStakeModel referralStakeModel, TransactionContext txContext = null);

        Task<ReferralStakeModel> GetByReferralIdAsync(string referralId, TransactionContext txContext = null);

        Task SetStatusAsync(string referralId, StakeStatus status, TransactionContext txContext = null);

        Task UpdateReferralStakeAsync(ReferralStakeModel stakeModel, TransactionContext txContext = null);

        Task<string[]> GetExpiredStakesIdsAsync();

        Task<ReferralStakeModel[]> GetStakesForWarningAsync();

        Task<Money18> GetNumberOfStakedTokensForCustomer(string customerId);

        Task<IEnumerable<ReferralStakeModel>> GetReferralStakesByCustomerAndCampaignIds(string customerId,
            string campaignId);

        Task SetWarningSentAsync(string referralId);

        Task<IEnumerable<string>> GetAllActiveStakesReferralIdsForCustomer(string customerId);
    }
}

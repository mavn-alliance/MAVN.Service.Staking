using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.Staking.Client.Models;
using Refit;

namespace MAVN.Service.Staking.Client
{
    // This is an example of service controller interfaces.
    // Actual interface methods must be placed here (not in IStakingClient interface).

    /// <summary>
    /// Staking client API interface.
    /// </summary>
    [PublicAPI]
    public interface IReferralStakesApi
    {
        /// <summary>
        /// Release a referral stake
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Put("/api/referral-stakes")]
        Task<ReferralStakeStatusUpdateResponse> ReleaseReferralStakeAsync(ReleaseReferralStakeRequest request);


        /// <summary>
        /// Create a referral stake
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/referral-stakes")]
        Task<ReferralStakeResponse> ReferralStakeAsync(ReferralStakeRequest request);

        /// <summary>
        /// Get referral stakes by customer and campaign
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Get("/api/referral-stakes")]
        Task<IEnumerable<ReferralStakeResponseModel>> GetReferralStakesAsync([Query] GetReferralStakesRequest request);
    }
}

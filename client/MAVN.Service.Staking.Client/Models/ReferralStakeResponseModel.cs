using System;
using Falcon.Numerics;

namespace MAVN.Service.Staking.Client.Models
{
    /// <summary>
    /// Response model
    /// </summary>
    public class ReferralStakeResponseModel
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Unique identifier of the referral stake
        /// </summary>
        public string ReferralId { get; set; }

        /// <summary>
        /// Id of the campaign
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Staked amount
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Status of the stake
        /// </summary>
        public StakeStatus Status { get; set; }

        /// <summary>
        /// Staking period measured in days
        /// </summary>
        public int StakingPeriodInDays { get; set; }

        /// <summary>
        /// Warning period measured in days
        /// </summary>
        public int WarningPeriodInDays { get; set; }

        /// <summary>
        /// Burn ratio of the stake
        /// </summary>
        public decimal ExpirationBurnRatio { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}

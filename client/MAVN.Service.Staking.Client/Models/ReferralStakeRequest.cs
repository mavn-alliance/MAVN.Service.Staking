using System.ComponentModel.DataAnnotations;
using MAVN.Numerics;

namespace MAVN.Service.Staking.Client.Models
{
    /// <summary>
    /// Request model for referral stake
    /// </summary>
    public class ReferralStakeRequest
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Unique identifier of the referral procedure
        /// </summary>
        [Required]
        public string ReferralId { get; set; }

        /// <summary>
        /// Id of the campaign
        /// </summary>
        [Required]
        public string CampaignId { get; set; }

        /// <summary>
        /// Amount of tokens to stake
        /// </summary>
        [Required]
        public Money18 Amount { get; set; }

        /// <summary>
        /// Period of the stake, after this period tokens will be burned if the stake action is not done
        /// </summary>
        [Required]
        public int StakingPeriodInDays { get; set; }

        /// <summary>
        /// Warning period, after this period warning will be sent to t he customer if the stake action is not done
        /// </summary>
        [Required]
        public int WarningPeriodInDays { get; set; }

        /// <summary>
        /// Indicates what part of the stake should be burned if the stake action is not done
        /// </summary>
        [Required]
        [Range(0, 100)]
        public decimal BurnRatio { get; set; }
    }
}

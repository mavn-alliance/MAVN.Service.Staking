using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.Staking.Client.Models
{
    /// <summary>
    /// Request model for referral stake release
    /// </summary>
    public class ReleaseReferralStakeRequest
    {
        /// <summary>
        /// Unique identifier of the referral stake
        /// </summary>
        [Required]
        public string ReferralId { get; set; }

        /// <summary>
        /// Burn ratio which will be used when releasing the stake
        /// </summary>
        [Range(0,100)]
        public decimal BurnRatio { get; set; }
    }
}

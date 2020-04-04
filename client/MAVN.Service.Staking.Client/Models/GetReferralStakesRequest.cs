using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.Staking.Client.Models
{
    /// <summary>
    /// Request model
    /// </summary>
    public class GetReferralStakesRequest
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the campaign
        /// </summary>
        public string CampaignId { get; set; }
    }
}

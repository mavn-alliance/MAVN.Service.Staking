using Falcon.Numerics;

namespace MAVN.Service.Staking.Client.Models
{
    /// <summary>
    /// Returns info about the stakes of the customer
    /// </summary>
    public class CustomerStakesInformationResponse
    {
        /// <summary>
        /// Reserved amount for referral stakes
        /// </summary>
        public Money18 ReferralReservedAmount { get; set; }
    }
}

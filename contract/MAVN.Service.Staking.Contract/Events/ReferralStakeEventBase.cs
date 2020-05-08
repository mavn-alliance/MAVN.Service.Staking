using System;
using MAVN.Numerics;

namespace MAVN.Service.Staking.Contract.Events
{
    public abstract class ReferralStakeEventBase
    {
        /// <summary>
        /// Unique id used to keep track of the operation
        /// </summary>
        public string ReferralId { get; set; }

        /// <summary>
        /// Id of the campaign used for the stake
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Amount of tokens
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}

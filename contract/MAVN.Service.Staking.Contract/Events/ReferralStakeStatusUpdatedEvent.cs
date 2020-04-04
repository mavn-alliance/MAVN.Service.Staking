namespace MAVN.Service.Staking.Contract.Events
{
    public class ReferralStakeStatusUpdatedEvent
    {
        /// <summary>
        /// Id of the stake
        /// </summary>
        public string ReferralId { get; set; }

        /// <summary>
        /// Status of the stake
        /// </summary>
        public StakeStatus Status { get; set; }
    }
}

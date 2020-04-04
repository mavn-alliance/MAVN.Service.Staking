namespace MAVN.Service.Staking.Domain.Models
{
    public interface IStakesBlockchainData
    {
        string StakeId { get; set; }

        /// <summary>
        /// Id of the last BC operation which was requested for this stake ID
        /// </summary>
        string LastOperationId { get; set; }
    }
}

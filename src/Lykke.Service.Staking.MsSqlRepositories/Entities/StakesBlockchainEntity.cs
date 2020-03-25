using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.Staking.Domain.Models;

namespace Lykke.Service.Staking.MsSqlRepositories.Entities
{
    [Table("stakes_blockchain_info")]
    public class StakesBlockchainEntity : IStakesBlockchainData
    {
        [Key, Required]
        [Column("stake_id")]
        public string StakeId { get; set; }

        /// <summary>
        /// Id of the last BC operation which was requested for this stake ID
        /// </summary>
        [Required]
        [Column("last_operation_id")]
        public string LastOperationId { get; set; }

        public static StakesBlockchainEntity Create(string stakeId, string lastOperationId)
        {
            return new StakesBlockchainEntity
            {
                StakeId = stakeId,
                LastOperationId = lastOperationId
            };
        }
    }
}

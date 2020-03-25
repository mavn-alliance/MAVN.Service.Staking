using System.Data.Common;
using Lykke.Common.MsSql;
using Lykke.Service.Staking.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.Staking.MsSqlRepositories
{
    public class StakingContext : MsSqlContext
    {
        private const string Schema = "staking";

        internal DbSet<ReferralStakeEntity> ReferralStakes { get; set; }

        internal DbSet<StakesBlockchainEntity> StakesBlockchainData { get; set; }

        public StakingContext() : base(Schema)
        {
        }

        public StakingContext(string connectionString, bool isTraceEnabled) : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public StakingContext(DbConnection dbConnection) : base(Schema, dbConnection)
        {
        }

        public StakingContext(DbContextOptions contextOptions) : base(Schema, contextOptions)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

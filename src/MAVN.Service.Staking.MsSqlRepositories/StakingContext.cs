using System.Data.Common;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.Staking.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.Staking.MsSqlRepositories
{
    public class StakingContext : PostgreSQLContext
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

        protected override void OnMAVNModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

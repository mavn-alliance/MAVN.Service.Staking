using System.Threading.Tasks;
using MAVN.Common.MsSql;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.Staking.MsSqlRepositories.Repositories
{
    public class StakesBlockchainDataRepository : IStakesBlockchainDataRepository
    {
        private readonly MsSqlContextFactory<StakingContext> _contextFactory;

        public StakesBlockchainDataRepository(MsSqlContextFactory<StakingContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task UpsertAsync(string stakeId, string operationId, TransactionContext txContext = null)
        {
            using (var context = _contextFactory.CreateDataContext(txContext))
            {
                var entity = await context.StakesBlockchainData.FindAsync(stakeId);

                if (entity == null)
                {
                    entity = StakesBlockchainEntity.Create(stakeId, operationId);
                    context.StakesBlockchainData.Add(entity);
                }
                else
                {
                    entity.LastOperationId = operationId;
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task<IStakesBlockchainData> GetByOperationIdAsync(string operationId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result =
                    await context.StakesBlockchainData.FirstOrDefaultAsync(
                        x => x.LastOperationId == operationId);

                return result;
            }
        }
    }
}

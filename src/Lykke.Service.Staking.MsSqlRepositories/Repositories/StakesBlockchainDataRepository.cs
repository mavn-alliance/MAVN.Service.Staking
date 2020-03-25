using System.Threading.Tasks;
using Lykke.Common.MsSql;
using Lykke.Service.Staking.Domain.Models;
using Lykke.Service.Staking.Domain.Repositories;
using Lykke.Service.Staking.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.Staking.MsSqlRepositories.Repositories
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

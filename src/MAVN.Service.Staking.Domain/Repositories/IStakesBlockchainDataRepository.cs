using System.Threading.Tasks;
using Lykke.Common.MsSql;
using MAVN.Service.Staking.Domain.Models;

namespace MAVN.Service.Staking.Domain.Repositories
{
    public interface IStakesBlockchainDataRepository
    {
        Task UpsertAsync(string stakeId, string operationId, TransactionContext txContext = null);
        Task<IStakesBlockchainData> GetByOperationIdAsync(string operationId);
    }
}

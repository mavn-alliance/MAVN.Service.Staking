using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Service.Staking.Domain.Enums;
using Lykke.Service.Staking.Domain.RabbitMq.Handlers;
using Lykke.Service.Staking.Domain.Repositories;
using Lykke.Service.Staking.Domain.Services;

namespace Lykke.Service.Staking.DomainServices.RabbitMq.Handlers
{
    public class TransactionFailedEventHandler : TransactionStatusChangedHandlerBase, ITransactionFailedEventHandler
    {
        private readonly IReferralStakesStatusUpdater _referralStakesStatusUpdater;

        public TransactionFailedEventHandler(
            IReferralStakesRepository referralStakesRepository,
            IStakesBlockchainDataRepository stakesBlockchainDataRepository,
            IReferralStakesStatusUpdater referralStakesStatusUpdater,
            ILogFactory logFactory) : base(referralStakesRepository, stakesBlockchainDataRepository, logFactory)
        {
            _referralStakesStatusUpdater = referralStakesStatusUpdater;
        }

        protected override Task<ReferralStakesStatusUpdateErrorCode> UpdateTokensReservationStartedAsync(string id)
        {
            return _referralStakesStatusUpdater.TokensReservationFailAsync(id);
        }

        protected override Task<ReferralStakesStatusUpdateErrorCode> UpdateTokensBurnAndReleaseStartedAsync(string id)
        {
            return _referralStakesStatusUpdater.TokensBurnAndReleaseFailAsync(id);
        }
    }
}

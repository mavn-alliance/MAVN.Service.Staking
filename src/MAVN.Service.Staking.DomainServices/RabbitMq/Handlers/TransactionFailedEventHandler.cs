using System.Threading.Tasks;
using Lykke.Common.Log;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;

namespace MAVN.Service.Staking.DomainServices.RabbitMq.Handlers
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

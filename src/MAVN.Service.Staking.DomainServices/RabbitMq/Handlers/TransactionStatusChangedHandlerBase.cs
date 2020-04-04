using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;

namespace MAVN.Service.Staking.DomainServices.RabbitMq.Handlers
{
    public abstract class TransactionStatusChangedHandlerBase
    {
        private readonly IReferralStakesRepository _referralStakesRepository;
        private readonly IStakesBlockchainDataRepository _stakesBlockchainDataRepository;
        private readonly ILog _log;

        public TransactionStatusChangedHandlerBase
        (
            IReferralStakesRepository referralStakesRepository,
            IStakesBlockchainDataRepository stakesBlockchainDataRepository,
            ILogFactory logFactory)
        {
            _referralStakesRepository = referralStakesRepository;
            _referralStakesRepository = referralStakesRepository;
            _stakesBlockchainDataRepository = stakesBlockchainDataRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string operationId)
        {
            var blockchainData = await _stakesBlockchainDataRepository.GetByOperationIdAsync(operationId);

            if (blockchainData == null)
                return;

            var id = blockchainData.StakeId;

            var referralStake = await _referralStakesRepository.GetByReferralIdAsync(id);

            if (referralStake == null)
            {
                _log.Error(
                    message:
                    "Referral stake not found for existing blockchain operation",
                    context: new { refferralId = id, operationId });
                return;
            }

            switch (referralStake.Status)
            {
                case StakeStatus.TokensReservationStarted:
                    var reservationResult =
                        await UpdateTokensReservationStartedAsync(id);
                    if (reservationResult != ReferralStakesStatusUpdateErrorCode.None)
                        _log.Error(
                            message:
                            "Could not change referral stake status to TokensReservationSucceeded/Failed because of error",
                            context: new {refferralId = id, Error = reservationResult});
                    break;
                case StakeStatus.TokensBurnAndReleaseStarted:
                    var burnAndReleaseResult =
                        await UpdateTokensBurnAndReleaseStartedAsync(id);
                    if (burnAndReleaseResult != ReferralStakesStatusUpdateErrorCode.None)
                        _log.Error(
                            message:
                            "Could not change referral stake status to TokensBurnAndReleaseSucceeded/Failed because of error",
                            context: new {PaymentRequestId = id, Error = burnAndReleaseResult});
                    break;
                default:
                    _log.Error(
                        message:
                        "Cannot change referral stake status because it is not in the appropriate status",
                        context: new {ReferralId = id, CurrentStatus = referralStake.Status});
                    break;
            }
        }

        protected abstract Task<ReferralStakesStatusUpdateErrorCode> UpdateTokensReservationStartedAsync(string id);
        protected abstract Task<ReferralStakesStatusUpdateErrorCode> UpdateTokensBurnAndReleaseStartedAsync(string id);
    }
}

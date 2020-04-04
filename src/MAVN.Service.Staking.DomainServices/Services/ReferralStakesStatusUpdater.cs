using System;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Common.MsSql;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.Staking.Contract.Events;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;

namespace MAVN.Service.Staking.DomainServices.Services
{
    public class ReferralStakesStatusUpdater : IReferralStakesStatusUpdater
    {
        private readonly IReferralStakesRepository _referralStakesRepository;
        private readonly IStakesBlockchainDataRepository _stakesBlockchainDataRepository;
        private readonly IBlockchainEncodingService _blockchainEncodingService;
        private readonly ISettingsService _settingsService;
        private readonly ITransactionRunner _transactionRunner;
        private readonly IWalletManagementClient _walletManagementClient;
        private readonly IPrivateBlockchainFacadeClient _pbfClient;
        private readonly IRabbitPublisher<ReferralStakeReservedEvent> _stakeReservedPublisher;
        private readonly IRabbitPublisher<ReferralStakeReleasedEvent> _stakeReleasedPublisher;
        private readonly IRabbitPublisher<ReferralStakeBurntEvent> _stakeBurntPublisher;
        private readonly IRabbitPublisher<ReferralStakeStatusUpdatedEvent> _statusUpdatedPublisher;

        public ReferralStakesStatusUpdater(
            IReferralStakesRepository referralStakesRepository,
            IStakesBlockchainDataRepository stakesBlockchainDataRepository,
            IBlockchainEncodingService blockchainEncodingService,
            ISettingsService settingsService,
            ITransactionRunner transactionRunner,
            IWalletManagementClient walletManagementClient,
            IPrivateBlockchainFacadeClient pbfClient,
            IRabbitPublisher<ReferralStakeReservedEvent> stakeReservedPublisher,
            IRabbitPublisher<ReferralStakeReleasedEvent> stakeReleasedPublisher,
            IRabbitPublisher<ReferralStakeBurntEvent> stakeBurntPublisher,
            IRabbitPublisher<ReferralStakeStatusUpdatedEvent> statusUpdatedPublisher)
        {
            _referralStakesRepository = referralStakesRepository;
            _stakesBlockchainDataRepository = stakesBlockchainDataRepository;
            _blockchainEncodingService = blockchainEncodingService;
            _settingsService = settingsService;
            _transactionRunner = transactionRunner;
            _walletManagementClient = walletManagementClient;
            _pbfClient = pbfClient;
            _stakeReservedPublisher = stakeReservedPublisher;
            _stakeReleasedPublisher = stakeReleasedPublisher;
            _stakeBurntPublisher = stakeBurntPublisher;
            _statusUpdatedPublisher = statusUpdatedPublisher;
        }

        public Task<ReferralStakesStatusUpdateErrorCode> TokensReservationSucceedAsync(string referralId)
        {
            return SetStatusAsync(referralId, StakeStatus.TokensReservationStarted, StakeStatus.TokensReservationSucceeded,
                stake =>
                    _stakeReservedPublisher.PublishAsync(new ReferralStakeReservedEvent
                    {
                        CustomerId = stake.CustomerId,
                        ReferralId = stake.ReferralId,
                        CampaignId = stake.CampaignId,
                        Amount = stake.Amount,
                        Timestamp = DateTime.UtcNow
                    }));
        }

        public Task<ReferralStakesStatusUpdateErrorCode> TokensReservationFailAsync(string referralId)
        {
            return SetStatusAsync(referralId, StakeStatus.TokensReservationStarted, StakeStatus.TokensReservationFailed);
        }

        public Task<ReferralStakesStatusUpdateErrorCode> TokensBurnAndReleaseSucceedAsync(string referralId)
        {
            return SetStatusAsync(referralId, StakeStatus.TokensBurnAndReleaseStarted,
                StakeStatus.TokensBurnAndReleaseSucceeded,
                async stake =>
                {
                    var burnRatio = stake.ReleaseBurnRatio ?? stake.ExpirationBurnRatio;
                    var (amountToBurn, amountToRelease) =
                        CalculateAmountToBurnAndRelease(stake.Amount, burnRatio);

                    if (amountToRelease > 0)
                    {
                        await _stakeReleasedPublisher.PublishAsync(new ReferralStakeReleasedEvent
                        {
                            CustomerId = stake.CustomerId,
                            ReferralId = stake.ReferralId,
                            CampaignId = stake.CampaignId,
                            Amount = amountToRelease,
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    if (amountToBurn > 0)
                    {
                        await _stakeBurntPublisher.PublishAsync(new ReferralStakeBurntEvent
                        {
                            CustomerId = stake.CustomerId,
                            ReferralId = stake.ReferralId,
                            CampaignId = stake.CampaignId,
                            Amount = amountToBurn,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                });
        }

        public Task<ReferralStakesStatusUpdateErrorCode> TokensBurnAndReleaseFailAsync(string referralId)
        {
            return SetStatusAsync(referralId, StakeStatus.TokensBurnAndReleaseStarted, StakeStatus.TokensBurnAndReleaseFailed);
        }

        public async Task<ReferralStakesStatusUpdateErrorCode> TokensBurnAndReleaseAsync(string referralId, decimal? releaseBurnRatio = null)
        {
            if (string.IsNullOrEmpty(referralId))
                throw new ArgumentNullException(nameof(referralId));

            var newStatus = StakeStatus.TokensBurnAndReleaseStarted;

            var transactionError = await _transactionRunner.RunWithTransactionAsync(async txContext =>
            {
                var referralStake = await _referralStakesRepository.GetByReferralIdAsync(referralId, txContext);

                if (referralStake == null)
                    return ReferralStakesStatusUpdateErrorCode.DoesNotExist;

                if (referralStake.Status != StakeStatus.TokensReservationSucceeded)
                    return ReferralStakesStatusUpdateErrorCode.InvalidStatus;

                var customerWalletAddress =
                    await _pbfClient.CustomersApi.GetWalletAddress(Guid.Parse(referralStake.CustomerId));

                if (customerWalletAddress.Error == CustomerWalletAddressError.CustomerWalletMissing)
                    return ReferralStakesStatusUpdateErrorCode.CustomerWalletIsMissing;

                var walletBlockStatus =
                    await _walletManagementClient.Api.GetCustomerWalletBlockStateAsync(referralStake.CustomerId);

                if (walletBlockStatus.Status == CustomerWalletActivityStatus.Blocked)
                    return ReferralStakesStatusUpdateErrorCode.CustomerWalletBlocked;

                referralStake.Status = newStatus;
                referralStake.ReleaseBurnRatio = releaseBurnRatio;

                var burnRatio = releaseBurnRatio ?? referralStake.ExpirationBurnRatio;

                var (amountToBurn, amountToRelease) =
                    CalculateAmountToBurnAndRelease(referralStake.Amount, burnRatio);

                var encodedData =
                    _blockchainEncodingService.EncodeDecreaseRequestData(customerWalletAddress.WalletAddress,
                        amountToBurn, amountToRelease);

                var operationId = await AddBlockchainOperation(encodedData);

                await _stakesBlockchainDataRepository.UpsertAsync(referralId, operationId, txContext);

                await _referralStakesRepository.UpdateReferralStakeAsync(referralStake, txContext);

                return ReferralStakesStatusUpdateErrorCode.None;
            });

            if (transactionError == ReferralStakesStatusUpdateErrorCode.None)
                await PublishStatusUpdatedEvent(referralId, newStatus);

            return transactionError;
        }

        private async Task<ReferralStakesStatusUpdateErrorCode> SetStatusAsync
            (string referralId, StakeStatus requiredPreviousStatus, StakeStatus newStatus, Func<ReferralStakeModel, Task> callback = null)
        {
            if (string.IsNullOrEmpty(referralId))
                throw new ArgumentNullException(nameof(referralId));

            var transactionError = await _transactionRunner.RunWithTransactionAsync(async txContext =>
            {
                var referralStake = await _referralStakesRepository.GetByReferralIdAsync(referralId, txContext);

                if (referralStake == null)
                    return ReferralStakesStatusUpdateErrorCode.DoesNotExist;

                if (referralStake.Status != requiredPreviousStatus)
                    return ReferralStakesStatusUpdateErrorCode.InvalidStatus;

                await _referralStakesRepository.SetStatusAsync(referralId, newStatus, txContext);

                if (callback != null)
                    await callback(referralStake);

                return ReferralStakesStatusUpdateErrorCode.None;
            });

            if (transactionError == ReferralStakesStatusUpdateErrorCode.None)
                await PublishStatusUpdatedEvent(referralId, newStatus);

            return transactionError;
        }

        private (Money18 amountToBurn, Money18 amountToRelease) CalculateAmountToBurnAndRelease(Money18 stakeAmount, decimal burnRatio)
        {
            var amountToBurn = (stakeAmount * burnRatio) / 100;

            var amountToRelease = stakeAmount - amountToBurn;

            return (amountToBurn, amountToRelease);
        }

        private Task PublishStatusUpdatedEvent(string referralId, StakeStatus status)
        {
            return _statusUpdatedPublisher.PublishAsync(new ReferralStakeStatusUpdatedEvent
            {
                ReferralId = referralId,
                Status = (Contract.StakeStatus)status
            });
        }

        private async Task<string> AddBlockchainOperation(string encodedData)
        {
            var result = await _pbfClient.OperationsApi.AddGenericOperationAsync(new GenericOperationRequest
            {
                Data = encodedData,
                SourceAddress = _settingsService.GetMasterWalletAddress(),
                TargetAddress = _settingsService.GetTokenContractAddress()
            });

            return result.OperationId.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Falcon.Numerics;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.Common.MsSql;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.Campaign.Client;
using Lykke.Service.Campaign.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.Staking.Domain.Enums;
using Lykke.Service.Staking.Domain.Models;
using Lykke.Service.Staking.Domain.Repositories;
using Lykke.Service.Staking.Domain.Services;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;

namespace Lykke.Service.Staking.DomainServices.Services
{
    public class ReferralStakesService : IReferralStakesService
    {
        private readonly IReferralStakesRepository _referralStakesRepository;
        private readonly IStakesBlockchainDataRepository _stakesBlockchainDataRepository;
        private readonly IReferralStakesStatusUpdater _referralStakesStatusUpdater;
        private readonly IBlockchainEncodingService _blockchainEncodingService;
        private readonly IRabbitPublisher<PushNotificationEvent> _pushNotificationsPublisher;
        private readonly ITransactionRunner _transactionRunner;
        private readonly IPrivateBlockchainFacadeClient _pbfClient;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ICampaignClient _campaignClient;
        private readonly IWalletManagementClient _walletManagementClient;
        private readonly IMoneyFormatter _moneyFormatter;
        private readonly ISettingsService _settingsService;
        private readonly IPushNotificationsSettingsService _pushNotificationsSettingsService;
        private readonly ILog _log;
        private readonly string _componentSourceName;

        public ReferralStakesService(
            IReferralStakesRepository referralStakesRepository,
            IStakesBlockchainDataRepository stakesBlockchainDataRepository,
            IReferralStakesStatusUpdater referralStakesStatusUpdater,
            IBlockchainEncodingService blockchainEncodingService,
            IRabbitPublisher<PushNotificationEvent> pushNotificationsPublisher,
            ITransactionRunner transactionRunner,
            IPrivateBlockchainFacadeClient pbfClient,
            ICustomerProfileClient customerProfileClient,
            ICampaignClient campaignClient,
            IWalletManagementClient walletManagementClient,
            IMoneyFormatter moneyFormatter,
            ISettingsService settingsService,
            IPushNotificationsSettingsService pushNotificationsSettingsService,
            ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
            _referralStakesRepository = referralStakesRepository;
            _stakesBlockchainDataRepository = stakesBlockchainDataRepository;
            _referralStakesStatusUpdater = referralStakesStatusUpdater;
            _blockchainEncodingService = blockchainEncodingService;
            _pushNotificationsPublisher = pushNotificationsPublisher;
            _transactionRunner = transactionRunner;
            _pbfClient = pbfClient;
            _customerProfileClient = customerProfileClient;
            _campaignClient = campaignClient;
            _walletManagementClient = walletManagementClient;
            _moneyFormatter = moneyFormatter;
            _settingsService = settingsService;
            _pushNotificationsSettingsService = pushNotificationsSettingsService;
            _componentSourceName = $"{AppEnvironment.Name} - {AppEnvironment.Version}";
        }

        public async Task<ReferralStakeRequestErrorCode> ReferralStakeAsync(ReferralStakeModel model)
        {
            #region Validation

            if (string.IsNullOrEmpty(model.CustomerId))
                throw new ArgumentNullException(nameof(model.CustomerId));

            if (string.IsNullOrEmpty(model.CampaignId))
                throw new ArgumentNullException(nameof(model.CampaignId));

            if (string.IsNullOrEmpty(model.ReferralId))
                throw new ArgumentNullException(nameof(model.ReferralId));

            if (model.Amount <= 0)
                return ReferralStakeRequestErrorCode.InvalidAmount;

            if (model.WarningPeriodInDays < 0)
                return ReferralStakeRequestErrorCode.InvalidWarningPeriodInDays;

            if (model.StakingPeriodInDays <= 0)
                return ReferralStakeRequestErrorCode.InvalidStakingPeriodInDays;

            if (model.StakingPeriodInDays <= model.WarningPeriodInDays)
                return ReferralStakeRequestErrorCode.WarningPeriodShouldSmallerThanStakingPeriod;

            if (model.ExpirationBurnRatio < 0 || model.ExpirationBurnRatio > 100)
                return ReferralStakeRequestErrorCode.InvalidBurnRatio;

            var existingStake = await _referralStakesRepository.GetByReferralIdAsync(model.ReferralId);

            if (existingStake != null)
                return ReferralStakeRequestErrorCode.StakeAlreadyExist;

            var customerProfile = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(model.CustomerId);

            if (customerProfile.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
                return ReferralStakeRequestErrorCode.CustomerDoesNotExist;

            var campaign = await _campaignClient.Campaigns.GetByIdAsync(model.CampaignId);

            if (campaign.ErrorCode == CampaignServiceErrorCodes.EntityNotFound)
                return ReferralStakeRequestErrorCode.CampaignDoesNotExist;

            var walletBlockStatus =
                await _walletManagementClient.Api.GetCustomerWalletBlockStateAsync(model.CustomerId);

            if (walletBlockStatus.Status == CustomerWalletActivityStatus.Blocked)
                return ReferralStakeRequestErrorCode.CustomerWalletBlocked;

            var isCustomerIdValidGuid = Guid.TryParse(model.CustomerId, out var customerIdGuid);

            if (!isCustomerIdValidGuid)
                return ReferralStakeRequestErrorCode.InvalidCustomerId;

            var customerWalletAddress = await _pbfClient.CustomersApi.GetWalletAddress(customerIdGuid);

            if (customerWalletAddress.Error == CustomerWalletAddressError.CustomerWalletMissing)
                return ReferralStakeRequestErrorCode.CustomerWalletIsMissing;

            var balance = await _pbfClient.CustomersApi.GetBalanceAsync(customerIdGuid);

            if (balance.Total < model.Amount)
                return ReferralStakeRequestErrorCode.NotEnoughBalance;

            #endregion

            model.Status = StakeStatus.TokensReservationStarted;
            model.Timestamp = DateTime.UtcNow;

            var encodedData =
                _blockchainEncodingService.EncodeStakeRequestData(customerWalletAddress.WalletAddress, model.Amount);

            var blockchainOperationId = await _pbfClient.OperationsApi.AddGenericOperationAsync(new GenericOperationRequest
            {
                Data = encodedData,
                SourceAddress = _settingsService.GetMasterWalletAddress(),
                TargetAddress = _settingsService.GetTokenContractAddress()
            });

            return await _transactionRunner.RunWithTransactionAsync(async txContext =>
            {
                await _referralStakesRepository.AddAsync(model, txContext);
                await _stakesBlockchainDataRepository.UpsertAsync(model.ReferralId, blockchainOperationId.OperationId.ToString(),
                    txContext);

                return ReferralStakeRequestErrorCode.None;
            });

        }

        public async Task ProcessExpiredReferralStakes()
        {
            var expiredReferralStakesIds = await _referralStakesRepository.GetExpiredStakesIdsAsync();

            foreach (var stakeId in expiredReferralStakesIds)
            {
                var resultError = await _referralStakesStatusUpdater.TokensBurnAndReleaseAsync(stakeId);

                if (resultError != ReferralStakesStatusUpdateErrorCode.None && resultError != ReferralStakesStatusUpdateErrorCode.CustomerWalletBlocked)
                    _log.Error(message: "Could not start burn process for expired stake because of error",
                        context: new {stakeId, resultError});
            }
        }

        public async Task ProcessWarningsForReferralStakes()
        {
            var referralStakesForWarning = await _referralStakesRepository.GetStakesForWarningAsync();

            foreach (var stake in referralStakesForWarning)
            {
                var campaignResult = await _campaignClient.History.GetEarnRuleByIdAsync(Guid.Parse(stake.CampaignId));
                if (campaignResult.ErrorCode != CampaignServiceErrorCodes.None)
                {
                    _log.Error(message:"Cannot find campaign for existing stake", context:stake);
                }

                var evt = new PushNotificationEvent
                {
                    CustomerId = stake.CustomerId,
                    MessageTemplateId = _pushNotificationsSettingsService.ReferralStakeWarningTemplateId,
                    Source = _componentSourceName,
                    TemplateParameters =
                        new Dictionary<string, string>
                        {
                            {"Amount", _moneyFormatter.FormatAmountToDisplayString(stake.Amount)},
                            {"Offer", campaignResult.Name}
                        },
                };

                await _pushNotificationsPublisher.PublishAsync(evt);
                await _referralStakesRepository.SetWarningSentAsync(stake.ReferralId);
            }
        }

        public Task<Money18> GetNumberOfStakedTokensForCustomer(string customerId)
        {
            return _referralStakesRepository.GetNumberOfStakedTokensForCustomer(customerId);
        }

        public Task<IEnumerable<ReferralStakeModel>> GetReferralStakesByCustomerAndCampaignIds(string customerId, string campaignId)
        {
            return _referralStakesRepository.GetReferralStakesByCustomerAndCampaignIds(customerId, campaignId);
        }
    }
}

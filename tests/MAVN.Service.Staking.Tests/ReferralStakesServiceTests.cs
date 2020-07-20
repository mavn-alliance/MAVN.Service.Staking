using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.Campaign.Client;
using MAVN.Service.Campaign.Client.Models.Campaign.Responses;
using MAVN.Service.Campaign.Client.Models.Enums;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.CustomerProfile.Client.Models.Enums;
using MAVN.Service.CustomerProfile.Client.Models.Responses;
using MAVN.Service.NotificationSystem.SubscriberContract;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;
using MAVN.Service.Staking.DomainServices.Services;
using MAVN.Service.Staking.MsSqlRepositories;
using MAVN.Service.WalletManagement.Client;
using MAVN.Service.WalletManagement.Client.Enums;
using MAVN.Service.WalletManagement.Client.Models.Responses;
using Moq;
using Xunit;

namespace MAVN.Service.Staking.Tests
{
    public class ReferralStakesServiceTests
    {
        private const string FakeReferralStakeId = "d4c59fa8-eb3e-42ca-bd68-4475a7eeb25d";
        private const string FakeCampaignId = "6f3110ed-6e8e-4ed7-96fc-7a8204a60ad8";
        private const string FakeCustomerId = "2cd3dd78-1fc9-4bbe-8e51-ea8e594cb104";
        private const string FakeOperationId = "4316ffc2-877b-4bb5-a3ed-a8011f8139a4";
        private const string FakeWalletAddress = "0xAddress";
        private const long FakeAmount = 100;
        private const decimal FakeBurnRatio = 50;
        private const int FakeWarningPeriodInDays = 2;
        private const int FakeStakingPeriodInDays = 5;

        private readonly Mock<IReferralStakesRepository> _referralStakesRepoMock = new Mock<IReferralStakesRepository>();
        private readonly Mock<IStakesBlockchainDataRepository> _stakesBlockchainDataRepoMock = new Mock<IStakesBlockchainDataRepository>();
        private readonly Mock<IWalletManagementClient> _wmClientMock = new Mock<IWalletManagementClient>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClientMock = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<IBlockchainEncodingService> _blockchainEncodingServiceMock = new Mock<IBlockchainEncodingService>();
        private readonly Mock<ICampaignClient> _campaignClientMock = new Mock<ICampaignClient>();
        private readonly Mock<ICustomerProfileClient> _cpClientMock = new Mock<ICustomerProfileClient>();
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IRabbitPublisher<PushNotificationEvent>> _pushNotificationPublisherMock = new Mock<IRabbitPublisher<PushNotificationEvent>>();
        private readonly Mock<IReferralStakesStatusUpdater> _referralStakeStatusUpdaterMock = new Mock<IReferralStakesStatusUpdater>();
        private readonly Mock<IPushNotificationsSettingsService> _pushNotificationsServiceMock = new Mock<IPushNotificationsSettingsService>();
        private readonly Mock<IMoneyFormatter> _moneyFormatterMock = new Mock<IMoneyFormatter>();

        [Theory]
        [InlineData(null, FakeCampaignId, FakeReferralStakeId)]
        [InlineData(FakeCustomerId, null, FakeReferralStakeId)]
        [InlineData(FakeCustomerId, FakeCampaignId, null)]
        public async Task ReferralStakeAsync_NullInputParameters_ExceptionIsThrown(string customerId, string campaignId, string referralId)
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = customerId,
                CampaignId = campaignId,
                ReferralId = referralId
            }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ReferralStakeAsync_InvalidAmount_ErrorReturned(long amount)
        {
            var sut = CreateSutInstance();
            var result = await  sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = amount
            });

            Assert.Equal(ReferralStakeRequestErrorCode.InvalidAmount, result);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-1)]
        public async Task ReferralStakeAsync_InvalidWarningPeriodInDays_ErrorReturned(int warningPeriodInDays)
        {
            var sut = CreateSutInstance();
            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = warningPeriodInDays
            });

            Assert.Equal(ReferralStakeRequestErrorCode.InvalidWarningPeriodInDays, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ReferralStakeAsync_InvalidStakingPeriodInDays_ErrorReturned(int stakingPeriodInDays)
        {
            var sut = CreateSutInstance();
            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = stakingPeriodInDays
            });

            Assert.Equal(ReferralStakeRequestErrorCode.InvalidStakingPeriodInDays, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_WarningPeriodBiggerThanStakingPeriod_ErrorReturned()
        {
            var sut = CreateSutInstance();
            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = 1
            });

            Assert.Equal(ReferralStakeRequestErrorCode.WarningPeriodShouldSmallerThanStakingPeriod, result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(101)]
        [InlineData(10000)]
        public async Task ReferralStakeAsync_InvalidBurnRatio_ErrorReturned(decimal burnRatio)
        {
            var sut = CreateSutInstance();
            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = burnRatio
            });

            Assert.Equal(ReferralStakeRequestErrorCode.InvalidBurnRatio, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_StakeAlreadyExists_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel());

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.StakeAlreadyExist, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_CustomerDoesNotExist_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync( new CustomerProfileResponse{ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist});

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.CustomerDoesNotExist, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_CampaignDoesNotExist_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel {ErrorCode = CampaignServiceErrorCodes.EntityNotFound});

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.CampaignDoesNotExist, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_CustomerWalletIsBlocked_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel { ErrorCode = CampaignServiceErrorCodes.None });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse {Status = CustomerWalletActivityStatus.Blocked});

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.CustomerWalletBlocked, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_CustomerIdIsNotAValidGuid_ErrorReturned()
        {
            var invalidCustomerId = "asd";
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(invalidCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel { ErrorCode = CampaignServiceErrorCodes.None });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(invalidCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = invalidCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.InvalidCustomerId, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_CustomerWalletIsMissing_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel { ErrorCode = CampaignServiceErrorCodes.None });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.CustomerWalletMissing
                });

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.CustomerWalletIsMissing, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_NotEnoughBalance_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel { ErrorCode = CampaignServiceErrorCodes.None });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeWalletAddress
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetBalanceAsync(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerBalanceResponseModel {Total = 0});

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.NotEnoughBalance, result);
        }

        [Fact]
        public async Task ReferralStakeAsync_SuccessfulOperation_StakesReposCalled()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, false, false))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.None });

            _campaignClientMock.Setup(x => x.Campaigns.GetByIdAsync(FakeCampaignId))
                .ReturnsAsync(new CampaignDetailResponseModel { ErrorCode = CampaignServiceErrorCodes.None });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeWalletAddress
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetBalanceAsync(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerBalanceResponseModel { Total = FakeAmount });

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse {OperationId = Guid.Parse(FakeOperationId)});

            var sut = CreateSutInstance();

            var result = await sut.ReferralStakeAsync(new ReferralStakeModel
            {
                CustomerId = FakeCustomerId,
                CampaignId = FakeCampaignId,
                ReferralId = FakeReferralStakeId,
                Amount = FakeAmount,
                WarningPeriodInDays = FakeWarningPeriodInDays,
                StakingPeriodInDays = FakeStakingPeriodInDays,
                ExpirationBurnRatio = FakeBurnRatio,
            });

            Assert.Equal(ReferralStakeRequestErrorCode.None, result);
            _referralStakesRepoMock.Verify(
                x => x.AddAsync(It.IsAny<ReferralStakeModel>(), It.IsAny<TransactionContext>()), Times.Once);
            _stakesBlockchainDataRepoMock.Verify(
                x => x.UpsertAsync(FakeReferralStakeId, FakeOperationId, It.IsAny<TransactionContext>()), Times.Once);
        }

        private ReferralStakesService CreateSutInstance()
        {
            return new ReferralStakesService(
                _referralStakesRepoMock.Object,
                _stakesBlockchainDataRepoMock.Object,
                _referralStakeStatusUpdaterMock.Object,
                _blockchainEncodingServiceMock.Object,
                _pushNotificationPublisherMock.Object,
                new SqlContextFactoryFake<StakingContext>(x => new StakingContext(x)),
                _pbfClientMock.Object,
                _cpClientMock.Object,
                _campaignClientMock.Object,
                _wmClientMock.Object,
                _moneyFormatterMock.Object,
                _settingsServiceMock.Object,
                _pushNotificationsServiceMock.Object,
                EmptyLogFactory.Instance);
        }

    }
}

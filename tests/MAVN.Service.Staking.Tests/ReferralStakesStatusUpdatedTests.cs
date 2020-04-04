using System;
using System.Threading.Tasks;
using Lykke.Common.MsSql;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.Staking.Contract.Events;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;
using MAVN.Service.Staking.DomainServices.Services;
using MAVN.Service.Staking.MsSqlRepositories;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;
using Lykke.Service.WalletManagement.Client.Models.Responses;
using Moq;
using Xunit;

namespace MAVN.Service.Staking.Tests
{
    public class ReferralStakesStatusUpdatedTests
    {
        private const string FakeReferralStakeId = "d4c59fa8-eb3e-42ca-bd68-4475a7eeb25d";
        private const string FakeCampaignId = "6f3110ed-6e8e-4ed7-96fc-7a8204a60ad8";
        private const string FakeCustomerId = "2cd3dd78-1fc9-4bbe-8e51-ea8e594cb104";
        private const string FakeOperationId = "4316ffc2-877b-4bb5-a3ed-a8011f8139a4";
        private const string FakeWalletAddress = "0xAddress";
        private const long Amount = 100;
        private const decimal BurnRatio = 50;

        private readonly Mock<IReferralStakesRepository> _referralStakesRepoMock = new Mock<IReferralStakesRepository>();
        private readonly Mock<IStakesBlockchainDataRepository> _stakesBlockchainDataRepoMock = new Mock<IStakesBlockchainDataRepository>();
        private readonly Mock<IWalletManagementClient> _wmClientMock = new Mock<IWalletManagementClient>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClientMock = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<IBlockchainEncodingService> _blockchainEncodingServiceMock = new Mock<IBlockchainEncodingService>();
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IRabbitPublisher<ReferralStakeReservedEvent>> _stakeReservedPublisherMock = new Mock<IRabbitPublisher<ReferralStakeReservedEvent>>();
        private readonly Mock<IRabbitPublisher<ReferralStakeReleasedEvent>> _stakeReleasedPublisherMock = new Mock<IRabbitPublisher<ReferralStakeReleasedEvent>>();
        private readonly Mock<IRabbitPublisher<ReferralStakeBurntEvent>> _stakeBurntPublisherMock = new Mock<IRabbitPublisher<ReferralStakeBurntEvent>>();
        private readonly Mock<IRabbitPublisher<ReferralStakeStatusUpdatedEvent>> _stakeStatusUpdatedPublisherMock = new Mock<IRabbitPublisher<ReferralStakeStatusUpdatedEvent>>();

        [Fact]
        public async Task TokensReservationSucceedAsync_ReferralIdIsNull_ArgumentNullExceptionThrown()
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TokensReservationSucceedAsync(null));
        }

        [Fact]
        public async Task TokensReservationSucceedAsync_ReferralStakeNotFound_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel) null);

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.DoesNotExist, result);
        }

        [Fact]
        public async Task TokensReservationSucceedAsync_ReferralStakeIsInInvalidStatus_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationFailed
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.InvalidStatus, result);
        }

        [Fact]
        public async Task TokensReservationSucceedAsync_ReferralStakeReservedPublisherCalled()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
            _stakeReservedPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeReservedEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
        }

        [Fact]
        public async Task TokensReservationFailAsync_ReferralIdIsNull_ArgumentNullExceptionThrown()
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TokensReservationFailAsync(null));
        }

        [Fact]
        public async Task TokensReservationFailAsync_ReferralStakeNotFound_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.DoesNotExist, result);
        }

        [Fact]
        public async Task TokensReservationFailAsync_ReferralStakeIsInInvalidStatus_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseFailed
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.InvalidStatus, result);
        }

        [Fact]
        public async Task TokensReservationFailAsync_UpdateSuccessfully()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensReservationFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
        }


        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_ReferralIdIsNull_ArgumentNullExceptionThrown()
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TokensBurnAndReleaseSucceedAsync(null));
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_ReferralStakeNotFound_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.DoesNotExist, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_ReferralStakeIsInInvalidStatus_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationStarted
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.InvalidStatus, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_BurnRatioIs0_StakeReleasedPublisherCalledWithWholeAmount()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId,
                    ExpirationBurnRatio = 0
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
            _stakeReleasedPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeReleasedEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
            _stakeBurntPublisherMock.Verify(x => x.PublishAsync(It.IsAny<ReferralStakeBurntEvent>()), Times.Never);
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_BurnRatioIs100_StakeBurntPublisherCalledWithWholeAmount()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId,
                    ExpirationBurnRatio = 100
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
            _stakeBurntPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeBurntEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
            _stakeReleasedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<ReferralStakeReleasedEvent>()), Times.Never);
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_BurnRatioIs50_StakeBurntPublisherAndStakeReleasedPublisherCalledWithHalfOfTheAmount()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId,
                    ExpirationBurnRatio = BurnRatio
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
            _stakeBurntPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeBurntEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount/2 && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
            _stakeReleasedPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeReleasedEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount/2 && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
        }

        [Fact]
        public async Task TokensBurnAndReleaseSucceedAsync_ReleaseBurnRatioIs50ExpirationBurnRatioIs100_StakeBurntPublisherAndStakeReleasedPublisherCalledWithHalfOfTheAmount()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId,
                    ExpirationBurnRatio = 100,
                    ReleaseBurnRatio = BurnRatio
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseSucceedAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
            _stakeBurntPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeBurntEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount / 2 && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
            _stakeReleasedPublisherMock.Verify(x => x.PublishAsync(It.Is<ReferralStakeReleasedEvent>(r =>
                r.CustomerId == FakeCustomerId && r.Amount == Amount / 2 && r.CampaignId == FakeCampaignId &&
                r.ReferralId == FakeReferralStakeId)));
        }

        [Fact]
        public async Task TokensBurnAndReleaseFailAsync_ReferralIdIsNull_ArgumentNullExceptionThrown()
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TokensBurnAndReleaseFailAsync(null));
        }

        [Fact]
        public async Task TokensBurnAndReleaseFailAsync_ReferralStakeNotFound_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.DoesNotExist, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseFailAsync_ReferralStakeIsInInvalidStatus_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationStarted
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.InvalidStatus, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseFailAsync_UpdateSuccessfully()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ReferralId = FakeReferralStakeId,
                    CampaignId = FakeCampaignId
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseFailAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseAsync_ReferralIdIsNull_ArgumentNullExceptionThrown()
        {
            var sut = CreateSutInstance();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TokensBurnAndReleaseAsync(null));
        }

        [Fact]
        public async Task TokensBurnAndReleaseAsync_ReferralStakeNotFound_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync((ReferralStakeModel)null);

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.DoesNotExist, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseAsync_ReferralStakeIsInInvalidStatus_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationFailed,
                });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.InvalidStatus, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseAsync_CustomerDoesNotHaveAWallet_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationSucceeded,
                    CustomerId = FakeCustomerId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel{Error = CustomerWalletAddressError.CustomerWalletMissing});

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.CustomerWalletIsMissing, result);
        }

        [Fact]
        public async Task TokensBurnAndReleaseAsync_CustomerWalletIsBlocked_ErrorReturned()
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationSucceeded,
                    CustomerId = FakeCustomerId
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeWalletAddress
                });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse {Status = CustomerWalletActivityStatus.Blocked});

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.CustomerWalletBlocked, result);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(35)]
        [InlineData(20)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(20.55)]
        public async Task TokensBurnAndReleaseAsync_SuccessfulOperation(decimal burnRatio)
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    ReferralId = FakeReferralStakeId,
                    Status = StakeStatus.TokensReservationSucceeded,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ExpirationBurnRatio = burnRatio
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeWalletAddress
                });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse {OperationId = Guid.Parse(FakeOperationId)});

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);

            _stakesBlockchainDataRepoMock.Verify(x =>
                x.UpsertAsync(FakeReferralStakeId, FakeOperationId, It.IsAny<TransactionContext>()));

            _referralStakesRepoMock.Verify(x =>
                x.UpdateReferralStakeAsync(It.Is<ReferralStakeModel>(r => r.Status == StakeStatus.TokensBurnAndReleaseStarted), It.IsAny<TransactionContext>()));

            _blockchainEncodingServiceMock.Verify(x =>
                x.EncodeDecreaseRequestData(FakeWalletAddress, (Amount * burnRatio / 100),
                    Amount * (100 - burnRatio) / 100));
        }

        [Theory]
        [InlineData(50)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(23)]
        [InlineData(74.55)]
        public async Task TokensBurnAndReleaseAsync_WithReleaseBurnRatio_ReleaseBurnRationUsedForTheCalculations(decimal releaseBurnRatio)
        {
            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralStakeId, It.IsAny<TransactionContext>()))
                .ReturnsAsync(new ReferralStakeModel
                {
                    ReferralId = FakeReferralStakeId,
                    Status = StakeStatus.TokensReservationSucceeded,
                    CustomerId = FakeCustomerId,
                    Amount = Amount,
                    ExpirationBurnRatio = BurnRatio,
                    ReleaseBurnRatio = releaseBurnRatio
                });

            _pbfClientMock.Setup(x => x.CustomersApi.GetWalletAddress(Guid.Parse(FakeCustomerId)))
                .ReturnsAsync(new CustomerWalletAddressResponseModel
                {
                    Error = CustomerWalletAddressError.None,
                    WalletAddress = FakeWalletAddress
                });

            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse { Status = CustomerWalletActivityStatus.Active });

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse { OperationId = Guid.Parse(FakeOperationId) });

            var sut = CreateSutInstance();
            var result = await sut.TokensBurnAndReleaseAsync(FakeReferralStakeId,releaseBurnRatio);

            Assert.Equal(ReferralStakesStatusUpdateErrorCode.None, result);

            _stakesBlockchainDataRepoMock.Verify(x =>
                x.UpsertAsync(FakeReferralStakeId, FakeOperationId, It.IsAny<TransactionContext>()));

            _referralStakesRepoMock.Verify(x =>
                x.UpdateReferralStakeAsync(It.Is<ReferralStakeModel>(r => r.Status == StakeStatus.TokensBurnAndReleaseStarted), It.IsAny<TransactionContext>()));

            _blockchainEncodingServiceMock.Verify(x =>
                x.EncodeDecreaseRequestData(FakeWalletAddress, (Amount * releaseBurnRatio / 100),
                    Amount * (100 - releaseBurnRatio) / 100));
        }

        private ReferralStakesStatusUpdater CreateSutInstance()
        {
            return new ReferralStakesStatusUpdater(
                _referralStakesRepoMock.Object,
                _stakesBlockchainDataRepoMock.Object,
                _blockchainEncodingServiceMock.Object,
                _settingsServiceMock.Object,
                new SqlContextFactoryFake<StakingContext>(x => new StakingContext(x)), 
                _wmClientMock.Object,
                _pbfClientMock.Object,
                _stakeReservedPublisherMock.Object,
                _stakeReleasedPublisherMock.Object,
                _stakeBurntPublisherMock.Object,
                _stakeStatusUpdatedPublisherMock.Object);
        }
    }
}

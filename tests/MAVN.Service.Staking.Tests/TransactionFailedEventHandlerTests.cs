using System.Threading.Tasks;
using Lykke.Logs;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;
using MAVN.Service.Staking.DomainServices.RabbitMq.Handlers;
using MAVN.Service.Staking.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace MAVN.Service.Staking.Tests
{
    public class TransactionFailedEventHandlerTests
    {
        private const string FakeOperationId = "opId";
        private const string FakeReferralId = "refId";

        private readonly Mock<IReferralStakesRepository> _referralStakesRepoMock = new Mock<IReferralStakesRepository>();
        private readonly Mock<IStakesBlockchainDataRepository> _stakeBlockchainDataRepoMock = new Mock<IStakesBlockchainDataRepository>();
        private readonly Mock<IReferralStakesStatusUpdater> _referralStakesStatusUpdaterMock = new Mock<IReferralStakesStatusUpdater>(MockBehavior.Strict);


        [Fact]
        public async Task HandleAsync_OperationNotFoundInBlockchainDataRepo_ReferralStakesRepoNotCalled()
        {
            _stakeBlockchainDataRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync((IStakesBlockchainData)null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _referralStakesRepoMock.Verify(x => x.GetByReferralIdAsync(It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_OperationIsMissingInReferralStakesRepo_StatusUpdaterNotCalled()
        {
            _stakeBlockchainDataRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new StakesBlockchainEntity { StakeId = FakeReferralId });

            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralId, null))
                .ReturnsAsync((ReferralStakeModel)null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _referralStakesStatusUpdaterMock.Verify();
        }

        [Fact]
        public async Task HandleAsync_OperationIsInStatusReservationStarted_StatusUpdatedCalledToUpdateStatusToFailed()
        {
            _stakeBlockchainDataRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new StakesBlockchainEntity { StakeId = FakeReferralId });

            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralId, null))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensReservationStarted
                });

            _referralStakesStatusUpdaterMock.Setup(x => x.TokensReservationFailAsync(FakeReferralId))
                .ReturnsAsync(ReferralStakesStatusUpdateErrorCode.None);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _referralStakesStatusUpdaterMock.Verify(x => x.TokensReservationFailAsync(FakeReferralId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_OperationIsInStatusBurnAndReleaseStarted_StatusUpdatedCalledToUpdateStatusToFailed()
        {
            _stakeBlockchainDataRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new StakesBlockchainEntity { StakeId = FakeReferralId });

            _referralStakesRepoMock.Setup(x => x.GetByReferralIdAsync(FakeReferralId, null))
                .ReturnsAsync(new ReferralStakeModel
                {
                    Status = StakeStatus.TokensBurnAndReleaseStarted
                });

            _referralStakesStatusUpdaterMock.Setup(x => x.TokensBurnAndReleaseFailAsync(FakeReferralId))
                .ReturnsAsync(ReferralStakesStatusUpdateErrorCode.None);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _referralStakesStatusUpdaterMock.Verify(x => x.TokensBurnAndReleaseFailAsync(FakeReferralId), Times.Once);
        }

        private TransactionFailedEventHandler CreateSutInstance()
        {
            return new TransactionFailedEventHandler(
                _referralStakesRepoMock.Object,
                _stakeBlockchainDataRepoMock.Object,
                _referralStakesStatusUpdaterMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}

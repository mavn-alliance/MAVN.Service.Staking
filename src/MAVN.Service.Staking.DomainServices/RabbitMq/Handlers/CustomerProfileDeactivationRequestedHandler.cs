using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.Domain.Services;

namespace MAVN.Service.Staking.DomainServices.RabbitMq.Handlers
{
    public class CustomerProfileDeactivationRequestedHandler : ICustomerProfileDeactivationRequestedHandler
    {
        private readonly IReferralStakesRepository _referralStakesRepository;
        private readonly IReferralStakesStatusUpdater _referralStakesStatusUpdater;
        private readonly ILog _log;

        public CustomerProfileDeactivationRequestedHandler(
            IReferralStakesRepository referralStakesRepository,
            IReferralStakesStatusUpdater referralStakesStatusUpdater,
            ILogFactory logFactory)
        {
            _referralStakesRepository = referralStakesRepository;
            _referralStakesStatusUpdater = referralStakesStatusUpdater;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string customerId)
        {
            var stakeIds = await _referralStakesRepository.GetAllActiveStakesReferralIdsForCustomer(customerId);

            _log.Info("Start releasing all stakes for customer which will be deactivated", context:new {customerId, stakeIds});

            foreach (var referralId in stakeIds)
            {
                var resultError = await _referralStakesStatusUpdater.TokensBurnAndReleaseAsync(referralId, releaseBurnRatio: 100);

                if (resultError != ReferralStakesStatusUpdateErrorCode.None)
                    _log.Error(
                        "Could not start burning process of stake for customer which will be deactivated because of error",
                        context: resultError);
            }
        }
    }
}

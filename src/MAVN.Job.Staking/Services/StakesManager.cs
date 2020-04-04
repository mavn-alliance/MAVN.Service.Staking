using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.Staking.Domain.Services;

namespace Lykke.Job.Staking.Services
{
    public class StakesManager : IStartable, IStopable
    {
        private readonly IReferralStakesService _referralStakesService;
        private readonly TimerTrigger _timerTrigger;
        private readonly ILog _log;

        public StakesManager(IReferralStakesService referralStakesService, TimeSpan idlePeriod, ILogFactory logFactory)
        {
            _referralStakesService = referralStakesService;
            _log = logFactory.CreateLog(this);
            _timerTrigger = new TimerTrigger(nameof(StakesManager), idlePeriod, logFactory);
            _timerTrigger.Triggered += Execute;
        }

        public void Start()
        {
            _timerTrigger.Start();
        }

        public void Stop()
        {
            _timerTrigger?.Stop();
        }

        public void Dispose()
        {
            _timerTrigger?.Stop();
            _timerTrigger?.Dispose();
        }

        private async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {

            _log.Info("Starting checking and updating stakes");
            var processExpiredReferralStakesTask = _referralStakesService.ProcessExpiredReferralStakes();
            var processWarningsForReferralStakesTask = _referralStakesService.ProcessWarningsForReferralStakes();
            await Task.WhenAll(processExpiredReferralStakesTask, processWarningsForReferralStakesTask);
            _log.Info("Finished checking and updating stakes");
        }
    }
}

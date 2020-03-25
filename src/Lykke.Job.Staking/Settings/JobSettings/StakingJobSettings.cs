using System;

namespace Lykke.Job.Staking.Settings.JobSettings
{
    public class StakingJobSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public TimeSpan IdlePeriod { get; set; }
    }
}

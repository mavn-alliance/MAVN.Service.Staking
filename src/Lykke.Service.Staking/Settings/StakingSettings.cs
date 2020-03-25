using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Staking.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class StakingSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}

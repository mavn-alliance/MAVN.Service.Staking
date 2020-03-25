using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Staking.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }

        [AmqpCheck]
        public string NotificationRabbitMqConnectionString { get; set; }
    }
}

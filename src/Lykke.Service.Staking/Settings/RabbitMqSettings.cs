using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Staking.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }

        [AmqpCheck]
        public string NotificationRabbitMqConnectionString { get; set; }
    }
}

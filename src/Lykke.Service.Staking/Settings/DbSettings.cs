using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Staking.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DataConnString { get; set; }
    }
}

using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Staking.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DataConnString { get; set; }
    }
}

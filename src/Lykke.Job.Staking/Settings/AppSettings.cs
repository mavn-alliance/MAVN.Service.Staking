using Lykke.Job.Staking.Settings.JobSettings;
using Lykke.Job.Staking.Settings.Notifications;
using Lykke.Sdk.Settings;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.WalletManagement.Client;

namespace Lykke.Job.Staking.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public StakingJobSettings StakingJob { get; set; }

        public PrivateBlockchainFacadeServiceClientSettings PrivateBlockchainFacadeService { get; set; }

        public WalletManagementServiceClientSettings WalletManagementService { get; set; }

        public CampaignServiceClientSettings CampaignService { get; set; }

        public CustomerProfileServiceClientSettings CustomerProfileService { get; set; }

        public NotificationsSettings Notifications { get; set; }

        public Constants Constants { get; set; }

        public string MasterWalletAddress { get; set; }

        public string TokenContractAddress { get; set; }
    }
}

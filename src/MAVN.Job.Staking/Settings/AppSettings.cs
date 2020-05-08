using Lykke.Job.Staking.Settings.JobSettings;
using Lykke.Job.Staking.Settings.Notifications;
using Lykke.Sdk.Settings;
using MAVN.Service.Campaign.Client;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.WalletManagement.Client;

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

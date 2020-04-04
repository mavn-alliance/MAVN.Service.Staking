using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Staking.Settings.Notifications;
using Lykke.Service.WalletManagement.Client;

namespace MAVN.Service.Staking.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public StakingSettings StakingService { get; set; }

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

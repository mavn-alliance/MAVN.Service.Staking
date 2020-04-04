using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;
using MAVN.Service.Staking.Domain.Services;
using MAVN.Service.Staking.DomainServices.Common;
using MAVN.Service.Staking.DomainServices.RabbitMq.Handlers;
using MAVN.Service.Staking.DomainServices.Services;
using MAVN.Service.Staking.Services;
using MAVN.Service.Staking.Settings;
using Lykke.Service.WalletManagement.Client;
using Lykke.SettingsReader;

namespace MAVN.Service.Staking.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly AppSettings _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterWalletManagementClient(_appSettings.WalletManagementService, null);
            builder.RegisterPrivateBlockchainFacadeClient(_appSettings.PrivateBlockchainFacadeService, null);
            builder.RegisterCampaignClient(_appSettings.CampaignService, null);
            builder.RegisterCustomerProfileClient(_appSettings.CustomerProfileService, null);

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<MoneyFormatter>()
                .WithParameter("tokenFormatCultureInfo",
                    _appSettings.Constants.TokenFormatCultureInfo)
                .WithParameter("tokenNumberDecimalPlaces",
                    _appSettings.Constants.TokenNumberDecimalPlaces)
                .WithParameter("tokenIntegerPartFormat",
                    _appSettings.Constants.TokenIntegerPartFormat)
                .As<IMoneyFormatter>()
                .SingleInstance();

            builder.RegisterType<SettingsService>()
                .WithParameter("tokenContractAddress", _appSettings.TokenContractAddress)
                .WithParameter("masterWalletAddress", _appSettings.MasterWalletAddress)
                .As<ISettingsService>()
                .SingleInstance();

            builder.RegisterType<PushNotificationsSettingsService>()
                .WithParameter("referralStakeWarningTemplateId",
                    _appSettings.Notifications.PushNotifications.ReferralStakeWarningTemplateId)
                .As<IPushNotificationsSettingsService>()
                .SingleInstance();

            builder.RegisterType<BlockchainEncodingService>()
                .As<IBlockchainEncodingService>()
                .SingleInstance();

            builder.RegisterType<ReferralStakesService>()
                .As<IReferralStakesService>()
                .SingleInstance();

            builder.RegisterType<ReferralStakesStatusUpdater>()
                .As<IReferralStakesStatusUpdater>()
                .SingleInstance();

            builder.RegisterType<TransactionFailedEventHandler>()
                .As<ITransactionFailedEventHandler>()
                .SingleInstance();

            builder.RegisterType<TransactionSucceededEventHandler>()
                .As<ITransactionSucceededEventHandler>()
                .SingleInstance();

            builder.RegisterType<CustomerProfileDeactivationRequestedHandler>()
                .As<ICustomerProfileDeactivationRequestedHandler>()
                .SingleInstance();
        }

    }
}

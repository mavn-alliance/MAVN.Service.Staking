using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Job.Staking.Services;
using Lykke.Job.Staking.Settings;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;
using MAVN.Service.Staking.Domain.Services;
using MAVN.Service.Staking.DomainServices.Common;
using MAVN.Service.Staking.DomainServices.RabbitMq.Handlers;
using MAVN.Service.Staking.DomainServices.Services;
using Lykke.Service.WalletManagement.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.Staking.Modules
{
    [UsedImplicitly]
    public class JobModule : Module
    {
        private readonly AppSettings _settings;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<StakesManager>()
                .WithParameter("idlePeriod", _settings.StakingJob.IdlePeriod)
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            builder.RegisterWalletManagementClient(_settings.WalletManagementService, null);
            builder.RegisterPrivateBlockchainFacadeClient(_settings.PrivateBlockchainFacadeService, null);
            builder.RegisterCampaignClient(_settings.CampaignService, null);
            builder.RegisterCustomerProfileClient(_settings.CustomerProfileService, null);

            builder.RegisterType<MoneyFormatter>()
                .WithParameter("tokenFormatCultureInfo",
                    _settings.Constants.TokenFormatCultureInfo)
                .WithParameter("tokenNumberDecimalPlaces",
                    _settings.Constants.TokenNumberDecimalPlaces)
                .WithParameter("tokenIntegerPartFormat",
                    _settings.Constants.TokenIntegerPartFormat)
                .As<IMoneyFormatter>()
                .SingleInstance();

            builder.RegisterType<SettingsService>()
                .WithParameter("tokenContractAddress", _settings.TokenContractAddress)
                .WithParameter("masterWalletAddress", _settings.MasterWalletAddress)
                .As<ISettingsService>()
                .SingleInstance();

            builder.RegisterType<PushNotificationsSettingsService>()
                .WithParameter("referralStakeWarningTemplateId",
                    _settings.Notifications.PushNotifications.ReferralStakeWarningTemplateId)
                .As<IPushNotificationsSettingsService>()
                .SingleInstance();

            builder.RegisterType<TransactionFailedEventHandler>()
                .As<ITransactionFailedEventHandler>()
                .SingleInstance();

            builder.RegisterType<TransactionSucceededEventHandler>()
                .As<ITransactionSucceededEventHandler>()
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
        }
    }
}

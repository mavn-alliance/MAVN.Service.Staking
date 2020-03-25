using Autofac;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Job.Staking.Settings;
using Lykke.Job.Staking.Settings.JobSettings;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.Service.Staking.Contract.Events;
using Lykke.Service.Staking.DomainServices.RabbitMq.Subscribers;
using Lykke.SettingsReader;

namespace Lykke.Job.Staking.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string DefaultQueueName = "staking";
        private const string NotificationSystemPushNotificationsExchangeName = "notificationsystem.command.pushnotification";
        private const string TransactionFailedExchange = "lykke.wallet.transactionfailed";
        private const string ReferralStakeReservedExchange = "lykke.wallet.referralstakereserved";
        private const string ReferralStakeReleasedExchange = "lykke.wallet.referralstakereleased";
        private const string ReferralStakeBurntExchange = "lykke.wallet.referralstakeburnt";
        private const string ReferralStakeStatusUpdatedExchange = "lykke.wallet.referralstakestatusupdated";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.StakingJob.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var rabbitMqConnString = _settings.RabbitMqConnectionString;

            builder.RegisterJsonRabbitPublisher<ReferralStakeReservedEvent>(
                rabbitMqConnString,
                ReferralStakeReservedExchange);

            builder.RegisterJsonRabbitPublisher<ReferralStakeReleasedEvent>(
                rabbitMqConnString,
                ReferralStakeReleasedExchange);

            builder.RegisterJsonRabbitPublisher<ReferralStakeBurntEvent>(
                rabbitMqConnString,
                ReferralStakeBurntExchange);

            builder.RegisterJsonRabbitPublisher<ReferralStakeStatusUpdatedEvent>(
                rabbitMqConnString,
                ReferralStakeStatusUpdatedExchange);

            var notificationRabbitMqConnString = _settings.NotificationRabbitMqConnectionString;

            builder.RegisterJsonRabbitPublisher<PushNotificationEvent>(
                notificationRabbitMqConnString,
                NotificationSystemPushNotificationsExchangeName);

            builder.RegisterType<TransactionFailedSubscriber>()
                .As<IStartStop>()
                .WithParameter("connectionString", rabbitMqConnString)
                .WithParameter("exchangeName", TransactionFailedExchange)
                .WithParameter("queueName", DefaultQueueName)
                .SingleInstance();

            builder.RegisterType<TransactionSucceededSubscriber>()
                .As<IStartStop>()
                .WithParameter("connectionString", rabbitMqConnString)
                .WithParameter("exchangeName", TransactionFailedExchange)
                .WithParameter("queueName", DefaultQueueName)
                .SingleInstance();
        }
    }
}

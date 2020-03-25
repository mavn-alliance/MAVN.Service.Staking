using Autofac;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CustomerProfile.Contract;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.Staking.Contract.Events;
using Lykke.Service.Staking.DomainServices.RabbitMq.Subscribers;
using Lykke.Service.Staking.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Staking.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string DefaultQueueName = "staking";
        private const string NotificationSystemPushNotificationsExchangeName = "notificationsystem.command.pushnotification";
        private const string TransactionFailedExchange = "lykke.wallet.transactionfailed";
        private const string TransactionSucceededExchange = "lykke.wallet.transactionsucceeded";
        private const string ReferralStakeReservedExchange = "lykke.wallet.referralstakereserved";
        private const string ReferralStakeReleasedExchange = "lykke.wallet.referralstakereleased";
        private const string ReferralStakeBurntExchange = "lykke.wallet.referralstakeburnt";
        private const string ReferralStakeStatusUpdatedExchange = "lykke.wallet.referralstakestatusupdated";
        private const string CustomerProfileDeactivationRequestedExchangeName = "lykke.customer.profiledeactivationrequested";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.StakingService.RabbitMq;
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

            builder.RegisterJsonRabbitSubscriber<TransactionFailedSubscriber, TransactionFailedEvent>(
                rabbitMqConnString,
                TransactionFailedExchange,
                DefaultQueueName);

            builder.RegisterJsonRabbitSubscriber<TransactionSucceededSubscriber, TransactionSucceededEvent>(
                rabbitMqConnString,
                TransactionSucceededExchange,
                DefaultQueueName);

            builder.RegisterJsonRabbitSubscriber<CustomerProfileDeactivationRequestedSubscriber, CustomerProfileDeactivationRequestedEvent>(
                rabbitMqConnString,
                CustomerProfileDeactivationRequestedExchangeName,
                DefaultQueueName);
        }
    }
}

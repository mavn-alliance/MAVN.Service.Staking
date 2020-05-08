using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.CustomerProfile.Contract;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;

namespace MAVN.Service.Staking.DomainServices.RabbitMq.Subscribers
{
    public class CustomerProfileDeactivationRequestedSubscriber : JsonRabbitSubscriber<CustomerProfileDeactivationRequestedEvent>
    {
        private readonly ICustomerProfileDeactivationRequestedHandler _handler;
        private readonly ILog _log;

        public CustomerProfileDeactivationRequestedSubscriber(
            ICustomerProfileDeactivationRequestedHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }


        protected override async Task ProcessMessageAsync(CustomerProfileDeactivationRequestedEvent message)
        {
            await _handler.HandleAsync(message.CustomerId);

            _log.Info("Handled CustomerProfileDeactivationRequestedEvent", message);
        }
    }
}

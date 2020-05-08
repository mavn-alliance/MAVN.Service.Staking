using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.Staking.Domain.RabbitMq.Handlers;

namespace MAVN.Service.Staking.DomainServices.RabbitMq.Subscribers
{
    public class TransactionSucceededSubscriber : JsonRabbitSubscriber<TransactionSucceededEvent>
    {
        private readonly ITransactionSucceededEventHandler _handler;
        private readonly ILog _log;

        public TransactionSucceededSubscriber(
            ITransactionSucceededEventHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }


        protected override async Task ProcessMessageAsync(TransactionSucceededEvent message)
        {
            await _handler.HandleAsync(message.OperationId);

            _log.Info("Handled transaction succeeded event", message);
        }
    }
}

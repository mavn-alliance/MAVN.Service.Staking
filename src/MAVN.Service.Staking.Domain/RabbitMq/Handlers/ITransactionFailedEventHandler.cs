using System.Threading.Tasks;

namespace MAVN.Service.Staking.Domain.RabbitMq.Handlers
{
    public interface ITransactionFailedEventHandler
    {
        Task HandleAsync(string operationId);
    }
}

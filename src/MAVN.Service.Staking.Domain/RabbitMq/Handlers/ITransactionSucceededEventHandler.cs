using System.Threading.Tasks;

namespace MAVN.Service.Staking.Domain.RabbitMq.Handlers
{
    public interface ITransactionSucceededEventHandler
    {
        Task HandleAsync(string operationId);
    }
}

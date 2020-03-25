using System.Threading.Tasks;

namespace Lykke.Service.Staking.Domain.RabbitMq.Handlers
{
    public interface ITransactionSucceededEventHandler
    {
        Task HandleAsync(string operationId);
    }
}

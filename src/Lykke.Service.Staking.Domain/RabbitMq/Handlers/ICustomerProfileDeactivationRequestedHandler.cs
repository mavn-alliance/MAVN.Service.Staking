using System.Threading.Tasks;

namespace Lykke.Service.Staking.Domain.RabbitMq.Handlers
{
    public interface ICustomerProfileDeactivationRequestedHandler
    {
        Task HandleAsync(string customerId);
    }
}

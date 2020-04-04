using System.Threading.Tasks;

namespace MAVN.Service.Staking.Domain.RabbitMq.Handlers
{
    public interface ICustomerProfileDeactivationRequestedHandler
    {
        Task HandleAsync(string customerId);
    }
}

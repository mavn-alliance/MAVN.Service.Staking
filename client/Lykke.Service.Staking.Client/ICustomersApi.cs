using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Staking.Client.Models;
using Refit;

namespace Lykke.Service.Staking.Client
{
    /// <summary>
    /// Customer client API interface.
    /// </summary>
    [PublicAPI]
    public interface ICustomersApi
    {
        /// <summary>
        /// Customer's stakes information
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [Get("/api/customers/{customerId}")]
        Task<CustomerStakesInformationResponse> GetCustomerStakesInfoAsync(string customerId);
    }
}

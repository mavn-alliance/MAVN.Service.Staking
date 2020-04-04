using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.Staking.Client.Models;
using Refit;

namespace MAVN.Service.Staking.Client
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

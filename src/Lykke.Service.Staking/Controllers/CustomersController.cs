using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.Staking.Client;
using Lykke.Service.Staking.Client.Models;
using Lykke.Service.Staking.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Staking.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : ControllerBase, ICustomersApi
    {
        private readonly IReferralStakesService _referralStakesService;

        public CustomersController(IReferralStakesService referralStakesService)
        {
            _referralStakesService = referralStakesService;
        }

        /// <summary>
        /// Customer's stakes information
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(CustomerStakesInformationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<CustomerStakesInformationResponse> GetCustomerStakesInfoAsync([Required] string customerId)
        {
            var result = await _referralStakesService.GetNumberOfStakedTokensForCustomer(customerId);

            return new CustomerStakesInformationResponse{ReferralReservedAmount = result};
        }
    }
}

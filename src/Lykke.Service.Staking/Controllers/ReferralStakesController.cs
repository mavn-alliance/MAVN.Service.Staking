using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.Staking.Client;
using Lykke.Service.Staking.Client.Models;
using Lykke.Service.Staking.Domain.Models;
using Lykke.Service.Staking.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Staking.Controllers
{
    [Route("api/referral-stakes")]
    [ApiController]
    public class ReferralStakesController : ControllerBase, IReferralStakesApi
    {
        private readonly IReferralStakesService _referralStakesService;
        private readonly IReferralStakesStatusUpdater _referralStakesStatusUpdater;
        private readonly IMapper _mapper;

        public ReferralStakesController(
            IReferralStakesService referralStakesService,
            IReferralStakesStatusUpdater referralStakesStatusUpdater,
            IMapper mapper)
        {
            _referralStakesService = referralStakesService;
            _referralStakesStatusUpdater = referralStakesStatusUpdater;
            _mapper = mapper;
        }
        /// <summary>
        /// Create a referral stake
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReferralStakeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<ReferralStakeResponse> ReferralStakeAsync([FromBody] ReferralStakeRequest request)
        {

            var result = await _referralStakesService.ReferralStakeAsync(_mapper.Map<ReferralStakeModel>(request));

            return new ReferralStakeResponse {Error = (ReferralStakeErrorCodes) result};
        }

        /// <summary>
        /// Release a referral stake
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ReferralStakeStatusUpdateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<ReferralStakeStatusUpdateResponse> ReleaseReferralStakeAsync([FromBody] ReleaseReferralStakeRequest request)
        {
            var result = await _referralStakesStatusUpdater.TokensBurnAndReleaseAsync(request.ReferralId, request.BurnRatio);

            return new ReferralStakeStatusUpdateResponse{Error = (ReferralStakeStatusUpdateErrorCodes)result};
        }

        /// <summary>
        /// Get referral stakes by customer and campaign
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReferralStakeResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IEnumerable<ReferralStakeResponseModel>> GetReferralStakesAsync([FromQuery] GetReferralStakesRequest request)
        {
            var result = await _referralStakesService.GetReferralStakesByCustomerAndCampaignIds(request.CustomerId, request.CampaignId);

            return _mapper.Map<IEnumerable<ReferralStakeResponseModel>>(result);
        }
    }
}

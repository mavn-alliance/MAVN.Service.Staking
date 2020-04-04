using Lykke.HttpClientGenerator;

namespace MAVN.Service.Staking.Client
{
    /// <summary>
    /// Staking API aggregating interface.
    /// </summary>
    public class StakingClient : IStakingClient
    {
        // Note: Add similar ReferralStakesApi properties for each new service controller

        /// <summary>Interface to Staking ReferralStakesApi.</summary>
        public IReferralStakesApi ReferralStakesApi { get; private set; }

        /// <summary>Interface to Staking CustomersApi.</summary>
        public ICustomersApi CustomersApi { get; private set; }

        /// <summary>C-tor</summary>
        public StakingClient(IHttpClientGenerator httpClientGenerator)
        {
            ReferralStakesApi = httpClientGenerator.Generate<IReferralStakesApi>();
            CustomersApi = httpClientGenerator.Generate<ICustomersApi>();
        }
    }
}

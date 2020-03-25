using JetBrains.Annotations;

namespace Lykke.Service.Staking.Client
{
    /// <summary>
    /// Staking client interface.
    /// </summary>
    [PublicAPI]
    public interface IStakingClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IReferralStakesApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application ReferralStakesApi interface</summary>
        IReferralStakesApi ReferralStakesApi { get; }

        /// <summary>Application ReferralStakesApi interface</summary>
        ICustomersApi CustomersApi { get; }
    }
}

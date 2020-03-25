namespace Lykke.Service.Staking.Client.Models
{
    /// <summary>
    /// Error codes
    /// </summary>
    public enum ReferralStakeStatusUpdateErrorCodes
    {
        /// <summary>
        /// No error
        /// </summary>
        None,
        /// <summary>
        /// The stake is not in a valid status for this update
        /// </summary>
        InvalidStatus,
        /// <summary>
        /// The stake does not exist
        /// </summary>
        DoesNotExist,
        /// <summary>
        /// Customer does not have a wallet
        /// </summary>
        CustomerWalletIsMissing,
        /// <summary>
        /// Customer's wallet is blocked
        /// </summary>
        CustomerWalletBlocked
    }
}

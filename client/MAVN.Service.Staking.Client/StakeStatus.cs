namespace MAVN.Service.Staking.Client
{
    /// <summary>
    /// Represents the status of the stake
    /// </summary>
    public enum StakeStatus
    {
        /// <summary>
        /// Tokens reservation is in progress
        /// </summary>
        TokensReservationStarted,
        /// <summary>
        /// Tokens for the stake are reserved successfully
        /// </summary>
        TokensReservationSucceeded,
        /// <summary>
        /// Tokens reservation failed and tokens are NOT reserved
        /// </summary>
        TokensReservationFailed,
        /// <summary>
        /// Tokens burn and release per burn ratio is in progress
        /// </summary>
        TokensBurnAndReleaseStarted,
        /// <summary>
        /// Tokens burnt and released successfully
        /// </summary>
        TokensBurnAndReleaseSucceeded,
        /// <summary>
        /// Tokens burn and release failed and tokens are NOT burnt and released
        /// </summary>
        TokensBurnAndReleaseFailed
    }
}

namespace Lykke.Service.Staking.Contract
{
    public enum StakeStatus
    {
        /// <summary>
        /// Tokens staking is in progress
        /// </summary>
        TokensReservationStarted,
        /// <summary>
        /// Tokens staking succeeded
        /// </summary>
        TokensReservationSucceeded,
        /// <summary>
        /// Tokens staking failed
        /// </summary>
        TokensReservationFailed,
        /// <summary>
        /// Tokens burn and release is in progress
        /// </summary>
        TokensBurnAndReleaseStarted,
        /// <summary>
        /// Tokens burn and release succeeded
        /// </summary>
        TokensBurnAndReleaseSucceeded,
        /// <summary>
        /// tokens burn and release failed
        /// </summary>
        TokensBurnAndReleaseFailed
    }
}

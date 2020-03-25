namespace Lykke.Service.Staking.Client
{
    /// <summary>
    /// Error codes for referral stake
    /// </summary>
    public enum ReferralStakeErrorCodes
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        /// <summary>
        /// The amount passed is not valid
        /// </summary>
        InvalidAmount,
        /// <summary>
        /// Warning period in days is not valid
        /// </summary>
        InvalidWarningPeriodInDays,
        /// <summary>
        /// Staking period in days is not valid
        /// </summary>
        InvalidStakingPeriodInDays,
        /// <summary>
        /// Warning period must not be >= staking period
        /// </summary>
        WarningPeriodShouldSmallerThanStakingPeriod,
        /// <summary>
        /// Burn ratio is not valid
        /// </summary>
        InvalidBurnRatio,
        /// <summary>
        /// There is already existing stake with the same id
        /// </summary>
        StakeAlreadyExist,
        /// <summary>
        /// The customer does not exist in the system
        /// </summary>
        CustomerDoesNotExist,
        /// <summary>
        /// The campaign does not exist in the system
        /// </summary>
        CampaignDoesNotExist,
        /// <summary>
        /// The customer does not have a wallet in the system
        /// </summary>
        CustomerWalletIsMissing,
        /// <summary>
        /// Customer's wallet is blocked
        /// </summary>
        CustomerWalletBlocked,
        /// <summary>
        /// Customer Id is not valid
        /// </summary>
        InvalidCustomerId,
        /// <summary>
        /// The customer does not have enough balance
        /// </summary>
        NotEnoughBalance,
    }
}

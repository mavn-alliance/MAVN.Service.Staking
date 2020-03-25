namespace Lykke.Service.Staking.Domain.Enums
{
    public enum ReferralStakeRequestErrorCode
    {
        None,
        InvalidAmount,
        InvalidWarningPeriodInDays,
        InvalidStakingPeriodInDays,
        WarningPeriodShouldSmallerThanStakingPeriod,
        InvalidBurnRatio,
        StakeAlreadyExist,
        CustomerDoesNotExist,
        CampaignDoesNotExist,
        CustomerWalletIsMissing,
        CustomerWalletBlocked,
        InvalidCustomerId,
        NotEnoughBalance,
    }
}

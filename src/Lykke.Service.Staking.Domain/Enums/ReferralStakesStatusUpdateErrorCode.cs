namespace Lykke.Service.Staking.Domain.Enums
{
    public enum ReferralStakesStatusUpdateErrorCode
    {
        None,
        InvalidStatus,
        DoesNotExist,
        CustomerWalletIsMissing,
        CustomerWalletBlocked
    }
}

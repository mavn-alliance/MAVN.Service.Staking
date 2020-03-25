namespace Lykke.Service.Staking.Domain.Services
{
    public interface ISettingsService
    {
        string GetTokenContractAddress();

        string GetMasterWalletAddress();
    }
}

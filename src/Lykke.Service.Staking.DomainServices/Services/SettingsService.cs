using Lykke.Service.Staking.Domain.Services;

namespace Lykke.Service.Staking.DomainServices.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _tokenContractAddress;
        private readonly string _masterWalletAddress;

        public SettingsService(string tokenContractAddress, string masterWalletAddress)
        {
            _tokenContractAddress = tokenContractAddress;
            _masterWalletAddress = masterWalletAddress;
        }

        public string GetTokenContractAddress()
            => _tokenContractAddress;

        public string GetMasterWalletAddress()
            => _masterWalletAddress;
    }
}

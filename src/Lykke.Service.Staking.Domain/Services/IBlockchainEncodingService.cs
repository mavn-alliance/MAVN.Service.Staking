using Falcon.Numerics;

namespace Lykke.Service.Staking.Domain.Services
{
    public interface IBlockchainEncodingService
    {
        string EncodeDecreaseRequestData(string walletAddress, Money18 amountToBurn, Money18 amountToRelease);

        string EncodeStakeRequestData(string walletAddress, Money18 amount);
    }
}

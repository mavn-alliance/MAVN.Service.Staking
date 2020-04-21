using Falcon.Numerics;
using MAVN.PrivateBlockchain.Definitions;
using MAVN.Service.Staking.Domain.Services;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;

namespace MAVN.Service.Staking.DomainServices.Common
{
    public class BlockchainEncodingService : IBlockchainEncodingService
    {
        private readonly FunctionCallEncoder _functionCallEncoder;
        public BlockchainEncodingService()
        {
            _functionCallEncoder = new FunctionCallEncoder();
        }

        public string EncodeDecreaseRequestData(string walletAddress, Money18 amountToBurn, Money18 amountToRelease)
        {
            var func = new DecreaseStakeFunction()
            {
                AmountToBurn = amountToBurn.ToAtto(),
                AmountToRelease = amountToRelease.ToAtto(),
                Account = walletAddress
            };

            return EncodeRequestData(func);
        }

        public string EncodeStakeRequestData(string walletAddress, Money18 amount)
        {
            var func = new IncreaseStakeFunction
            {
                Amount = amount.ToAtto(),
                Account = walletAddress
            };

            return EncodeRequestData(func);
        }


        private string EncodeRequestData<T>(T func)
            where T : class, new()
        {
            var abiFunc = ABITypedRegistry.GetFunctionABI<T>();
            var result = _functionCallEncoder.EncodeRequest(func, abiFunc.Sha3Signature);

            return result;
        }
    }
}

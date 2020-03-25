using Falcon.Numerics;

namespace Lykke.Service.Staking.Domain.Services
{
    public interface IMoneyFormatter
    {
        string FormatAmountToDisplayString(Money18 money18);
    }
}

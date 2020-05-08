using MAVN.Numerics;

namespace MAVN.Service.Staking.Domain.Services
{
    public interface IMoneyFormatter
    {
        string FormatAmountToDisplayString(Money18 money18);
    }
}

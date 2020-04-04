using System.Globalization;
using Falcon.Numerics;
using MAVN.Service.Staking.Domain.Services;

namespace MAVN.Service.Staking.DomainServices.Services
{
    public class MoneyFormatter : IMoneyFormatter
    {
        private readonly string _tokenFormatCultureInfo;
        private readonly int _tokenNumberDecimalPlaces;
        private readonly string _tokenIntegerPartFormat;

        public MoneyFormatter(
            string tokenFormatCultureInfo,
            int tokenNumberDecimalPlaces,
            string tokenIntegerPartFormat)
        {
            _tokenNumberDecimalPlaces = tokenNumberDecimalPlaces;
            _tokenFormatCultureInfo = tokenFormatCultureInfo;
            _tokenIntegerPartFormat = tokenIntegerPartFormat;
        }

        public string FormatAmountToDisplayString(Money18 money18)
        {
            var formatInfo =
                new CultureInfo(_tokenFormatCultureInfo).NumberFormat;

            return money18.ToString(_tokenIntegerPartFormat, _tokenNumberDecimalPlaces, formatInfo);
        }
    }
}

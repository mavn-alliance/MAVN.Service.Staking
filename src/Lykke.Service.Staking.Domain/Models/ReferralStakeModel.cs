using System;
using Falcon.Numerics;
using Lykke.Service.Staking.Domain.Enums;

namespace Lykke.Service.Staking.Domain.Models
{
    public class ReferralStakeModel
    {
        public string CustomerId { get; set; }

        public string ReferralId { get; set; }

        public string CampaignId { get; set; }

        public Money18 Amount { get; set; }

        public StakeStatus Status { get; set; }

        public int StakingPeriodInDays { get; set; }

        public int WarningPeriodInDays { get; set; }

        public decimal ExpirationBurnRatio { get; set; }

        public decimal? ReleaseBurnRatio { get; set; }

        public bool IsWarningSent { get; set; }

        public DateTime Timestamp { get; set; }
    }
}

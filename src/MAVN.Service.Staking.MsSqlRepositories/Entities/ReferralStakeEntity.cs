using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Falcon.Numerics;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;

namespace MAVN.Service.Staking.MsSqlRepositories.Entities
{
    [Table("referral_stakes")]
    public class ReferralStakeEntity
    {
        [Key]
        [Required]
        [Column("referral_id")]
        public string ReferralId { get; set; }

        [Required]
        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Required]
        [Column("campaign_id")]
        public string CampaignId { get; set; }

        [Required]
        [Column("amount")]
        public Money18 Amount { get; set; }

        [Required]
        [Column("status")]
        public StakeStatus Status { get; set; }

        [Required]
        [Column("staking_period_in_days")]
        public int StakingPeriodInDays { get; set; }

        [Required]
        [Column("warning_period_in_days")]
        public int WarningPeriodInDays { get; set; }

        [Required]
        [Column("expiration_burn_ratio")]
        public decimal ExpirationBurnRatio { get; set; }

        [Column("release_burn_ratio")]
        public decimal? ReleaseBurnRatio { get; set; }

        [Column("is_warning_sent")]
        public bool IsWarningSent { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        public static ReferralStakeEntity Create(ReferralStakeModel model)
        {
            return new ReferralStakeEntity
            {
                ReferralId = model.ReferralId,
                CampaignId = model.CampaignId,
                CustomerId = model.CustomerId,
                Amount = model.Amount,
                WarningPeriodInDays = model.WarningPeriodInDays,
                ExpirationBurnRatio = model.ExpirationBurnRatio,
                StakingPeriodInDays = model.StakingPeriodInDays,
                Status = model.Status,
                Timestamp = model.Timestamp,
                ReleaseBurnRatio = model.ReleaseBurnRatio,
                IsWarningSent = model.IsWarningSent,
            };
        }
    }
}

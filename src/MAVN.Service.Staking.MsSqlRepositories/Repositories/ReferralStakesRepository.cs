using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Common.MsSql;
using MAVN.Service.Staking.Domain.Enums;
using MAVN.Service.Staking.Domain.Models;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace MAVN.Service.Staking.MsSqlRepositories.Repositories
{
    public class ReferralStakesRepository : IReferralStakesRepository
    {
        private static readonly Expression<Func<ReferralStakeEntity, ReferralStakeModel>> SelectReferralStakeModelExpression = s =>
            new ReferralStakeModel
            {
                CustomerId = s.CustomerId,
                WarningPeriodInDays = s.WarningPeriodInDays,
                ExpirationBurnRatio = s.ExpirationBurnRatio,
                CampaignId = s.CampaignId,
                Amount = s.Amount,
                ReferralId = s.ReferralId,
                StakingPeriodInDays = s.StakingPeriodInDays,
                Status = s.Status,
                Timestamp = s.Timestamp,
                ReleaseBurnRatio = s.ReleaseBurnRatio,
                IsWarningSent = s.IsWarningSent,
            };

        private readonly MsSqlContextFactory<StakingContext> _contextFactory;

        public ReferralStakesRepository(MsSqlContextFactory<StakingContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(ReferralStakeModel referralStakeModel, TransactionContext txContext = null)
        {
            using (var context = _contextFactory.CreateDataContext(txContext))
            {
                var entity = ReferralStakeEntity.Create(referralStakeModel);

                context.ReferralStakes.Add(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task<ReferralStakeModel> GetByReferralIdAsync(string referralId, TransactionContext txContext = null)
        {
            using (var context = _contextFactory.CreateDataContext(txContext))
            {
                var entity = await context.ReferralStakes.FindAsync(referralId);

                if (entity == null)
                    return null;

                return new ReferralStakeModel
                {
                    CustomerId = entity.CustomerId,
                    WarningPeriodInDays = entity.WarningPeriodInDays,
                    ExpirationBurnRatio = entity.ExpirationBurnRatio,
                    CampaignId = entity.CampaignId,
                    Amount = entity.Amount,
                    ReferralId = entity.ReferralId,
                    StakingPeriodInDays = entity.StakingPeriodInDays,
                    Status = entity.Status,
                    Timestamp = entity.Timestamp,
                    ReleaseBurnRatio = entity.ReleaseBurnRatio,
                    IsWarningSent = entity.IsWarningSent,
                };
            }
        }

        public async Task SetStatusAsync(string referralId, StakeStatus status, TransactionContext txContext = null)
        {
            using (var context = _contextFactory.CreateDataContext(txContext))
            {
                var entity = new ReferralStakeEntity { ReferralId = referralId };

                context.ReferralStakes.Attach(entity);

                entity.Status = status;

                try
                {
                    await context.SaveChangesAsync();

                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException("Entity was not found during status update");
                }
            }
        }

        public async Task UpdateReferralStakeAsync(ReferralStakeModel stakeModel, TransactionContext txContext = null)
        {
            using (var context = _contextFactory.CreateDataContext(txContext))
            {
                var entity = ReferralStakeEntity.Create(stakeModel);

                context.ReferralStakes.Update(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task<string[]> GetExpiredStakesIdsAsync()
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var now = DateTime.UtcNow;

                var expiredReferralStakesIds = await context.ReferralStakes
                    .Where(s => s.Timestamp.AddDays(s.StakingPeriodInDays) < now &&
                                s.Status == StakeStatus.TokensReservationSucceeded)
                    .Select(s => s.ReferralId)
                    .ToArrayAsync();

                return expiredReferralStakesIds;
            }
        }

        public async Task<ReferralStakeModel[]> GetStakesForWarningAsync()
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var now = DateTime.UtcNow;

                var expiredReferralStakesIds = await context.ReferralStakes
                    .Where(s => s.Timestamp.AddDays(s.WarningPeriodInDays) < now &&
                                s.Timestamp.AddDays(s.StakingPeriodInDays) > now &&
                                s.Status == StakeStatus.TokensReservationSucceeded &&
                                !s.IsWarningSent && s.WarningPeriodInDays > 0)
                    .Select(SelectReferralStakeModelExpression)
                    .ToArrayAsync();

                return expiredReferralStakesIds;
            }
        }

        public async Task<Money18> GetNumberOfStakedTokensForCustomer(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var amounts = await context.ReferralStakes
                    .Where(s => s.CustomerId == customerId 
                                && (s.Status == StakeStatus.TokensReservationSucceeded ||
                                    s.Status == StakeStatus.TokensBurnAndReleaseStarted ||
                                    s.Status == StakeStatus.TokensBurnAndReleaseFailed))
                    .Select(x => x.Amount)
                    .ToArrayAsync();

                Money18 sum = 0;
                amounts.ForEach(a => sum += a);

                return sum;
            }
        }

        public async Task<IEnumerable<ReferralStakeModel>> GetReferralStakesByCustomerAndCampaignIds(string customerId, string campaignId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.ReferralStakes
                    .Where(s => s.CustomerId == customerId && (string.IsNullOrEmpty(campaignId) || s.CampaignId == campaignId))
                    .Select(SelectReferralStakeModelExpression)
                    .ToArrayAsync();

                return result;
            }
        }

        public async Task SetWarningSentAsync(string referralId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = new ReferralStakeEntity { ReferralId = referralId };

                context.ReferralStakes.Attach(entity);

                entity.IsWarningSent = true;

                try
                {
                    await context.SaveChangesAsync();

                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException("Entity was not found during warning status update");
                }
            }
        }

        public async Task<IEnumerable<string>> GetAllActiveStakesReferralIdsForCustomer(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.ReferralStakes
                    .Where(s => s.CustomerId == customerId && s.Status == StakeStatus.TokensReservationSucceeded)
                    .Select(s => s.ReferralId)
                    .ToArrayAsync();

                return result;
            }
        }
    }
}

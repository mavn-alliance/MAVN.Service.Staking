using Lykke.Service.Staking.Domain.Services;

namespace Lykke.Service.Staking.DomainServices.Services
{
    public class PushNotificationsSettingsService : IPushNotificationsSettingsService
    {
        public PushNotificationsSettingsService(string referralStakeWarningTemplateId)
        {
            ReferralStakeWarningTemplateId = referralStakeWarningTemplateId;
        }

        public string ReferralStakeWarningTemplateId { get; }
    }
}

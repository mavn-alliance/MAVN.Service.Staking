using MAVN.Service.Staking.Domain.Services;

namespace MAVN.Service.Staking.DomainServices.Services
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

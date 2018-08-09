using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Admin
{
    public class ProfileSettingViewModel: GeneralEntity
    {
        public int profileSettingId { get; set; }

        public int minRequiredPasswordLength { get; set; }

        public int minrequiredNonAlphanumericChar { get; set; }

        public bool? enablePasswordRetrieval { get; set; }

        public bool? enablePasswordReset { get; set; }

        public bool? requiresQuestionAndAnswer { get; set; }

        public bool? requiresUniqueEmail { get; set; }

        public int maxInvalidPasswordAttempts { get; set; }

        public int allowPasswordReuseAfter { get; set; }

        public int expirePasswordAfter { get; set; }

        public int maxPeriodOfUserInactivity { get; set; }

        public int sessionTimeOut { get; set; }

        public int BusinessStartTime { get; set; }

        public int BusinessCloseTime { get; set; }

    }
}

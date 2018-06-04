using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces;
using FintrakBanking.ViewModels.Admin;

namespace FintrakBanking.Repositories
{
    public class ProfileSetupRepository :IProfileSetupRepository
    {
        private FinTrakBankingContext _context;

        public ProfileSetupRepository(FinTrakBankingContext context)
        {
            this._context = context;
        }


        public ProfileSettingViewModel ProfileConfiguration(ProfileSettingViewModel entity)
        {
            var settings = _context.TBL_PROFILE_SETTING.FirstOrDefault(p => p.PROFILESETTINGID == entity.profileSettingId);
            if (settings != null)
            {
                settings.ALLOWPASSWORDREUSEAFTER = entity.allowPasswordReuseAfter;
                settings.ENABLEPASSWORDRESET = entity.enablePasswordReset;
                settings.ENABLEPASSWORDRETRIEVAL = entity.enablePasswordRetrieval;
                settings.EXPIREPASSWORDAFTER = entity.expirePasswordAfter;
                settings.MAXINVALIDPASSWORDATTEMPTS = entity.maxInvalidPasswordAttempts;
                settings.MAXPERIODOFUSERINACTIVITY = entity.maxPeriodOfUserInactivity;
                settings.MINREQUIREDNONALPHANUMERICCHAR = entity.minrequiredNonAlphanumericChar;
                settings.MINREQUIREDPASSWORDLENGTH = entity.minRequiredPasswordLength;
                settings.REQUIRESQUESTIONANDANSWER = entity.requiresQuestionAndAnswer;
                settings.REQUIRESUNIQUEEMAIL = entity.requiresUniqueEmail;
                settings.SESSIONTIMEOUT = entity.sessionTimeOut;
                _context.SaveChanges() ;
            }
            else
            {
                throw new Exception("Record not fund");
            }

            return entity;
        }
    }
}

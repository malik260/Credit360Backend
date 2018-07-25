using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.Repositories
{
    public class ProfileSetupRepository : IProfileSetupRepository
    {
        private FinTrakBankingContext _context;

        public ProfileSetupRepository(FinTrakBankingContext context)
        {
            this._context = context;
        }


        public ProfileSettingViewModel UpdateProfileConfiguration(ProfileSettingViewModel entity)
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
                settings.BusinessStartTime = entity.BusinessStartTime;
                settings.BusinessCloseTime = entity.BusinessCloseTime;

                _context.SaveChanges() ;
            }
            else
            {
                throw new SecureException("Record not fund");
            }

            return entity;
        }

        public ProfileSettingViewModel GetProfileConfiguration()
        {
            var settings = _context.TBL_PROFILE_SETTING.Select(p => new ProfileSettingViewModel()
            {
                allowPasswordReuseAfter = p.ALLOWPASSWORDREUSEAFTER,
                enablePasswordReset = p.ENABLEPASSWORDRESET,
                enablePasswordRetrieval = p.ENABLEPASSWORDRETRIEVAL,
                expirePasswordAfter = p.EXPIREPASSWORDAFTER,
                maxInvalidPasswordAttempts = p.MAXINVALIDPASSWORDATTEMPTS,
                maxPeriodOfUserInactivity = p.MAXPERIODOFUSERINACTIVITY,
                minrequiredNonAlphanumericChar = p.MINREQUIREDNONALPHANUMERICCHAR,
                minRequiredPasswordLength = p.MINREQUIREDPASSWORDLENGTH,
                requiresQuestionAndAnswer = p.REQUIRESQUESTIONANDANSWER,
                requiresUniqueEmail = p.REQUIRESUNIQUEEMAIL,
                sessionTimeOut = p.SESSIONTIMEOUT,
                BusinessStartTime = p.BusinessStartTime,
                BusinessCloseTime = p.BusinessCloseTime,
            }).FirstOrDefault();
            if (settings == null)
            { 
                throw new SecureException("Record not fund");
            }

            return settings;
        }
    }
}

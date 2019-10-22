using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories
{
    public class ProfileSetupRepository : IProfileSetupRepository
    {
        private FinTrakBankingContext _context;
        private IAuditTrailRepository _auditTrail;
        private IGeneralSetupRepository _genSetup;

        public ProfileSetupRepository(FinTrakBankingContext context,
                                        IGeneralSetupRepository genSetup,
                                        IAuditTrailRepository auditTrail)
        {
            this._context = context;
            this._auditTrail = auditTrail;
            this._genSetup = genSetup;

        }

        public ProfileSettingViewModel GetProfileSettings()
        {
            var data = (from p in _context.TBL_PROFILE_SETTING
                        select new ProfileSettingViewModel
                        {
                            profileSettingId = p.PROFILESETTINGID,
                            minRequiredPasswordLength = p.MINREQUIREDPASSWORDLENGTH,
                            minrequiredNonAlphanumericChar = p.MINREQUIREDNONALPHANUMERICCHAR,
                            enablePasswordRetrieval = p.ENABLEPASSWORDRETRIEVAL,
                            enablePasswordReset = p.ENABLEPASSWORDRESET,
                            requiresQuestionAndAnswer = p.REQUIRESQUESTIONANDANSWER,
                            requiresUniqueEmail = p.REQUIRESUNIQUEEMAIL,
                            maxInvalidPasswordAttempts = p.MAXINVALIDPASSWORDATTEMPTS,
                            allowPasswordReuseAfter = p.ALLOWPASSWORDREUSEAFTER,
                            expirePasswordAfter = p.EXPIREPASSWORDAFTER,
                            maxPeriodOfUserInactivity = p.MAXPERIODOFUSERINACTIVITY,
                            sessionTimeOut = p.SESSIONTIMEOUT,
                        }).FirstOrDefault();
            return data;

        }

    

        public ProfileSettingViewModel UpdateProfileConfiguration(ProfileSettingViewModel entity)
        {
            var settings = _context.TBL_PROFILE_SETTING.FirstOrDefault(p => p.PROFILESETTINGID == entity.profileSettingId);
            if (settings != null)
            {
                //settings.ALLOWPASSWORDREUSEAFTER = entity.allowPasswordReuseAfter;
                //settings.ENABLEPASSWORDRESET = entity.enablePasswordReset;
                //settings.ENABLEPASSWORDRETRIEVAL = entity.enablePasswordRetrieval;
                settings.EXPIREPASSWORDAFTER = entity.expirePasswordAfter;
                settings.MAXINVALIDPASSWORDATTEMPTS = entity.maxInvalidPasswordAttempts;
                //settings.MAXPERIODOFUSERINACTIVITY = entity.maxPeriodOfUserInactivity;
                settings.MINREQUIREDNONALPHANUMERICCHAR = entity.minrequiredNonAlphanumericChar;
                settings.MINREQUIREDPASSWORDLENGTH = entity.minRequiredPasswordLength;
                settings.REQUIRESQUESTIONANDANSWER = entity.requiresQuestionAndAnswer;
                //settings.REQUIRESUNIQUEEMAIL = entity.requiresUniqueEmail;
                settings.SESSIONTIMEOUT = entity.sessionTimeOut;
                //     settings.BusinessStartTime = entity.BusinessStartTime;
                //     settings.BusinessCloseTime = entity.BusinessCloseTime;
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProfileSettingsUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Updated application profile setting",
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                  

                };
                this._auditTrail.AddAuditTrail(audit);
               _context.SaveChanges();


                //var audit = new TBL_AUDIT();

                //audit.AUDITTYPEID = (short)AuditTypeEnum.ProfileSettingsUpdated;
                //audit.STAFFID = entity.createdBy;
                //audit.BRANCHID = (short)entity.userBranchId;
                //audit.DETAIL = $"Updated application profile setting";
                //audit.IPADDRESS = entity.userIPAddress;
                //audit.URL = entity.applicationUrl;
                //audit.APPLICATIONDATE = _genSetup.GetApplicationDate();
                //audit.SYSTEMDATETIME = DateTime.Now;
                //_context.TBL_AUDIT.Add(audit);

                //this.SaveAll();

            }
            else
            {
                throw new SecureException("Record not fund");
            }

            return entity;
        }
        private bool SaveAll()
        {
            return this._context.SaveChanges() > 0;
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
         //       BusinessStartTime = p.BusinessStartTime,
          //      BusinessCloseTime = p.BusinessCloseTime,
            }).FirstOrDefault();
            if (settings == null)
            { 
                throw new SecureException("Record not fund");
            }

            return settings;
        }
    }
}

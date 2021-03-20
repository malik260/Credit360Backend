using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.APICore.Filters;
using System.Configuration;
using FintrakBanking.Interfaces;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ProfileSettingsController : ApiControllerBase
    {

        private IProfileSetupRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ProfileSettingsController(IProfileSetupRepository _repo)
        {
            this.repo = _repo;
        }

         [HttpGet]
        [ClaimsAuthorization]

        public int? GetMinRequiredPasswordLength()
        {
                var data = repo.GetProfileSettings();
                if (data != null)
                {
                    return data.minRequiredPasswordLength;
                }
                else
                {
                    return (Convert.ToInt32(ConfigurationManager.AppSettings["minRequiredPasswordLength"]));
                }

        }

        [HttpGet]
        [ClaimsAuthorization]

        public int? GetMinRequiredNonAlphaNumericChar()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.minrequiredNonAlphanumericChar;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["minRequiredNonAlphanumericChar"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public bool? GetEnablePassWordRetrieval()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.enablePasswordRetrieval;
            }
            else
            {
                return (Convert.ToBoolean(ConfigurationManager.AppSettings["enablePasswordRetrieval"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public bool? GetEnablePasswordReset()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.enablePasswordReset;
            }
            else
            {
                return (Convert.ToBoolean(ConfigurationManager.AppSettings["enablePasswordReset"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public bool? GetRequiresQuestionAndAnswer()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.requiresQuestionAndAnswer;
            }
            else
            {
                return (Convert.ToBoolean(ConfigurationManager.AppSettings["requiresQuestionAndAnswer"]));
            }

        }

        [HttpGet]
        [ClaimsAuthorization]

        public bool? GetRequiresUniqueEmail()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.requiresUniqueEmail;
            }
            else
            {
                return (Convert.ToBoolean(ConfigurationManager.AppSettings["requiresUniqueEmail"]));
            }

        }
        
       [HttpGet]
        [ClaimsAuthorization]

        public int? GetMaxInvalidPasswordAttempts()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.maxInvalidPasswordAttempts;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["maxInvalidPasswordAttempts"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public int? GetAllowPasswordReuseAfter()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.allowPasswordReuseAfter;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["allowPasswordReuseAfter"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public int? GetExpirePasswordAfter()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.expirePasswordAfter;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["expirePasswordAfter"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public int? GetMaxPeriodOfUserInactivity()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.maxPeriodOfUserInactivity;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["maxPeriodOfUserInactivity"]));
            }

        }
        [HttpGet]
        [ClaimsAuthorization]

        public int? GetSessionTimeout()
        {
            var data = repo.GetProfileSettings();
            if (data != null)
            {
                return data.sessionTimeOut;
            }
            else
            {
                return (Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeOut"]));
            }

        }

    }
}
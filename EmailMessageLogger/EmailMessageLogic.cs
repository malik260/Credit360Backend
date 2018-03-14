using EmailMessageLogger.Enum;
using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.AppEmail;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailMessageLogger
{
   

   public class EmailMessageLogic
    {
        private static FinTrakBankingContext context=new FinTrakBankingContext();
        private static IAuditTrailRepository auditTrail;
        private static EmailHelpers emailHelpers;
        private static IGeneralSetupRepository genSetup;
        private static DateTime applDate;
        private static IStaffRepository staffRepo;

        FinTrakBankingContext dbContext = new FinTrakBankingContext();

        EmailAndAlertsRepository repo = new EmailAndAlertsRepository(
                context,
                auditTrail,
                emailHelpers,
                genSetup,
                staffRepo
            );

        public string Start()
        {
            string title = string.Empty;
            string body = string.Empty;

          var alertSetups =  context.TBL_MONITORING_ALERT_SETUP.ToList();

           repo.SendAlertsForCovenantsApproachingDueDate(title,body, alertSetups);

           repo.SendAlertsForCovenantsOverDue(title, body, alertSetups);

            //repo.SendAlertsForCollateralPropertyRevaluation(title, body);

            //repo.SendAlertsForLoanNplMonitoring(title, body);

            //repo.SendAlertsOnSelfLiquidatingLoanExpiry(title, body);

            //repo.SendAlertsOnOverDraftLoansAlmostDue(title, body);

            //repo.SendAlertsOnLoanCASAwithPND(title, body);

            //repo.SendAlertsForExpiredInsurance(title, body);

            return repo.response;
            
        }
    }
}

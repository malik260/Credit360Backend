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

namespace FinTrakMail
{
   

   public class PopulateAlertTable
    {
        private static FinTrakBankingContext context=new FinTrakBankingContext();
        private static IAuditTrailRepository auditTrail;
        private static EmailHelpers emailHelpers;
        private static IGeneralSetupRepository genSetup;
        private static DateTime applDate;
        private static IStaffRepository staffRepo;

        EmailAndAlertsRepository repo = new EmailAndAlertsRepository(
                context,
                auditTrail,
                emailHelpers,
                genSetup,
                staffRepo
            );

        public void Start()
        {
          
            //repo.SendAlertsForCovenantsApproachingDueDate();

            //repo.SendAlertsForCovenantsOverDue();

            //repo.SendAlertsForCollateralPropertyRevaluation();

            //repo.SendAlertsForLoanNplMonitoring();

            //repo.SendAlertsOnSelfLiquidatingLoanExpiry();

            //repo.SendAlertsOnOverDraftLoansAlmostDue();

            //repo.SendAlertsOnLoanCASAwithPND();

            repo.SendAlertsForExpiredInsurance();

        }
    }
}

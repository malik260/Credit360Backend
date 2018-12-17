using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;


namespace FintrakBanking.Repositories.Finance
{
    public class EndOfDayRepository : IEndOfDayRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanOperationsRepository loanOperation;
        private IPublicHolidayRepository publicHoliday;
        private ICustomerCollateralRepository collateralItemPolicy;

        public EndOfDayRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup,
                                    ILoanOperationsRepository _loanOperation, IPublicHolidayRepository _publicHoliday,
                                    IAuditTrailRepository _auditTrail, ICustomerCollateralRepository _collateralItemPolicy)
        {
            this.context = _context;
            this.generalSetup = _generalSetup;
            this.publicHoliday = _publicHoliday;
            this.auditTrail = _auditTrail;
            this.loanOperation = _loanOperation;
            this.collateralItemPolicy = _collateralItemPolicy;
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public bool RunEndOfDay(EndOfDayViewModel model)
        {

            var applicationDate = generalSetup.GetApplicationDate();

            var financeEod = (from e in context.TBL_FINANCE_ENDOFDAY
                              where e.COMPANYID == model.companyId && e.DATE == applicationDate
                              select e.DATE).Any();


            if (financeEod == true)
                throw new ConditionNotMetException("End of Day for " + applicationDate + " has already been run.");

            var countryId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).COUNTRYID;

            var nextWorkDay = publicHoliday.GetNextWorkDay(applicationDate, countryId);

            if (applicationDate.AddDays(1) == nextWorkDay)
            {
                ProcessEndOfDay(applicationDate, model.companyId, model.createdBy);
            }
            else
            {
                DateTime runDate = applicationDate;

                do
                {
                    ProcessEndOfDay(runDate, model.companyId, model.createdBy);

                    runDate = runDate.AddDays(1);

                    var currentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
                    currentDate.CURRENTDATE = runDate;

                    //begin of day //

                    context.SaveChanges();
                }
                while (runDate < nextWorkDay);
            }

            var financeCurrentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
            financeCurrentDate.CURRENTDATE = nextWorkDay;
            financeCurrentDate.REFRESHSTATUS = false;
            //begin of day //


            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RanEndOfDay,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Ran end of day from {applicationDate.ToString("dd/mmm/yyyy")} to {nextWorkDay.AddDays(-1).ToString("dd/mmm/yyyy")} successfully",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
            };


            auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return true;
        }


        public IEnumerable<FinanceEndofdayViewModel> GetFinanceEndofday(int companyId)
        {
            var financeEod = (from e in context.TBL_FINANCE_ENDOFDAY
                              where e.COMPANYID == companyId // e.EndDateTime == null && e.StartDateTime == null
                              select new FinanceEndofdayViewModel()
                              {
                                  endOfDayId = e.ENDOFDAYID,
                                  date = e.DATE,
                                  startDateTime = e.STARTDATETIME,
                                  endDateTime = e.ENDDATETIME,
                                  createdBy = e.CREATEDBY,
                              });
            return financeEod;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ProcessEndOfDay(DateTime date, int companyId, int staffId)
        {
            TBL_FINANCE_ENDOFDAY endOfDay = new TBL_FINANCE_ENDOFDAY();

            endOfDay.COMPANYID = companyId;
            endOfDay.DATE = date;
            endOfDay.CREATEDBY = staffId;
            endOfDay.STARTDATETIME = DateTime.Now;

            loanOperation.ProcessAutomaticInterestRepricing(date, staffId);

            loanOperation.ProcessReleaseLien(date);

            loanOperation.ProcessDailyTermLoansInterestAccrual(date);

            //loanOperation.ProcessDailyUnauthorisedOverdraftInterestAccrual(date);

            //loanOperation.ProcessDailyUnauthorisedOverdraftInterestAccrual(date);

            loanOperation.ProcessDailyInterestOnPastDueInterestAccrual(date);

            loanOperation.ProcessDailyInterestOnPastDuePrincipalAccrual(date);


            loanOperation.ProcessDailyFeeAccrual(date);//TODO use batch posting and ensure the right accounting entries are passed

            loanOperation.ProcessDailyTaxAccrual(date); //TODO use batch posting and ensure the right accounting entries are passed


            //loanOperation.ProcessIntervalFeeandCommissionPosting(date); //TODO use batch posting and ensure the right accounting entries are passed

            loanOperation.ProcessLoanRepaymentPostingForceDebit(date);

            loanOperation.ProcessLoanRepaymentPostingPastDue(date);

            //loanOperation.ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(date);
            //loanOperation.ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(date);

            // loanOperation.ProcessIDFExpiryAndlocking(date);            
            //loanOperation.ProcessCFFExpiryAndlocking(date);
            //loanOperation.ProcessLPOExpiryAndlocking(date);

            //loanOperation.ProcessOverdraftBalanceSuspensionBaseOnCovenant(date);
            //loanOperation.ProcessOverdraftBalanceSuspensionBaseOnCleanUp(date);



            //collateralItemPolicy.CheckForExpiredItemPolicies(date);

            loanOperation.ProcessContingentLiabilityTerminationAtMaturity(date);

            loanOperation.CalculateLoanClassification(date);

            //loanOperation.GetRepaymentFromStaging();

            endOfDay.ENDDATETIME = DateTime.Now;

            context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);

            context.SaveChanges();
        }


    }
}

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Transactions;

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
        private ILoanCovenantRepository loanCovenantRepository;

        public EndOfDayRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup,
                                    ILoanOperationsRepository _loanOperation, IPublicHolidayRepository _publicHoliday,
                                    IAuditTrailRepository _auditTrail, ICustomerCollateralRepository _collateralItemPolicy, ILoanCovenantRepository _loanCovenantRepository)
        {
            this.context = _context;
            this.generalSetup = _generalSetup;
            this.publicHoliday = _publicHoliday;
            this.auditTrail = _auditTrail;
            this.loanOperation = _loanOperation;
            this.collateralItemPolicy = _collateralItemPolicy;
            this.loanCovenantRepository = _loanCovenantRepository;
        }

        
        [OperationBehavior(TransactionScopeRequired = true)]
        public bool RunEndOfDay(EndOfDayViewModel model)
        {
            
            var applicationDate = generalSetup.GetApplicationDate();

            var financeEod = (from e in context.TBL_FINANCE_ENDOFDAY
                              where e.COMPANYID == model.companyId && e.DATE == applicationDate && e.EODSTATUSID == (int)EodOperationStatusEnum.Completed
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
                                  eodStatus = context.TBL_EOD_STATUS.Where(x => x.EODSTATUSID == e.EODSTATUSID).Select(x => x.EODSTATUSNAME).FirstOrDefault(),

                                  // (decimal?)b.OVERDRAFTTOPUP ?? 0,
                              });
            return financeEod;
        }

        public IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLog(DateTime oedDate, int companyId)
        {
            var financeEod = (from e in context.TBL_EOD_OPERATION_LOG
                              join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                              where e.COMPANYID == companyId && e.EODDATE == oedDate 
                              select new FinanceEndofdayViewModel()
                              {
                                  eodOperationLogId = e.EODOPERATIONLOGID,
                                  eodOperationId = e.EODOPERATIONID,
                                  eodOperation = context.TBL_EOD_OPERATION.Where(x => x.EODOPERATIONID == e.EODOPERATIONID).Select(x => x.EODOPERATIONNAME).FirstOrDefault(),
                                  startDateTime = (DateTime)e.STARTDATETIME,
                                  endDateTime = e.ENDDATETIME,
                                  eodDate = e.EODDATE,
                                  eodStatusId = e.EODSTATUSID,
                                  eodStatus = context.TBL_EOD_STATUS.Where(x => x.EODSTATUSID == e.EODSTATUSID).Select(x => x.EODSTATUSNAME).FirstOrDefault(),
                                  companyId = e.COMPANYID,
                                  companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == companyId).Select(x => x.NAME).FirstOrDefault(),
                              }).ToList();
            return financeEod;
        }
        
        [OperationBehavior(TransactionScopeRequired = true)]
        public void ProcessEndOfDay(DateTime date, int companyId, int staffId)
        {

            //if (date.Day == 1)
            //{
            //    loanOperation.UpdateLoanClassification(date);
            //}

            TBL_FINANCE_ENDOFDAY endOfDay = new TBL_FINANCE_ENDOFDAY();

            var eod = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == date && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();
            
            if(eod == null)
            {
                endOfDay.COMPANYID = companyId;
                endOfDay.DATE = date;
                endOfDay.CREATEDBY = staffId;
                endOfDay.STARTDATETIME = DateTime.Now;
                endOfDay.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
            }

            context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);

            context.SaveChanges();



            using (TransactionScope transactionScope = new TransactionScope())
            {

                try
                {
                    var eodOperationProcess = context.TBL_EOD_OPERATION_LOG.Where(x => x.EODDATE == date && x.COMPANYID == companyId).FirstOrDefault();

                    
                    List<TBL_EOD_OPERATION_LOG> eod_Operation_Log_List = new List<TBL_EOD_OPERATION_LOG>();

                    if (eodOperationProcess == null)
                    {
                        var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                        foreach (TBL_EOD_OPERATION eodOperation in eodOperations)
                        {
                            TBL_EOD_OPERATION_LOG eod_Operation_Log = new TBL_EOD_OPERATION_LOG();

                            if (eodOperation.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanClassification)
                            {
                                if (date.Day == 1)
                                {
                                    eod_Operation_Log.EODOPERATIONID = eodOperation.EODOPERATIONID;
                                    eod_Operation_Log.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                                    eod_Operation_Log.EODDATE = date;
                                    eod_Operation_Log.COMPANYID = companyId;
                                    eod_Operation_Log_List.Add(eod_Operation_Log);
                                }
                            }
                            else
                            {
                                eod_Operation_Log.EODOPERATIONID = eodOperation.EODOPERATIONID;
                                eod_Operation_Log.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                                eod_Operation_Log.EODDATE = date;
                                eod_Operation_Log.COMPANYID = companyId;
                                eod_Operation_Log_List.Add(eod_Operation_Log);
                            }
                            
                        }

                        context.TBL_EOD_OPERATION_LOG.AddRange(eod_Operation_Log_List);

                        context.SaveChanges();

                    }

                    transactionScope.Complete();

                    transactionScope.Dispose();
                }
                catch (TransactionException ex)
                {
                    transactionScope.Dispose();
                    throw ex;
                }

            }

            //var eodOperationProcesses = context.TBL_EOD_OPERATION_LOG.Where(x => x.EODDATE == date && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).ToList();

            var eodOperationProcesses = (from e in context.TBL_EOD_OPERATION_LOG
                                         join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                                         where e.COMPANYID == companyId && e.EODDATE == date && e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
                                         select new FinanceEndofdayViewModel()
                                         {
                                             eodOperationLogId = e.EODOPERATIONLOGID,
                                             eodOperationId = e.EODOPERATIONID,
                                             eodDate = e.EODDATE,
                                             eodStatusId = e.EODSTATUSID,
                                             companyId = e.COMPANYID
                                         }).ToList();


            if (eodOperationProcesses != null)
            {


                foreach (FinanceEndofdayViewModel eodOperationProc in eodOperationProcesses)
                {
                    var eodOperationProcessesUpdate = context.TBL_EOD_OPERATION_LOG.Where(x => x.EODOPERATIONLOGID == eodOperationProc.eodOperationLogId).FirstOrDefault();

                    if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessAutomaticInterestRepricing)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessAutomaticInterestRepricing(date, staffId);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessReleaseLien)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessReleaseLien(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessDailyTermLoansInterestAccrual(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessDailyInterestOnPastDueInterestAccrual(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessDailyInterestOnPastDuePrincipalAccrual(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }


                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessLoanRepaymentPostingForceDebit(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessLoanRepaymentPostingPastDue(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    //else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessContingentLiabilityTerminationAtMaturity)
                    //{
                    //    eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                    //    context.SaveChanges();

                    //    using (TransactionScope transactionScope = new TransactionScope())
                    //    {

                    //        try
                    //        {

                    //            loanOperation.ProcessContingentLiabilityTerminationAtMaturity(date);

                    //            transactionScope.Complete();

                    //            transactionScope.Dispose();
                    //        }
                    //        catch (TransactionException ex)
                    //        {
                    //            transactionScope.Dispose();
                    //            throw ex;
                    //        }

                    //    }

                    //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                    //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                    //    context.SaveChanges();
                    //}
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.UpdateLoanApplicationCovenant)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanCovenantRepository.UpdateLoanApplicationCovenant(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.ProcessAutomaticCommercialLoanRollover(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.UpdateLoanClassification)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        using (TransactionScope transactionScope = new TransactionScope())
                        {

                            try
                            {

                                loanOperation.UpdateLoanClassification(date);

                                transactionScope.Complete();

                                transactionScope.Dispose();
                            }
                            catch (TransactionException ex)
                            {
                                transactionScope.Dispose();
                                throw ex;
                            }

                        }

                        eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                        context.SaveChanges();
                    }
                }



            }



            //loanOperation.ProcessAutomaticInterestRepricing(date, staffId);

            //loanOperation.ProcessReleaseLien(date);

            //loanOperation.ProcessDailyTermLoansInterestAccrual(date);

            ////loanOperation.ProcessDailyUnauthorisedOverdraftInterestAccrual(date);

            ////loanOperation.ProcessDailyUnauthorisedOverdraftInterestAccrual(date);

            //loanOperation.ProcessDailyInterestOnPastDueInterestAccrual(date);

            //loanOperation.ProcessDailyInterestOnPastDuePrincipalAccrual(date);


            ////loanOperation.ProcessDailyFeeAccrual(date);//TODO use batch posting and ensure the right accounting entries are passed

            ////loanOperation.ProcessDailyTaxAccrual(date); //TODO use batch posting and ensure the right accounting entries are passed


            ////loanOperation.ProcessIntervalFeeandCommissionPosting(date); //TODO use batch posting and ensure the right accounting entries are passed

            //loanOperation.ProcessLoanRepaymentPostingForceDebit(date);

            //loanOperation.ProcessLoanRepaymentPostingPastDue(date);

            //loanOperation.ProcessAutomaticCommercialLoanRollover(date);

            ////loanOperation.ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(date);
            ////loanOperation.ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(date);

            //// loanOperation.ProcessIDFExpiryAndlocking(date);            
            ////loanOperation.ProcessCFFExpiryAndlocking(date);
            ////loanOperation.ProcessLPOExpiryAndlocking(date);

            ////loanOperation.ProcessOverdraftBalanceSuspensionBaseOnCovenant(date);
            ////loanOperation.ProcessOverdraftBalanceSuspensionBaseOnCleanUp(date);



            ////collateralItemPolicy.CheckForExpiredItemPolicies(date);

            //loanOperation.ProcessContingentLiabilityTerminationAtMaturity(date);

            //loanOperation.CalculateLoanClassification(date);

            //loanCovenantRepository.UpdateLoanApplicationCovenant(date);

            ////loanOperation.GetRepaymentFromStaging();

            var eodNew = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == date && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();

            eodNew.ENDDATETIME = DateTime.Now;
            eodNew.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
            //context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);
            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool RefreshLoanClassification()
        {
            var applicationDate = generalSetup.GetApplicationDate();
            var result = loanOperation.UpdateLoanClassification(applicationDate);
            return result;
        }
        
        public bool GetRunningEndOfDayProcess(int companyId)
        {
            var applicationDate = generalSetup.GetApplicationDate();

            var eodOperationProcess = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == applicationDate && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();

            if (eodOperationProcess == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}

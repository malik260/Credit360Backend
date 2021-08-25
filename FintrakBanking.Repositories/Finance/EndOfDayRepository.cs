using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Entities.StagingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Transactions;
using System.Data.Entity;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.Common;
using System.Diagnostics;

namespace FintrakBanking.Repositories.Finance
{
    public class EndOfDayRepository : IEndOfDayRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanOperationsRepository loanOperation;
        private IPublicHolidayRepository publicHoliday;
        private FinTrakBankingStagingContext stagingContext;
        private ICustomerCollateralRepository collateralItemPolicy;
        private ILoanCovenantRepository loanCovenantRepository;
        private IFinacleIntegrationRepository finacleIntegration;

        public EndOfDayRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup,
                                    ILoanOperationsRepository _loanOperation, IPublicHolidayRepository _publicHoliday,
                                    IAuditTrailRepository _auditTrail, ICustomerCollateralRepository _collateralItemPolicy, ILoanCovenantRepository _loanCovenantRepository, FinTrakBankingStagingContext _stagingContext, IFinacleIntegrationRepository _finacleIntegration)
        {
            this.context = _context;
            this.generalSetup = _generalSetup;
            this.publicHoliday = _publicHoliday;
            this.auditTrail = _auditTrail;
            this.loanOperation = _loanOperation;
            this.collateralItemPolicy = _collateralItemPolicy;
            this.loanCovenantRepository = _loanCovenantRepository;
            this.stagingContext = _stagingContext;
            this.finacleIntegration = _finacleIntegration;
        }

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool RunEndOfDay(EndOfDayViewModel model)
        //{

        //    var applicationDate = generalSetup.GetApplicationDate();

        //    var financeEod = (from e in context.TBL_FINANCE_ENDOFDAY
        //                      where e.COMPANYID == model.companyId && e.DATE == applicationDate && e.EODSTATUSID == (int)EodOperationStatusEnum.Completed
        //                      select e.DATE).Any();


        //    if (financeEod == true)
        //        throw new ConditionNotMetException("End of Day for " + applicationDate + " has already been run.");

        //    var countryId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).COUNTRYID;

        //    var nextWorkDay = publicHoliday.GetNextWorkDay(applicationDate, countryId);

        //    //if (applicationDate.AddDays(1) == nextWorkDay)
        //    //{


        //    //    ProcessEndOfDay(applicationDate, model.companyId, model.createdBy);

        //    //}
        //    //else
        //    //{
        //    DateTime runDate = applicationDate;

        //    do
        //    {
        //        ProcessEndOfDay(runDate, model.companyId, model.createdBy);

        //        runDate = runDate.AddDays(1);

        //        var currentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
        //        currentDate.CURRENTDATE = runDate;
        //        currentDate.REFRESHSTATUS = false;
        //        //begin of day //

        //        ProcessBeginOfDay(runDate, model.companyId, model.createdBy);

        //        context.SaveChanges();


        //    }
        //    while (runDate < nextWorkDay);
        //    //}

        //    //var financeCurrentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
        //    //financeCurrentDate.CURRENTDATE = nextWorkDay;
        //    //financeCurrentDate.REFRESHSTATUS = false;

        //    //ProcessBeginOfDay(nextWorkDay, model.companyId, model.createdBy);

        //    //begin of day //


        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.RanEndOfDay,
        //        STAFFID = (int)model.createdBy,
        //        BRANCHID = (short)model.userBranchId,
        //        DETAIL = $"Ran end of day from {applicationDate.ToString("dd/mmm/yyyy")} to {nextWorkDay.AddDays(-1).ToString("dd/mmm/yyyy")} successfully",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = applicationDate,
        //        SYSTEMDATETIME = DateTime.Now
        //    };


        //    auditTrail.AddAuditTrail(audit);

        //    var response = context.SaveChanges();

        //    return true;
        //}
        // [OperationBehavior(TransactionScopeRequired = true)]
        public bool RunEndOfDay(EndOfDayViewModel model)
        {

            bool status = false;

            var applicationDate = generalSetup.GetApplicationDate();

            var financeEod = (from e in context.TBL_FINANCE_ENDOFDAY
                              where e.COMPANYID == model.companyId && e.DATE == applicationDate && e.EODSTATUSID == (int)EodOperationStatusEnum.Completed
                              select e.DATE).Any();


            if (financeEod == true)
            {
                throw new ConditionNotMetException("End of Day for " + applicationDate + " has already been run.");
            }
            var countryId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).COUNTRYID;

            var nextWorkDay = publicHoliday.GetNextWorkDay(applicationDate, countryId);

            //if (applicationDate.AddDays(1) == nextWorkDay)
            //{


            //    ProcessEndOfDay(applicationDate, model.companyId, model.createdBy);

            //}
            //else
            //{
            DateTime runDate = applicationDate;


            do
            {
                ProcessEndOfDay(runDate, model.companyId, model.createdBy);

                runDate = runDate.AddDays(1);

                //begin of day //
                ProcessBeginOfDay(runDate, model.companyId, model.createdBy);

                DateTime currentDateNew = runDate.AddDays(-1);

                var validateTotalCompletion = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == currentDateNew && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODSTATUSID != (int)EodOperationStatusEnum.PartialCompletion).FirstOrDefault();

                validateTotalCompletion = null;
                if (validateTotalCompletion == null)
                {
                    var currentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
                    currentDate.CURRENTDATE = runDate;
                    currentDate.REFRESHSTATUS = false;
                    status = true;
                }
                else
                {
                    status = false;
                    runDate = nextWorkDay;
                }

                context.SaveChanges();


            }
            while (runDate < nextWorkDay);

            //do
            //{
            //    ProcessEndOfDay(runDate, model.companyId, model.createdBy);

            //    runDate = runDate.AddDays(1);

            //    var currentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
            //    currentDate.CURRENTDATE = runDate;
            //    currentDate.REFRESHSTATUS = false;
            //    //begin of day //

            //    ProcessBeginOfDay(runDate, model.companyId, model.createdBy);

            //    context.SaveChanges();


            //}
            //while (runDate < nextWorkDay);
            //}



            //var financeCurrentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
            //financeCurrentDate.CURRENTDATE = nextWorkDay;
            //financeCurrentDate.REFRESHSTATUS = false;

            //ProcessBeginOfDay(nextWorkDay, model.companyId, model.createdBy);

            //begin of day //


            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RanEndOfDay,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Ran end of day from {applicationDate.ToString("dd/MM/yyyy")} to {nextWorkDay.AddDays(-1).ToString("dd/MM/yyyy")} successfully",
                IPADDRESS = model.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()

            };


            auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return status;
        }

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool RunEndOfDayVersion2(EndOfDayViewModel model)
        //{
        //    var pendingEOD = context.TBL_FINANCE_ENDOFDAY.Where(x => x.COMPANYID == model.companyId).OrderByDescending(x => x.DATE).Take(1).FirstOrDefault();


        //    if (pendingEOD.EODSTATUSID == (int)EodOperationStatusEnum.Processing)
        //    {
        //        ProcessEndOfDay(pendingEOD.DATE, model.companyId, model.createdBy);

        //        DateTime previousEodDateNew = pendingEOD.DATE.AddDays(1);

        //        //Begin of day //
        //        ProcessBeginOfDay(previousEodDateNew, model.companyId, model.createdBy);
        //    }

        //    var applicationDate = generalSetup.GetApplicationDate();

        //    DateTime previousEodDate = context.TBL_FINANCE_ENDOFDAY.Where(x => x.COMPANYID == model.companyId).Select(x => x.DATE).Max();             

        //    var countryId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).COUNTRYID;

        //    //var nextWorkDay = publicHoliday.GetNextWorkDay(previousEodDate.AddDays(1), countryId);

        //    //DateTime runDate = applicationDate;

        //    if (applicationDate > previousEodDate)
        //    {

        //        while (applicationDate != previousEodDate)
        //        {

        //            //do
        //            //{

        //            previousEodDate = previousEodDate.AddDays(1);

        //            //End of day //
        //            ProcessEndOfDay(previousEodDate, model.companyId, model.createdBy);

        //            DateTime nextEodDate = previousEodDate.AddDays(1);

        //            //Begin of day //
        //            ProcessBeginOfDay(nextEodDate, model.companyId, model.createdBy);

        //            //}
        //            //while (previousEodDate < nextWorkDay);

        //        }


        //        var audit = new TBL_AUDIT
        //        {
        //            AUDITTYPEID = (short)AuditTypeEnum.RanEndOfDay,
        //            STAFFID = (int)model.createdBy,
        //            BRANCHID = (short)model.userBranchId,
        //            DETAIL = $"Ran end of day from {previousEodDate.ToString("dd/MM/yyyy")} to {applicationDate.AddDays(-1).ToString("dd/MM/yyyy")} successfully",
        //            IPADDRESS = model.userIPAddress,
        //            URL = model.applicationUrl,
        //            APPLICATIONDATE = applicationDate,
        //            SYSTEMDATETIME = DateTime.Now
        //        };


        //        auditTrail.AddAuditTrail(audit);

        //        var response = context.SaveChanges();

        //    }
        //    else
        //    {
        //        throw new ConditionNotMetException($"End of day for {previousEodDate.ToString("dd/MM/yyyy")} already exist");  
        //    }

        //    return true;
        //}

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ChangeApplicationDate(EndOfDayViewModel model)
        {

            var applicationDate = generalSetup.GetApplicationDate();

            var countryId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).COUNTRYID;

            var nextWorkDay = publicHoliday.GetNextWorkDay(applicationDate, countryId);


            DateTime runDate = applicationDate;

            do
            {
                runDate = runDate.AddDays(1);

                var currentDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault();
                currentDate.CURRENTDATE = runDate;
                currentDate.REFRESHSTATUS = true;

                context.SaveChanges();


            }
            while (runDate < nextWorkDay);


            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RanEndOfDay,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Ran end of day from {applicationDate.ToString("dd/mmm/yyyy")} to {nextWorkDay.AddDays(-1).ToString("dd/mmm/yyyy")} successfully",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                              }).OrderByDescending(x => x.date);
            return financeEod;
        }

        public IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLog(DateTime eodDate, int companyId)
        {
            var financeEod = (from e in context.TBL_EOD_OPERATION_LOG
                              join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                              where e.COMPANYID == companyId && e.EODDATE == eodDate
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
                                  errorInformation = e.ERRORINFORMATION,
                              }).ToList();
            return financeEod;
        }

        //[OperationBehavior(TransactionScopeRequired = true)]
        public void ProcessEndOfDay(DateTime date, int companyId, int staffId)
        {



            //if (date.Day == 1)
            //{
            //    loanOperation.UpdateLoanClassification(date);
            //}

            //var stringData = "";

            TBL_FINANCE_ENDOFDAY endOfDay = new TBL_FINANCE_ENDOFDAY();

            var eod = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == date && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();

            if (eod == null)
            {
                endOfDay.COMPANYID = companyId;
                endOfDay.DATE = date;
                endOfDay.CREATEDBY = staffId;
                endOfDay.STARTDATETIME = DateTime.Now;
                endOfDay.EODSTATUSID = (int)EodOperationStatusEnum.Processing;

                context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);
                context.SaveChanges();
            }

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
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    throw ex;
                }

            }

            //var eodOperationProcesses = context.TBL_EOD_OPERATION_LOG.Where(x => x.EODDATE == date && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).ToList();

            //var eodOperationProcesses = (from e in context.TBL_EOD_OPERATION_LOG
            //                             join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
            //                             where e.COMPANYID == companyId && e.EODDATE == date && e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
            //                             select new FinanceEndofdayViewModel()
            //                             {
            //                                 eodOperationLogId = e.EODOPERATIONLOGID,
            //                                 eodOperationId = e.EODOPERATIONID,
            //                                 eodDate = e.EODDATE,
            //                                 eodStatusId = e.EODSTATUSID,
            //                                 companyId = e.COMPANYID
            //                             }).ToList();

            var eodOperationProcesses = (from e in context.TBL_EOD_OPERATION_LOG
                                         join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                                         where e.COMPANYID == companyId && e.EODDATE == date && (e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
                                         && e.EODOPERATIONID != (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit && e.EODOPERATIONID != (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue && e.EODOPERATIONID != (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover)
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

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessAutomaticInterestRepricing(date, staffId, companyId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;

                        }

                        // }



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticInterestRepricing && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}


                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessReleaseLien)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessReleaseLien(date, companyId, staffId); 

                            //transactionScope.Complete();
                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessReleaseLien && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessDailyTermLoansInterestAccrual(date, companyId, staffId, context);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex) //TransactionException
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException?.InnerException?.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}





                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyTermLoansInterestAccrual && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessDailyInterestOnPastDueInterestAccrual(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException?.InnerException?.Message;
                            }




                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDueInterestAccrual && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessDailyInterestOnPastDuePrincipalAccrual(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyInterestOnPastDuePrincipalAccrual && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.UpdateLoanClassification)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.UpdateLoanClassification(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanClassification && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.UpdateLoanApplicationCovenant)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        string transactionReferenceNo = "";

                        try
                        {
                            loanCovenantRepository.UpdateLoanApplicationCovenant(date, companyId, staffId, out transactionReferenceNo);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException?.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();
                            //throw ex;

                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.DailyWrittenOffFacilityAccrual(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.DailyWrittenOffFacilityAccrual && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}

                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessDailyFeeAccrual)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            //loanOperation.DailyWrittenOffFacilityAccrual(date, companyId);

                            loanOperation.ProcessDailyFeeAccrual(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();
                            // Get stack trace for the exception with source file information
                            var st = new StackTrace(ex, true);
                            // Get the top stack frame
                            var frame = st.GetFrame(0);
                            // Get the line number from the stack frame
                            var line = frame.GetFileLineNumber();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException?.InnerException?.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.ProcessDailyFeeAccrual && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
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

            //////var eodNew = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == date && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();

            //////eodNew.ENDDATETIME = DateTime.Now;
            //////eodNew.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
            ////////context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);
            //////context.SaveChanges();
        }

        //[OperationBehavior(TransactionScopeRequired = true)]
        public void ProcessBeginOfDay(DateTime date, int companyId, int staffId)
        {

            string stringData = "";

            DateTime dateChange = date;

            dateChange = dateChange.AddDays(-1);

            var eodOperationProcesses = (from e in context.TBL_EOD_OPERATION_LOG
                                         join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                                         where e.COMPANYID == companyId && e.EODDATE == dateChange
                                         &&
                                         (e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
                                         &&
                                         (e.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit
                                         || e.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue
                                         || e.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover))
                                         select new FinanceEndofdayViewModel()
                                         {
                                             eodOperationLogId = e.EODOPERATIONLOGID,
                                             eodOperationId = e.EODOPERATIONID,
                                             eodDate = e.EODDATE,
                                             eodStatusId = e.EODSTATUSID,
                                             companyId = e.COMPANYID
                                         }).ToList();


            if (eodOperationProcesses.Count() > 0)
            {


                foreach (FinanceEndofdayViewModel eodOperationProc in eodOperationProcesses)
                {
                    var eodOperationProcessesUpdate = context.TBL_EOD_OPERATION_LOG.Where(x => x.EODOPERATIONLOGID == eodOperationProc.eodOperationLogId).FirstOrDefault();


                    if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessLoanRepaymentPostingForceDebit(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();
                            // Get stack trace for the exception with source file information
                            var st = new StackTrace(ex, true);
                            // Get the top stack frame
                            var frame = st.GetFrame(0);
                            // Get the line number from the stack frame
                            var line = frame.GetFileLineNumber();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException?.InnerException?.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();
                            //throw ex;

                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingForceDebit && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();
                            
                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {
                            loanOperation.ProcessLoanRepaymentPostingPastDue(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;
                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessLoanRepaymentPostingPastDue && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }
                    else if (eodOperationProc.eodOperationId == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover)
                    {
                        eodOperationProcessesUpdate.STARTDATETIME = DateTime.Now;
                        context.SaveChanges();

                        //using (TransactionScope transactionScope = new TransactionScope())
                        //{

                        try
                        {

                            loanOperation.ProcessAutomaticCommercialLoanRollover(date, companyId, staffId);

                            //transactionScope.Complete();

                            //transactionScope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //transactionScope.Dispose();

                            var innerException = "";
                            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                            {
                                innerException = ex.InnerException.InnerException.Message;
                            }

                            //stringData = $"Ref No - {loanOperation.GetTransactionReferenceNo()} Exception - {ex.Message}  - inner exception -  {innerException}";

                            eodOperationProcessesUpdate.ERRORINFORMATION = innerException;
                            context.SaveChanges();

                            //throw ex;

                        }

                        //}



                        var validateTransactionCompleted = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == dateChange && c.EODOPERATIONID == (int)EodOperationEnum.ProcessAutomaticCommercialLoanRollover && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

                        if (validateTransactionCompleted == null)
                        {
                            eodOperationProcessesUpdate.ERRORINFORMATION = "No Error";
                            eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                            eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            context.SaveChanges();
                        }
                        //else
                        //{
                        //    eodOperationProcessesUpdate.ERRORINFORMATION = "Some Error";
                        //    eodOperationProcessesUpdate.ENDDATETIME = DateTime.Now;
                        //    eodOperationProcessesUpdate.EODSTATUSID = (int)EodOperationStatusEnum.PartialCompletion;
                        //    context.SaveChanges();

                        //}
                    }

                }

            }


            var validateTotalCompletion = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.EODDATE == dateChange && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed).FirstOrDefault();

            if (validateTotalCompletion == null)
            {
                var eodNew = context.TBL_FINANCE_ENDOFDAY.Where(x => x.DATE == dateChange && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing).FirstOrDefault();

                eodNew.ENDDATETIME = DateTime.Now;
                eodNew.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                //context.TBL_FINANCE_ENDOFDAY.Add(endOfDay);
                context.SaveChanges();
            }

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool RefreshLoanClassification(int companyId)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            var result = loanOperation.UpdateLoanClassification(applicationDate, companyId);
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

        public IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLogMonitoring(int companyId)
        {
            //var financeEodNew = (from e in context.TBL_FINANCE_ENDOFDAY
            //                     where e.COMPANYID == companyId && e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
            //                     select e.DATE).FirstOrDefault();

            var financeEodNew = (from e in context.TBL_FINANCECURRENTDATE
                                 where e.COMPANYID == companyId
                                 //&& e.EODSTATUSID == (int)EodOperationStatusEnum.Processing
                                 select e.CURRENTDATE).FirstOrDefault();

            var financeEod = (from e in context.TBL_EOD_OPERATION_LOG
                              join f in context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION) on e.EODOPERATIONID equals f.EODOPERATIONID
                              where e.COMPANYID == companyId && e.EODDATE == financeEodNew.Date
                              select new FinanceEndofdayViewModel()
                              {
                                  eodOperationLogId = e.EODOPERATIONLOGID,
                                  eodOperationId = e.EODOPERATIONID,
                                  eodOperation = context.TBL_EOD_OPERATION.Where(x => x.EODOPERATIONID == e.EODOPERATIONID).Select(x => x.EODOPERATIONNAME).FirstOrDefault(),
                                  startDateTime = e.STARTDATETIME,
                                  endDateTime = e.ENDDATETIME,
                                  eodDate = e.EODDATE,
                                  eodStatusId = e.EODSTATUSID,
                                  eodStatus = context.TBL_EOD_STATUS.Where(x => x.EODSTATUSID == e.EODSTATUSID).Select(x => x.EODSTATUSNAME).FirstOrDefault(),
                                  companyId = e.COMPANYID,
                                  companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == companyId).Select(x => x.NAME).FirstOrDefault(),
                                  errorInformation = e.ERRORINFORMATION,
                              }).ToList();
            return financeEod;
        }

        public IEnumerable<RefreshStagingMonitoringModel> RefreshStagingMonitoring(DateTime stateDate, DateTime endDate)
        {

            var financeEod = (from e in stagingContext.FINTRAK_TRAN_PROC_DETAILS
                              where DbFunctions.TruncateTime(e.RCRE_DATE) >= stateDate && DbFunctions.TruncateTime(e.RCRE_DATE) <= endDate
                              group e by e.STATUS into g
                              select new RefreshStagingMonitoringModel()
                              {
                                  status = g.Key,
                                  count = g.Count()
                              }).ToList();

            return financeEod;

        }

    }
}

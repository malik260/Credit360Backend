using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.Credit;

namespace FintrakBanking.Repositories.credit
{
    public class LcIssuanceRepository : ILcIssuanceRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;
        private ICreditDrawdownRepository drawdownRepo;

        public LcIssuanceRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow,
                ICreditDrawdownRepository _drawdownRepo
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
            this.drawdownRepo = _drawdownRepo;
        }
        
        #region LCISSUANCE

        public List<LcIssuanceApprovalViewModel> SearchLc(string searchString)
        {
                int[] operations = { (int)OperationsEnum.lcIssuance, (int)OperationsEnum.lcReleaseOfShippingDocuments, (int)OperationsEnum.lcUssance};
            int[] currentApprovalLevelStatuses = {(int)LoanApplicationStatusEnum.LcIssuanceInProgress, (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress};

                searchString = searchString.Trim().ToLower();

            var applications = (from x in context.TBL_LC_ISSUANCE
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                join y in context.TBL_APPROVAL_TRAIL on x.LCISSUANCEID equals y.TARGETID
                                join z in context.TBL_LCRELEASE_AMOUNT on x.LCISSUANCEID equals z.LCISSUANCEID into xz
                                from rel in xz.DefaultIfEmpty()
                                join z2 in context.TBL_APPROVAL_TRAIL on rel.LCRELEASEAMOUNTID equals z2.TARGETID into relz2
                                from reltrail in relz2.DefaultIfEmpty()
                                join u in context.TBL_LC_USSANCE on x.LCISSUANCEID equals u.LCISSUANCEID into usance
                                from u in usance.DefaultIfEmpty()
                                join ut in context.TBL_APPROVAL_TRAIL on u.LCUSSANCEID equals ut.TARGETID into ustr
                                from usstrail in ustr.DefaultIfEmpty()
                                where
                                //y.RESPONSESTAFFID == null
                                (
                                 (y.OPERATIONID == (int)OperationsEnum.lcIssuance
                                 &&
                                 ((reltrail.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments || reltrail == null)
                                 && (usstrail.OPERATIONID == (int)OperationsEnum.lcUssance || usstrail == null))
                                 )
                                 &&
                                (
                                (x.LCREFERENCENUMBER.Trim().ToLower().Contains(searchString))
                                || c.FIRSTNAME.ToLower().Contains(searchString)
                                || c.LASTNAME.ToLower().Contains(searchString)
                                || c.MIDDLENAME.ToLower().Contains(searchString)
                                || c.CUSTOMERCODE.Contains(searchString)
                                || x.FORMMNUMBER.ToString().Trim().ToLower().Contains(searchString)
                                || x.LCISSUANCEID.ToString().Trim().ToLower().Contains(searchString)
                                )
                                )
                                select new LcIssuanceApprovalViewModel
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    lcReferenceNumber = x.LCREFERENCENUMBER,
                                    lcIssuanceId = x.LCISSUANCEID,
                                    customerId = c.CUSTOMERID,
                                    currencyId = x.CURRENCYID,
                                    operationId = y.OPERATIONID,
                                    lcReleaseAmountId = rel.LCRELEASEAMOUNTID,
                                    releaseAmount = rel.RELEASEAMOUNT,
                                    releaseApplicationStatus = rel.RELEASEAPPLICATIONSTATUSID == null ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == rel.RELEASEAPPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(),
                                    releaseApprovalStatus = rel.RELEASEAPPROVALSTATUSID == null ? "n/a" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == reltrail.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    releaseApprovalTrailId = reltrail.APPROVALTRAILID,
                                    releaseCurrentApprovalLevel = reltrail.TOAPPROVALLEVELID == null ? "n/a" : ((reltrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && reltrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == reltrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == reltrail.TOAPPROVALLEVELID).LEVELNAME),
                                    lcUssanceId = u.LCUSSANCEID,
                                    arrivalDate = y.ARRIVALDATE,
                                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                    //approvedAmount = x.APPROVEDAMOUNT,
                                    applicationStatusId = x.APPLICATIONSTATUSID,
                                    lcApplicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                    approvalStatusId = (short)y.APPROVALSTATUSID,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == y.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    currentApprovalLevelId = y.TOAPPROVALLEVELID,
                                    currentApprovalLevel = y.TOAPPROVALLEVELID != null ? ((y.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && y.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == y.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == y.TOAPPROVALLEVELID).LEVELNAME) : "n/a",
                                    lcApprovalTrailId = y.APPROVALTRAILID,
                                    responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                    usanceStatus = (u.USANCEAPPLICATIONSTATUSID == null || u == null) ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == u.USANCEAPPLICATIONSTATUSID).APPLICATIONSTATUSNAME,
                                    usanceApprovalStatus = (u.USANCEAPPROVALSTATUSID == null || u == null) ? "n/a" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == u.USANCEAPPROVALSTATUSID).APPROVALSTATUSNAME,
                                    UsanceCurrentApprovalLevel = (usstrail.TOAPPROVALLEVELID != null) ? ((usstrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && usstrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.TOAPPROVALLEVELID).LEVELNAME) : "n/a",
                                    usanceApprovalTrailId = usstrail.APPROVALTRAILID,
                                    totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                    createdBy = (int)x.CREATEDBY,
                                    lcReleases = (from rel in context.TBL_LCRELEASE_AMOUNT
                                                  let reltrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments && 
                                                  t.TARGETID == rel.LCRELEASEAMOUNTID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                                                  where x.LCISSUANCEID == rel.LCISSUANCEID &&
                                                  
                                                  rel.DELETED == false
                                                  select new LcReleaseAmountViewModel
                                                  {
                                                      lcReleaseAmountId = rel.LCRELEASEAMOUNTID,
                                                      lcIssuanceId = rel.LCISSUANCEID,
                                                      releaseAmount = rel.RELEASEAMOUNT,
                                                      releaseApplicationStatus = rel.RELEASEAPPLICATIONSTATUSID == null ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == rel.RELEASEAPPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(),
                                                      releaseApprovalStatus = rel.RELEASEAPPROVALSTATUSID == null ? "n/a" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == reltrail.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                      releaseCurrentApprovalLevel = reltrail.TOAPPROVALLEVELID == null ? "n/a" : ((reltrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && reltrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == reltrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == reltrail.TOAPPROVALLEVELID).LEVELNAME),
                                                  }).ToList(),
                                    lcUsances = (from u in context.TBL_LC_USSANCE
                                                 let usstrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.lcUssance &&
                                                 t.TARGETID == u.LCUSSANCEID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                                                 where x.LCISSUANCEID == u.LCISSUANCEID &&
                                                 u.DELETED == false
                                                 select new LcUssanceViewModel
                                                 {
                                                     lcUssanceId = u.LCUSSANCEID,
                                                     lcIssuanceId = u.LCISSUANCEID,
                                                     ussanceAmount = u.USSANCEAMOUNT,
                                                     ussanceRate = u.USSANCERATE,
                                                     ussanceTenor = u.USSANCETENOR,
                                                     lcEffectiveDate = u.LCUSSANCEEFFECTIVEDATE,
                                                     lcMaturityDate = u.LCUSSANCEMATURITYDATE,
                                                     usanceAmountCurrencyId = u.USANCEAMOUNTCURRENCYID,
                                                     usanceStatus = (u.USANCEAPPLICATIONSTATUSID == null || u == null) ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == u.USANCEAPPLICATIONSTATUSID).APPLICATIONSTATUSNAME,
                                                     usanceApprovalStatus = (u.USANCEAPPROVALSTATUSID == null || u == null) ? "n/a" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == u.USANCEAPPROVALSTATUSID).APPROVALSTATUSNAME,
                                                     UsanceCurrentApprovalLevel = (usstrail.TOAPPROVALLEVELID != null) ? ((usstrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && usstrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.TOAPPROVALLEVELID).LEVELNAME) : "n/a",
                                                     totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                                 }).ToList(),
                                    //operationId = x.OPERATIONID,
                                })
                                    .GroupBy(a => a.lcIssuanceId).Select(g => g.OrderByDescending(l => l.lcApprovalTrailId)
                                    .ThenByDescending(l => l.releaseApprovalTrailId).ThenByDescending(l => l.usanceApprovalTrailId)
                                    .FirstOrDefault())
                                    .ToList();
            //foreach (var app in applications)
            //{
            //    //var releases = context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == app.lcIssuanceId).ToList();
            //    //foreach (var r in releases)
            //    //{
            //    //    //app.lcReleaseAmountId
            //    //}
            //    //VerifyIssuanceOrReleaseApprovalLevelId(app);
            //}
            List<LcIssuanceApprovalViewModel> apps = new List<LcIssuanceApprovalViewModel>();
                apps.AddRange(applications);
                return apps;
        }

        public List<LcIssuanceApprovalViewModel> SearchLcLMS(string searchString)
        {
            int[] operations = { (int)OperationsEnum.lcIssuance, (int)OperationsEnum.LCEnhancementApproval, (int)OperationsEnum.LCTerminationApproval,
                (int)OperationsEnum.LCIssuanceExtensionApproval, (int)OperationsEnum.LCUsanceExtensionApproval};

            searchString = searchString.Trim().ToLower();

            var applications = (from a in context.TBL_LC_ISSUANCE
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                join canct in context.TBL_APPROVAL_TRAIL on a.LCISSUANCEID equals canct.TARGETID into xcanct
                                from canct in xcanct.DefaultIfEmpty()
                                where
                                //y.RESPONSESTAFFID == null
                                (
                                 (operations.Contains(canct.OPERATIONID) || canct == null)
                                 //((canct.OPERATIONID == (int)OperationsEnum.LCTerminationApproval) || canct == null)
                                 &&
                                (
                                (a.LCREFERENCENUMBER.Trim().ToLower().Contains(searchString))
                                || c.FIRSTNAME.ToLower().Contains(searchString)
                                || c.LASTNAME.ToLower().Contains(searchString)
                                || c.MIDDLENAME.ToLower().Contains(searchString)
                                || c.CUSTOMERCODE.Contains(searchString)
                                || a.FORMMNUMBER.ToString().Trim().ToLower().Contains(searchString)
                                || a.LCISSUANCEID.ToString().Trim().ToLower().Contains(searchString)
                                )
                                )
                                select new LcIssuanceApprovalViewModel
                                {
                                    lcIssuanceId = a.LCISSUANCEID,
                                    isDraftRequired = a.ISDRAFTREQUIRED,
                                    lcReferenceNumber = a.LCREFERENCENUMBER,
                                    letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                                    beneficiaryName = a.BENEFICIARYNAME,
                                    totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                                    totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                                    availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                                    cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                                    cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCENUMBER,
                                    cashBuildUpReferenceType = a.CASHBUILDUPREFERENCETYPE,
                                    percentageToCover = a.PERCENTAGETOCOVER,
                                    lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                                    lcToleranceValue = a.LCTOLERANCEVALUE,
                                    releaseAmount = a.RELEASEDAMOUNT,
                                    beneficiaryAddress = a.BENEFICIARYADDRESS,
                                    beneficiaryEmail = a.BENEFICIARYEMAIL,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerId = a.CUSTOMERID,
                                    fundSourceId = a.FUNDSOURCEID,
                                    fundSourceDetails = a.FUNDSOURCEDETAILS,
                                    formMNumber = a.FORMMNUMBER,
                                    beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                                    beneficiaryBank = a.BENEFICIARYBANK,
                                    currencyId = a.CURRENCYID,
                                    proformaInvoiceId = a.PROFORMAINVOICEID,
                                    availableAmount = a.AVAILABLEAMOUNT,
                                    letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                                    letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                                    invoiceDate = a.INVOICEDATE,
                                    invoiceDueDate = a.INVOICEDUEDATE,
                                    transactionCycle = a.TRANSACTIONCYCLE,
                                    lastComment = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? "N/A" : canct.COMMENT,
                                    currentApprovalLevel = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? "N/A" : canct.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                    currentlyWith = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? "N/A" : (canct.TOSTAFFID > 0) ? canct.TBL_STAFF2.FIRSTNAME + " " + canct.TBL_STAFF2.MIDDLENAME + " " + canct.TBL_STAFF2.LASTNAME : (canct.LOOPEDSTAFFID > 0) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == canct.LOOPEDSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == canct.LOOPEDSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == canct.LOOPEDSTAFFID).LASTNAME : canct.TBL_APPROVAL_LEVEL1.TBL_STAFF_ROLE.STAFFROLENAME,
                                    responsiblePerson = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? "N/A" : canct.TBL_STAFF1.STAFFCODE + " - " + canct.TBL_STAFF1.FIRSTNAME + " " + canct.TBL_STAFF1.MIDDLENAME + " " + canct.TBL_STAFF1.LASTNAME,
                                    currentApprovalLevelTypeId = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? 0 : canct.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                                    lcApprovalTrailId = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? 0 : canct.APPROVALTRAILID, // for inner sequence ordering
                                    approvalStatus = (canct == null || canct.OPERATIONID != (int)OperationsEnum.LCTerminationApproval) ? "N/A" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == canct.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    lcApplicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                    createdBy = (int)a.CREATEDBY,
                                    dateTimeCreated = (DateTime)a.DATETIMECREATED,
                                    lcEnhancements = (from b in context.TBL_TEMP_LC_ISSUANCE
                                                     let usstrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.LCEnhancementApproval &&
                                                     t.TARGETID == b.TEMPLCISSUANCEID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                                                     where b.LCISSUANCEID == a.LCISSUANCEID &&
                                                     (b.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcEnhancementInProgress || b.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcEnhancementCompleted) &&
                                                     b.DELETED == false
                                                     select new LcIssuanceViewModel
                                                     {
                                                         lcIssuanceId = b.LCISSUANCEID,
                                                         tempLcIssuanceId = b.TEMPLCISSUANCEID,
                                                         beneficiaryName = b.BENEFICIARYNAME,
                                                         totalApprovedAmount = b.TOTALAPPROVEDAMOUNT,
                                                         totalApprovedAmountCurrencyId = b.TOTALAPPROVEDAMOUNTCURRENCYID,
                                                         availableAmountCurrencyId = b.AVAILABLEAMOUNTCURRENCYID,
                                                         customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                                         formMNumber = b.FORMMNUMBER,
                                                         currencyId = b.CURRENCYID,
                                                         availableAmount = b.AVAILABLEAMOUNT,
                                                         letterOfCreditAmount = b.LETTEROFCREDITAMOUNT,
                                                         letterOfcreditExpirydate = b.LETTEROFCREDITEXPIRYDATE,
                                                         createdBy = (int)b.CREATEDBY,
                                                         operationId = (int)OperationsEnum.LCEnhancementApproval,
                                                         dateTimeCreated = (DateTime)b.DATETIMECREATED,
                                                         currentlyWith = (usstrail == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).LASTNAME : (usstrail.TOSTAFFID > 0) ? usstrail.TBL_STAFF2.FIRSTNAME + " " + usstrail.TBL_STAFF2.MIDDLENAME + " " + usstrail.TBL_STAFF2.LASTNAME : (usstrail.LOOPEDSTAFFID > 0) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).LASTNAME : usstrail.TBL_APPROVAL_LEVEL1.TBL_STAFF_ROLE.STAFFROLENAME,
                                                         arrivalDate = (usstrail == null) ? (DateTime)b.DATETIMECREATED : usstrail.SYSTEMARRIVALDATETIME,
                                                         requestApprovalLevel = (usstrail != null) ? ((usstrail.FROMAPPROVALLEVELID == usstrail.TOAPPROVALLEVELID || usstrail.FROMAPPROVALLEVELID == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.FROMAPPROVALLEVELID).LEVELNAME) : "N/A",
                                                         requestStaffName = (usstrail == null) ? "N/A" : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).LASTNAME,
                                                         approvalStatus = (usstrail == null) ? "N/A" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == usstrail.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                         currentApprovalLevel = (usstrail.TOAPPROVALLEVELID != null) ? ((usstrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && usstrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.TOAPPROVALLEVELID).LEVELNAME) : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).TBL_STAFF_ROLE.STAFFROLENAME,
                                                     }).ToList(),
                                    lcExtensions = (from b in context.TBL_TEMP_LC_ISSUANCE
                                                      let usstrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.LCIssuanceExtensionApproval &&
                                                      t.TARGETID == b.TEMPLCISSUANCEID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                                                      where b.LCISSUANCEID == a.LCISSUANCEID &&
                                                      (b.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceExtensionInProgress || b.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceExtensionCompleted) &&
                                                      b.DELETED == false
                                                      select new LcIssuanceViewModel
                                                      {
                                                          lcIssuanceId = b.LCISSUANCEID,
                                                          tempLcIssuanceId = b.TEMPLCISSUANCEID,
                                                          beneficiaryName = b.BENEFICIARYNAME,
                                                          totalApprovedAmount = b.TOTALAPPROVEDAMOUNT,
                                                          totalApprovedAmountCurrencyId = b.TOTALAPPROVEDAMOUNTCURRENCYID,
                                                          availableAmountCurrencyId = b.AVAILABLEAMOUNTCURRENCYID,
                                                          customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                                          formMNumber = b.FORMMNUMBER,
                                                          currencyId = b.CURRENCYID,
                                                          availableAmount = b.AVAILABLEAMOUNT,
                                                          letterOfCreditAmount = b.LETTEROFCREDITAMOUNT,
                                                          letterOfcreditExpirydate = b.LETTEROFCREDITEXPIRYDATE,
                                                          createdBy = (int)b.CREATEDBY,
                                                          operationId = (int)OperationsEnum.LCIssuanceExtensionApproval,
                                                          dateTimeCreated = (DateTime)b.DATETIMECREATED,
                                                          currentlyWith = (usstrail == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).LASTNAME : (usstrail.TOSTAFFID > 0) ? usstrail.TBL_STAFF2.FIRSTNAME + " " + usstrail.TBL_STAFF2.MIDDLENAME + " " + usstrail.TBL_STAFF2.LASTNAME : (usstrail.LOOPEDSTAFFID > 0) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).LASTNAME : usstrail.TBL_APPROVAL_LEVEL1.TBL_STAFF_ROLE.STAFFROLENAME,
                                                          arrivalDate = (usstrail == null) ? (DateTime)b.DATETIMECREATED : usstrail.SYSTEMARRIVALDATETIME,
                                                          requestApprovalLevel = (usstrail != null) ? ((usstrail.FROMAPPROVALLEVELID == usstrail.TOAPPROVALLEVELID || usstrail.FROMAPPROVALLEVELID == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.FROMAPPROVALLEVELID).LEVELNAME) : "N/A",
                                                          requestStaffName = (usstrail == null) ? "N/A" : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).LASTNAME,
                                                          approvalStatus = (usstrail == null) ? "N/A" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == usstrail.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                          currentApprovalLevel = (usstrail.TOAPPROVALLEVELID != null) ? ((usstrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && usstrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.TOAPPROVALLEVELID).LEVELNAME) : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == b.CREATEDBY).TBL_STAFF_ROLE.STAFFROLENAME,
                                                      }).ToList(),
                                    lcUsanceExtensions = (from ux in context.TBL_TEMP_LC_USSANCE
                                                          join u in context.TBL_LC_USSANCE on ux.LCUSSANCEID equals u.LCUSSANCEID
                                                          let usstrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.LCUsanceExtensionApproval &&
                                                          t.TARGETID == ux.TEMPLCUSSANCEID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                                                          where ux.LCISSUANCEID == a.LCISSUANCEID &&
                                                          ux.USANCEEXTENSIONAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcUsanceExtensionCompleted
                                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                                          select new LcUssanceViewModel
                                                          {
                                                                lcIssuanceId = a.LCISSUANCEID,
                                                                lcUssanceId = u.LCUSSANCEID,
                                                                tempLcUsanceId = ux.TEMPLCUSSANCEID,
                                                                ussanceAmount = u.USSANCEAMOUNT,
                                                                ussanceRate = u.USSANCERATE,
                                                                oldUssanceTenor = ux.OLDUSSANCETENOR,
                                                                ussanceTenor = ux.NEWUSSANCETENOR,
                                                                lcEffectiveDate = u.LCUSSANCEEFFECTIVEDATE,
                                                                oldLcMaturityDate = ux.OLDLCUSSANCEMATURITYDATE,
                                                                lcMaturityDate = ux.NEWLCUSSANCEMATURITYDATE,
                                                                currencyId = (int)u.USANCEAMOUNTCURRENCYID,
                                                                customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                                                createdBy = ux.CREATEDBY,
                                                                operationId = (int)OperationsEnum.LCIssuanceExtensionApproval,
                                                                dateTimeCreated = ux.DATETIMECREATED,
                                                                currentlyWith = (usstrail == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == ux.CREATEDBY).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == ux.CREATEDBY).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == ux.CREATEDBY).LASTNAME : (usstrail.TOSTAFFID > 0) ? usstrail.TBL_STAFF2.FIRSTNAME + " " + usstrail.TBL_STAFF2.MIDDLENAME + " " + usstrail.TBL_STAFF2.LASTNAME : (usstrail.LOOPEDSTAFFID > 0) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).LASTNAME : usstrail.TBL_APPROVAL_LEVEL1.TBL_STAFF_ROLE.STAFFROLENAME,
                                                                arrivalDate = (usstrail == null) ? (DateTime)ux.DATETIMECREATED : usstrail.SYSTEMARRIVALDATETIME,
                                                                requestApprovalLevel = (usstrail != null) ? ((usstrail.FROMAPPROVALLEVELID == usstrail.TOAPPROVALLEVELID || usstrail.FROMAPPROVALLEVELID == null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.FROMAPPROVALLEVELID).LEVELNAME) : "N/A",
                                                                requestStaffName = (usstrail == null) ? "N/A" : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).FIRSTNAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).MIDDLENAME + " " + context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.REQUESTSTAFFID).LASTNAME,
                                                                approvalStatus = (usstrail == null) ? "N/A" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == usstrail.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                                currentApprovalLevel = (usstrail.TOAPPROVALLEVELID != null) ? ((usstrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && usstrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == usstrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == usstrail.TOAPPROVALLEVELID).LEVELNAME) : context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == ux.CREATEDBY).TBL_STAFF_ROLE.STAFFROLENAME,
                                                            }).ToList(),
                                                lcUsances = (from u in context.TBL_LC_USSANCE
                                                             where a.LCISSUANCEID == u.LCISSUANCEID &&
                                                             u.DELETED == false
                                                             select new LcUssanceViewModel
                                                             {
                                                                 lcIssuanceId = u.LCISSUANCEID,
                                                                 lcUssanceId = u.LCUSSANCEID,
                                                                 tempLcUsanceId = 0,
                                                                 ussanceAmount = u.USSANCEAMOUNT,
                                                                 ussanceRate = u.USSANCERATE,
                                                                 oldUssanceTenor = 0,
                                                                 ussanceTenor = u.USSANCETENOR,
                                                                 lcEffectiveDate = u.LCUSSANCEEFFECTIVEDATE,
                                                                 oldLcMaturityDate = DateTime.Now,
                                                                 lcMaturityDate = u.LCUSSANCEMATURITYDATE,
                                                                 currencyId = (int)u.USANCEAMOUNTCURRENCYID,
                                                                 customerName = "N/A",
                                                                 createdBy = 1,
                                                                 operationId = 1,
                                                                 dateTimeCreated = DateTime.Now,
                                                                 currentlyWith = "N/A",
                                                                 arrivalDate = DateTime.Now,
                                                                 requestApprovalLevel = "N/A",
                                                                 requestStaffName = "N/A",
                                                                 approvalStatus = "N/A",
                                                                 currentApprovalLevel = "N/A",
                                                             }).ToList(),
                                                //operationId = x.OPERATIONID,
                                }).GroupBy(a => a.lcIssuanceId).Select(g => g.OrderByDescending(l => l.lcApprovalTrailId)
                                    .FirstOrDefault())
                                    .ToList();
            List<LcIssuanceApprovalViewModel> apps = new List<LcIssuanceApprovalViewModel>();
            apps.AddRange(applications);
            return apps;
        }

        public short? VerifyApplicationStatus(int lcIssuanceId,int operationId)
        {
            var lc = context.TBL_LC_ISSUANCE.FirstOrDefault(l => l.DELETED == false && l.LCISSUANCEID == lcIssuanceId);
            if (lc == null)
            {
                throw new SecureException("LC not existing!");
            }
            if (operationId == (int)OperationsEnum.lcIssuance || operationId == (int)OperationsEnum.lcReleaseOfShippingDocuments)
            {
                return lc.APPLICATIONSTATUSID;
            }
            if (operationId == (int)OperationsEnum.lcUssance)
            {
                return lc.LCUSSANCESTATUSID;
            }
            return null;
        }

        public LcIssuanceApprovalViewModel VerifyIssuanceOrReleaseApprovalLevelId(LcIssuanceApprovalViewModel lc)
        {
            if (lc.currentApprovalLevel == "n/a") return lc;
            int[] issuanceStatuses = { (int)LoanApplicationStatusEnum.LcIssuanceInProgress, (int)LoanApplicationStatusEnum.LcIssuanceCompleted};
            int[] releaseStatuses = { (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress, (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted };
            if (issuanceStatuses.Contains((int)lc.applicationStatusId))
            {
                var trail = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == lc.lcIssuanceId && t.OPERATIONID == (int)OperationsEnum.lcIssuance).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault();
                if (trail == null)
                {
                    lc.currentApprovalLevel = "n/a";
                    return lc;
                }
                lc.currentApprovalLevel = context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == trail.TOAPPROVALLEVELID).LEVELNAME;
                return lc;
            }
            //var lc = context.TBL_APPROVAL_TRAIL.FirstOrDefault(l => l.OPERATIONID == operationId && l.TARGETID == lcReleaseAmountId);
            if (releaseStatuses.Contains((int)lc.applicationStatusId))
            {
                var trail = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == lc.lcReleaseAmountId && t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault();
                lc.currentApprovalLevel = context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == trail.TOAPPROVALLEVELID).LEVELNAME;
                return lc;
            }
            return null;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuances(int staffId)
        {
            var staffs = general.GetStaffRlieved(staffId).ToList();
            var lcsInProgress = (from x in context.TBL_LC_ISSUANCE 
                                join t in context.TBL_APPROVAL_TRAIL on x.LCISSUANCEID equals t.TARGETID
                                //join t in context.TBL_APPROVAL_TRAIL on x.LCISSUANCEID equals t.TARGETID into xy
                                //from itrail in xy.DefaultIfEmpty() where 
                                where
                                (
                                x.DELETED == false
                                && t.OPERATIONID == (int)OperationsEnum.lcIssuance
                                && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceInProgress
                                )
                                select new LcIssuanceApprovalViewModel
                                {
                                    lcIssuanceId = x.LCISSUANCEID,
                                    lcApprovalTrailId = t.APPROVALTRAILID,
                                    approvalStatusId = t.APPROVALSTATUSID,
                                    loopedStaffId = t.LOOPEDSTAFFID,
                                    beneficiaryName = x.BENEFICIARYNAME,
                                    totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                    totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                    availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                    cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                    cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                    cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                    percentageToCover = x.PERCENTAGETOCOVER,
                                    lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                    lcToleranceValue = x.LCTOLERANCEVALUE,
                                    releaseAmount = x.RELEASEDAMOUNT,
                                    letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                    isDraftRequired = x.ISDRAFTREQUIRED,
                                    beneficiaryAddress = x.BENEFICIARYADDRESS,
                                    beneficiaryEmail = x.BENEFICIARYEMAIL,
                                    customerId = x.CUSTOMERID,
                                    fundSourceId = x.FUNDSOURCEID,
                                    fundSourceDetails = x.FUNDSOURCEDETAILS,
                                    formMNumber = x.FORMMNUMBER,
                                    beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                    beneficiaryBank = x.BENEFICIARYBANK,
                                    currencyId = x.CURRENCYID,
                                    customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                    proformaInvoiceId = x.PROFORMAINVOICEID,
                                    availableAmount = x.AVAILABLEAMOUNT,
                                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                    letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                    invoiceDate = x.INVOICEDATE,
                                    invoiceDueDate = x.INVOICEDUEDATE,
                                    transactionCycle = x.TRANSACTIONCYCLE,
                                    lcReferenceNumber = x.LCREFERENCENUMBER,
                                    dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                }).GroupBy(l => l.lcIssuanceId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                                .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var lcsNotStarted = (from x in context.TBL_LC_ISSUANCE
                                where
                                (
                                x.DELETED == false
                                && x.APPLICATIONSTATUSID == null
                                && staffs.Contains(x.CREATEDBY ?? 0)
                                )
                                select new LcIssuanceApprovalViewModel
                                {
                                    lcIssuanceId = x.LCISSUANCEID,
                                    beneficiaryName = x.BENEFICIARYNAME,
                                    totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                    totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                    availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                    cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                    cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                    cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                    percentageToCover = x.PERCENTAGETOCOVER,
                                    lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                    lcToleranceValue = x.LCTOLERANCEVALUE,
                                    releaseAmount = x.RELEASEDAMOUNT,
                                    letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                    isDraftRequired = x.ISDRAFTREQUIRED,
                                    beneficiaryAddress = x.BENEFICIARYADDRESS,
                                    beneficiaryEmail = x.BENEFICIARYEMAIL,
                                    customerId = x.CUSTOMERID,
                                    fundSourceId = x.FUNDSOURCEID,
                                    fundSourceDetails = x.FUNDSOURCEDETAILS,
                                    formMNumber = x.FORMMNUMBER,
                                    beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                    beneficiaryBank = x.BENEFICIARYBANK,
                                    currencyId = x.CURRENCYID,
                                    customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                    proformaInvoiceId = x.PROFORMAINVOICEID,
                                    availableAmount = x.AVAILABLEAMOUNT,
                                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                    letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                    invoiceDate = x.INVOICEDATE,
                                    invoiceDueDate = x.INVOICEDUEDATE,
                                    transactionCycle = x.TRANSACTIONCYCLE,
                                    lcReferenceNumber = x.LCREFERENCENUMBER,
                                    dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                }).ToList();
            var lcs = lcsNotStarted.Union(lcsInProgress);
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForEnhancement(int staffId)
        {
            var staffs = general.GetStaffRlieved(staffId).ToList();

            var lcEnhancementsInProgress = (from x in context.TBL_TEMP_LC_ISSUANCE
                                 join l in context.TBL_LC_ISSUANCE on x.LCISSUANCEID equals l.LCISSUANCEID
                                 join t in context.TBL_APPROVAL_TRAIL on x.TEMPLCISSUANCEID equals t.TARGETID
                                 where
                                 (
                                 x.DELETED == false
                                 && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                 && t.OPERATIONID == (int)OperationsEnum.LCEnhancementApproval
                                 && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcEnhancementInProgress
                                 )
                                 select new LcIssuanceApprovalViewModel
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     tempLcIssuanceId = x.TEMPLCISSUANCEID,
                                     lcApprovalTrailId = t.APPROVALTRAILID,
                                     approvalStatusId = t.APPROVALSTATUSID,
                                     loopedStaffId = t.LOOPEDSTAFFID,
                                     beneficiaryName = x.BENEFICIARYNAME,
                                     totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = x.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = x.LCTOLERANCEVALUE,
                                     releaseAmount = x.RELEASEDAMOUNT,
                                     letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                     isDraftRequired = x.ISDRAFTREQUIRED,
                                     beneficiaryAddress = x.BENEFICIARYADDRESS,
                                     beneficiaryEmail = x.BENEFICIARYEMAIL,
                                     customerId = x.CUSTOMERID,
                                     fundSourceId = x.FUNDSOURCEID,
                                     fundSourceDetails = x.FUNDSOURCEDETAILS,
                                     formMNumber = x.FORMMNUMBER,
                                     beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = x.BENEFICIARYBANK,
                                     currencyId = x.CURRENCYID,
                                     customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                     proformaInvoiceId = x.PROFORMAINVOICEID,
                                     availableAmount = x.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = x.INVOICEDATE,
                                     invoiceDueDate = x.INVOICEDUEDATE,
                                     transactionCycle = x.TRANSACTIONCYCLE,
                                     lcReferenceNumber = x.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                 }).GroupBy(l => l.tempLcIssuanceId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                                .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var lcEnhancementsNotStarted = (from x in context.TBL_TEMP_LC_ISSUANCE
                                 join l in context.TBL_LC_ISSUANCE on x.LCISSUANCEID equals l.LCISSUANCEID
                                 where
                                 (
                                 x.DELETED == false
                                 && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                 && x.APPLICATIONSTATUSID == null
                                 && staffs.Contains(x.CREATEDBY ?? 0)
                                 )
                                 select new LcIssuanceApprovalViewModel
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     tempLcIssuanceId = x.TEMPLCISSUANCEID,
                                     beneficiaryName = x.BENEFICIARYNAME,
                                     totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = x.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = x.LCTOLERANCEVALUE,
                                     releaseAmount = x.RELEASEDAMOUNT,
                                     letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                     isDraftRequired = x.ISDRAFTREQUIRED,
                                     beneficiaryAddress = x.BENEFICIARYADDRESS,
                                     beneficiaryEmail = x.BENEFICIARYEMAIL,
                                     customerId = x.CUSTOMERID,
                                     fundSourceId = x.FUNDSOURCEID,
                                     fundSourceDetails = x.FUNDSOURCEDETAILS,
                                     formMNumber = x.FORMMNUMBER,
                                     beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = x.BENEFICIARYBANK,
                                     currencyId = x.CURRENCYID,
                                     customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                     proformaInvoiceId = x.PROFORMAINVOICEID,
                                     availableAmount = x.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = x.INVOICEDATE,
                                     invoiceDueDate = x.INVOICEDUEDATE,
                                     transactionCycle = x.TRANSACTIONCYCLE,
                                     lcReferenceNumber = x.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                 }).ToList();
            var lcs = lcEnhancementsNotStarted.Union(lcEnhancementsInProgress);
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForExtension(int staffId)
        {
            var staffs = general.GetStaffRlieved(staffId).ToList();
            var lcExtensionsInProgress = (from x in context.TBL_TEMP_LC_ISSUANCE
                                            join l in context.TBL_LC_ISSUANCE on x.LCISSUANCEID equals l.LCISSUANCEID
                                            join t in context.TBL_APPROVAL_TRAIL on x.TEMPLCISSUANCEID equals t.TARGETID
                                            where
                                            (
                                            x.DELETED == false
                                            && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                            && t.OPERATIONID == (int)OperationsEnum.LCIssuanceExtensionApproval
                                            && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceExtensionInProgress
                                            )
                                            select new LcIssuanceApprovalViewModel
                                            {
                                                lcIssuanceId = x.LCISSUANCEID,
                                                tempLcIssuanceId = x.TEMPLCISSUANCEID,
                                                lcApprovalTrailId = t.APPROVALTRAILID,
                                                approvalStatusId = t.APPROVALSTATUSID,
                                                loopedStaffId = t.LOOPEDSTAFFID,
                                                beneficiaryName = x.BENEFICIARYNAME,
                                                totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                                totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                                availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                                cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                                cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                                cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                                percentageToCover = x.PERCENTAGETOCOVER,
                                                lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                                lcToleranceValue = x.LCTOLERANCEVALUE,
                                                releaseAmount = x.RELEASEDAMOUNT,
                                                letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                                isDraftRequired = x.ISDRAFTREQUIRED,
                                                beneficiaryAddress = x.BENEFICIARYADDRESS,
                                                beneficiaryEmail = x.BENEFICIARYEMAIL,
                                                customerId = x.CUSTOMERID,
                                                fundSourceId = x.FUNDSOURCEID,
                                                fundSourceDetails = x.FUNDSOURCEDETAILS,
                                                formMNumber = x.FORMMNUMBER,
                                                beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                                beneficiaryBank = x.BENEFICIARYBANK,
                                                currencyId = x.CURRENCYID,
                                                customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                                proformaInvoiceId = x.PROFORMAINVOICEID,
                                                availableAmount = x.AVAILABLEAMOUNT,
                                                letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                                letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                                invoiceDate = x.INVOICEDATE,
                                                invoiceDueDate = x.INVOICEDUEDATE,
                                                transactionCycle = x.TRANSACTIONCYCLE,
                                                lcReferenceNumber = x.LCREFERENCENUMBER,
                                                dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                            }).GroupBy(l => l.tempLcIssuanceId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                                .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var lcExtensionsNotStarted = (from x in context.TBL_TEMP_LC_ISSUANCE
                                            join l in context.TBL_LC_ISSUANCE on x.LCISSUANCEID equals l.LCISSUANCEID
                                            where
                                            (
                                            x.DELETED == false
                                            && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                            && x.APPLICATIONSTATUSID == null
                                            && staffs.Contains(x.CREATEDBY ?? 0)
                                            )
                                            select new LcIssuanceApprovalViewModel
                                            {
                                                lcIssuanceId = x.LCISSUANCEID,
                                                tempLcIssuanceId = x.TEMPLCISSUANCEID,
                                                beneficiaryName = x.BENEFICIARYNAME,
                                                totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                                totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                                availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                                cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                                cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                                cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                                percentageToCover = x.PERCENTAGETOCOVER,
                                                lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                                lcToleranceValue = x.LCTOLERANCEVALUE,
                                                releaseAmount = x.RELEASEDAMOUNT,
                                                letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                                isDraftRequired = x.ISDRAFTREQUIRED,
                                                beneficiaryAddress = x.BENEFICIARYADDRESS,
                                                beneficiaryEmail = x.BENEFICIARYEMAIL,
                                                customerId = x.CUSTOMERID,
                                                fundSourceId = x.FUNDSOURCEID,
                                                fundSourceDetails = x.FUNDSOURCEDETAILS,
                                                formMNumber = x.FORMMNUMBER,
                                                beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                                beneficiaryBank = x.BENEFICIARYBANK,
                                                currencyId = x.CURRENCYID,
                                                customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                                proformaInvoiceId = x.PROFORMAINVOICEID,
                                                availableAmount = x.AVAILABLEAMOUNT,
                                                letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                                letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                                invoiceDate = x.INVOICEDATE,
                                                invoiceDueDate = x.INVOICEDUEDATE,
                                                transactionCycle = x.TRANSACTIONCYCLE,
                                                lcReferenceNumber = x.LCREFERENCENUMBER,
                                                dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                            }).ToList();
            var lcs = lcExtensionsNotStarted.Union(lcExtensionsInProgress);
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcIssuance;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            //var querytest1 = (from a in context.TBL_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceInProgress
            //                    && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();
            // query
            var query = (from a in context.TBL_LC_ISSUANCE where
                        (a.DELETED == false 
                        && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceInProgress
                        && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                         orderby a.LCISSUANCEID
                        join b in context.TBL_APPROVAL_TRAIL on a.LCISSUANCEID equals b.TARGETID where
                        (
                        (b.OPERATIONID == operationId)
                        && b.APPROVALSTATEID != (int)ApprovalState.Ended
                        && b.RESPONSESTAFFID == null
                        && b.LOOPEDSTAFFID == null
                        && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                        && (b.TOSTAFFID == null || staffs.Contains(b.TOSTAFFID ?? 0))
                        )
                            select new LcIssuanceApprovalViewModel()
                            {
                                lcIssuanceId = a.LCISSUANCEID,
                                isDraftRequired = a.ISDRAFTREQUIRED,
                                lcReferenceNumber = a.LCREFERENCENUMBER,
                                letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                                beneficiaryName = a.BENEFICIARYNAME,
                                totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                                totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                                availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                                cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                                cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCENUMBER,
                                cashBuildUpReferenceType = a.CASHBUILDUPREFERENCETYPE,
                                percentageToCover = a.PERCENTAGETOCOVER,
                                lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                                lcToleranceValue = a.LCTOLERANCEVALUE,
                                releaseAmount = a.RELEASEDAMOUNT,
                                beneficiaryAddress = a.BENEFICIARYADDRESS,
                                beneficiaryEmail = a.BENEFICIARYEMAIL,
                                customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                customerId = a.CUSTOMERID,
                                fundSourceId = a.FUNDSOURCEID,
                                fundSourceDetails = a.FUNDSOURCEDETAILS,
                                formMNumber = a.FORMMNUMBER,
                                beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                                beneficiaryBank = a.BENEFICIARYBANK,
                                currencyId = a.CURRENCYID,
                                proformaInvoiceId = a.PROFORMAINVOICEID,
                                availableAmount = a.AVAILABLEAMOUNT,
                                letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                                letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                                invoiceDate = a.INVOICEDATE,
                                invoiceDueDate = a.INVOICEDUEDATE,
                                transactionCycle = a.TRANSACTIONCYCLE,
                                lastComment = b.COMMENT,
                                currentApprovalStateId = b.APPROVALSTATEID,
                                currentApprovalLevelId = b.TOAPPROVALLEVELID,
                                currentApprovalLevel = b.TBL_APPROVAL_LEVEL.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                                lcApprovalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                                toStaffId = b.TOSTAFFID,
                                approvalStatusId = b.APPROVALSTATUSID,
                                applicationStatusId = a.APPLICATIONSTATUSID,
                                createdBy = (int)a.CREATEDBY,
                                operationId = operationId,
                                dateTimeCreated = (DateTime)a.DATETIMECREATED
                            }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            return applications.ToList();
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForEnhancementApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.LCEnhancementApproval;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            //var querytest1 = (from a in context.TBL_TEMP_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcEnhancementInProgress
            //                    && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();
            // query
            var query = (from a in context.TBL_TEMP_LC_ISSUANCE
                            join l in context.TBL_LC_ISSUANCE on a.LCISSUANCEID equals l.LCISSUANCEID
                             where
                            (a.DELETED == false
                            && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcEnhancementInProgress
                            && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                            )
                            orderby a.TEMPLCISSUANCEID
                            join b in context.TBL_APPROVAL_TRAIL on a.TEMPLCISSUANCEID equals b.TARGETID
                            where
                            (
                            (b.OPERATIONID == operationId)
                            && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            && b.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RESPONSESTAFFID == null
                            && b.LOOPEDSTAFFID == null
                            && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                            && (b.TOSTAFFID == null || staffs.Contains(b.TOSTAFFID ?? 0))
                            )
                             select new LcIssuanceApprovalViewModel()
                             {
                                lcIssuanceId = a.LCISSUANCEID,
                                tempLcIssuanceId = a.TEMPLCISSUANCEID,
                                isDraftRequired = a.ISDRAFTREQUIRED,
                                lcReferenceNumber = a.LCREFERENCENUMBER,
                                letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                                beneficiaryName = a.BENEFICIARYNAME,
                                totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                                totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                                availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                                cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                                cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCENUMBER,
                                cashBuildUpReferenceType = a.CASHBUILDUPREFERENCETYPE,
                                percentageToCover = a.PERCENTAGETOCOVER,
                                lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                                lcToleranceValue = a.LCTOLERANCEVALUE,
                                releaseAmount = a.RELEASEDAMOUNT,
                                beneficiaryAddress = a.BENEFICIARYADDRESS,
                                beneficiaryEmail = a.BENEFICIARYEMAIL,
                                customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                customerId = a.CUSTOMERID,
                                fundSourceId = a.FUNDSOURCEID,
                                fundSourceDetails = a.FUNDSOURCEDETAILS,
                                formMNumber = a.FORMMNUMBER,
                                beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                                beneficiaryBank = a.BENEFICIARYBANK,
                                currencyId = a.CURRENCYID,
                                proformaInvoiceId = a.PROFORMAINVOICEID,
                                availableAmount = a.AVAILABLEAMOUNT,
                                letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                                letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                                invoiceDate = a.INVOICEDATE,
                                invoiceDueDate = a.INVOICEDUEDATE,
                                transactionCycle = a.TRANSACTIONCYCLE,
                                lastComment = b.COMMENT,
                                currentApprovalStateId = b.APPROVALSTATEID,
                                currentApprovalLevelId = b.TOAPPROVALLEVELID,
                                currentApprovalLevel = b.TBL_APPROVAL_LEVEL.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                                lcApprovalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                                toStaffId = b.TOSTAFFID,
                                approvalStatusId = b.APPROVALSTATUSID,
                                applicationStatusId = a.APPLICATIONSTATUSID,
                                createdBy = (int)a.CREATEDBY,
                                operationId = operationId,
                                dateTimeCreated = (DateTime)a.DATETIMECREATED
                             }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.tempLcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            return applications.ToList();
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForExtensionApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.LCIssuanceExtensionApproval;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            //var querytest1 = (from a in context.TBL_TEMP_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcExtensionInProgress
            //                    && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();
            // query
            var query = (from a in context.TBL_TEMP_LC_ISSUANCE
                         join l in context.TBL_LC_ISSUANCE on a.LCISSUANCEID equals l.LCISSUANCEID
                         where
                        (a.DELETED == false
                        && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceExtensionInProgress
                        && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                        )
                         orderby a.TEMPLCISSUANCEID
                         join b in context.TBL_APPROVAL_TRAIL on a.TEMPLCISSUANCEID equals b.TARGETID
                         where
                         (
                         (b.OPERATIONID == operationId)
                         && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                         && b.APPROVALSTATEID != (int)ApprovalState.Ended
                         && b.RESPONSESTAFFID == null
                         && b.LOOPEDSTAFFID == null
                         && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                         && (b.TOSTAFFID == null || staffs.Contains(b.TOSTAFFID ?? 0))
                         )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             tempLcIssuanceId = a.TEMPLCISSUANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCENUMBER,
                             cashBuildUpReferenceType = a.CASHBUILDUPREFERENCETYPE,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             releaseAmount = a.RELEASEDAMOUNT,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                             customerId = a.CUSTOMERID,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             transactionCycle = a.TRANSACTIONCYCLE,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             approvalStatusId = b.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED
                         }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.tempLcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            return applications.ToList();
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForCancelationApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.LCTerminationApproval;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            var query = (from a in context.TBL_LC_ISSUANCE
                             where
                                (a.DELETED == false
                            && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress)
                            orderby a.LCISSUANCEID
                            join b in context.TBL_APPROVAL_TRAIL on a.LCISSUANCEID equals b.TARGETID
                            where
                            (
                            (b.OPERATIONID == operationId)
                            && b.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RESPONSESTAFFID == null
                            && b.LOOPEDSTAFFID == null
                            && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                            && (b.TOSTAFFID == null || staffs.Contains(b.TOSTAFFID ?? 0))
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCENUMBER,
                             cashBuildUpReferenceType = a.CASHBUILDUPREFERENCETYPE,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             releaseAmount = a.RELEASEDAMOUNT,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                             customerId = a.CUSTOMERID,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             transactionCycle = a.TRANSACTIONCYCLE,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             approvalStatusId = b.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED
                         }).ToList();

            var cancelationsForApproval = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            return cancelationsForApproval.ToList();
        }

        public IEnumerable<CamProcessedLoanViewModel> GetIFFLinesForLCByCustomerId(int customerId, int companyId, int staffId, int branchId)
        {
            //var lines = drawdownRepo.GetAvailedLoanApplicationsDueForInitiateBooking(companyId, staffId, branchId, true).ToList();
            var lines = GetAvailedLoanApplicationsDueForInitiateBooking(staffId, companyId, customerId, branchId).ToList();
            //var test1 = lines.Where(l => l.customerId == customerId).ToList();
            //var test2 = lines.Where(l => l.productClassId == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            //var test3 = lines.Where(l => l.customerAvailableAmount > 0).ToList();
            var linesForLC = lines.Where
                (l => l.customerId == customerId && l.productClassId == (int)ProductClassEnum.ImportFinanceFacilities && l.customerAvailableAmount > 0).ToList();

            return linesForLC;
        }

        private IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int staffId, int companyId, int customerId, int branchId)
        {
            var systemDate = general.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);
            //var staffIds = generalSetup.GetStaffRlieved(staffId);
            var data2 = new List<CamProcessedLoanViewModel>();

            //data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
            //          join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
            //          join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
            //          where a.COMPANYID == companyId && d.DELETED == false
            //          //&& staffIds.Contains(a.OWNEDBY)
            //          && customerId == d.CUSTOMERID
            //          && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
            //          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
            //          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
            //          && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
            //          && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
            //          orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
            //          select new CamProcessedLoanViewModel
            //          {
            //              loanBookingRequestId = 0,
            //              approvalTrailId = 0,
            //              isLineFacility = d.ISLINEFACILITY,
            //              isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
            //              isLineMaintained = a.APPROVEDLINESTATUSID != null,
            //              customerTypeId = (int)context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == d.CUSTOMERID).Select(s => s.CUSTOMERTYPEID).FirstOrDefault(),
            //              appraisalOperationId = a.OPERATIONID,
            //              requestedAmount = 0,
            //              loanApplicationId = a.LOANAPPLICATIONID,
            //              loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
            //              applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
            //              applicationStatusId = a.APPLICATIONSTATUSID,
            //              customerId = d.CUSTOMERID,
            //              customerCode = d.TBL_CUSTOMER.CUSTOMERCODE,
            //              customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
            //              customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
            //              customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
            //              customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
            //              isRelatedParty = a.ISRELATEDPARTY,
            //              customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
            //              customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
            //              customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
            //              operationId = a.OPERATIONID,
            //              isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
            //              isInvestmentGrade = a.ISINVESTMENTGRADE,
            //              productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
            //              companyId = a.COMPANYID,
            //              branchId = a.BRANCHID,
            //              branchName = a.TBL_BRANCH.BRANCHNAME,
            //              subSectorId = d.SUBSECTORID,
            //              subSectorName = d.TBL_SUB_SECTOR.NAME,
            //              sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
            //              applicationTenor = a.APPLICATIONTENOR,
            //              effectiveDate = (DateTime)d.EFFECTIVEDATE,
            //              expiryDate = (DateTime)d.EXPIRYDATE,
            //              relationshipOfficerId = a.RELATIONSHIPOFFICERID,
            //              relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
            //              relationshipManagerId = a.RELATIONSHIPMANAGERID,
            //              relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

            //              currencyId = d.CURRENCYID,
            //              currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
            //              isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
            //              exchangeRate = d.EXCHANGERATE,
            //              loanTypeId = a.LOANAPPLICATIONTYPEID,
            //              loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            //              camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
            //              productId = d.APPROVEDPRODUCTID,
            //              productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
            //              productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
            //              productName = d.TBL_PRODUCT.PRODUCTNAME,
            //              productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
            //              productClassId = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,
            //              misCode = a.MISCODE,
            //              teamMisCode = a.TEAMMISCODE,
            //              casaAccountId = d.CASAACCOUNTID,

            //              interestRate = d.APPROVEDINTERESTRATE,
            //              submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
            //              approvedAmount = d.APPROVEDAMOUNT,
            //              approvedDate = a.APPROVEDDATE,
            //              groupApprovedAmount = a.APPROVEDAMOUNT,
            //              approvedTenor = d.APPROVEDTENOR,
            //              createdBy = a.OWNEDBY,
            //              newApplicationDate = a.APPLICATIONDATE,
            //              dateTimeCreated = d.DATETIMECREATED,
            //              availmentDate = a.AVAILMENTDATE,
            //              systemCurrentDate = systemDate,
            //              isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
            //              loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

            //              approvalStatusId = (short)a.APPROVALSTATUSID,
            //              apiRequestId = a.APIREQUESTID,
            //              approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
            //          }).ToList();
            data2 = drawdownRepo.GetAvailedLoanApplicationsDueForInitiateBooking(companyId,staffId,branchId, customerId,true).ToList();
            var data = data2;


            //var referredItem = GetBookingRequestAwaitingApproval(staffId, companyId, true).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Referred).ToList();
            // data.AddRange(referredItem);

            //foreach (var item in data)
            //{
            //    var approvedLCIssuanceIds = context.TBL_LC_ISSUANCE.Where(t => t.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted).Select(t => t.LCISSUANCEID).ToList();
            //    var lcIFFRequests = context.TBL_LC_ISSUANCE.Where(l => l.DELETED == false && l.FUNDSOURCEID == (int)LCFundSource.IFF);
            //    var lcapprovedLCIFFs = lcIFFRequests.Where(i => approvedLCIssuanceIds.Contains(i.LCISSUANCEID)).Select(i => new { i.FUNDSOURCEDETAILS, i.LETTEROFCREDITAMOUNT });
            //    var lcapprovedLCIFFsRecords = lcapprovedLCIFFs.Where(i => i.FUNDSOURCEDETAILS == item.loanApplicationId);
            //    var lcApprovedAmounts = lcapprovedLCIFFsRecords.Count() > 0 ? lcapprovedLCIFFsRecords?.Sum(i => i.LETTEROFCREDITAMOUNT) : 0;
            //    var product = context.TBL_PRODUCT.Find(item.productId);

            //    var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && r.DELETED == false);
            //    var test = requests.ToList();
            //    var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
            //    var disbursedOverdraft = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
            //    var disbursedContingent = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
            //    //requests = requests.Where(x => x.APPROVEDLINESTATUSID != null && !disbursedLoan.Select(c => c.LOAN_BOOKING_REQUESTID).Contains(x.LOAN_BOOKING_REQUESTID));

            //    //item.operationId = GetDrawdownOperationId(item.loanApplicationDetailId);
            //    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
            //    { item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED); }

            //    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
            //    //{ item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
            //    { item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED); }

            //    if (requests.Where(n => (n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending) && n.APPROVEDLINESTATUSID == null).Count() > 0)
            //    //{ item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
            //    {
            //        var validRequests = requests.Where(n => (n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending) && n.APPROVEDLINESTATUSID == null).ToList();
            //        item.allRequestAmount = validRequests.Sum(s => s.AMOUNT_REQUESTED);
            //        if (product.ISFACILITYLINE == true || item.isLineFacility == true)
            //        {
            //            if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
            //            {
            //                if (disbursedOverdraft.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedOverdraft.Sum(x => x.OVERDRAFTLIMIT);
            //            }
            //            if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
            //            {
            //                if (disbursedContingent.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedContingent.Sum(x => x.CONTINGENTAMOUNT);
            //            }
            //            else
            //            {
            //                if (disbursedLoan.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedLoan.Sum(x => x.PRINCIPALAMOUNT);
            //            }
            //        }
            //    }

            //    item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

            //    if (item.disapprovedCount > 0)
            //    { item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED); }


            //    if (disbursedLoan.Any())
            //    {
            //        item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
            //    }


            //    //item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);
            //    item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount);
            //    if (lcApprovedAmounts > 0)
            //    {
            //        item.customerAvailableAmount -= lcApprovedAmounts;
            //    }
            //}

            //data = (from a in data where ((a.customerAvailableAmount > 0) || (a.customerAvailableAmount == null)) select a).ToList();

            return data;

        }

        public IEnumerable<LcIssuanceViewModel> GetLcIssuance(int id)
        {
            var lcs = context.TBL_LC_ISSUANCE.Where(x => x.LCISSUANCEID == id && x.DELETED == false)
                 .Select( x => new LcIssuanceViewModel
                {
                    lcIssuanceId = x.LCISSUANCEID,
                    beneficiaryName = x.BENEFICIARYNAME,
                    totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                    totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                    availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                    cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                    cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                    cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                    percentageToCover = x.PERCENTAGETOCOVER,
                    lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                    lcToleranceValue = x.LCTOLERANCEVALUE,
                    releaseAmount = x.RELEASEDAMOUNT,
                    letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                    customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                    isDraftRequired = x.ISDRAFTREQUIRED,
                    beneficiaryAddress = x.BENEFICIARYADDRESS,
                    beneficiaryEmail = x.BENEFICIARYEMAIL,
                    customerId = x.CUSTOMERID,
                    fundSourceId = x.FUNDSOURCEID,
                    fundSourceDetails = x.FUNDSOURCEDETAILS,
                    formMNumber = x.FORMMNUMBER,
                    beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                    beneficiaryBank = x.BENEFICIARYBANK,
                    currencyId = x.CURRENCYID,
                    proformaInvoiceId = x.PROFORMAINVOICEID,
                    availableAmount = x.AVAILABLEAMOUNT,
                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                    letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                    invoiceDate = x.INVOICEDATE,
                    invoiceDueDate = x.INVOICEDUEDATE,
                    transactionCycle = x.TRANSACTIONCYCLE,
                    lcReferenceNumber = x.LCREFERENCENUMBER,
                }).ToList();
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcEnhancementByLcEnhancementId(int tempLcIssuanceId)
        {
            var lcEnhancement = (from x in context.TBL_TEMP_LC_ISSUANCE
                                join l in context.TBL_LC_ISSUANCE on x.LCISSUANCEID equals l.LCISSUANCEID
                                where
                                (
                                x.DELETED == false
                                && x.TEMPLCISSUANCEID == tempLcIssuanceId
                                )
                                select new LcIssuanceApprovalViewModel
                                {
                                    lcIssuanceId = x.LCISSUANCEID,
                                    tempLcIssuanceId = x.TEMPLCISSUANCEID,
                                    beneficiaryName = x.BENEFICIARYNAME,
                                    totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                    totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                    availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                    cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                    cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                    cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                    percentageToCover = x.PERCENTAGETOCOVER,
                                    lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                    lcToleranceValue = x.LCTOLERANCEVALUE,
                                    releaseAmount = x.RELEASEDAMOUNT,
                                    letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                    isDraftRequired = x.ISDRAFTREQUIRED,
                                    beneficiaryAddress = x.BENEFICIARYADDRESS,
                                    beneficiaryEmail = x.BENEFICIARYEMAIL,
                                    customerId = x.CUSTOMERID,
                                    fundSourceId = x.FUNDSOURCEID,
                                    fundSourceDetails = x.FUNDSOURCEDETAILS,
                                    formMNumber = x.FORMMNUMBER,
                                    beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                    beneficiaryBank = x.BENEFICIARYBANK,
                                    currencyId = x.CURRENCYID,
                                    customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                    proformaInvoiceId = x.PROFORMAINVOICEID,
                                    availableAmount = x.AVAILABLEAMOUNT,
                                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                    letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                    invoiceDate = x.INVOICEDATE,
                                    invoiceDueDate = x.INVOICEDUEDATE,
                                    transactionCycle = x.TRANSACTIONCYCLE,
                                    lcReferenceNumber = x.LCREFERENCENUMBER,
                                    dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                }).ToList();
            return lcEnhancement;
        }

        public void ValidateAmounts(LcIssuanceViewModel model)
        {
            var rates = context.TBL_CURRENCY_EXCHANGERATE.ToList();
            Decimal lcAmount;
            Decimal availableAmount;
            var lcAmountCurrencyRecord = context.TBL_CURRENCY_EXCHANGERATE.Where(r => r.CURRENCYID == model.currencyId).FirstOrDefault();
            var availAmtCurrencyRecord = context.TBL_CURRENCY_EXCHANGERATE.Where(r => r.CURRENCYID == model.availableAmountCurrencyId).FirstOrDefault();
            lcAmount = lcAmountCurrencyRecord == null ? 0 :(decimal)lcAmountCurrencyRecord.EXCHANGERATE * model.letterOfCreditAmount;

            availableAmount = availAmtCurrencyRecord == null ? 0 : (decimal)availAmtCurrencyRecord.EXCHANGERATE * model.availableAmount;
            if (lcAmount > availableAmount)
            {
                throw new SecureException("LC amount cannot be greater than available amount!");
            }
            
        }

        public LcIssuanceViewModel AddLcIssuance(LcIssuanceViewModel model)
        {
            ValidateAmounts(model);


            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            if (!(model.lcReferenceNumber.Trim().Length > 1))
            {
                model.lcReferenceNumber = referenceNumber;
            }
            var entity = new TBL_LC_ISSUANCE
            {
                LCREFERENCENUMBER = model.lcReferenceNumber,
                BENEFICIARYNAME = model.beneficiaryName,
                TOTALAPPROVEDAMOUNT = model.totalApprovedAmount,
                TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId,
                AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId,
                CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable,
                CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType,
                CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber,
                PERCENTAGETOCOVER = model.percentageToCover,
                LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage,
                LCTOLERANCEVALUE = model.lcToleranceValue,
                RELEASEDAMOUNT = model.releaseAmount,
                LETTEROFCREDITTYPEID = model.letterOfCreditTypeId,
                ISDRAFTREQUIRED = model.isDraftRequired,
                BENEFICIARYADDRESS = model.beneficiaryAddress,
                BENEFICIARYEMAIL = model.beneficiaryEmail,
                CUSTOMERID = model.customerId,
                FUNDSOURCEID = model.fundSourceId,
                FUNDSOURCEDETAILS = model.fundSourceDetails,
                FORMMNUMBER = model.formMNumber,
                BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber,
                BENEFICIARYBANK = model.beneficiaryBank,
                CURRENCYID = model.currencyId,
                PROFORMAINVOICEID = model.proformaInvoiceId,
                AVAILABLEAMOUNT = model.availableAmount,
                LETTEROFCREDITAMOUNT = model.letterOfCreditAmount,
                LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate,
                INVOICEDATE = model.invoiceDate,
                INVOICEDUEDATE = model.invoiceDueDate,
                TRANSACTIONCYCLE = model.transactionCycle,
                //COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                TOTALUSANCEAMOUNTLOCAL = 0,
                DATETIMECREATED = DateTime.Now
            };

            context.TBL_LC_ISSUANCE.Add(entity);
            var systemDate = general.GetApplicationDate();
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            var aud = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcIssuanceAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Issuance '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------

            context.SaveChanges();
            var createdlcRecord = context.TBL_LC_ISSUANCE.FirstOrDefault(lc => lc.LCREFERENCENUMBER == model.lcReferenceNumber);
            if(createdlcRecord != null)
            {
                model.lcIssuanceId = createdlcRecord.LCISSUANCEID;
                model.lcReferenceNumber = createdlcRecord.LCREFERENCENUMBER;
            }
           
            return model;
        }

        public void ValidateLcEnhancement(LcIssuanceViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            if(lc == null)
            {
                throw new SecureException("You Cannot Enhance an Lc that does not exist!");
            }

            if(lc.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcIssuanceCompleted)
            {
                var status = context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == lc.APPLICATIONSTATUSID)?.APPLICATIONSTATUSNAME;
                throw new SecureException("You Cannot Enhance an LC with status " + status + "!");
            }

            //if (model.letterOfCreditAmount < lc.LETTEROFCREDITAMOUNT)
            //{
            //    throw new SecureException("New Lc Amount cannot be less than Initial Lc Amount!");
            //}

            //if (model.letterOfcreditExpirydate < lc.LETTEROFCREDITEXPIRYDATE)
            //{
            //    throw new SecureException("New Lc Expiry Date cannot be less than Initial Lc Expiry Date!");
            //}
        }

        public void ValidateLcEnhancementUpdate(LcIssuanceViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            if (lc == null)
            {
                throw new SecureException("You Cannot Enhance an Lc that does not exist!");
            }

            //if (model.letterOfCreditAmount < lc.LETTEROFCREDITAMOUNT)
            //{
            //    throw new SecureException("New Lc Amount cannot be less than Initial Lc Amount!");
            //}

            //if (model.letterOfcreditExpirydate < lc.LETTEROFCREDITEXPIRYDATE)
            //{
            //    throw new SecureException("New Lc Expiry Date cannot be less than Initial Lc Expiry Date!");
            //}
        }

        public LcIssuanceViewModel AddLcEnhancement(LcIssuanceViewModel model)
        {
            ValidateLcEnhancement(model);
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            //var lc = context.TBL_LC_ISSUANCE.FirstOrDefault(l => l.LCREFERENCENUMBER == model.lcReferenceNumber);

            var entity = new TBL_TEMP_LC_ISSUANCE
            {
                ENHANCEMENTREFERENCENUMBER = referenceNumber,
                LCISSUANCEID = model.lcIssuanceId,
                LCREFERENCENUMBER = model.lcReferenceNumber,
                BENEFICIARYNAME = model.beneficiaryName,
                TOTALAPPROVEDAMOUNT = model.totalApprovedAmount,
                TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId,
                AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId,
                CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable,
                CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType,
                CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber,
                PERCENTAGETOCOVER = model.percentageToCover,
                LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage,
                LCTOLERANCEVALUE = model.lcToleranceValue,
                RELEASEDAMOUNT = model.releaseAmount,
                LETTEROFCREDITTYPEID = model.letterOfCreditTypeId,
                ISDRAFTREQUIRED = model.isDraftRequired,
                BENEFICIARYADDRESS = model.beneficiaryAddress,
                BENEFICIARYEMAIL = model.beneficiaryEmail,
                CUSTOMERID = model.customerId,
                FUNDSOURCEID = model.fundSourceId,
                FUNDSOURCEDETAILS = model.fundSourceDetails,
                FORMMNUMBER = model.formMNumber,
                BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber,
                BENEFICIARYBANK = model.beneficiaryBank,
                CURRENCYID = model.currencyId,
                PROFORMAINVOICEID = model.proformaInvoiceId,
                AVAILABLEAMOUNT = model.availableAmount,
                LETTEROFCREDITAMOUNT = model.letterOfCreditAmount,
                LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate,
                INVOICEDATE = model.invoiceDate,
                INVOICEDUEDATE = model.invoiceDueDate,
                OPERATIONID = (int)OperationsEnum.LCEnhancementApproval,
                TRANSACTIONCYCLE = model.transactionCycle,
                //COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            context.TBL_TEMP_LC_ISSUANCE.Add(entity);
            var systemDate = general.GetApplicationDate();
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            var aud = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcEnhancementAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Issuance Enhancement '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------

            context.SaveChanges();
            var createdlcRecord = context.TBL_TEMP_LC_ISSUANCE.FirstOrDefault(l => l.ENHANCEMENTREFERENCENUMBER == referenceNumber);
            if (createdlcRecord != null)
            {
                model.tempLcIssuanceId = createdlcRecord.TEMPLCISSUANCEID;
            }

            return model;
        }

        public LcIssuanceViewModel AddLcExtension(LcIssuanceViewModel model)
        {
            ValidateLcEnhancement(model);
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            //var lc = context.TBL_LC_ISSUANCE.FirstOrDefault(l => l.LCREFERENCENUMBER == model.lcReferenceNumber);

            var entity = new TBL_TEMP_LC_ISSUANCE
            {
                ENHANCEMENTREFERENCENUMBER = referenceNumber,
                LCISSUANCEID = model.lcIssuanceId,
                LCREFERENCENUMBER = model.lcReferenceNumber,
                BENEFICIARYNAME = model.beneficiaryName,
                TOTALAPPROVEDAMOUNT = model.totalApprovedAmount,
                TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId,
                AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId,
                CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable,
                CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType,
                CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber,
                PERCENTAGETOCOVER = model.percentageToCover,
                LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage,
                LCTOLERANCEVALUE = model.lcToleranceValue,
                RELEASEDAMOUNT = model.releaseAmount,
                LETTEROFCREDITTYPEID = model.letterOfCreditTypeId,
                ISDRAFTREQUIRED = model.isDraftRequired,
                BENEFICIARYADDRESS = model.beneficiaryAddress,
                BENEFICIARYEMAIL = model.beneficiaryEmail,
                CUSTOMERID = model.customerId,
                FUNDSOURCEID = model.fundSourceId,
                FUNDSOURCEDETAILS = model.fundSourceDetails,
                FORMMNUMBER = model.formMNumber,
                BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber,
                BENEFICIARYBANK = model.beneficiaryBank,
                CURRENCYID = model.currencyId,
                PROFORMAINVOICEID = model.proformaInvoiceId,
                AVAILABLEAMOUNT = model.availableAmount,
                LETTEROFCREDITAMOUNT = model.letterOfCreditAmount,
                LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate,
                INVOICEDATE = model.invoiceDate,
                INVOICEDUEDATE = model.invoiceDueDate,
                OPERATIONID = (int)OperationsEnum.LCIssuanceExtensionApproval,
                TRANSACTIONCYCLE = model.transactionCycle,
                //COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            context.TBL_TEMP_LC_ISSUANCE.Add(entity);
            var systemDate = general.GetApplicationDate();
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            var aud = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcExtensionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Issuance Extension '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------

            context.SaveChanges();
            var createdlcRecord = context.TBL_TEMP_LC_ISSUANCE.FirstOrDefault(l => l.ENHANCEMENTREFERENCENUMBER == referenceNumber);
            if (createdlcRecord != null)
            {
                model.tempLcIssuanceId = createdlcRecord.TEMPLCISSUANCEID;
            }

            return model;
        }

        public bool UpdateLcIssuance(LcIssuanceViewModel model, int id, UserInfo user)
        {

            ValidateAmounts(model);

            var entity = this.context.TBL_LC_ISSUANCE.Find(id);
            entity.LCREFERENCENUMBER = model.lcReferenceNumber;
            entity.BENEFICIARYNAME = model.beneficiaryName;
            entity.TOTALAPPROVEDAMOUNT = model.totalApprovedAmount;
            entity.TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId;
            entity.AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId;
            entity.CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable;
            entity.CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType;
            entity.CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber;
            entity.PERCENTAGETOCOVER = model.percentageToCover;
            entity.LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage;
            entity.LCTOLERANCEVALUE = model.lcToleranceValue;
            entity.RELEASEDAMOUNT = model.releaseAmount;
            entity.LETTEROFCREDITTYPEID = model.letterOfCreditTypeId;
            entity.ISDRAFTREQUIRED = model.isDraftRequired;
            entity.BENEFICIARYADDRESS = model.beneficiaryAddress;
            entity.BENEFICIARYEMAIL = model.beneficiaryEmail;
            entity.CUSTOMERID = model.customerId;
            entity.FUNDSOURCEID = model.fundSourceId;
            entity.FUNDSOURCEDETAILS = model.fundSourceDetails;
            entity.FORMMNUMBER = model.formMNumber;
            entity.BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber;
            entity.BENEFICIARYBANK = model.beneficiaryBank;
            entity.CURRENCYID = model.currencyId;
            entity.PROFORMAINVOICEID = model.proformaInvoiceId;
            entity.AVAILABLEAMOUNT = model.availableAmount;
            entity.LETTEROFCREDITAMOUNT = model.letterOfCreditAmount;
            entity.LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate;
            entity.INVOICEDATE = model.invoiceDate;
            entity.INVOICEDUEDATE = model.invoiceDueDate;
            entity.TRANSACTIONCYCLE = model.transactionCycle;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcIssuanceUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcEnhancement(LcIssuanceViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_TEMP_LC_ISSUANCE.Find(id);
            ValidateLcEnhancementUpdate(model);

            //entity.LCREFERENCENUMBER = model.lcReferenceNumber;
            entity.BENEFICIARYNAME = model.beneficiaryName;
            entity.TOTALAPPROVEDAMOUNT = model.totalApprovedAmount;
            entity.TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId;
            entity.AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId;
            entity.CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable;
            entity.CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType;
            entity.CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber;
            entity.PERCENTAGETOCOVER = model.percentageToCover;
            entity.LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage;
            entity.LCTOLERANCEVALUE = model.lcToleranceValue;
            entity.RELEASEDAMOUNT = model.releaseAmount;
            entity.LETTEROFCREDITTYPEID = model.letterOfCreditTypeId;
            entity.ISDRAFTREQUIRED = model.isDraftRequired;
            entity.BENEFICIARYADDRESS = model.beneficiaryAddress;
            entity.BENEFICIARYEMAIL = model.beneficiaryEmail;
            entity.CUSTOMERID = model.customerId;
            entity.FUNDSOURCEID = model.fundSourceId;
            entity.FUNDSOURCEDETAILS = model.fundSourceDetails;
            entity.FORMMNUMBER = model.formMNumber;
            entity.BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber;
            entity.BENEFICIARYBANK = model.beneficiaryBank;
            entity.CURRENCYID = model.currencyId;
            entity.PROFORMAINVOICEID = model.proformaInvoiceId;
            entity.AVAILABLEAMOUNT = model.availableAmount;
            entity.LETTEROFCREDITAMOUNT = model.letterOfCreditAmount;
            entity.LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate;
            entity.INVOICEDATE = model.invoiceDate;
            entity.INVOICEDUEDATE = model.invoiceDueDate;
            entity.TRANSACTIONCYCLE = model.transactionCycle;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcEnhancementUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance Enhancement '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.TEMPLCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcExtension(LcIssuanceViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_TEMP_LC_ISSUANCE.Find(id);
            ValidateLcEnhancementUpdate(model);

            //entity.LCREFERENCENUMBER = model.lcReferenceNumber;
            entity.BENEFICIARYNAME = model.beneficiaryName;
            entity.TOTALAPPROVEDAMOUNT = model.totalApprovedAmount;
            entity.TOTALAPPROVEDAMOUNTCURRENCYID = model.totalApprovedAmountCurrencyId;
            entity.AVAILABLEAMOUNTCURRENCYID = model.availableAmountCurrencyId;
            entity.CASHBUILDUPAVAILABLE = model.cashBuildUpAvailable;
            entity.CASHBUILDUPREFERENCETYPE = model.cashBuildUpReferenceType;
            entity.CASHBUILDUPREFERENCENUMBER = model.cashBuildUpReferenceNumber;
            entity.PERCENTAGETOCOVER = model.percentageToCover;
            entity.LCTOLERANCEPERCENTAGE = model.lcTolerancePercentage;
            entity.LCTOLERANCEVALUE = model.lcToleranceValue;
            entity.RELEASEDAMOUNT = model.releaseAmount;
            entity.LETTEROFCREDITTYPEID = model.letterOfCreditTypeId;
            entity.ISDRAFTREQUIRED = model.isDraftRequired;
            entity.BENEFICIARYADDRESS = model.beneficiaryAddress;
            entity.BENEFICIARYEMAIL = model.beneficiaryEmail;
            entity.CUSTOMERID = model.customerId;
            entity.FUNDSOURCEID = model.fundSourceId;
            entity.FUNDSOURCEDETAILS = model.fundSourceDetails;
            entity.FORMMNUMBER = model.formMNumber;
            entity.BENEFICIARYPHONENUMBER = model.beneficiaryPhoneNumber;
            entity.BENEFICIARYBANK = model.beneficiaryBank;
            entity.CURRENCYID = model.currencyId;
            entity.PROFORMAINVOICEID = model.proformaInvoiceId;
            entity.AVAILABLEAMOUNT = model.availableAmount;
            entity.LETTEROFCREDITAMOUNT = model.letterOfCreditAmount;
            entity.LETTEROFCREDITEXPIRYDATE = model.letterOfcreditExpirydate;
            entity.INVOICEDATE = model.invoiceDate;
            entity.INVOICEDUEDATE = model.invoiceDueDate;
            entity.TRANSACTIONCYCLE = model.transactionCycle;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcExtensionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance Extension '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.TEMPLCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcIssuance(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_ISSUANCE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcIssuanceDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcEnhancement(int id, UserInfo user)
        {
            var entity = this.context.TBL_TEMP_LC_ISSUANCE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcEnhancementDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance Enhancement '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.TEMPLCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcExtension(int id, UserInfo user)
        {
            var entity = this.context.TBL_TEMP_LC_ISSUANCE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcExtensionDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Issuance Extension '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.TEMPLCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddLcArchive(int LcIssuanceId, int operationId)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(LcIssuanceId);
            if (lc == null)
            {
                throw new SecureException("LC NOT FOUND TO BE ARCHIVED!");
            }
            var newArch = new TBL_LC_ISSUANCE_ARCHIVE()
            {
                LCISSUANCEID = lc.LCISSUANCEID,
                LCREFERENCENUMBER = lc.LCREFERENCENUMBER,
                BENEFICIARYNAME = lc.BENEFICIARYNAME,
                TOTALAPPROVEDAMOUNT = lc.TOTALAPPROVEDAMOUNT,
                LETTEROFCREDITTYPEID = lc.LETTEROFCREDITTYPEID,
                ISDRAFTREQUIRED = lc.ISDRAFTREQUIRED,
                BENEFICIARYADDRESS = lc.BENEFICIARYADDRESS,
                BENEFICIARYEMAIL = lc.BENEFICIARYEMAIL,
                CUSTOMERID = lc.CUSTOMERID,
                FUNDSOURCEID = lc.FUNDSOURCEID,
                FUNDSOURCEDETAILS = lc.FUNDSOURCEDETAILS,
                FORMMNUMBER = lc.FORMMNUMBER,
                BENEFICIARYPHONENUMBER = lc.BENEFICIARYPHONENUMBER,
                BENEFICIARYBANK = lc.BENEFICIARYBANK,
                CURRENCYID = lc.CURRENCYID,
                PROFORMAINVOICEID = lc.PROFORMAINVOICEID,
                AVAILABLEAMOUNT = lc.AVAILABLEAMOUNT,
                LETTEROFCREDITAMOUNT = lc.LETTEROFCREDITAMOUNT,
                LETTEROFCREDITEXPIRYDATE = lc.LETTEROFCREDITEXPIRYDATE,
                INVOICEDATE = lc.INVOICEDATE,
                INVOICEDUEDATE = lc.INVOICEDUEDATE,
                TRANSACTIONCYCLE = lc.TRANSACTIONCYCLE,
                DATETIMECREATED = lc.DATETIMECREATED,
                DATETIMEUPDATED = lc.DATETIMEUPDATED,
                DELETED = lc.DELETED,
                DELETEDBY = lc.DELETEDBY,
                CREATEDBY = lc.CREATEDBY,
                LASTUPDATEDBY = lc.LASTUPDATEDBY,
                DATETIMEDELETED = lc.DATETIMEDELETED,
                APPROVEDBY = lc.APPROVEDBY,
                APPROVED = lc.APPROVED,
                APPROVALSTATUSID = lc.APPROVALSTATUSID,
                LCUSSANCESTATUSID = lc.LCUSSANCESTATUSID,
                LCUSSANCEAPPROVALSTATUSID = lc.LCUSSANCEAPPROVALSTATUSID,
                APPLICATIONSTATUSID = lc.APPLICATIONSTATUSID,
                FINALAPPROVAL_LEVELID = lc.FINALAPPROVAL_LEVELID,
                LCUSSANCEFINALAPPROVAL_LEVELID = lc.LCUSSANCEFINALAPPROVAL_LEVELID,
                LCUSSANCEAPPROVEDDATE = lc.LCUSSANCEAPPROVEDDATE,
                DATEACTEDON = lc.DATEACTEDON,
                ACTEDONBY = lc.ACTEDONBY,
                APPROVEDDATE = lc.APPROVEDDATE,
                TOTALAPPROVEDAMOUNTCURRENCYID = lc.TOTALAPPROVEDAMOUNTCURRENCYID,
                AVAILABLEAMOUNTCURRENCYID = lc.AVAILABLEAMOUNTCURRENCYID,
                CASHBUILDUPAVAILABLE = lc.CASHBUILDUPAVAILABLE,
                CASHBUILDUPREFERENCETYPE = lc.CASHBUILDUPREFERENCENUMBER,
                CASHBUILDUPREFERENCENUMBER = lc.CASHBUILDUPREFERENCENUMBER,
                PERCENTAGETOCOVER = lc.PERCENTAGETOCOVER,
                LCTOLERANCEPERCENTAGE = lc.LCTOLERANCEPERCENTAGE,
                LCTOLERANCEVALUE = lc.LCTOLERANCEVALUE,
                RELEASEDAMOUNT = lc.RELEASEDAMOUNT,
                OPERATIONID = lc.OPERATIONID,
                ARCHIVINGOPERATIONID = operationId,
                DATETIMEARCHIVED = DateTime.Now
            };
            context.TBL_LC_ISSUANCE_ARCHIVE.Add(newArch);
            return context.SaveChanges() != 0;
        }

        public bool UpdateOldLcWithEnhancement(int tempLcIssuanceId)
        {
            var newLc = context.TBL_TEMP_LC_ISSUANCE.Find(tempLcIssuanceId);
            var oldLc = context.TBL_LC_ISSUANCE.Find(newLc.LCISSUANCEID);
            if (newLc == null)
            {
                throw new SecureException("LC Enhancement Data Not Found!");
            }
            if (newLc == null)
            {
                throw new SecureException("LC Issuance Data Not Found!");
            }

            oldLc.BENEFICIARYNAME = newLc.BENEFICIARYNAME;
            oldLc.TOTALAPPROVEDAMOUNT = newLc.TOTALAPPROVEDAMOUNT;
            oldLc.LETTEROFCREDITTYPEID = newLc.LETTEROFCREDITTYPEID;
            oldLc.ISDRAFTREQUIRED = newLc.ISDRAFTREQUIRED;
            oldLc.BENEFICIARYADDRESS = newLc.BENEFICIARYADDRESS;
            oldLc.BENEFICIARYEMAIL = newLc.BENEFICIARYEMAIL;
            oldLc.CUSTOMERID = newLc.CUSTOMERID;
            oldLc.FUNDSOURCEID = newLc.FUNDSOURCEID;
            oldLc.FUNDSOURCEDETAILS = newLc.FUNDSOURCEDETAILS;
            oldLc.FORMMNUMBER = newLc.FORMMNUMBER;
            oldLc.BENEFICIARYPHONENUMBER = newLc.BENEFICIARYPHONENUMBER;
            oldLc.BENEFICIARYBANK = newLc.BENEFICIARYBANK;
            oldLc.CURRENCYID = newLc.CURRENCYID;
            oldLc.PROFORMAINVOICEID = newLc.PROFORMAINVOICEID;
            oldLc.AVAILABLEAMOUNT = newLc.AVAILABLEAMOUNT;
            oldLc.LETTEROFCREDITAMOUNT = newLc.LETTEROFCREDITAMOUNT;
            oldLc.LETTEROFCREDITEXPIRYDATE = newLc.LETTEROFCREDITEXPIRYDATE;
            oldLc.INVOICEDATE = newLc.INVOICEDATE;
            oldLc.INVOICEDUEDATE = newLc.INVOICEDUEDATE;
            oldLc.TRANSACTIONCYCLE = newLc.TRANSACTIONCYCLE;
            oldLc.DATETIMEUPDATED = DateTime.Now;
            oldLc.LASTUPDATEDBY = newLc.LASTUPDATEDBY;
            oldLc.TOTALAPPROVEDAMOUNTCURRENCYID = newLc.TOTALAPPROVEDAMOUNTCURRENCYID;
            oldLc.AVAILABLEAMOUNTCURRENCYID = newLc.AVAILABLEAMOUNTCURRENCYID;
            oldLc.CASHBUILDUPAVAILABLE = newLc.CASHBUILDUPAVAILABLE;
            oldLc.CASHBUILDUPREFERENCETYPE = newLc.CASHBUILDUPREFERENCENUMBER;
            oldLc.CASHBUILDUPREFERENCENUMBER = newLc.CASHBUILDUPREFERENCENUMBER;
            oldLc.PERCENTAGETOCOVER = newLc.PERCENTAGETOCOVER;
            oldLc.LCTOLERANCEPERCENTAGE = newLc.LCTOLERANCEPERCENTAGE;
            oldLc.LCTOLERANCEVALUE = newLc.LCTOLERANCEVALUE;
            oldLc.RELEASEDAMOUNT = newLc.RELEASEDAMOUNT;
            return context.SaveChanges() != 0;
        }

        #endregion LCISSUANCE

        #region RELEASEOFSHIPPINGDOCUMENTS
        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForRelease(int staffId) 
        {
            //var lcsReleasesInTrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments).Select(t => t.TARGETID);
            //var lcReleases = context.TBL_LCRELEASE_AMOUNT.Where(y => !lcsReleasesInTrail.Contains(y.LCRELEASEAMOUNTID)).ToList();
            //var lcIssuanceIds = lcReleases.Select(r => r.LCISSUANCEID).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();
            var releases = context.TBL_LCRELEASE_AMOUNT.ToList();
            var lcsInProgress = (from i in context.TBL_LC_ISSUANCE
                       join r in context.TBL_LCRELEASE_AMOUNT on i.LCISSUANCEID equals r.LCISSUANCEID
                       join rt in context.TBL_APPROVAL_TRAIL on r.LCRELEASEAMOUNTID equals rt.TARGETID into irt
                       from t in irt.DefaultIfEmpty()
                       where
                       (
                       i.DELETED == false
                       && i.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                       && t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments
                       && r.RELEASEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                       && i.LCTOLERANCEVALUE > (context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == i.LCISSUANCEID).Sum(r => r.RELEASEAMOUNT) ?? 0)
                       )
                       select new LcIssuanceApprovalViewModel
                       {
                           lcIssuanceId = i.LCISSUANCEID,
                           lcReleaseAmountId = r.LCRELEASEAMOUNTID,
                           approvalStatusId = (short)t.APPROVALSTATUSID,
                           lcApprovalTrailId = t.APPROVALTRAILID,
                           loopedStaffId = t.LOOPEDSTAFFID,
                           beneficiaryName = i.BENEFICIARYNAME,
                           totalApprovedAmount = i.TOTALAPPROVEDAMOUNT,
                           totalApprovedAmountCurrencyId = i.TOTALAPPROVEDAMOUNTCURRENCYID,
                           availableAmountCurrencyId = i.AVAILABLEAMOUNTCURRENCYID,
                           cashBuildUpAvailable = i.CASHBUILDUPAVAILABLE,
                           cashBuildUpReferenceNumber = i.CASHBUILDUPREFERENCETYPE,
                           cashBuildUpReferenceType = i.CASHBUILDUPREFERENCENUMBER,
                           percentageToCover = i.PERCENTAGETOCOVER,
                           lcTolerancePercentage = i.LCTOLERANCEPERCENTAGE,
                           lcToleranceValue = i.LCTOLERANCEVALUE,
                           releaseAmount = r.RELEASEAMOUNT,
                           releasedAmount = i.RELEASEDAMOUNT,
                           availableAmountForRelease = ((i.LCTOLERANCEVALUE ?? 0) - i.RELEASEDAMOUNT),
                           letterOfCreditTypeId = i.LETTEROFCREDITTYPEID,
                           isDraftRequired = i.ISDRAFTREQUIRED,
                           beneficiaryAddress = i.BENEFICIARYADDRESS,
                           beneficiaryEmail = i.BENEFICIARYEMAIL,
                           customerId = i.CUSTOMERID,
                           customerName = i.TBL_CUSTOMER.FIRSTNAME + " " + i.TBL_CUSTOMER.MIDDLENAME + " " + i.TBL_CUSTOMER.LASTNAME,
                           fundSourceId = i.FUNDSOURCEID,
                           fundSourceDetails = i.FUNDSOURCEDETAILS,
                           formMNumber = i.FORMMNUMBER,
                           beneficiaryPhoneNumber = i.BENEFICIARYPHONENUMBER,
                           beneficiaryBank = i.BENEFICIARYBANK,
                           currencyId = i.CURRENCYID,
                           proformaInvoiceId = i.PROFORMAINVOICEID,
                           availableAmount = i.AVAILABLEAMOUNT,
                           letterOfCreditAmount = i.LETTEROFCREDITAMOUNT,
                           letterOfcreditExpirydate = i.LETTEROFCREDITEXPIRYDATE,
                           invoiceDate = i.INVOICEDATE,
                           invoiceDueDate = i.INVOICEDUEDATE,
                           transactionCycle = i.TRANSACTIONCYCLE,
                           lcReferenceNumber = i.LCREFERENCENUMBER,
                           dateTimeCreated = (DateTime)i.DATETIMECREATED,
                       }).GroupBy(l => l.lcReleaseAmountId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                       .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var lcsNotStarted = (from i in context.TBL_LC_ISSUANCE
                                 join r in context.TBL_LCRELEASE_AMOUNT on i.LCISSUANCEID equals r.LCISSUANCEID into ir
                                 from r in ir.DefaultIfEmpty()
                                 where
                                 (
                                 i.DELETED == false
                                 //&& i.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                 && i.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                 && (r.RELEASEAPPLICATIONSTATUSID == null || r == null)
                                 && (staffs.Contains(r.CREATEDBY ?? 0) || r == null)
                                 && i.LCTOLERANCEVALUE > (context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == i.LCISSUANCEID).Sum(r => r.RELEASEAMOUNT) ?? 0)
                                 )
                                 select new LcIssuanceApprovalViewModel
                                 {
                                     lcIssuanceId = i.LCISSUANCEID,
                                     lcReleaseAmountId = r.LCRELEASEAMOUNTID,
                                     approvalStatusId = (short)r.RELEASEAPPROVALSTATUSID,
                                     beneficiaryName = i.BENEFICIARYNAME,
                                     totalApprovedAmount = i.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = i.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = i.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = i.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = i.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = i.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = i.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = i.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = i.LCTOLERANCEVALUE,
                                     releaseAmount = r.RELEASEAMOUNT,
                                     releasedAmount = i.RELEASEDAMOUNT,
                                     availableAmountForRelease = ((i.LCTOLERANCEVALUE ?? 0) - i.RELEASEDAMOUNT),
                                     letterOfCreditTypeId = i.LETTEROFCREDITTYPEID,
                                     isDraftRequired = i.ISDRAFTREQUIRED,
                                     beneficiaryAddress = i.BENEFICIARYADDRESS,
                                     beneficiaryEmail = i.BENEFICIARYEMAIL,
                                     customerId = i.CUSTOMERID,
                                     customerName = i.TBL_CUSTOMER.FIRSTNAME + " " + i.TBL_CUSTOMER.MIDDLENAME + " " + i.TBL_CUSTOMER.LASTNAME,
                                     fundSourceId = i.FUNDSOURCEID,
                                     fundSourceDetails = i.FUNDSOURCEDETAILS,
                                     formMNumber = i.FORMMNUMBER,
                                     beneficiaryPhoneNumber = i.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = i.BENEFICIARYBANK,
                                     currencyId = i.CURRENCYID,
                                     proformaInvoiceId = i.PROFORMAINVOICEID,
                                     availableAmount = i.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = i.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = i.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = i.INVOICEDATE,
                                     invoiceDueDate = i.INVOICEDUEDATE,
                                     transactionCycle = i.TRANSACTIONCYCLE,
                                     lcReferenceNumber = i.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)i.DATETIMECREATED,
                                 }).ToList();
            //foreach (var lc in lcs)
            //{
            //    if (lcReleases.Exists(r => r.LCISSUANCEID == lc.lcIssuanceId))
            //    {
            //        lc.lcReleaseId = lcReleases.FirstOrDefault(r => r.LCISSUANCEID == lc.lcIssuanceId).LCRELEASEAMOUNTID;
            //        lc.releaseAmount = (decimal)lcReleases.FirstOrDefault(r => r.LCISSUANCEID == lc.lcIssuanceId).RELEASEAMOUNT;
            //    }
            //}
            var lcs = lcsNotStarted.Union(lcsInProgress);
            return lcs;
        }

        public IEnumerable<LcReleaseAmountViewModel> GetReleasesForLcIssuance(int lcIssuanceId)
        {
            var lcReleases = (from rel in context.TBL_LCRELEASE_AMOUNT
                              where lcIssuanceId == rel.LCISSUANCEID &&
                              rel.DELETED == false
                              select new LcReleaseAmountViewModel
                              {
                                  lcReleaseAmountId = rel.LCRELEASEAMOUNTID,
                                  releaseAmount = rel.RELEASEAMOUNT,
                                  releaseApplicationStatus = rel.RELEASEAPPLICATIONSTATUSID == null ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == rel.RELEASEAPPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(),
                              }).ToList();
            return lcReleases;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForReleaseApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcReleaseOfShippingDocuments;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            var releasesForApproval = (from a in context.TBL_LC_ISSUANCE
                         where
                            (a.DELETED == false
                            && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                            && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                         orderby a.LCISSUANCEID
                         join b in context.TBL_LCRELEASE_AMOUNT on a.LCISSUANCEID equals b.LCISSUANCEID
                         join c in context.TBL_APPROVAL_TRAIL on b.LCRELEASEAMOUNTID equals c.TARGETID
                         where
                            (
                            (c.OPERATIONID == operationId)
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            && c.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RELEASEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted
                            && b.RELEASEAPPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                            && c.RESPONSESTAFFID == null
                            && c.LOOPEDSTAFFID == null
                            && levelIds.Contains((int)c.TOAPPROVALLEVELID)
                            && (c.TOSTAFFID == null || staffs.Contains(c.TOSTAFFID ?? 0))
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcReleaseAmountId = b.LCRELEASEAMOUNTID,
                             releaseAmount = (decimal)b.RELEASEAMOUNT,
                             releasedAmount = a.RELEASEDAMOUNT,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCETYPE,
                             cashBuildUpReferenceType = a.CASHBUILDUPREFERENCENUMBER,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             transactionCycle = a.TRANSACTIONCYCLE,
                             lastComment = c.COMMENT,
                             currentApprovalStateId = c.APPROVALSTATEID,
                             currentApprovalLevelId = c.TOAPPROVALLEVELID,
                             currentApprovalLevel = c.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = c.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = c == null ? 0 : c.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = c.TOSTAFFID,
                             approvalStatusId = c.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED
                         }).Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault()).ToList();

            var referredReleases = (from a in context.TBL_LC_ISSUANCE
                                    where
                                       (a.DELETED == false
                                       && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                       && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                                    orderby a.LCISSUANCEID
                                    join b in context.TBL_LCRELEASE_AMOUNT on a.LCISSUANCEID equals b.LCISSUANCEID
                                    join c in context.TBL_APPROVAL_TRAIL on b.LCRELEASEAMOUNTID equals c.TARGETID
                                    where
                                       (
                                       (c.OPERATIONID == operationId)
                                       //&& c.APPROVALSTATEID != (int)ApprovalState.Ended
                                       && b.RELEASEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                                       //&& c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                                       && c.RESPONSESTAFFID == null
                                       //&& c.LOOPEDSTAFFID == staffId
                                       )
                                    select new LcIssuanceApprovalViewModel()
                                    {
                                        lcIssuanceId = a.LCISSUANCEID,
                                        lcReleaseAmountId = b.LCRELEASEAMOUNTID,
                                        releaseAmount = (decimal)b.RELEASEAMOUNT,
                                        releasedAmount = a.RELEASEDAMOUNT,
                                        isDraftRequired = a.ISDRAFTREQUIRED,
                                        lcReferenceNumber = a.LCREFERENCENUMBER,
                                        letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                                        beneficiaryName = a.BENEFICIARYNAME,
                                        totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                                        totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                                        availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                                        cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                                        cashBuildUpReferenceNumber = a.CASHBUILDUPREFERENCETYPE,
                                        cashBuildUpReferenceType = a.CASHBUILDUPREFERENCENUMBER,
                                        percentageToCover = a.PERCENTAGETOCOVER,
                                        lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                                        lcToleranceValue = a.LCTOLERANCEVALUE,
                                        beneficiaryAddress = a.BENEFICIARYADDRESS,
                                        beneficiaryEmail = a.BENEFICIARYEMAIL,
                                        customerId = a.CUSTOMERID,
                                        fundSourceId = a.FUNDSOURCEID,
                                        fundSourceDetails = a.FUNDSOURCEDETAILS,
                                        formMNumber = a.FORMMNUMBER,
                                        beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                                        beneficiaryBank = a.BENEFICIARYBANK,
                                        currencyId = a.CURRENCYID,
                                        proformaInvoiceId = a.PROFORMAINVOICEID,
                                        availableAmount = a.AVAILABLEAMOUNT,
                                        letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                                        letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                                        invoiceDate = a.INVOICEDATE,
                                        invoiceDueDate = a.INVOICEDUEDATE,
                                        transactionCycle = a.TRANSACTIONCYCLE,
                                        lastComment = c.COMMENT,
                                        currentApprovalStateId = c.APPROVALSTATEID,
                                        currentApprovalLevelId = c.TOAPPROVALLEVELID,
                                        currentApprovalLevel = c.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                        //currentApprovalLevelTypeId = c.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                                        lcApprovalTrailId = c == null ? 0 : c.APPROVALTRAILID, // for inner sequence ordering
                                        toStaffId = c.TOSTAFFID,
                                        approvalStatusId = c.APPROVALSTATUSID,
                                        loopedStaffId = c.LOOPEDSTAFFID,
                                        applicationStatusId = a.APPLICATIONSTATUSID,
                                        createdBy = (int)a.CREATEDBY,
                                        customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                        operationId = operationId,
                                        dateTimeCreated = (DateTime)a.DATETIMECREATED
                                    }).GroupBy(d => d.lcIssuanceId).Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault())
                                    .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Referred && l.loopedStaffId == staffId)).ToList();
            var applications = releasesForApproval.Union(referredReleases);
            return applications.ToList();
        }

        public LcReleaseAmountViewModel AddLCReleaseAmount(LcReleaseAmountViewModel entity)
        {
            ValidateReleaseAmount(entity);
            var reference = CommonHelpers.GenerateRandomDigitCode(10);
            context.TBL_LCRELEASE_AMOUNT.Add( new TBL_LCRELEASE_AMOUNT
            {
                LCISSUANCEID = entity.lcIssuanceId,
                RELEASEREF = reference,
                RELEASEAMOUNT = entity.releaseAmount,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = entity.createdBy
            });

            var systemDate = general.GetApplicationDate();
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == entity.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            var aud = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LCReleaseAmountAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"TBL_LCRELEASE_AMOUNT '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = systemDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------
            context.SaveChanges();
            var newLC = context.TBL_LCRELEASE_AMOUNT.FirstOrDefault(l => l.RELEASEREF == reference);
            entity.lcReleaseAmountId = newLC.LCRELEASEAMOUNTID;
            return entity;
        }

        public LcReleaseAmountViewModel UpdateLCReleaseAmount(LcReleaseAmountViewModel entity)
        {
            ValidateReleaseAmount(entity);
            var lc = context.TBL_LCRELEASE_AMOUNT.FirstOrDefault(l => l.LCRELEASEAMOUNTID == entity.lcReleaseAmountId);

            //lc.LCISSUANCEID = entity.lcIssuanceId;
            lc.RELEASEAMOUNT = entity.releaseAmount;
            lc.DATETIMEUPDATED = DateTime.Now;

            var systemDate = general.GetApplicationDate();
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == entity.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            var aud = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LCReleaseAmountUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"TBL_LCRELEASE_AMOUNT '{entity.ToString()}' updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = systemDate,
                SYSTEMDATETIME = DateTime.Now,
                  DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
             
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------
            context.SaveChanges();
            return entity;
        }

        public LcReleaseAmountViewModel GetLCReleaseAmount(int lcReleaseAmountId)
        {
            var lc = context.TBL_LCRELEASE_AMOUNT.Find(lcReleaseAmountId);
            return new LcReleaseAmountViewModel
            {
                lcReleaseAmountId = lc.LCRELEASEAMOUNTID,
                lcIssuanceId = lc.LCISSUANCEID,
                releaseAmount = lc.RELEASEAMOUNT
            };
        }

        private bool ValidateReleaseAmount(LcReleaseAmountViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var currCode = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == lc.CURRENCYID).CURRENCYCODE;
            var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => (r.RELEASEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted 
                                                                         && r.RELEASEAPPROVALSTATUSID == (int)ApprovalStatusEnum.Approved) && r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT) ?? 0;
            var availableAmount = lc.LCTOLERANCEVALUE - totalReleasedAmount;
            if (model.releaseAmount > availableAmount)
            {
                throw new SecureException("Release Amount cannot be greater than remainder tolerance amount " + currCode + " " + availableAmount);
            }
            lc.RELEASEDAMOUNT = totalReleasedAmount;
            context.SaveChanges();
            return true;
        }
        #endregion RELEASEOFSHIPPINGDOCUMENTS


        //#region LCDOCUMENT
        //public IEnumerable<LcDocumentViewModel> GetLcDocuments()
        //{
        //    return context.TBL_LC_DOCUMENT.Where(x => x.DELETED == false)
        //        .Select(x => new LcDocumentViewModel
        //        {
        //            lcDocumentId = x.LCDOCUMENTID,
        //            lcIssuanceId = x.LCISSUANCEID,
        //            documentTitle = x.DOCUMENTTITLE,
        //            isSentToIssuingBank = x.ISSENTTOISSUINGBANK,
        //            numberOfCopies = x.NUMBEROFCOPIES,
        //            isSentToApplicant = x.ISSENTTOAPPLICANT,
        //        })
        //        .ToList();
        //}

        //public LcDocumentViewModel GetLcDocument(int id)
        //{
        //    var entity = context.TBL_LC_DOCUMENT.FirstOrDefault(x => x.LCDOCUMENTID == id && x.DELETED == false);

        //    return new LcDocumentViewModel
        //    {
        //        lcDocumentId = entity.LCDOCUMENTID,
        //        lcIssuanceId = entity.LCISSUANCEID,
        //        documentTitle = entity.DOCUMENTTITLE,
        //        isSentToIssuingBank = entity.ISSENTTOISSUINGBANK,
        //        numberOfCopies = entity.NUMBEROFCOPIES,
        //        isSentToApplicant = entity.ISSENTTOAPPLICANT,
        //    };
        //}

        //public bool AddLcDocument(LcDocumentViewModel model)
        //{
        //    var entity = new TBL_LC_DOCUMENT
        //    {
        //        LCISSUANCEID = model.lcIssuanceId,
        //        DOCUMENTTITLE = model.documentTitle,
        //        ISSENTTOISSUINGBANK = model.isSentToIssuingBank,
        //        NUMBEROFCOPIES = model.numberOfCopies,
        //        ISSENTTOAPPLICANT = model.isSentToApplicant,
        //        // COMPANYID = model.companyId,
        //        CREATEDBY = model.createdBy,
        //        DATETIMECREATED = general.GetApplicationDate(),
        //    };

        //    context.TBL_LC_DOCUMENT.Add(entity);

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcDocumentAdded,
        //        STAFFID = model.createdBy,
        //        BRANCHID = (short)model.userBranchId,
        //        DETAIL = $"TBL_Lc Document '{entity.DESCRIPTION}' created by {auditStaff}",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool UpdateLcDocument(LcDocumentViewModel model, int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_DOCUMENT.Find(id);
        //    entity.LCISSUANCEID = model.lcIssuanceId;
        //    entity.DOCUMENTTITLE = model.documentTitle;
        //    entity.ISSENTTOISSUINGBANK = model.isSentToIssuingBank;
        //    entity.NUMBEROFCOPIES = model.numberOfCopies;
        //    entity.ISSENTTOAPPLICANT = model.isSentToApplicant;

        //    entity.LASTUPDATEDBY = user.createdBy;
        //    entity.DATETIMEUPDATED = DateTime.Now;

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcDocumentUpdated,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Document '{entity.DESCRIPTION}' was updated by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCDOCUMENTID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool DeleteLcDocument(int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_DOCUMENT.Find(id);
        //    entity.DELETED = true;
        //    entity.DELETEDBY = user.createdBy;
        //    entity.DATETIMEDELETED = general.GetApplicationDate();

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcDocumentDeleted,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Document '{entity.DESCRIPTION}' was deleted by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCDOCUMENTID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //#endregion LCDOCUMENT

        //#region SHIPPING
        //public IEnumerable<LcShippingViewModel> GetLcShippings()
        //{
        //    return context.TBL_LC_SHIPPING.Where(x => x.DELETED == false)
        //        .Select(x => new LcShippingViewModel
        //        {
        //            lcShippingId = x.LCSHIPPINGID,
        //            lcIssuanceId = x.LCISSUANCEID,
        //            partyName = x.PARTYNAME,
        //            partyAddress = x.PARTYADDRESS,
        //            portOfDischarge = x.PORTOFDISCHARGE,
        //            portOfShipment = x.PORTOFSHIPMENT,
        //            latestShipmentDate = x.LATESTSHIPMENTDATE,
        //            isPartShipmentAllowed = x.ISPARTSHIPMENTALLOWED,
        //            isTransShipmentAllowed = x.ISTRANSSHIPMENTALLOWED,
        //        })
        //        .ToList();
        //}

        //public LcShippingViewModel GetLcShipping(int id)
        //{
        //    var entity = context.TBL_LC_SHIPPING.FirstOrDefault(x => x.LCSHIPPINGID == id && x.DELETED == false);

        //    return new LcShippingViewModel
        //    {
        //        lcShippingId = entity.LCSHIPPINGID,
        //        lcIssuanceId = entity.LCISSUANCEID,
        //        partyName = entity.PARTYNAME,
        //        partyAddress = entity.PARTYADDRESS,
        //        portOfDischarge = entity.PORTOFDISCHARGE,
        //        portOfShipment = entity.PORTOFSHIPMENT,
        //        latestShipmentDate = entity.LATESTSHIPMENTDATE,
        //        isPartShipmentAllowed = entity.ISPARTSHIPMENTALLOWED,
        //        isTransShipmentAllowed = entity.ISTRANSSHIPMENTALLOWED,
        //    };
        //}

        //public bool AddLcShipping(LcShippingViewModel model)
        //{
        //    var entity = new TBL_LC_SHIPPING
        //    {
        //        LCISSUANCEID = model.lcIssuanceId,
        //        PARTYNAME = model.partyName,
        //        PARTYADDRESS = model.partyAddress,
        //        PORTOFDISCHARGE = model.portOfDischarge,
        //        PORTOFSHIPMENT = model.portOfShipment,
        //        LATESTSHIPMENTDATE = model.latestShipmentDate,
        //        ISPARTSHIPMENTALLOWED = model.isPartShipmentAllowed,
        //        ISTRANSSHIPMENTALLOWED = model.isTransShipmentAllowed,
        //        // COMPANYID = model.companyId,
        //        CREATEDBY = model.createdBy,
        //        DATETIMECREATED = general.GetApplicationDate(),
        //    };

        //    context.TBL_LC_SHIPPING.Add(entity);

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcShippingAdded,
        //        STAFFID = model.createdBy,
        //        BRANCHID = (short)model.userBranchId,
        //        DETAIL = $"TBL_Lc Shipping '{entity.DESCRIPTION}' created by {auditStaff}",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool UpdateLcShipping(LcShippingViewModel model, int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_SHIPPING.Find(id);
        //    entity.LCISSUANCEID = model.lcIssuanceId;
        //    entity.PARTYNAME = model.partyName;
        //    entity.PARTYADDRESS = model.partyAddress;
        //    entity.PORTOFDISCHARGE = model.portOfDischarge;
        //    entity.PORTOFSHIPMENT = model.portOfShipment;
        //    entity.LATESTSHIPMENTDATE = model.latestShipmentDate;
        //    entity.ISPARTSHIPMENTALLOWED = model.isPartShipmentAllowed;
        //    entity.ISTRANSSHIPMENTALLOWED = model.isTransShipmentAllowed;

        //    entity.LASTUPDATEDBY = user.createdBy;
        //    entity.DATETIMEUPDATED = DateTime.Now;

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcShippingUpdated,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Shipping '{entity.DESCRIPTION}' was updated by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCSHIPPINGID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool DeleteLcShipping(int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_SHIPPING.Find(id);
        //    entity.DELETED = true;
        //    entity.DELETEDBY = user.createdBy;
        //    entity.DATETIMEDELETED = general.GetApplicationDate();

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcShippingDeleted,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Shipping '{entity.DESCRIPTION}' was deleted by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCSHIPPINGID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}
        //#endregion SHIPPING

        ////#region LCCONDITIONS
        //public IEnumerable<LcConditionViewModel> GetLcConditions()
        //{
        //    return context.TBL_LC_CONDITION.Where(x => x.DELETED == false)
        //        .Select(x => new LcConditionViewModel
        //        {
        //            lcConditionId = x.LCCONDITIONID,
        //            lcIssuanceId = x.LCISSUANCEID,
        //            condition = x.CONDITION,
        //            isSatisfied = x.ISSATISFIED,
        //        })
        //        .ToList();
        //}

        //public LcConditionViewModel GetLcCondition(int id)
        //{
        //    var entity = context.TBL_LC_CONDITION.FirstOrDefault(x => x.LCCONDITIONID == id && x.DELETED == false);

        //    return new LcConditionViewModel
        //    {
        //        lcConditionId = entity.LCCONDITIONID,
        //        lcIssuanceId = entity.LCISSUANCEID,
        //        condition = entity.CONDITION,
        //        isSatisfied = entity.ISSATISFIED,
        //    };
        //}

        //public bool AddLcCondition(LcConditionViewModel model)
        //{
        //    var entity = new TBL_LC_CONDITION
        //    {
        //        LCISSUANCEID = model.lcIssuanceId,
        //        CONDITION = model.condition,
        //        ISSATISFIED = model.isSatisfied,
        //        // COMPANYID = model.companyId,
        //        CREATEDBY = model.createdBy,
        //        DATETIMECREATED = general.GetApplicationDate(),
        //    };

        //    context.TBL_LC_CONDITION.Add(entity);

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcConditionAdded,
        //        STAFFID = model.createdBy,
        //        BRANCHID = (short)model.userBranchId,
        //        DETAIL = $"TBL_Lc Condition '{entity.DESCRIPTION}' created by {auditStaff}",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool UpdateLcCondition(LcConditionViewModel model, int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_CONDITION.Find(id);
        //    entity.LCISSUANCEID = model.lcIssuanceId;
        //    entity.CONDITION = model.condition;
        //    entity.ISSATISFIED = model.isSatisfied;

        //    entity.LASTUPDATEDBY = user.createdBy;
        //    entity.DATETIMEUPDATED = DateTime.Now;

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcConditionUpdated,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Condition '{entity.DESCRIPTION}' was updated by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCCONDITIONID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //public bool DeleteLcCondition(int id, UserInfo user)
        //{
        //    var entity = this.context.TBL_LC_CONDITION.Find(id);
        //    entity.DELETED = true;
        //    entity.DELETEDBY = user.createdBy;
        //    entity.DATETIMEDELETED = general.GetApplicationDate();

        //    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
        //    // Audit Section ---------------------------
        //    this.audit.AddAuditTrail(new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LcConditionDeleted,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"TBL_Lc Condition '{entity.DESCRIPTION}' was deleted by {auditStaff}",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = general.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now,
        //        TARGETID = entity.LCCONDITIONID
        //    });
        //    // Audit Section end ------------------------

        //    return context.SaveChanges() != 0;
        //}

        //#endregion LCCONDITIONS
    }
}

           // kernel.Bind<ILcIssuanceRepository>().To<LcIssuanceRepository>();
           // LcIssuanceAdded = ???, LcIssuanceUpdated = ???, LcIssuanceDeleted = ???,

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
        private ILoanRepository loanRepository;

        public LcIssuanceRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow,
                ILoanRepository _loanRepository
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
            this.loanRepository = _loanRepository;
        }
        
        #region LCISSUANCE

        public List<LcIssuanceApprovalViewModel> SearchLc(string searchString)
        {
                int[] operations = { (int)OperationsEnum.lcIssuance, (int)OperationsEnum.lcReleaseOfShippingDocuments, (int)OperationsEnum.lcUssance};
            int[] currentApprovalLevelStatuses = {(int)LoanApplicationStatusEnum.LcIssuanceCompleted, (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted};

                searchString = searchString.Trim().ToLower();

                var applications = (from x in context.TBL_LC_ISSUANCE
                                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                    join y in context.TBL_APPROVAL_TRAIL on x.LCISSUANCEID equals y.TARGETID
                                    join z in context.TBL_LCRELEASE_AMOUNT on x.LCISSUANCEID equals z.LCISSUANCEID into xz
                                    from rel in xz.DefaultIfEmpty() 
                                    join z2 in context.TBL_APPROVAL_TRAIL on rel.LCRELEASEAMOUNTID equals z2.TARGETID into resz2
                                    from reltrail in resz2.DefaultIfEmpty()
                                    join u in context.TBL_LC_USSANCE on x.LCISSUANCEID equals u.LCISSUANCEID into usance
                                    from u in usance.DefaultIfEmpty()
                                    join ut in context.TBL_APPROVAL_TRAIL on u.LCUSSANCEID equals ut.TARGETID into ustr
                                    from left3 in ustr.DefaultIfEmpty()
                                    where 
                                    //y.RESPONSESTAFFID == null
                                    (operations.Contains(y.OPERATIONID)
                               //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
                               && (x.LCREFERENCENUMBER.Contains(searchString))
                            || c.FIRSTNAME.ToLower().Contains(searchString)
                            || c.LASTNAME.ToLower().Contains(searchString)
                            || c.MIDDLENAME.ToLower().Contains(searchString)
                            || c.CUSTOMERCODE.Contains(searchString)
                            || x.FORMMNUMBER.ToString().Contains(searchString)
                            || x.LCISSUANCEID.ToString().Contains(searchString)
                            )
                                    from final in resz2.DefaultIfEmpty() select new LcIssuanceApprovalViewModel
                                    {
                                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                        customerCode = c.CUSTOMERCODE,
                                        lcReferenceNumber = x.LCREFERENCENUMBER,
                                        lcIssuanceId = x.LCISSUANCEID,
                                        customerId = c.CUSTOMERID,
                                        operationId = y.OPERATIONID,
                                        lcReleaseAmountId = rel.LCRELEASEAMOUNTID,
                                        lcUssanceId = u.LCUSSANCEID,
                                        arrivalDate = y.ARRIVALDATE,
                                        letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                        //approvedAmount = x.APPROVEDAMOUNT,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        currentApprovalLevelId = y.TOAPPROVALLEVELID,
                                        currentApprovalLevel = ((currentApprovalLevelStatuses.Contains((int)x.APPLICATIONSTATUSID)) && y.TOAPPROVALLEVELID != null) ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == y.TOAPPROVALLEVELID).LEVELNAME : "n/a",
                                        approvalTrailId = y.APPROVALTRAILID,
                                        responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        lcApplicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        usanceStatus = (x.LCUSSANCESTATUSID == null || x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted) ? "n/a" : context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == x.LCUSSANCESTATUSID).APPLICATIONSTATUSNAME,
                                        usanceApprovalStatus = x.LCUSSANCEAPPROVALSTATUSID == null ? "n/a" : context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.LCUSSANCEAPPROVALSTATUSID).APPROVALSTATUSNAME,
                                        UsanceCurrentApprovalLevel = (left3.TOAPPROVALLEVELID != null) ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == left3.TOAPPROVALLEVELID).LEVELNAME : "n/a",

                                        //misCode = x.MISCODE,
                                        //customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        createdBy = (int)x.CREATEDBY,
                                        //operationId = x.OPERATIONID,
                                    }).GroupBy(a => a.lcReferenceNumber).Select(g => g.OrderByDescending(l => l.approvalTrailId).FirstOrDefault()).ToList();
            foreach (var app in applications)
            {
                //var releases = context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == app.lcIssuanceId).ToList();
                //foreach (var r in releases)
                //{
                //    //app.lcReleaseAmountId
                //}
                VerifyIssuanceOrReleaseApprovalLevelId(app);
            }
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

        public IEnumerable<LcIssuanceViewModel> GetLcIssuances()
        {
            var lcs = context.TBL_LC_ISSUANCE.Where(x => x.DELETED == false
                                    && (x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.lcUssanceCompleted
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.lcUssanceInProgress
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcIssuanceInProgress
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CAMInProgress)
                                    || (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceInProgress
                                    && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved))
                .Select(x => new LcIssuanceViewModel
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
                    customerName = x.TBL_CUSTOMER.FIRSTNAME + x.TBL_CUSTOMER.MIDDLENAME + x.TBL_CUSTOMER.LASTNAME,
                    proformaInvoiceId = x.PROFORMAINVOICEID,
                    availableAmount = x.AVAILABLEAMOUNT,
                    letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                    letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                    invoiceDate = x.INVOICEDATE,
                    invoiceDueDate = x.INVOICEDUEDATE,
                    lcReferenceNumber = x.LCREFERENCENUMBER,
                    dateTimeCreated = (DateTime)x.DATETIMECREATED,
                    
                })
                .ToList();
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcIssuance;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var querytest1 = (from a in context.TBL_LC_ISSUANCE
                              where
                                a.DELETED == false && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceInProgress
                                && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                              select a).ToList();

            var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
                              where
                                (b.OPERATIONID == operationId)
                                && b.APPROVALSTATEID != (int)ApprovalState.Ended
                                && b.RESPONSESTAFFID == null
                                && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                                && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                              select b).ToList();
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
                        && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                        && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
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
                                cashBuildUpReferenceNumber = (string)a.CASHBUILDUPREFERENCETYPE,
                                cashBuildUpReferenceType = (string)a.CASHBUILDUPREFERENCENUMBER,
                                percentageToCover = a.PERCENTAGETOCOVER,
                                lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                                lcToleranceValue = a.LCTOLERANCEVALUE,
                                releaseAmount = a.RELEASEDAMOUNT,
                                beneficiaryAddress = a.BENEFICIARYADDRESS,
                                beneficiaryEmail = a.BENEFICIARYEMAIL,
                                customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.MIDDLENAME + a.TBL_CUSTOMER.LASTNAME,
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
                                lastComment = b.COMMENT,
                                currentApprovalStateId = b.APPROVALSTATEID,
                                currentApprovalLevelId = b.TOAPPROVALLEVELID,
                                currentApprovalLevel = b.TBL_APPROVAL_LEVEL.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                                currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                                approvalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                                toStaffId = b.TOSTAFFID,
                                approvalStatusId = (short)a.APPROVALSTATUSID,
                                applicationStatusId = a.APPLICATIONSTATUSID,
                                createdBy = (int)a.CREATEDBY,
                                operationId = operationId,
                                dateTimeCreated = (DateTime)a.DATETIMECREATED
                            }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications.ToList();
        }

        public IEnumerable<CamProcessedLoanViewModel> GetIFFLinesForLCByCustomerId(int customerId, int companyId, int staffId, int branchId)
        {
            var lines = loanRepository.GetAvailedLoanApplicationsDueForInitiateBooking(companyId, staffId, branchId).Where
                (l => l.customerId == customerId && l.productClassId == (int)ProductClassEnum.ImportFinanceFacilities && l.customerAvailableAmount > 0).ToList();

            return lines;
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
                    lcReferenceNumber = x.LCREFERENCENUMBER,
                }).ToList();
            return lcs;
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
                //COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED =DateTime.Now
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = systemDate,
                SYSTEMDATETIME = DateTime.Now
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCISSUANCEID
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCISSUANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        #endregion LCISSUANCE

        #region RELEASEOFSHIPPINGDOCUMENTS
        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForRelease() 
        {
            //var lcsReleasesInTrail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments).Select(t => t.TARGETID);
            //var lcReleases = context.TBL_LCRELEASE_AMOUNT.Where(y => !lcsReleasesInTrail.Contains(y.LCRELEASEAMOUNTID)).ToList();
            //var lcIssuanceIds = lcReleases.Select(r => r.LCISSUANCEID).ToList();
            var releases = context.TBL_LCRELEASE_AMOUNT.ToList();
            var lcs = (from i in context.TBL_LC_ISSUANCE
                       join r in context.TBL_LCRELEASE_AMOUNT on i.LCISSUANCEID equals r.LCISSUANCEID into ir
                       from r in ir.DefaultIfEmpty()
                       join rt in context.TBL_APPROVAL_TRAIL on r.LCRELEASEAMOUNTID equals rt.TARGETID into irt
                       from t in irt.DefaultIfEmpty()
                       where
                       (
                       i.DELETED == false
                       && i.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                       && i.LCTOLERANCEVALUE > (context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == i.LCISSUANCEID).Sum(r => r.RELEASEAMOUNT) ?? 0)
                       &&
                       ((r.RELEASEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                          && r.RELEASEAPPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                       || (r.RELEASEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                       && r.RELEASEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted))
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
                           letterOfCreditTypeId = i.LETTEROFCREDITTYPEID,
                           isDraftRequired = i.ISDRAFTREQUIRED,
                           beneficiaryAddress = i.BENEFICIARYADDRESS,
                           beneficiaryEmail = i.BENEFICIARYEMAIL,
                           customerId = i.CUSTOMERID,
                           customerName = i.TBL_CUSTOMER.FIRSTNAME + i.TBL_CUSTOMER.MIDDLENAME + i.TBL_CUSTOMER.LASTNAME,
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
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForReleaseApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcReleaseOfShippingDocuments;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            //var querytest1 = (from a in context.TBL_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false
            //                    && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
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
            var query = (from a in context.TBL_LC_ISSUANCE
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
                            && c.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RELEASEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted
                            && b.RELEASEAPPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                            && c.RESPONSESTAFFID == null
                            && levelIds.Contains((int)c.TOAPPROVALLEVELID)
                            && (c.TOSTAFFID == null || c.TOSTAFFID == staffId)
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcReleaseAmountId = b.LCRELEASEAMOUNTID,
                             releaseAmount = (decimal)b.RELEASEAMOUNT,
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
                             lastComment = c.COMMENT,
                             currentApprovalStateId = c.APPROVALSTATEID,
                             currentApprovalLevelId = c.TOAPPROVALLEVELID,
                             currentApprovalLevel = c.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = c.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             approvalTrailId = c == null ? 0 : c.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = c.TOSTAFFID,
                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.MIDDLENAME + a.TBL_CUSTOMER.LASTNAME,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED
                         }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications.ToList();
        }

        public LcReleaseAmountViewModel AddLCReleaseAmount(LcReleaseAmountViewModel entity)
        {
            ValidateReleaseAmount(entity);
            context.TBL_LCRELEASE_AMOUNT.Add( new TBL_LCRELEASE_AMOUNT
            {
                LCISSUANCEID = entity.lcIssuanceId,
                RELEASEAMOUNT = entity.releaseAmount,
                DATETIMECREATED = DateTime.Now
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = systemDate,
                SYSTEMDATETIME = DateTime.Now
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------
            context.SaveChanges();
            var newLC = context.TBL_LCRELEASE_AMOUNT.Where(l => l.LCISSUANCEID == entity.lcIssuanceId && l.RELEASEAMOUNT == entity.releaseAmount).OrderByDescending(l => l.LCRELEASEAMOUNTID).FirstOrDefault();
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = systemDate,
                SYSTEMDATETIME = DateTime.Now
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
            var approvedReleaseIds = context.TBL_APPROVAL_TRAIL.Where(t => t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved 
                                                                        && t.OPERATIONID == (int)OperationsEnum.lcReleaseOfShippingDocuments).Select(t => t.TARGETID).ToList();
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var currCode = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == lc.CURRENCYID).CURRENCYCODE;
            var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => approvedReleaseIds.Contains(r.LCRELEASEAMOUNTID) && r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT) ?? 0;
            var availableAmount = lc.LCTOLERANCEVALUE - totalReleasedAmount;
            if (model.releaseAmount > availableAmount)
            {
                throw new SecureException("Release Amount cannot be greater than remainder tolerance amount " + currCode + " " + availableAmount);
            }
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

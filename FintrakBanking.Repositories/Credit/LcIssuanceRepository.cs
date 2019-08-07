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

                searchString = searchString.Trim().ToLower();

                var applications = (from x in context.TBL_LC_ISSUANCE
                                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                    join y in context.TBL_APPROVAL_TRAIL on x.LCISSUANCEID equals y.TARGETID
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
                            )
                                    //|| x.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                    select new LcIssuanceApprovalViewModel
                                    {
                                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                        customerCode = c.CUSTOMERCODE,
                                        lcReferenceNumber = x.LCREFERENCENUMBER,
                                        lcIssuanceId = x.LCISSUANCEID,
                                        customerId = c.CUSTOMERID,
                                        operationId = y.OPERATIONID,
                                        //branchId = c.BRANCHID,
                                        //relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                        //relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                        arrivalDate = y.ARRIVALDATE,
                                        letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                        //approvedAmount = x.APPROVEDAMOUNT,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        currentApprovalLevelId = y.TOAPPROVALLEVELID,
                                        currentApprovalLevel = y.TOAPPROVALLEVELID != null ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == y.TOAPPROVALLEVELID).LEVELNAME : "n/a",
                                        approvalTrailId = y.APPROVALTRAILID,
                                        responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        //relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                        //relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                        //misCode = x.MISCODE,
                                        //customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        //loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                        createdBy = (int)x.CREATEDBY,
                                        //operationId = x.OPERATIONID,
                                        // accountNumber = ca.PRODUCTACCOUNTNUMBER,
                                        //isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == x.APPLICATIONREFERENCENUMBER).Any()
                                    }).OrderByDescending(l => l.approvalTrailId);
                foreach (var app in applications)
                {
                    app.applicationStatusId = VerifyApplicationStatus(app.lcIssuanceId, app.lcIssuanceId);
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
                    cashBuildUpReferenceNumber = (string)x.CASHBUILDUPREFERENCETYPE,
                    cashBuildUpReferenceType = (string)x.CASHBUILDUPREFERENCENUMBER,
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
                    formNumber = x.FORMMNUMBER,
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
                                beneficiaryAddress = a.BENEFICIARYADDRESS,
                                beneficiaryEmail = a.BENEFICIARYEMAIL,
                                customerId = a.CUSTOMERID,
                                fundSourceId = a.FUNDSOURCEID,
                                fundSourceDetails = a.FUNDSOURCEDETAILS,
                                formNumber = a.FORMMNUMBER,
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
                    formNumber = x.FORMMNUMBER,
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
            
            
            //var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
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
                FORMMNUMBER = model.formNumber,
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
                //model.lcReferenceNumber = createdlcRecord.LCREFERENCENUMBER;
            }
           
            return model;
        }

        public bool UpdateLcIssuance(LcIssuanceViewModel model, int id, UserInfo user)
        {

            ValidateAmounts(model);

            var entity = this.context.TBL_LC_ISSUANCE.Find(id);
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
            entity.FORMMNUMBER = model.formNumber;
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
        public IEnumerable<LcIssuanceViewModel> GetLcIssuancesForRelease()
        {
            var lcs = context.TBL_LC_ISSUANCE.Where(x => x.DELETED == false
                                    && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                    || (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                                        && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved))
                .Select(x => new LcIssuanceViewModel
                {
                    lcIssuanceId = x.LCISSUANCEID,
                    beneficiaryName = x.BENEFICIARYNAME,
                    totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                    totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                    availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                    cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                    cashBuildUpReferenceNumber = (string)x.CASHBUILDUPREFERENCETYPE,
                    cashBuildUpReferenceType = (string)x.CASHBUILDUPREFERENCENUMBER,
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
                    formNumber = x.FORMMNUMBER,
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
                    dateTimeCreated = (DateTime)x.DATETIMECREATED,
                })
                .ToList();
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
            //                    && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
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
                            && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
                            && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                         orderby a.LCISSUANCEID
                         join b in context.TBL_LCRELEASE_AMOUNT on a.LCISSUANCEID equals b.LCISSUANCEID
                         join c in context.TBL_APPROVAL_TRAIL on b.LCRELEASEAMOUNTID equals c.TARGETID
                         where
                            (
                            (c.OPERATIONID == operationId)
                            && c.APPROVALSTATEID != (int)ApprovalState.Ended
                            && c.RESPONSESTAFFID == null
                            && levelIds.Contains((int)c.TOAPPROVALLEVELID)
                            && (c.TOSTAFFID == null || c.TOSTAFFID == staffId)
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcReleaseAmountId = b.LCRELEASEAMOUNTID,
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
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formNumber = a.FORMMNUMBER,
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
                             //customerName = context.TBL_CUSTOMER.Find(a.CUSTOMERID).FIRSTNAME + context.TBL_CUSTOMER.Find(a.CUSTOMERID).LASTNAME,
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
            var lcs = context.TBL_LCRELEASE_AMOUNT.ToList();
            var lc = new TBL_LCRELEASE_AMOUNT
            {
                LCISSUANCEID = entity.lcIssuanceId,
                RELEASEAMOUNT = entity.releaseAmount
            };
            lcs.Add(lc);

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
            var newLC = context.TBL_LCRELEASE_AMOUNT.Where(l => l.LCISSUANCEID == entity.lcIssuanceId && l.RELEASEAMOUNT == entity.releaseAmount).OrderBy(l => l.LCRELEASEAMOUNTID).LastOrDefault();
            entity.lcReleaseAmountId = newLC.LCRELEASEAMOUNTID;
            return entity;
        }

        public LcReleaseAmountViewModel UpdateLCReleaseAmount(LcReleaseAmountViewModel entity)
        {
            ValidateReleaseAmount(entity);
            var lc = context.TBL_LCRELEASE_AMOUNT.FirstOrDefault(l => l.LCRELEASEAMOUNTID == entity.lcReleaseAmountId);

            //lc.LCISSUANCEID = entity.lcIssuanceId;
            lc.RELEASEAMOUNT = entity.releaseAmount;

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
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT);
            var availableAmount = lc.LCTOLERANCEVALUE - totalReleasedAmount;
            if (model.releaseAmount > availableAmount)
            {
                throw new SecureException("Release Amount cannot be greater than remainder tolerance amount" + availableAmount);
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

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FintrakBanking.Repositories.Credit
{
    public class OfferLetterAndAvailmentRepository : IOfferLetterAndAvailmentRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workflow;

        //private IApprovalLevelStaffRepository approvalLevel;
        //private ILoanRepository loans;

        public OfferLetterAndAvailmentRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            //IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workflow
            //ILoanRepository _loans  
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            //approvalLevel = _approvallevel;
            workflow = _workflow;
            //loans = _loans;
        }

        #region OfferLetter & Availment Process
                public bool UpdateLoadDetails(int applicationId, ApprovedLoanDetailViewModel model)
        {
            bool output = false;
            var LoanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == applicationId).FirstOrDefault();
            LoanDetails.SECUREDBYCOLLATERAL = model.securedByCollateral;
            LoanDetails.CRMSCOLLATERALTYPEID = model.crmsCollateralTypeId;
            LoanDetails.ISSPECIALISED = model.isSpecialised;

            var auditRec = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffReliefUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Record Added For CRMS Collateral On Loan Detail '{model.applicationId}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.applicationId
            };


                try
                {


                    this.auditTrail.AddAuditTrail(auditRec);
                    //end of Audit section -------------------------------


                    output = context.SaveChanges() > 0;


                    if (output)
                    {
                        return output;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }

        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int companyId) // Control Generation
        {
            var exceptIds = context.TBL_LOAN_RATE_FEE_CONCESSION
                    .Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    .Select(x => (int)x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID).ToList();

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OfferLetterApproval).ToList();
            IQueryable<CamProcessedLoanViewModel> data = null;

            data = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
                    && ids.Contains((int)x.TOAPPROVALLEVELID)
                    && x.RESPONSESTAFFID == null
                    && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised
                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                        )
                    ),
                    c => c.b.LOANAPPLICATIONID, d => d.TARGETID, (c, d) => new { c, d })
                .Select(x => new CamProcessedLoanViewModel
                {
                    loanApplicationId = x.c.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.c.a.APPLICATIONREFERENCENUMBER,
                    customerCode = x.c.a.TBL_CUSTOMER.CUSTOMERCODE,
                    //customerName = x.c.a.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME : x.c.a.TBL_CUSTOMER.FIRSTNAME + " " + x.c.a.TBL_CUSTOMER.MIDDLENAME + " " + x.c.a.TBL_CUSTOMER.LASTNAME,
                    customerName = x.c.b.TBL_CUSTOMER.FIRSTNAME + " " + x.c.b.TBL_CUSTOMER.MIDDLENAME + " " + x.c.b.TBL_CUSTOMER.LASTNAME,
                    customerId = x.c.a.CUSTOMERID,
                    customerGroupName = x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME,
                    customerGroupCode = x.c.a.TBL_CUSTOMER_GROUP.GROUPCODE,
                    relationshipOfficerId = x.c.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.c.a.RELATIONSHIPMANAGERID,

                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.a.APPROVEDAMOUNT,
                    interestRate = x.c.a.INTERESTRATE,
                    applicationTenor = x.c.a.APPLICATIONTENOR,
                    relationshipOfficerName = x.c.a.TBL_STAFF.FIRSTNAME + " " + x.c.a.TBL_STAFF.MIDDLENAME + " " + x.c.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.c.a.TBL_STAFF1.FIRSTNAME + " " + x.c.a.TBL_STAFF1.MIDDLENAME + " " + x.c.a.TBL_STAFF1.LASTNAME,

                    loanTypeId = x.c.a.LOANAPPLICATIONTYPEID,
                    productTypeId = x.c.b.TBL_PRODUCT.PRODUCTTYPEID,
                    productTypeName = x.c.b.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                    productName = x.c.b.TBL_PRODUCT.PRODUCTNAME,
                    loanTypeName = x.c.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                    //camReference = c.CAMREF != null ? c.CAMREF : "N/A",
                    //camDocumentation = d.CAMDOCUMENTATION,
                    approvalDate = x.c.a.APPROVEDDATE,
                    applicationStatusId = x.c.a.APPLICATIONSTATUSID,
                    subSectorId = x.c.b.TBL_SUB_SECTOR.SUBSECTORID,
                    //approvalLevelId = staffApprovalLevelId,
                    operationId = (int)OperationsEnum.LoanAvailment,
                    currentApprovalStateId = x.d.APPROVALSTATEID,
                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    undergoingConcession = exceptIds.Contains(x.c.a.LOANAPPLICATIONID),
                    productPriceIndex = x.c.b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == x.c.b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",

                });

            data = data.Where(x =>
                x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationCompleted
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.ApplicationUnderReview
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.CAMCompleted
                )
                .GroupBy(c => c.loanApplicationId)
                .Select(y => y.FirstOrDefault())
                .OrderByDescending(c => c.loanApplicationId)
                ;
            //var testList = data.ToList();
            //var testCount = data.Count();

            return data;
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int branchId, int companyId) // RM Review
        {
            var exceptIds = context.TBL_LOAN_RATE_FEE_CONCESSION
                    .Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    .Select(x => (int)x.TBL_LOAN_APPLICATION_DETAIL.PROPOSEDPRODUCTID).ToList();

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OfferLetterApproval).ToList();
            IQueryable<CamProcessedLoanViewModel> data = null;

            data = context.TBL_LOAN_APPLICATION.Where(x => x.BRANCHID == branchId && !exceptIds.Contains(x.LOANAPPLICATIONID) && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
                    && ids.Contains((int)x.TOAPPROVALLEVELID)
                    && x.RESPONSESTAFFID == null
                    && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised)),
                    c => c.b.LOANAPPLICATIONID, d => d.TARGETID, (c, d) => new { c, d })
                .Select(x => new CamProcessedLoanViewModel
                {
                    loanApplicationId = x.c.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.c.a.APPLICATIONREFERENCENUMBER,
                    customerCode = x.c.a.TBL_CUSTOMER.CUSTOMERCODE,
                    customerName = x.c.a.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME : x.c.a.TBL_CUSTOMER.FIRSTNAME + " " + x.c.a.TBL_CUSTOMER.MIDDLENAME + " " + x.c.a.TBL_CUSTOMER.LASTNAME,
                    customerId = x.c.a.CUSTOMERID,
                    customerGroupName = x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME,
                    customerGroupCode = x.c.a.TBL_CUSTOMER_GROUP.GROUPCODE,
                    relationshipOfficerId = x.c.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.c.a.RELATIONSHIPMANAGERID,

                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.a.APPROVEDAMOUNT,
                    interestRate = x.c.a.INTERESTRATE,
                    applicationTenor = x.c.a.APPLICATIONTENOR,
                    relationshipOfficerName = x.c.a.TBL_STAFF.FIRSTNAME + " " + x.c.a.TBL_STAFF.MIDDLENAME + " " + x.c.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.c.a.TBL_STAFF1.FIRSTNAME + " " + x.c.a.TBL_STAFF1.MIDDLENAME + " " + x.c.a.TBL_STAFF1.LASTNAME,

                    loanTypeId = x.c.a.LOANAPPLICATIONTYPEID,
                    productTypeId = x.c.b.TBL_PRODUCT.PRODUCTTYPEID,
                    productTypeName = x.c.b.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                    productId = x.c.b.APPROVEDPRODUCTID,
                    productName = x.c.b.TBL_PRODUCT.PRODUCTNAME,
                    loanTypeName = x.c.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                    //camReference = c.CAMREF != null ? c.CAMREF : "N/A",
                    //camDocumentation = d.CAMDOCUMENTATION,
                    approvalDate = x.c.a.APPROVEDDATE,
                    applicationStatusId = x.c.a.APPLICATIONSTATUSID,
                    subSectorId = x.c.b.TBL_SUB_SECTOR.SUBSECTORID,
                    //approvalLevelId = staffApprovalLevelId,
                    operationId = (int)OperationsEnum.LoanAvailment,
                    currentApprovalStateId = x.d.APPROVALSTATEID,
                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    isFinal = context.TBL_OFFERLETTER.Where(o => o.APPLICATIONREFERENCENUMBER == x.c.a.APPLICATIONREFERENCENUMBER).Select(o => o.ISFINAL).FirstOrDefault(),
                    productPriceIndex = x.c.b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == x.c.b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                });

            data = data.Where(x =>
                x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationCompleted
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.ApplicationUnderReview
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.CAMCompleted

                )
                .GroupBy(c => c.loanApplicationId)
                .Select(y => y.FirstOrDefault())
                .OrderByDescending(c => c.loanApplicationId)
                ;
            //var testList = data.ToList();
            //var testCount = data.Count();

            return data;
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanAvailment).ToList();

            var data = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.LoanAvailment
                    && x.RESPONSESTAFFID == null
                    && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing ||
                        x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised ||
                        x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                    && ids.Contains((int)x.TOAPPROVALLEVELID)
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                ),
                    c => c.b.LOANAPPLICATIONID, d => d.TARGETID, (c, d) => new { c, d })
                .Select(x => new CamProcessedLoanViewModel
                {
                    loanApplicationId = x.c.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.c.a.APPLICATIONREFERENCENUMBER,
                    customerCode = x.c.a.TBL_CUSTOMER.CUSTOMERCODE,
                    customerName = x.c.a.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME : x.c.a.TBL_CUSTOMER.FIRSTNAME + " " + x.c.a.TBL_CUSTOMER.MIDDLENAME + " " + x.c.a.TBL_CUSTOMER.LASTNAME,
                    customerId = x.c.a.CUSTOMERID,
                    customerGroupName = x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME,
                    customerGroupCode = x.c.a.TBL_CUSTOMER_GROUP.GROUPCODE,
                    relationshipOfficerId = x.c.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.c.a.RELATIONSHIPMANAGERID,

                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.a.APPROVEDAMOUNT,
                    interestRate = x.c.a.INTERESTRATE,
                    applicationTenor = x.c.a.APPLICATIONTENOR,
                    relationshipOfficerName = x.c.a.TBL_STAFF.FIRSTNAME + " " + x.c.a.TBL_STAFF.MIDDLENAME + " " + x.c.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.c.a.TBL_STAFF1.FIRSTNAME + " " + x.c.a.TBL_STAFF1.MIDDLENAME + " " + x.c.a.TBL_STAFF1.LASTNAME,

                    loanTypeId = x.c.a.LOANAPPLICATIONTYPEID,
                    productTypeId = x.c.b.TBL_PRODUCT.PRODUCTTYPEID,
                    productTypeName = x.c.b.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                    productId = x.c.b.APPROVEDPRODUCTID,
                    productName = x.c.b.TBL_PRODUCT.PRODUCTNAME,
                    loanTypeName = x.c.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                    //camReference = c.CAMREF != null ? c.CAMREF : "N/A",
                    //camDocumentation = d.CAMDOCUMENTATION,
                    approvalDate = x.c.a.APPROVEDDATE,
                    applicationStatusId = x.c.a.APPLICATIONSTATUSID,
                    isInvestmentGrade = x.c.a.ISINVESTMENTGRADE,
                    isPoliticallyExposed = x.c.a.ISPOLITICALLYEXPOSED,
                    isRelatedParty = x.c.a.ISRELATEDPARTY,
                    submittedForAppraisal = x.c.a.SUBMITTEDFORAPPRAISAL,
                    loanInformation = x.c.a.LOANINFORMATION,
                    approvalStatusId = x.d.APPROVALSTATUSID,
                    subSectorId = x.c.b.TBL_SUB_SECTOR.SUBSECTORID,
                    //approvalLevelId = staffApprovalLevelId,
                    operationId = (short)OperationsEnum.LoanAvailment,

                    currentApprovalStateId = x.d.APPROVALSTATEID,
                    approvalTrailId = x.d.APPROVALTRAILID,
                    currentApprovalLevelId = x.d.TOAPPROVALLEVELID,
                    currentApprovalLevel = x.d.TBL_APPROVAL_LEVEL1.LEVELNAME,

                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    atInitiator = x.c.a.CREATEDBY == staffId,
                    productPriceIndex = x.c.b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == x.c.b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                    loanApplicationCollateral = (from r in context.TBL_LOAN_APPLICATION_COLLATERL.Where(s => s.LOANAPPLICATIONID == x.c.a.LOANAPPLICATIONID)
                                                 select new LoanApplicationCollateralViewModel
                                                 {
                                                     collateralValue = r.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                     collateralType = r.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                                     collateralCustomerId = r.COLLATERALCUSTOMERID,
                                                     collateralSubtype = r.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB
                                                     .Where(p => p.COLLATERALSUBTYPEID == r.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                                                     collateralReferenceNumber = r.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                                                     haircut = r.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                     valuationCycle = r.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                     allowSharing = r.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                                                     currencyCode = r.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE
                                                 }).ToList()
                })
                .OrderByDescending(c => c.loanApplicationId);

            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentCheckList(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_LOAN_CONDITION_PRECEDENT on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID

                        where a.COMPANYID == companyId && a.DELETED == false
                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                               (a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                               || a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted)
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANAPPLICATIONTYPEID,
                            productTypeId = b.TBL_PRODUCT.PRODUCTTYPEID,
                            productName = b.TBL_PRODUCT.PRODUCTNAME,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            newApplicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                        });

            return data.GroupBy(x => x.loanApplicationId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanApplicationId).ToList();
        }

        public Form3800ViewModel GenerateForm3800Template(string applicationRefNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            var currentDate = DateTime.Now;

            var targetAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted);

            if (targetAppl.PRODUCTCLASSID == null)
            {
                targetAppl.PRODUCTCLASSID = 1;
            }

            var productClassProcess = context.TBL_PRODUCT_CLASS.FirstOrDefault(x => x.PRODUCTCLASSID == targetAppl.PRODUCTCLASSID);

            var templateLink = GetProductSpecificTemplate(productClassProcess.PRODUCT_CLASS_PROCESSID, (short?)targetAppl.PRODUCTCLASSID ?? 1);

            //var conditionPrecedentsSub = (from a in context.TBL_LOAN_APPLICATION
            //                           join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
            //                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
            //                           where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
            //                           && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
            //                           && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
            //                           && (b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived || b.CHECKLISTSTATUSID == null)
            //                           && b.ISSUBSEQUENT == false
            //                           select new OfferLetterConditionPrecidentViewModel()
            //                           {
            //                               conditionPrecident = b.CONDITION,
            //                               loanApplicationId = a.LOANAPPLICATIONID,
            //                               isExternal = b.ISEXTERNAL,
            //                               productName = c.TBL_PRODUCT.PRODUCTNAME
            //                           }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionPrecedentsSub = (from a in context.TBL_LOAN_APPLICATION
                                          join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                          join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                          && (b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived || b.CHECKLISTSTATUSID == null)
                                          && b.ISSUBSEQUENT == false
                                          select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionId = b.CONDITIONID,
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = a.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).Distinct().ToList();

            var conditionPrecedents = conditionPrecedentsSub;

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                        && b.ISSUBSEQUENT == true
                                        select new OfferLetterConditionPrecidentViewModel()
                                        {
                                            conditionPrecident = b.CONDITION,
                                            loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                            isExternal = b.ISEXTERNAL,
                                            productName = c.TBL_PRODUCT.PRODUCTNAME
                                        }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var products = (from a in context.TBL_LOAN_APPLICATION
                            join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                            where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            select new ProductViewModel()
                            {
                                productId = c.TBL_PRODUCT.PRODUCTID,
                                productName = c.TBL_PRODUCT.PRODUCTNAME,
                                productClassId = a.PRODUCTCLASSID,
                                productClassProcessId = productClassProcess.PRODUCT_CLASS_PROCESSID //a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID
                            }).ToList();


            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        where d.APPLICATIONREFERENCENUMBER == applicationRefNumber
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                         && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new ProductFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE
                        }).ToList();

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CURRENCY on b.CURRENCYID equals e.CURRENCYID

                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                               && b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   purpose = b.LOANPURPOSE,
                                   approvedAmountCurrency = e.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   productPriceIndex = b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                                   approvedDate = a.APPROVEDDATE,
                                   newApplicationDate = a.APPLICATIONDATE,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER
                                   //approvedAmount = b.APPROVEDAMOUNT
                               }).ToList();

            var transactionDynamicsDetails = (from a in context.TBL_LOAN_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where c.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                              && c.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                              && c.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  dynamics = a.DYNAMICS,
                                              }).Distinct().ToList();

            // var transactionDynamicsDetails = transactionDynamic.Select(x => x.dynamics).Distinct();



            var loanCollaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                                   join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                   where y.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                                       facilityAmount = y.APPROVEDAMOUNT
                                   }).ToList();


            var loanMonitoringTriggers = (from x in context.TBL_LOAN_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals y.LOANAPPLICATIONDETAILID
                                          join z in context.TBL_LOAN_APPLICATION on y.LOANAPPLICATIONID equals z.LOANAPPLICATIONID
                                          where z.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          && z.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                          && z.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                          select new MonitoringTriggersViewModel()
                                          {
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                          }).Distinct().ToList();

            //var loanMonitoringTriggers = monitoringTriggers.Select(x => x.monitoringTrigger).Distinct();


            var loanComments = (from x in context.TBL_LOAN_APPLICATION_COMMENT
                                join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                where y.APPLICATIONREFERENCENUMBER == applicationRefNumber && x.OPERATIONID == (int)CommentsTypeEnum.LOS
                                select new LoanApplicationCommentViewModel()
                                {
                                    comments = x.COMMENTS,
                                }).ToList();


            var conditions = string.Empty;

            var fee = string.Empty;

            var loanDetail = string.Empty;

            var loanCollateral = string.Empty;

            var loanComment = string.Empty;

            var loanMonitoringTrigger = string.Empty;

            var loanTransactionDynamics = string.Empty;

            var internalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == false).ToList();

            var externalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == true).ToList();

            var internalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == false).ToList();

            var externalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == true).ToList();

            int noOfInternalConditions = 0;

            int noOfExternalConditions = 0;

            var finalConditionPrecedents = string.Empty;

            var finalConditionSubsequents = string.Empty;

            var loanfee = string.Empty;

            var detail = string.Empty;

            var collateral = string.Empty;

            var comment = string.Empty;

            var monitoringTrigger = string.Empty;

            var transactionDynamics = string.Empty;

            int noOfDetails = 0;

            int noOfFees = 0;

            int noOfCollaterals = 0;

            int noOfComments = 0;

            int noOfMonitoringTriggers = 0;

            int noOfTransactionDynamics = 0;

            foreach (var prod in products)
            {
                var productExternalConditions = externalConditionsPrecedents.Where(x => x.productName == prod.productName);

                conditions = $"<p><strong> Conditions Precedent(to be satisfied before drawdown) {prod.productName}</strong></p>";

                conditions = conditions +
                        $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                        $"<tr>" +
                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<p> &nbsp;</p><p><strong> S/No </strong></p></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:225.05pt'><p> &nbsp;</p>" +

                        $"<strong> Conditions Precedent </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:100.05pt'><p> &nbsp;</p>" +

                        $"<strong> Applicable Facility </strong ></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:1.0in'>" +

                        $"<strong> *Credit Verification Officer&rsquo; s initial for compliance only</strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:67.5pt'>" +

                        $"<strong> Location of document </strong><strong><em> (Corporate workflow)</em ></strong></td></tr>";

                foreach (var item in productExternalConditions)
                {
                    conditions = conditions +
                        $"<tr>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.conditionPrecident}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'><p>{prod.productName}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'><p> &nbsp;</p></td>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:67.5pt'><p>&nbsp;</p></td>" +
                        $"</tr>";
                }

                noOfExternalConditions = 0;

                conditions = conditions +
                    "<tr class='removeConditions_OL'><td colspan='5' style='height:18.4pt; vertical-align:top; width:490.5pt'>" +
                    "<p><strong> Other Conditions Precedent for Internal usage which does not have to be included in the offer " +
                    "letter.The RM must ensure compliance with these conditions before drawdown.</strong></p></td></tr> ";

                var productInternalConditions = internalConditionsPrecedents.Where(x => x.productName == prod.productName);

                foreach (var item in productInternalConditions)
                {
                    conditions = conditions +
                        $"<tr class='removeConditions_OL'>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfInternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.conditionPrecident}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'><p>{prod.productName}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'><p> &nbsp;</p></td>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:67.5pt'><p>&nbsp;</p></td>" +
                        $"</tr>";
                }

                noOfInternalConditions = 0;

                conditions = conditions + "</tbody></table><p> &nbsp;</p>";

                finalConditionPrecedents += conditions;
            }

            foreach (var prod in products)
            {
                var productExternalConditions = externalConditionsSubsequents.Where(x => x.productName == prod.productName);

                conditions = $"<p><strong>Conditions Subsequent (to be satisfied after drawdown) {prod.productName}</strong></p>";

                conditions = conditions +
                        $"<table border='1' cellpadding='5' cellspacing='2'><tbody>" +
                        $"<tr>" +
                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                        $"<strong> S/No </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:225.05pt'><p> &nbsp;</p>" +

                        $"<strong> Conditions Subsequent </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:100.05pt'><p> &nbsp;</p>" +

                        $"<strong> Timeline for compliance </strong ></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:1.0in'><p> &nbsp;</p>" +

                        $"<strong> Credit Monitoring Officer’s initial for compliance only</strong></td>" +

                        $"</tr>";

                foreach (var item in productExternalConditions)
                {
                    conditions = conditions +
                        $"<tr>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'>{item.conditionPrecident}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'>&nbsp</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'> &nbsp;</td>" +
                        $"</tr>";
                }

                noOfExternalConditions = 0;

                conditions = conditions +
                    "<tr class='removeConditions_OL'><td colspan='5' style='height:18.4pt; vertical-align:top; width:490.5pt'>" +
                    "<strong> Other Conditions Subsequent for Internal usage which does not have to be included in the offer " +
                    "letter.The RM must ensure compliance with these conditions after drawdown.</strong></td></tr> ";

                var productInternalConditions = internalConditionsSubsequents.Where(x => x.productName == prod.productName);

                foreach (var item in productInternalConditions)
                {
                    conditions = conditions +
                        $"<tr class='removeConditions_OL'>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfInternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'>{item.conditionPrecident}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'>{prod.productName}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'> &nbsp;</td>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:67.5pt'>&nbsp;</td>" +
                        $"</tr>";
                }

                noOfInternalConditions = 0;

                conditions = conditions + "</tbody></table><p> &nbsp;</p>";

                finalConditionSubsequents += conditions;
            }

            fee = $"<br/>" + $"<br/>" + $"<br/>" + $"<br/>" + $"<br/>" + $"<br/>" + $"<br/>" + $"<br/>" +
                $"<p><strong> Fee Details: </strong></p>";

            fee = fee +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Name </strong></p></td>" +

                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Rate </strong></td></tr>";



            foreach (var item in fees)
            {
                fee = fee +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.feeName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.rateValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"</tr>";
            }

            noOfFees = 0;

            fee = fee + "</tbody></table><p> &nbsp;</p>";

            loanfee += fee;

            var feeData = $"{loanfee}";



            loanDetail = $" ";//<p><strong> Facility Details: </strong></p>

            loanDetail = loanDetail +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Facility Type </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Purpose </strong></td>" +
                     $"<td style='height:29.65pt; vertical-align:top; width:119.8pt'><p> &nbsp;</p>" +
                    $"<strong> Limits N </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:49.51n'><p> &nbsp;</p>" +
                    $"<strong> Tenor </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:119.8pt'><p> &nbsp;</p>" +
                    $"<strong> Interest/Margin </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Review Date </strong></td></tr>";



            foreach (var item in loanDetails)
            {
                loanDetail = loanDetail +
                    $"<tr>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.purpose}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.approvedAmountCurrency}</p> % p.a </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.tenor}</p> Days </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.interestRate}</p> % p.a </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.applicationDate.ToString("dd/MM/yyyy")}</p></td>" +
                    $"</tr>";
            }

            noOfDetails = 0;

            loanDetail = loanDetail + "</tbody></table><p> &nbsp;</p>";

            detail += loanDetail;

            var loanDetailData = $"{detail}";


            loanCollateral = $" ";//<p><strong> Collateral: </strong></p>

            loanCollateral = loanCollateral +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Type and description of security </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Value(<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Facility Amount (<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Amount Stamped To Cover (<s>N</s>) </strong></td></tr>";

            foreach (var item in loanCollaterals)
            {
                loanCollateral = loanCollateral +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfCollaterals}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralDetail}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.stapedToCoverAmount.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.facilityAmount.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"</tr>";
            }

            noOfCollaterals = 0;

            loanCollateral = loanCollateral + "</tbody></table><p> &nbsp;</p>";

            collateral += loanCollateral;

            var loanCollateralData = $"{collateral}";



            ////comments

            loanComment = $" ";//<p><strong> Collateral: </strong></p>

            loanComment = loanComment +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Comments </strong></p></td></tr>";

            foreach (var item in loanComments)
            {
                loanComment = loanComment +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfComments}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.comments}</p></td>" +
                    $"</tr>";
            }

            noOfComments = 0;

            loanComment = loanComment + "</tbody></table><p> &nbsp;</p>";

            comment += loanComment;

            var loanCommentData = $"{comment}";

            ////comments end



            loanMonitoringTrigger = $"<p><strong> Monitoring Triggers: </strong></p>";

            loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong>  </strong></td></tr>";



            foreach (var item in loanMonitoringTriggers)
            {
                loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.monitoringTrigger}</p></td>" +
                    $"</tr>";
            }

            noOfMonitoringTriggers = 0;

            loanMonitoringTrigger = loanMonitoringTrigger + "</tbody></table><p> &nbsp;</p>";

            monitoringTrigger += loanMonitoringTrigger;

            var monitoringTriggerData = $"{monitoringTrigger}";

            // transactionDynamicsDetails

            loanTransactionDynamics = $"<p><strong> Transaction Dynamics: </strong></p>";

            loanTransactionDynamics = loanTransactionDynamics +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                    $"<strong> Dynamics </strong></p></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                    $"<strong> Credit Verification Officer’s initial for compliance only </strong></p></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong>  </strong></td></tr>";



            foreach (var item in transactionDynamicsDetails)
            {
                loanTransactionDynamics = loanTransactionDynamics +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.dynamics}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p></p></td>" +
                    $"</tr>";
            }

            noOfTransactionDynamics = 0;

            loanTransactionDynamics = loanTransactionDynamics + "</tbody></table><p> &nbsp;</p>";

            transactionDynamics += loanTransactionDynamics;

            var transactionDynamicsData = $"{transactionDynamics}";
            var customer = "";
            var conditionPrecedentData = $"{finalConditionPrecedents} {finalConditionSubsequents}";
            var customerExist = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).CUSTOMERID;
            if (customerExist != null)
            {
                customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_CUSTOMER.FIRSTNAME;
            }
            else
            {
                customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_CUSTOMER_GROUP.GROUPNAME;
            }

            var branch = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_BRANCH.BRANCHNAME;
            //var info = data;

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData, templateLink, branch, customer, feeData, loanDetailData, currentDate, loanCollateralData, monitoringTriggerData, transactionDynamicsData, loanCommentData);

            if (preparedTemplate != null)
            {
                return new Form3800ViewModel { documentTemplate = preparedTemplate };
            }

            return new Form3800ViewModel { };
        }

        public Form3800ViewModel GenerateForm3800TemplateLMS(string applicationRefNumber)
        {


            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            var currentDate = DateTime.Now;

            var targetAppl = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber);

            var templateLink = GetProductSpecificTemplate(1, 1);


            //var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            //var currentDate = DateTime.Now;
            //var loanApplId = 0;
            //var facility = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == refNumber);
            //if (facility == null)
            //{
            //    var facility1 = context.TBL_LOAN_REVOLVING.Where(x => x.LOANREFERENCENUMBER == refNumber);
            //    loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility1.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;
            //}

            //loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;

            //var targetAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == loanApplId);

            //if (targetAppl.PRODUCTCLASSID == null)
            //{
            //    targetAppl.PRODUCTCLASSID = 1;
            //}

            //var productClassProcess = context.TBL_PRODUCT_CLASS.FirstOrDefault(x => x.PRODUCTCLASSID == targetAppl.PRODUCTCLASSID);

            //var templateLink = GetProductSpecificTemplate(productClassProcess.PRODUCT_CLASS_PROCESSID, (short?)targetAppl.PRODUCTCLASSID ?? 1);


            var conditionPrecedents = (from a in context.TBL_LMSR_APPLICATION
                                       join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER && b.ISSUBSEQUENT == false
                                        && b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                          //&& b.CHECKLISTSTATUSID == null
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionSubsequents = (from a in context.TBL_LMSR_APPLICATION
                                        join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER && b.ISSUBSEQUENT == true
                                        select new OfferLetterConditionPrecidentViewModel()
                                        {
                                            conditionPrecident = b.CONDITION,
                                            loanApplicationId = b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                            isExternal = b.ISEXTERNAL,
                                            productName = c.TBL_PRODUCT.PRODUCTNAME
                                        }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var products = (from a in context.TBL_LMSR_APPLICATION
                            join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                            where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                            select new ProductViewModel()
                            {
                                productId = c.TBL_PRODUCT.PRODUCTID,
                                productName = c.TBL_PRODUCT.PRODUCTNAME,
                                productClassId = c.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,//a.PRODUCTCLASSID,
                                //productClassProcessId = productClassProcess.PRODUCT_CLASS_PROCESSID //a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID
                            }).ToList();


            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        where d.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        select new ProductFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE
                        }).ToList();

            var loanDetails = (from a in context.TBL_LMSR_APPLICATION
                               join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join e in context.TBL_LOAN on b.LOANID equals e.TERMLOANID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                               into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                               into cg
                               from d in cg.DefaultIfEmpty()
                               join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                   interestRate = e.INTERESTRATE,
                                   purpose = b.REVIEWDETAILS,
                                   applicationDate = applDate,
                                   approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   //approvedAmount = b.APPROVEDAMOUNT
                                   approvedDate = a.APPROVEDDATE,
                                   newApplicationDate =a.APPLICATIONDATE

                               }).ToList();

            var transactionDynamicsDetails = (from a in context.TBL_LMSR_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where b.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  dynamics = a.DYNAMICS,
                                              }).Distinct().ToList();

            //var transactionDynamicsDetails = transactionDynamic.Select(x => x.dynamics).Distinct();
            //transactionDynamic.Select(x => x.dynamics).Distinct();

            var loanCollaterals = (from x in context.TBL_LMSR_APPLICATION_COLLATRL2
                                   join y in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANREVIEWAPPLICATIONID equals y.LOANREVIEWAPPLICATIONID
                                   where y.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                                       facilityAmount = y.APPROVEDAMOUNT
                                   }).ToList();


            var loanComments = (from x in context.TBL_LOAN_APPLICATION_COMMENT
                                join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                where y.APPLICATIONREFERENCENUMBER == applicationRefNumber && x.OPERATIONID == (int)CommentsTypeEnum.LMS
                                select new LoanApplicationCommentViewModel()
                                {
                                    comments = x.COMMENTS,
                                }).ToList();

            var loanMonitoringTriggers = (from x in context.TBL_LMSR_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANREVIEWAPPLICATIONID equals y.LOANREVIEWAPPLICATIONID
                                          where y.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                          select new MonitoringTriggersViewModel()
                                          {
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                          }).Distinct().ToList();


            //var loanMonitoringTriggers = monitoringTriggers.Select(x => x.monitoringTrigger).Distinct().ToList();


            ////
            var loanComment = string.Empty;
            var comment = string.Empty;
            int noOfComments = 0;


            ////

            var conditions = string.Empty;

            var fee = string.Empty;

            var loanDetail = string.Empty;

            var loanCollateral = string.Empty;

            var loanMonitoringTrigger = string.Empty;

            var loanTransactionDynamics = string.Empty;

            var internalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == false).ToList();

            var externalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == true).ToList();

            var internalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == false).ToList();

            var externalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == true).ToList();

            int noOfInternalConditions = 0;

            int noOfExternalConditions = 0;

            var finalConditionPrecedents = string.Empty;

            var finalConditionSubsequents = string.Empty;

            var loanfee = string.Empty;

            var detail = string.Empty;

            var collateral = string.Empty;

            var monitoringTrigger = string.Empty;

            var transactionDynamics = string.Empty;

            int noOfDetails = 0;

            int noOfFees = 0;

            int noOfCollaterals = 0;

            int noOfMonitoringTriggers = 0;

            int noOfTransactionDynamics = 0;

            foreach (var prod in products)
            {
                var productExternalConditions = externalConditionsPrecedents.Where(x => x.productName == prod.productName);

                conditions = $"<p><strong> Conditions Precedent(to be satisfied before drawdown) {prod.productName}</strong></p>";

                conditions = conditions +
                        $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                        $"<tr>" +
                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<p> &nbsp;</p><p><strong> S/No </strong></p></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:225.05pt'><p> &nbsp;</p>" +

                        $"<strong> Conditions Precedent </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:100.05pt'><p> &nbsp;</p>" +

                        $"<strong> Applicable Facility </strong ></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:1.0in'>" +

                        $"<strong> *Credit Verification Officer&rsquo; s initial for compliance only</strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:67.5pt'>";

                foreach (var item in productExternalConditions)
                {
                    conditions = conditions +
                        $"<tr>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.conditionPrecident}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'><p>{prod.productName}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'><p> &nbsp;</p></td>" +
                        $"</tr>";
                }

                noOfExternalConditions = 0;

                conditions = conditions +
                    "<tr class='removeConditions_OL'><td colspan='5' style='height:18.4pt; vertical-align:top; width:490.5pt'>" +
                    "<p><strong> Other Conditions Precedent for Internal usage which does not have to be included in the offer " +
                    "letter.The RM must ensure compliance with these conditions before drawdown.</strong></p></td></tr> ";

                var productInternalConditions = internalConditionsPrecedents.Where(x => x.productName == prod.productName);

                foreach (var item in productInternalConditions)
                {
                    conditions = conditions +
                        $"<tr class='removeConditions_OL'>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfInternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.conditionPrecident}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'><p>{prod.productName}</p></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'><p> &nbsp;</p></td>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:67.5pt'><p>&nbsp;</p></td>" +
                        $"</tr>";
                }

                noOfInternalConditions = 0;

                conditions = conditions + "</tbody></table><p> &nbsp;</p>";

                finalConditionPrecedents += conditions;
            }

            foreach (var prod in products)
            {
                var productExternalConditions = externalConditionsSubsequents.Where(x => x.productName == prod.productName);

                conditions = $"<p><strong>Conditions Subsequent (to be satisfied after drawdown) {prod.productName}</strong></p>";

                conditions = conditions +
                        $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                        $"<tr>" +
                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                        $"<strong> S/No </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:225.05pt'><p> &nbsp;</p>" +

                        $"<strong> Conditions Subsequent </strong></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:100.05pt'><p> &nbsp;</p>" +

                        $"<strong> Timeline for compliance </strong ></td>" +

                        $"<td style='height:31.0pt; vertical-align:top; width:1.0in'><p> &nbsp;</p>" +

                        $"<strong> Credit Monitoring Officer’s initial for compliance only</strong></td>" +

                        $"</tr>";

                foreach (var item in productExternalConditions)
                {
                    conditions = conditions +
                        $"<tr>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'>{item.conditionPrecident}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'>&nbsp</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'> &nbsp;</td>" +
                        $"</tr>";
                }

                noOfExternalConditions = 0;

                conditions = conditions +
                    "<tr class='removeConditions_OL'><td colspan='5' style='height:18.4pt; vertical-align:top; width:490.5pt'>" +
                    "<strong> Other Conditions Subsequent for Internal usage which does not have to be included in the offer " +
                    "letter.The RM must ensure compliance with these conditions after drawdown.</strong></td></tr> ";

                var productInternalConditions = internalConditionsSubsequents.Where(x => x.productName == prod.productName);

                foreach (var item in productInternalConditions)
                {
                    conditions = conditions +
                        $"<tr class='removeConditions_OL'>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:40.45pt'>" + $"<ol><li>{++noOfInternalConditions}</li></ol></td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'>{item.conditionPrecident}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 100.05pt'>{prod.productName}</td>" +
                        $"<td style='height: 18.4pt; vertical - align:top; width: 1.0in'> &nbsp;</td>" +
                        $"<td style='height:18.4pt; vertical-align:top; width:67.5pt'>&nbsp;</td>" +
                        $"</tr>";
                }

                noOfInternalConditions = 0;

                conditions = conditions + "</tbody></table><p> &nbsp;</p>";

                finalConditionSubsequents += conditions;
            }

            fee = $"<p><strong> Fee Deatils: </strong></p>";

            fee = fee +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Name </strong></p></td>" +

                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Rate </strong></td></tr>";



            foreach (var item in fees)
            {
                fee = fee +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.feeName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.rateValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"</tr>";
            }

            noOfFees = 0;

            fee = fee + "</tbody></table><p> &nbsp;</p>";

            loanfee += fee;

            var feeData = $"{loanfee}";



            loanDetail = $" ";//<p><strong> Facility Deatils: </strong></p>

            loanDetail = loanDetail +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Facility Type </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Purpose </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:49.5pt'><p> &nbsp;</p>" +
                    $"<strong> Tenor </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:119.8pt'><p> &nbsp;</p>" +
                    $"<strong>  Limits N </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Review Date </strong></td></tr>";



            foreach (var item in loanDetails)
            {
                loanDetail = loanDetail +
                    $"<tr>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.purpose}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.tenor}</p> Days </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.approvedAmountCurrency}</p> % p.a </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.applicationDate.ToString("dd/MM/yyyy")}</p></td>" +
                    $"</tr>";
            }

            noOfDetails = 0;

            loanDetail = loanDetail + "</tbody></table><p> &nbsp;</p>";

            detail += loanDetail;

            var loanDetailData = $"{detail}";


            loanCollateral = $" ";//<p><strong> Collateral: </strong></p>

            loanCollateral = loanCollateral +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Type and description of security </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Value(<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Facility Amount (<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Amount Stamped To Cover (<s>N</s>) </strong></td></tr>";

            foreach (var item in loanCollaterals)
            {
                //facilityAmount
                loanCollateral = loanCollateral +
                 $"<tr>" +
                 $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfCollaterals}</li></ol></td>" +
                 $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralDetail}</p></td>" +
                 $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                 $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.stapedToCoverAmount.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                 $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.facilityAmount.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                 $"</tr>";
            }

            noOfCollaterals = 0;

            loanCollateral = loanCollateral + "</tbody></table><p> &nbsp;</p>";

            collateral += loanCollateral;

            var loanCollateralData = $"{collateral}";


            ////comments

            loanComment = $" ";//<p><strong> Collateral: </strong></p>

            loanComment = loanComment +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Comments </strong></p></td></tr>";

            foreach (var item in loanComments)
            {
                loanComment = loanComment +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfComments}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.comments}</p></td>" +
                    $"</tr>";
            }

            noOfComments = 0;

            loanComment = loanComment + "</tbody></table><p> &nbsp;</p>";

            comment += loanComment;

            var loanCommentData = $"{comment}";

            ////comments end


            loanMonitoringTrigger = $"<p><strong> Monitoring Triggers: </strong></p>";

            loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong>  </strong></td></tr>";



            foreach (var item in loanMonitoringTriggers)
            {
                loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.monitoringTrigger}</p></td>" +
                    $"</tr>";
            }

            noOfMonitoringTriggers = 0;

            loanMonitoringTrigger = loanMonitoringTrigger + "</tbody></table><p> &nbsp;</p>";

            monitoringTrigger += loanMonitoringTrigger;

            var monitoringTriggerData = $"{monitoringTrigger}";

            // transactionDynamicsDetails

            loanTransactionDynamics = $"<p><strong> Transaction Dynamics: </strong></p>";

            loanTransactionDynamics = loanTransactionDynamics +
                    $"<table border='1' cellpadding='5' cellspacing='2' ><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                    $"<strong> Dynamics </strong></p></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'><p> &nbsp;</p>" +
                    $"<strong> Credit Verification Officer’s initial for compliance only </strong></p></td>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong>  </strong></td></tr>";



            foreach (var item in transactionDynamicsDetails)
            {
                loanTransactionDynamics = loanTransactionDynamics +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.dynamics}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p></p></td>" +
                    $"</tr>";
            }

            noOfTransactionDynamics = 0;

            loanTransactionDynamics = loanTransactionDynamics + "</tbody></table><p> &nbsp;</p>";

            transactionDynamics += loanTransactionDynamics;

            var transactionDynamicsData = $"{transactionDynamics}";

            var conditionPrecedentData = $"{finalConditionPrecedents} {finalConditionSubsequents}";

            var customer = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).TBL_CUSTOMER.FIRSTNAME;
            var branch = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).BRANCHID.ToString();
            //var info = data;

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData, templateLink, branch, customer, feeData, loanDetailData, currentDate, loanCollateralData, monitoringTriggerData, transactionDynamicsData, loanCommentData);

            if (preparedTemplate != null)
            {
                return new Form3800ViewModel { documentTemplate = preparedTemplate };
            }

            return new Form3800ViewModel { };
        }

        public OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber)
        {
            throw new NotImplementedException();
        }

        private static string GetProductSpecificTemplate(short? productClassProcessId, short? productClassId)
        {
            var templateLink = string.Empty;

            var links = new
            {
                General = "~/EmailTemplates/FORM-3800B-Template.html",
                IDF = "~/EmailTemplates/FORM-3800B-IDF.html",
                FirstEdu = "~/EmailTemplates/FORM-3800B-FirstEdu.html",
                FirstTrader = "~/EmailTemplates/FORM-3800B-FirstTrader.html",
                BondsAndGuarantees = "~/EmailTemplates/FORM-3800B-BG.html",
                ImportFinance = "~/EmailTemplates/FORM-3800B-ImportFinance.html",
                CashBackedOnly = "~/EmailTemplates/FORM-3800B-CashBackedOnly.html"
            };

            if (productClassProcessId == (short)ProductClassProcessEnum.CAMBased)
            {
                templateLink = links.General;

                return templateLink;
            }
            else //if (productClassProcessId == (short)ProductClassProcessEnum.ProductBased)
            {
                switch (productClassId)
                {
                    case (short)ProductClassEnum.BondAndGuarantees:
                        templateLink = links.BondsAndGuarantees;
                        break;
                    case (short)ProductClassEnum.CashBackedOnly:
                        templateLink = links.CashBackedOnly;
                        break;

                    case (short)ProductClassEnum.FirstEdu:
                        templateLink = links.FirstEdu;
                        break;

                    case (short)ProductClassEnum.FirstTrader:
                        templateLink = links.FirstTrader;
                        break;

                    case (short)ProductClassEnum.ImportFinance:
                        templateLink = links.ImportFinance;
                        break;

                    case (short)ProductClassEnum.InvoiceDiscountingFacility:
                        templateLink = links.IDF;
                        break;

                    default:
                        templateLink = links.General;
                        break;
                }

                return templateLink;
            }

            //templateLink = links.General;

            //return templateLink;
        }

        private static string PopulateTemplatePlaceholders(DateTime applicationDate, string conditionPrecedent, string template, string branch, string customer, string feecondition, string facilitycondition, DateTime currentDate, string collateralcondition, string monitoringTrigger, string transactionDynamics, string loanComments)
        {
            string body;

            string templateLink = template;

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{@ApplicationDate}", applicationDate.ToString("dd-MMM-yyyy", null));
            body = body.Replace("{@ConditionPrecedents}", conditionPrecedent);
            body = body.Replace("{@Branch}", branch);
            body = body.Replace("{@Customer}", customer);
            body = body.Replace("{@Fees}", feecondition);
            body = body.Replace("{@facility}", facilitycondition);
            body = body.Replace("{@CurrentDate}", currentDate.ToString("dd-MMM-yyyy", null));
            body = body.Replace("{@Collateral}", collateralcondition);
            body = body.Replace("{@monitoringTrigger}", monitoringTrigger);
            body = body.Replace("{@transactionDynamics}", transactionDynamics);
            body = body.Replace("{@loanComments}", loanComments);

            return body;
        }

        public bool SaveDraftOfferLetter(OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var exisitingDocument = context.TBL_TEMP_OFFERLETTER.Where(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault();

                    if (exisitingDocument != null)
                    {
                        exisitingDocument.LOANAPPLICATIONDOCUMENT = model.documentTemplate;
                        exisitingDocument.APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber;
                        exisitingDocument.COMMENTS = model.comments;
                        exisitingDocument.PRODUCTID = model.productId;
                        exisitingDocument.ISACCEPTED = model.isAccepted;
                    }
                    else
                    {
                        var document = new TBL_TEMP_OFFERLETTER
                        {
                            LOANAPPLICATIONDOCUMENT = model.documentTemplate,
                            APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                            COMMENTS = model.comments,
                            PRODUCTID = model.productId,
                            ISACCEPTED = model.isAccepted
                        };

                        context.TBL_TEMP_OFFERLETTER.Add(document);
                    }

                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public bool UpdateDraftOfferLetter(int documentId, OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var exisitingDocument = context.TBL_TEMP_OFFERLETTER.Find(documentId);

                    exisitingDocument.LOANAPPLICATIONDOCUMENT = model.documentTemplate;
                    exisitingDocument.APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber;
                    exisitingDocument.COMMENTS = model.comments;
                    exisitingDocument.PRODUCTID = model.productId;
                    exisitingDocument.ISACCEPTED = model.isAccepted;

                    if (model.isAccepted == true)
                    {
                        return SaveFinalOfferLetter(model);
                    }

                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public bool UpdateFinalOfferLetter(string applicationRef, OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    SaveFinalOfferLetter(model);
                    TBL_OFFERLETTER result = (from p in context.TBL_OFFERLETTER
                                              where p.APPLICATIONREFERENCENUMBER == applicationRef
                                              select p).SingleOrDefault();

                    result.ISFINAL = model.isFinal;

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public IEnumerable<OfferLetterTemplateViewModel> GetAllDraftOfferLetters()
        {
            var data = (from a in context.TBL_TEMP_OFFERLETTER
                        select new OfferLetterTemplateViewModel
                        {
                            documentId = a.DOCUMENTID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            documentTemplate = a.LOANAPPLICATIONDOCUMENT,
                            comments = a.COMMENTS,
                            productId = a.PRODUCTID,
                            isAccepted = (bool)a.ISACCEPTED
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<OfferLetterTemplateViewModel> { };
        }

        public OfferLetterTemplateViewModel GetDraftOfferLetterByApplRefNumber(string applicationRefNumber)
        {
            var data = GetAllDraftOfferLetters().Where(x => x.applicationReferenceNumber == applicationRefNumber).FirstOrDefault();

            if (data != null)
            {
                return data;
            }

            return new OfferLetterTemplateViewModel { };
        }

        public IEnumerable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters()
        {
            var data = (from a in context.TBL_OFFERLETTER
                        select new OfferLetterTemplateViewModel
                        {
                            documentId = a.DOCUMENTID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            documentTemplate = a.LOANAPPLICATIONDOCUMENT,
                            comments = a.COMMENTS,
                            productId = a.PRODUCTID,
                            isAccepted = (bool)a.ISACCEPTED,
                            isFinal = a.ISFINAL,

                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<OfferLetterTemplateViewModel> { };
        }

        public OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(string applicationRefNumber)
        {
            var data = GetAllFinalOfferLetters().Where(x => x.applicationReferenceNumber == applicationRefNumber).FirstOrDefault();

            if (data != null)
            {
                return data;
            }

            return new OfferLetterTemplateViewModel { };
        }

        public bool SaveFinalOfferLetter(OfferLetterTemplateViewModel model)
        {
            bool result = false;
            try
            {
                var exisitingDocument = context.TBL_OFFERLETTER.Where(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault();

                if (exisitingDocument != null)
                {
                    exisitingDocument.LOANAPPLICATIONDOCUMENT = model.documentTemplate;
                    exisitingDocument.APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber;
                    exisitingDocument.COMMENTS = model.comments;
                    exisitingDocument.PRODUCTID = model.productId;
                    exisitingDocument.ISACCEPTED = model.isAccepted;

                    //if (!model.isAccepted)
                    //{
                    //    UpdateLoanApplicationStatus(model.applicationReferenceNumber, (short)LoanApplicationStatusEnum.ApplicationUnderReview);
                    //}
                }
                else
                {
                    var document = new TBL_OFFERLETTER
                    {
                        LOANAPPLICATIONDOCUMENT = model.documentTemplate,
                        APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                        COMMENTS = model.comments,
                        PRODUCTID = model.productId,
                        ISACCEPTED = model.isAccepted
                    };

                    context.TBL_OFFERLETTER.Add(document);
                }

                if (model.isAccepted == false && model.saveOnly != true)
                {
                    var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);
                    if (appl == null)
                    {
                        result = false;
                        throw new SecureException("Loan application with the given reference number not found!");
                    }
                    else
                    {
                        result = true;
                    }

                    // appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterRejected;
                }

                context.SaveChanges();



            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (result == true)
                return true;
            else
                return false;
        }

        public int ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            int operationId = (int)OperationsEnum.LoanAvailment;
            // var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == operationId).ToList();
            var loanApplication = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID);

            var initiated = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == loanApplication.LOANAPPLICATIONID).Any();

            workflow.StaffId = entity.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = loanApplication.LOANAPPLICATIONID;
            workflow.CompanyId = loanApplication.COMPANYID;
            //workflow.ProductClassId = loanApplication.PRODUCTCLASSID; // commented out to allow B&G approval fly
            workflow.StatusId = initiated == true ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Processing;
            workflow.Comment = entity.comment;
            workflow.Amount = entity.amount;
            workflow.DeferredExecution = true;
            workflow.LogActivity(); // ------------------- LOG ONCE

            PendingJobRequestCheck(loanApplicationDetails); // austin!
            int invalids = BeforeAvailmentValidationChecks(loanApplication.LOANAPPLICATIONID);

            bool workflowEnded = false;
            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                workflowEnded = true;
                loanApplication.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                loanApplication.AVAILMENTDATE = DateTime.Now;
                short? productProcessId = loanApplication.TBL_PRODUCT_CLASS?.PRODUCT_CLASS_PROCESSID;
                LogLoanBookingRequest(entity, loanApplication.PRODUCTCLASSID, productProcessId, loanApplicationDetails); // austin!
            }

            context.SaveChanges();

            return workflowEnded ? 0 : 1;
        }

        private int BeforeAvailmentValidationChecks(int applicationId)
        {
            int result = 0;

            // CRMS VALIDATION
            var Record = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId && x.CRMSCOLLATERALTYPEID == null).Count();
            if (Record > 0)
            {
                result = 1;
                throw new ConditionNotMetException("Kindly Ensure All Loan Details Have CRMS Collateral Type Attached");
            }

            return result;
        }

        private void LogLoanBookingRequest(LoanAvailmentApprovalViewModel entity, short? productClassId, short? processId, IQueryable<TBL_LOAN_APPLICATION_DETAIL> loanApplicationDetails)
        {
            FinTrakBankingContext ctx = new FinTrakBankingContext();
            foreach (var record in loanApplicationDetails.ToList())
            {
                var systemdate = genSetup.GetApplicationDate();

                var currentLoanApplicationDetailRow = context.TBL_LOAN_APPLICATION_DETAIL.Find(record.LOANAPPLICATIONDETAILID);
                currentLoanApplicationDetailRow.EFFECTIVEDATE = systemdate;
                currentLoanApplicationDetailRow.EXPIRYDATE = (systemdate.AddDays(record.APPROVEDTENOR));

                ctx.SaveChanges();

                if ((record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability))
                {
                    var request = ctx.TBL_LOAN_BOOKING_REQUEST.Add(new TBL_LOAN_BOOKING_REQUEST
                    {
                        AMOUNT_REQUESTED = record.APPROVEDAMOUNT,
                        APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                        LOANAPPLICATIONDETAILID = record.LOANAPPLICATIONDETAILID,
                        DATETIMECREATED = DateTime.Now,
                        ISUSED = false,
                        CREATEDBY = entity.staffId,
                    });

                    ctx.SaveChanges();
                    this.LogBookingApproval(entity, record, request.LOAN_BOOKING_REQUESTID);
                    record.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                }

                //if ((record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                //&& (productClassId != 0 && productClassId != null)
                //   && (processId == (short)ProductClassProcessEnum.ProductBased))
                //{
                //    var request = ctx.TBL_LOAN_BOOKING_REQUEST.Add(new TBL_LOAN_BOOKING_REQUEST
                //    {
                //        AMOUNT_REQUESTED = record.APPROVEDAMOUNT,
                //        APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                //        LOANAPPLICATIONDETAILID = record.LOANAPPLICATIONDETAILID,
                //        DATETIMECREATED = DateTime.Now,
                //        ISUSED = false,
                //        CREATEDBY = entity.staffId,
                //    });
                //    ctx.SaveChanges();
                //    this.LogBookingApproval(entity, record, request.LOAN_BOOKING_REQUESTID);
                //    record.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                //}
            };
        }

        public void LogBookingApproval(LoanAvailmentApprovalViewModel entity, TBL_LOAN_APPLICATION_DETAIL appDetail, int targetId)
        {
            var operationId = 0;
            if (appDetail.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                operationId = (short)OperationsEnum.RevolvingLoanBooking;
            if (appDetail.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                operationId = (short)OperationsEnum.ContigentLoanBooking;
            if (appDetail.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || appDetail.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || appDetail.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                operationId = (short)OperationsEnum.TermLoanBooking;

            if (operationId > 0)
            {
                workflow.StaffId = entity.createdBy;
                workflow.OperationId = operationId;
                workflow.TargetId = targetId;
                workflow.CompanyId = entity.companyId;
                workflow.Comment = "Loan Ready for Booking";
                workflow.ExternalInitialization = true;
                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                workflow.Amount = entity.amount;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
            }
        }

        private void PendingJobRequestCheck(IQueryable<TBL_LOAN_APPLICATION_DETAIL> loanApplicationDetails)
        {
            foreach (var item in loanApplicationDetails)
            {
                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.OPERATIONSID == (short)OperationsEnum.LoanApplication && x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");
                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.OPERATIONSID == (short)OperationsEnum.LoanApplication && x.JOBTYPEID != (short)JobTypeEnum.legal && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending).Any())
                    throw new ConditionNotMetException("There are unattended job request which must be attended to.");
            }
        }


        private bool ReferApplicationToSpecificLevel(LoanAvailmentApprovalViewModel model)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.targetId;
            workflow.CompanyId = model.companyId;
            workflow.NextLevelId = model.nextLevelId;
            workflow.ToStaffId = model.toStaffId;
            workflow.StatusId = model.approvalStatusId;
            workflow.Comment = model.comment;

            workflow.Amount = model.amount;

            return workflow.LogActivity();
        }

        public WorkflowResponse ApproveOfferLetterGeneration(LoanAvailmentApprovalViewModel model)
        {
            var operationId = (int)OperationsEnum.OfferLetterApproval;
            var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber
            && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
            && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted);
            if (appl == null) throw new SecureException("Loan application with the given reference number not found!");

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = null;// appl.PRODUCTCLASSID; <--- reserved for product programs!
            workflow.ProductId = null;
            workflow.StatusId = model.approvalStatusId;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;

            // log
            workflow.LogActivity();

            if (appl.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress;
            }

            if (appl.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress && model.approvalStatusId == (int)ApprovalStatusEnum.Referred)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
            }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                bool cleared = OfferLetterChecklistValidation(appl.LOANAPPLICATIONID, 1);

                if (cleared == false) throw new SecureException("Checklist not cleared to go further!");

                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;

                if (appl.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees) // Bonds and Guarantees adapter
                {
                    if (PendingBondsAndGuaranteeJobRequest(appl.LOANAPPLICATIONID) == false)
                    {
                        throw new ConditionNotMetException("There is no Job Request sent to Legal for the B&G document. Please send one to proceed to availment!.");
                    }

                    //appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress;
                    //context.SaveChanges(); // save changes at this point

                    //workflow.ProductClassId = appl.PRODUCTCLASSID;
                    //workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    //workflow.Comment = "Bonds and Guarantees document process started";
                    //workflow.DeferredExecution = true;
                    //workflow.ExternalInitialization = true;
                    //workflow.LogActivity();
                }
                //else
                //{
                    int staffId = model.createdBy;
                    int? receiverLevelId = null;

                    receiverLevelId = GetFirstReceiverLevel(staffId, (int)OperationsEnum.LoanAvailment, appl.PRODUCTCLASSID, true);

                    workflow.StaffId = staffId;
                    workflow.NextLevelId = receiverLevelId; // BREAKING!

                    workflow.OperationId = (int)OperationsEnum.LoanAvailment;
                    workflow.ProductClassId = appl.PRODUCTCLASSID;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.Comment = "Offer letter approved";
                    workflow.DeferredExecution = true;

                    workflow.LogActivity();
                //}
            }

            var success = context.SaveChanges() > 0;
            workflow.Response.success = success;
            return workflow.Response;
        }

        private bool PendingBondsAndGuaranteeJobRequest(int applicationId)
        {
            var requests = context.TBL_JOB_REQUEST
                .Where(x => x.TARGETID == applicationId
                && x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval
                && x.JOBTYPEID == (short)JobTypeEnum.legal
                && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending
            ).ToList();

            var test = requests;

            return requests.Count() > 0;
        }

        private bool OfferLetterChecklistValidation(int id, int type)
        {
            int count = 0;
            if (type == 1)
            {
                var detailids = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == id)
                    .Select(x => x.LOANAPPLICATIONDETAILID)
                    .ToList();
                count = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => detailids.Contains(x.LOANAPPLICATIONDETAILID)
                        && x.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred
                        && x.ISSUBSEQUENT == false
                    )
                    .Count();
            }

            return count == 0;
        }

        private int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        #endregion OfferLetter & Availment Process

        #region Bonds and Guarantees

        public bool ForwardBondsAndGuarantee(ForwardViewModel model)
        {
            var operationId = (int)OperationsEnum.BondsAndGuarantees;

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = model.productClassId;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;

            // log
            workflow.LogActivity();

            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                int nextOperationId = (int)OperationsEnum.LoanAvailment;
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.AvailmentInProgress;
                workflow.NextLevelId = GetFirstReceiverLevel(model.createdBy, nextOperationId, null, true);
                workflow.NextProcess(appl.COMPANYID, model.createdBy, nextOperationId, appl.LOANAPPLICATIONID, null, "New application", true, true); // model.operationId must be used here!
                // PassApplicationToOperation(model.companyId, model.createdBy, (int)OperationsEnum.LoanAvailment, model.applicationId, "B&G application for availment...");
            }

            return context.SaveChanges() > 0;
        }

        #endregion Bonds and Guarantees

        public bool OfferLetterRejection(ForwardViewModel model)
        {
            var operationId = (int)OperationsEnum.CAM;
            var o = context.TBL_APPROVAL_TRAIL.Find(model.trailId); // here we try to get the staffid on the trail row
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);

            var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == operationId
                && x.TARGETID == appl.LOANAPPLICATIONID
                && x.REQUESTSTAFFID == o.REQUESTSTAFFID
            );

            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = trail.FROMAPPROVALLEVELID;//
            workflow.ToStaffId = o.REQUESTSTAFFID;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            // Take out of offer letter screen
            var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
                && x.RESPONSESTAFFID == null
                && x.TARGETID == appl.LOANAPPLICATIONID
            );
            if (currentTrail != null)
            {
                currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                currentTrail.COMMENT = model.comment;
                currentTrail.TOAPPROVALLEVELID = null;
                currentTrail.TOSTAFFID = null;
            }
            appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;

            return context.SaveChanges() > 0;
        }

        public IEnumerable<CommentOnLoanAvailmentViewModel> GetCommentOnLoanAvailment(string applicationRefNumber)
        {
            var data = (from a in context.TBL_APPROVAL_TRAIL
                        join b in context.TBL_LOAN_APPLICATION on a.TARGETID equals b.LOANAPPLICATIONID
                        join c in context.TBL_STAFF on a.REQUESTSTAFFID equals c.STAFFID
                        join d in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals d.APPROVALSTATEID
                        where b.APPLICATIONREFERENCENUMBER == applicationRefNumber
                        && b.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && b.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && a.OPERATIONID == (int)OperationsEnum.LoanAvailment
                        select new CommentOnLoanAvailmentViewModel
                        {
                            name = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                            comments = a.COMMENT,
                            date = a.SYSTEMARRIVALDATETIME,
                            approvalState = d.APPROVALSTATE,
                            approvalTrailId = a.APPROVALTRAILID,
                        }).ToList();
            //().OrderByDescending(a =>a.approvalTrailId); 

            return data;
        }

        //private void PassApplicationToOperation(int companyId, int staffId, int operationId, int targetId, string comment)
        //{
        //    workflow.StaffId = staffId;
        //    workflow.CompanyId = companyId;
        //    workflow.OperationId = operationId;
        //    workflow.TargetId = targetId;
        //    workflow.ProductClassId = null;
        //    workflow.StatusId = (int)ApprovalStatusEnum.Pending;
        //    workflow.Comment = comment;
        //    workflow.ExternalInitialization = true;
        //    workflow.DeferredExecution = true;
        //    workflow.LogActivity();
        //}

        public bool SendBackToBusinessAvailment(LoanAvailmentApprovalViewModel model)
        {
            int? productClassId = 0;
            int staffId = 0;
            if (model.operationId == (int)OperationsEnum.LoanAvailment)
            {
                var appla = context.TBL_LOAN_APPLICATION.Find(model.targetId);
                productClassId = appla.PRODUCTCLASSID;
                staffId = appla.CREATEDBY;
            }
            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAvailment)
            {
                var applb = context.TBL_LMSR_APPLICATION.Find(model.targetId);
                productClassId = null;
                staffId = applb.CREATEDBY;
            }

            var staff = context.TBL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId && x.PRODUCTCLASSID == productClassId)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         staffRoleId = l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList()
                     ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.targetId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = null;
            workflow.ProductId = null;
            workflow.NextLevelId = staffRoleLevelId;
            workflow.ToStaffId = staffId;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;

            // log
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }
        public LoanApplicationUpdateMessage AvailmentChecklistValidation(int applicationId, int staffId)
        {
            LoanApplicationUpdateMessage result = new LoanApplicationUpdateMessage();
            string str = string.Empty;
            List<int> operations = new List<int>();
            operations.Add((int)OperationsEnum.LoanApplication);
            operations.Add((int)OperationsEnum.CAM);
            operations.Add((int)OperationsEnum.LoanAvailment);

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanAvailment).ToList();
            int checkListIndex = (int)ChecklistErrorEnum.GoodChecklist;
            bool isCheckListDone = true;
            var dat = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId).ToList();

            if (dat != null)
            {
                foreach (var d in dat)
                {
                    var types = from a in context.TBL_CHECKLIST_TYPE select a;
                    foreach (var item in types)
                    {

                        int targetId = 0;
                        if (item.ISPRODUCT_BASED)
                        {
                            targetId = d.LOANAPPLICATIONDETAILID;
                        }
                        else
                        {
                            targetId = applicationId;
                        }


                        var detail = (from a in context.TBL_CHECKLIST_DEFINITION
                                      join b in context.TBL_CHECKLIST_DETAIL on a.CHECKLISTDEFINITIONID
                                      equals b.CHECKLISTDEFINITIONID
                                      where b.TARGETID == targetId && b.TARGETTYPEID == (item.ISPRODUCT_BASED ? (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist : (short)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist)
                                      && a.CHECKLIST_TYPEID == item.CHECKLIST_TYPEID && operations.Contains(a.OPERATIONID)
                                      select b).ToList();
                        var PRODUCTID = (item.ISPRODUCT_BASED ? (short?)d.APPROVEDPRODUCTID : null);

                        //if (item.CHECKLIST_TYPEID == (int)CheckTypeEnum.AvailmentCheckList)
                        //{
                        //    var availmentDetail = (from a in context.TBL_CHECKLIST_DEFINITION
                        //                  join b in context.TBL_CHECKLIST_DETAIL on a.CHECKLISTDEFINITIONID
                        //                  equals b.CHECKLISTDEFINITIONID
                        //                  where b.TARGETID == targetId && b.TARGETTYPEID == (item.ISPRODUCT_BASED ? (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist : (short)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist)
                        //                  && a.CHECKLIST_TYPEID == item.CHECKLIST_TYPEID && a.OPERATIONID == (int)OperationsEnum.LoanAvailment
                        //                           select b).ToList();

                        //    var definition = (from a in context.TBL_CHECKLIST_DEFINITION
                        //                      join b in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals b.CHECKLISTITEMID
                        //                      where ids.Contains((int)a.APPROVALLEVELID) && a.CHECKLIST_TYPEID == item.CHECKLIST_TYPEID
                        //                      && a.OPERATIONID == (int)OperationsEnum.LoanAvailment && a.PRODUCTID == PRODUCTID
                        //                      select a).ToList();

                        //    if (definition.Count() != availmentDetail.Count())
                        //    {
                        //        isCheckListDone = false;
                        //        str = str + Environment.NewLine + item.CHECKLIST_TYPE_NAME + " " + " is not complete. ";
                        //        checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
                        //    }
                        //    var avail = detail.Where(c => c.CHECKLISTSTATUSID == (int)CheckListStatusEnum.No);
                        //    if (avail.Any())
                        //    {
                        //        isCheckListDone = false;
                        //        str = str + $"One or more {item.CHECKLIST_TYPE_NAME} item(s) did not meet with the condition. " + Environment.NewLine
                        //            + " Please check your response to confirm. " + Environment.NewLine;
                        //        checkListIndex = (int)ChecklistErrorEnum.NegetiveChecklist;
                        //    }
                        //}
                        var ab = detail.Where(c => c.CHECKLISTSTATUSID3 == false || c.CHECKLISTSTATUSID3 == null);
                        if (ab.Any())
                        {
                            isCheckListDone = false;
                            str = str + $"One or more {item.CHECKLIST_TYPE_NAME} item(s) is not validated. " + Environment.NewLine
                                + " Please check your response to confirm. " + Environment.NewLine;
                            checkListIndex = (int)ChecklistErrorEnum.NegetiveChecklist;
                        }
                    }
                }
            }

            return new LoanApplicationUpdateMessage
            {
                isdone = isCheckListDone,
                messageStr = str,
                checkListIndex = checkListIndex,
            };

        }



    } 
}
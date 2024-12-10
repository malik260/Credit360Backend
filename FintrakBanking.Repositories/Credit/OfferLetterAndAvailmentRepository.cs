using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.Interfaces.Reports;
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
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FintrakBanking.Repositories.Credit
{
    public class OfferLetterAndAvailmentRepository : IOfferLetterAndAvailmentRepository
    {
        private FinTrakBankingContext context; 
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IReportRoutes reportRoutes;
        private IWorkflow workflow;
        private ICreditLimitValidationsRepository limitValidation;
        private CreditCommonRepository creditCommon;
        private ICRMSRegulatories crmsRegulatories;

        //private IApprovalLevelStaffRepository approvalLevel;
        //private ILoanRepository loans;

        public OfferLetterAndAvailmentRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            //IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workflow,
            ICreditLimitValidationsRepository _limitValidation,
            CreditCommonRepository _creditCommon, IReportRoutes _reportRoutes,
            ICRMSRegulatories _crmsRegulatories
            //ILoanRepository _loans  
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            //approvalLevel = _approvallevel;
            workflow = _workflow;
            limitValidation = _limitValidation;
            creditCommon = _creditCommon;
            reportRoutes = _reportRoutes;
            crmsRegulatories = _crmsRegulatories;
            //loans = _loans;
        }

        #region OfferLetter & Availment Process

        public bool AddCRMSCollateralType(int applicationId, ApprovedLoanDetailViewModel model)
        {
            
            bool output = false;
            var bookingRequest = context.TBL_LOAN_BOOKING_REQUEST.FirstOrDefault(r => r.LOAN_BOOKING_REQUESTID == applicationId && r.DELETED == false);
            var refNumber = bookingRequest.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
            var userModel = new UserViewModel()
            {
                companyId = bookingRequest.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.COMPANYID
            };
            bookingRequest.SECUREDBYCOLLATERAL = model.securedByCollateral;
            bookingRequest.CRMSCOLLATERALTYPEID = model.crmsCollateralTypeId;
            bookingRequest.CRMSREPAYMENTAGREEMENTID = model.crmsRepaymentTypeId;
            bookingRequest.MORATORIUMDURATION = model.moratoriumPeriod;
            bookingRequest.TBL_LOAN_APPLICATION_DETAIL.ISSPECIALISED = model.isSpecialised;
            var crmsRecordGenerated = crmsRegulatories.GenerateCRMSCode(bookingRequest.LOAN_BOOKING_REQUESTID, userModel);
            //var LoanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == applicationId).FirstOrDefault();
            //LoanDetails.SECUREDBYCOLLATERAL = model.securedByCollateral;
            //LoanDetails.CRMSCOLLATERALTYPEID = model.crmsCollateralTypeId;
            //LoanDetails.CRMSREPAYMENTAGREEMENTID = model.crmsRepaymentTypeId;
            //LoanDetails.ISSPECIALISED = model.isSpecialised;
            //LoanDetails.MORATORIUMDURATION = model.moratoriumPeriod;

            var auditRec = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CrmsRecordAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Record Added For CRMS Collateral On Loan Detail '{model.applicationId}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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

        //public bool GenerateCRMSCode(int loanBookingRequestId, UserViewModel model)
        //{
        //    int ctr = 1;
        //    try
        //    {
        //        var crmsModel = GenerateCRMSReport(loanBookingRequestId, model.companyId, ctr).FirstOrDefault();

        //        //var result = "<INFO>Number of Records in Return file: 1.</INFO><INFO>Credit Profile successfully created for Borrower with Unique Identification Number: |22184798858|. Assigned Credit Reference Number is: |00044/20191113/24424880|.</INFO>"; 

        //        var result = integration.FetchCBNCRMSCode(crmsModel, crmsModel.loanSystemTypeId);
        //        var bookingRequest = context.TBL_LOAN_BOOKING_REQUEST.FirstOrDefault(r => r.LOAN_BOOKING_REQUESTID == loanBookingRequestId);

        //        if (result.responseMessage.ToLower().Contains("successfully"))
        //        {
        //            var resultArray = result.responseMessage.Split('|');

        //            if (bookingRequest != null)
        //            {
        //                bookingRequest.CRMSCODE = resultArray[3];
        //            }
        //        }

        //        //TODO method to save CRMSCODE into TBL_LOAN_BOOKING_REQUEST
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ConditionNotMetException("Automatic CRMS Code Generation Failed." + "Core Banking API error: " + " Please Try Again or Choose to add the CRMS code Manually");
        //    }
        //    return true;
        //}

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int companyId) // Control Generation
        {
            var exceptIds = context.TBL_LOAN_RATE_FEE_CONCESSION
                    .Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    .Select(x => (int)x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID).ToList();

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OfferLetterApproval).ToList();
            IQueryable<CamProcessedLoanViewModel> data = null;

            //data = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
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
                    customerId = x.c.a.CUSTOMERID != null ? x.c.a.CUSTOMERID : x.c.b.CUSTOMERID,
                    customerGroupName = x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME,
                    customerGroupCode = x.c.a.TBL_CUSTOMER_GROUP.GROUPCODE,
                    customerGroupId = x.c.a.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                    relationshipOfficerId = x.c.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.c.a.RELATIONSHIPMANAGERID,
                    appraisalOperationId = x.c.a.OPERATIONID,
                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.a.APPLICATIONAMOUNT, //x.c.b.APPROVEDAMOUNT,
                    interestRate = x.c.a.INTERESTRATE,
                    applicationTenor = x.c.a.APPLICATIONTENOR,
                    relationshipOfficerName = x.c.a.TBL_STAFF.FIRSTNAME + " " + x.c.a.TBL_STAFF.MIDDLENAME + " " + x.c.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.c.a.TBL_STAFF1.FIRSTNAME + " " + x.c.a.TBL_STAFF1.MIDDLENAME + " " + x.c.a.TBL_STAFF1.LASTNAME,
                    currentApprovalLevelId = x.d.TOAPPROVALLEVELID,
                    systemArrivalDateTime = x.d.SYSTEMARRIVALDATETIME,
                    loanTypeId = x.c.a.LOANAPPLICATIONTYPEID,
                    productTypeId = x.c.b.TBL_PRODUCT.PRODUCTTYPEID,
                    productTypeName = x.c.b.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                    productName = x.c.b.TBL_PRODUCT.PRODUCTNAME,
                    productId = x.c.b.TBL_PRODUCT.PRODUCTID,
                    loanTypeName = x.c.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                    //camReference = c.CAMREF != null ? c.CAMREF : "N/A",
                    //camDocumentation = d.CAMDOCUMENTATION,
                    approvalDate = x.c.a.APPROVEDDATE,
                    applicationStatusId = x.c.a.APPLICATIONSTATUSID,
                    subSectorId = x.c.b.TBL_SUB_SECTOR.SUBSECTORID,
                    //approvalLevelId = staffApprovalLevelId,
                    operationId = x.d.OPERATIONID,// (int)OperationsEnum.LoanAvailment,
                    appraiselOperationId = x.c.a.OPERATIONID,
                    currentApprovalStateId = x.d.APPROVALSTATEID,
                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    undergoingConcession = exceptIds.Contains(x.c.a.LOANAPPLICATIONID),
                    apiRequestId = x.c.a.APIREQUESTID,
                    productPriceIndex = x.c.b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == x.c.b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                });

            var testList = data.ToList();
            data = data.Where(x =>
                x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationCompleted
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.ApplicationUnderReview
                || x.applicationStatusId == (int)LoanApplicationStatusEnum.CAMCompleted
                )
                .GroupBy(c => c.loanApplicationId)
                .Select(y => y.FirstOrDefault())
                .OrderByDescending(c => c.systemArrivalDateTime)
                ;
            //var testList = data.ToList();
            var testCount = data.Count();

            return data;
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int branchId, int companyId) // RM Review
        {
            var exceptIds = context.TBL_LOAN_RATE_FEE_CONCESSION
                    .Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    .Select(x => (int)x.TBL_LOAN_APPLICATION_DETAIL.PROPOSEDPRODUCTID).ToList();

            var operationId = (int)OperationsEnum.OfferLetterApproval;
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var staffIds = genSetup.GetStaffRlieved(staffId);

            var acceptIds = (from a in context.TBL_LOAN_APPLICATION
                             join b in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals b.TARGETID
                             where a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && a.AVAILMENTDATE == null
                             && 
                             ((b.OPERATIONID == a.OPERATIONID && staffIds.Contains(b.RESPONSESTAFFID ?? 0)) || (b.OPERATIONID == operationId && staffIds.Contains(b.TOSTAFFID ?? 0)))
                             select new { TARGETID = b.TARGETID }).Select(t => t.TARGETID).ToList();


            //var acceptIds2 = context.TBL_LOAN_APPLICATION.Where(x => x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && x.AVAILMENTDATE == null)
            //    .Join(context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == 6 && t.RESPONSESTAFFID == staffId),
            //        a => a.LOANAPPLICATIONID, b => b.TARGETID, (a, b) => new { a, b })
            //        .Select(x => new { TARGETID = x.b.TARGETID })
            //        .Select(t => t.TARGETID)
            //        .ToList()
            //        ;

            IQueryable<CamProcessedLoanViewModel> data = null;

            data = context.TBL_LOAN_APPLICATION
                .Where(x => //x.BRANCHID == branchId && 
                acceptIds.Contains(x.LOANAPPLICATIONID) &&
                !exceptIds.Contains(x.LOANAPPLICATIONID) && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
                    && ((ids.Contains((int)x.TOAPPROVALLEVELID) && x.LOOPEDSTAFFID ==null) || (!ids.Contains((int)x.TOAPPROVALLEVELID) && staffIds.Contains(x.LOOPEDSTAFFID ?? 0)))
                    && x.RESPONSESTAFFID == null
                    && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised)),
                    c => c.b.LOANAPPLICATIONID, d => d.TARGETID, (c, d) => new { c, d })
                .Select(x => new CamProcessedLoanViewModel
                {
                    loanApplicationId = x.c.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.c.a.APPLICATIONREFERENCENUMBER,
                    appraisalOperationId = x.c.a.OPERATIONID,
                    customerCode = x.c.a.TBL_CUSTOMER.CUSTOMERCODE,
                    customerName = x.c.a.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME : x.c.a.TBL_CUSTOMER.FIRSTNAME + " " + x.c.a.TBL_CUSTOMER.MIDDLENAME + " " + x.c.a.TBL_CUSTOMER.LASTNAME,
                    customerId = x.c.a.CUSTOMERID,
                    customerGroupName = x.c.a.TBL_CUSTOMER_GROUP.GROUPNAME,
                    customerGroupCode = x.c.a.TBL_CUSTOMER_GROUP.GROUPCODE,
                    relationshipOfficerId = x.c.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.c.a.RELATIONSHIPMANAGERID,
                    apiRequestId = x.c.a.APIREQUESTID,
                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.a.APPLICATIONAMOUNT, //x.c.b.APPROVEDAMOUNT,
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
                    isFinal = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == x.c.a.LOANAPPLICATIONID).Select(o => o.ISFINAL).FirstOrDefault(),
                    productPriceIndex = x.c.b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == x.c.b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                    currentApprovalLevelId = x.d.TOAPPROVALLEVELID,

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

        public List<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentRoute(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanAvailment).ToList();

            var dueForAvailmentDate = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.LoanAvailment
                    && x.RESPONSESTAFFID == null
                    && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing ||
                        x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised ||
                        x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                    && (ids.Contains((int)x.TOAPPROVALLEVELID) || x.REQUESTSTAFFID == staffId || ((x.TOSTAFFID != null && x.TOSTAFFID == staffId) || (x.TOSTAFFID == null)))
                    //&& ids.Contains((int)x.TOAPPROVALLEVELID)
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
                    capRegionId = x.c.a.CAPREGIONID,
                    //regionId = context.TBL_BRANCH_REGION.FirstOrDefault(c => c.REGIONID == x.c.a.CAPREGIONID).REGIONID2.Value,
                    timeIn = x.d.SYSTEMARRIVALDATETIME,
                    toStaffId = x.d.TOSTAFFID,

                    applicationDate = x.c.a.APPLICATIONDATE,
                    newApplicationDate = x.c.a.APPLICATIONDATE,
                    applicationAmount = x.c.a.APPLICATIONAMOUNT,
                    approvedAmount = x.c.b.APPROVEDAMOUNT,
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

                    responsiblePerson = context.TBL_STAFF
                                            .Where(s => s.STAFFID == x.d.TOSTAFFID)
                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                            .FirstOrDefault().name ?? "",
                    requestStaffId = x.d.REQUESTSTAFFID,
                    toApprovalLevelId = x.d.TOAPPROVALLEVELID,

                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    atInitiator = x.c.a.OWNEDBY == staffId,
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


            var branchRegionStaff = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId && x.DELETED == false).ToList();
            List<int> staffRegionIds = new List<int>();
            foreach (var item in branchRegionStaff)
            {
                staffRegionIds.Add(item.REGIONID);
            }

            var data = (from b in dueForAvailmentDate where staffRegionIds.Contains((int)b.regionId) select b);

            return data.ToList();
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
                    && (ids.Contains((int)x.TOAPPROVALLEVELID) || (!ids.Contains((int)x.TOAPPROVALLEVELID) && x.LOOPEDSTAFFID == staffId))
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
                    approvedAmount = x.c.b.APPROVEDAMOUNT,
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

                    responsiblePerson = context.TBL_STAFF
                                            .Where(s => s.STAFFID == x.d.TOSTAFFID)
                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                            .FirstOrDefault().name ?? "",
                    requestStaffId = x.d.REQUESTSTAFFID,
                    toApprovalLevelId = x.d.TOAPPROVALLEVELID,




                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
                    atInitiator = x.c.a.OWNEDBY == staffId,
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


        private List<ProductFeeViewModel> Los_Fee(int applicationDeatailId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        where b.LOANAPPLICATIONDETAILID == applicationDeatailId
                        select new ProductFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE,
                            productName = b.TBL_PRODUCT.PRODUCTNAME
                        }).ToList();

            return fees;
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
                                          join b in context.TBL_LOAN_CONDITION_PRECEDENT on c.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                          && (b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived || b.CHECKLISTSTATUSID == null)
                                         && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                          && b.ISSUBSEQUENT == false
                                          select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionId = b.CONDITIONID,
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = a.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionPrecedents = conditionPrecedentsSub;

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on c.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                        && b.ISSUBSEQUENT == true
                                        && b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived
                                        && c.STATUSID == (int)ApprovalStatusEnum.Approved
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
                            && c.STATUSID == (int)ApprovalStatusEnum.Approved
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
                            productName = b.TBL_PRODUCT.PRODUCTNAME,
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
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
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
                                              && b.STATUSID == (int)ApprovalStatusEnum.Approved
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  productName = b.TBL_PRODUCT.PRODUCTNAME,
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
                                          && y.STATUSID == (int)ApprovalStatusEnum.Approved
                                          select new MonitoringTriggersViewModel()
                                          {
                                              productName = y.TBL_PRODUCT.PRODUCTNAME,
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
                    $"<strong> Rate </strong></td>" +


                                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Product </strong></td></tr>";



            foreach (var item in fees)
            {
                fee = fee +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.feeName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.rateValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.approvedAmountCurrency}</p> </td>" +
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
                    $"<strong> Monitoring Trigger </strong></td>" +
                                       $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> Product Name </strong></td>" + 

                    $"</tr>";



            foreach (var item in loanMonitoringTriggers)
            {
                loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.monitoringTrigger}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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
                    $"<strong> Product Name </strong></td></tr>";



            foreach (var item in transactionDynamicsDetails)
            {
                loanTransactionDynamics = loanTransactionDynamics +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.dynamics}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p></p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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

          

            var conditionPrecedents = (from a in context.TBL_LMSR_APPLICATION
                                       join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER && b.ISSUBSEQUENT == false
                                        && b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                        && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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
                                        join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER 
                                        && b.ISSUBSEQUENT == true
                                        && b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived
                                     && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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
                          && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            select new ProductViewModel()
                            {
                                productId = c.TBL_PRODUCT.PRODUCTID,
                                productName = c.TBL_PRODUCT.PRODUCTNAME,
                                productClassId = c.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,//a.PRODUCTCLASSID,
                                //productClassProcessId = productClassProcess.PRODUCT_CLASS_PROCESSID //a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID
                            }).ToList();


            var fees = (from a in context.TBL_LMSR_APPLICATION_DETL_FEE
                        join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        where d.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                    && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                        select new ProductFeeViewModel()
                        {
                            productName =b.TBL_PRODUCT.PRODUCTNAME,
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
                               && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
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

            if (loanDetails.Count==0)
            {
                 loanDetails = (from a in context.TBL_LMSR_APPLICATION
                                 join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                 join e in context.TBL_LOAN_CONTINGENT on b.LOANID equals e.CONTINGENTLOANID
                                 join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                 into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                                 into cg
                                 from d in cg.DefaultIfEmpty()
                                 join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                                 where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                                 && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                                 && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                 select new CamProcessedLoanViewModel()
                                 {
                                     productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                     tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                                             // interestRate = e.INTERESTRATE,
                                     purpose = b.REVIEWDETAILS,
                                     applicationDate = applDate,
                                     approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                     //approvedAmount = b.APPROVEDAMOUNT
                                     approvedDate = a.APPROVEDDATE,
                                     newApplicationDate = a.APPLICATIONDATE

                                 }).ToList();
            }
            if (loanDetails.Count==0)
            {
                 loanDetails = (from a in context.TBL_LMSR_APPLICATION
                          join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                          join e in context.TBL_LOAN_REVOLVING on b.LOANID equals e.REVOLVINGLOANID
                          join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                          into cc
                          from c in cc.DefaultIfEmpty()
                          join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                          into cg
                          from d in cg.DefaultIfEmpty()
                          join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                          where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                          && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
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
                              newApplicationDate = a.APPLICATIONDATE

                          }).ToList();
            }


            var transactionDynamicsDetails = (from a in context.TBL_LMSR_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where b.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                              && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == b.PRODUCTID).Select(x=>x.PRODUCTNAME).FirstOrDefault(),
                                                  dynamics = a.DYNAMICS,
                                              }).Distinct().ToList();

            //var transactionDynamicsDetails = transactionDynamic.Select(x => x.dynamics).Distinct();
            //transactionDynamic.Select(x => x.dynamics).Distinct();

            var loanCollaterals = (from x in context.TBL_LMSR_APPLICATION_COLLATRL2
                                   join y in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANREVIEWAPPLICATIONID equals y.LOANREVIEWAPPLICATIONID
                                   where y.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                   && y.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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
                                              productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == y.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
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
                    $"<strong> Rate </strong></td>" +


                                        $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> Product </strong></td></tr>";



            foreach (var item in fees)
            {
                fee = fee +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.feeName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.rateValue.ToString("N", new CultureInfo("en-US"))}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.approvedAmountCurrency}</p> </td>" +
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
                    $"<strong> Monitoring Trigger </strong></td>" +
                                       $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                        $"<strong> Product Name </strong></td>" +

                    $"</tr>";



            foreach (var item in loanMonitoringTriggers)
            {
                loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.monitoringTrigger}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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
                    $"<strong> Product Name </strong></td></tr>";



            foreach (var item in transactionDynamicsDetails)
            {
                loanTransactionDynamics = loanTransactionDynamics +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfExternalConditions}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.dynamics}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p></p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
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
                    //case (short)ProductClassEnum.CashBackedOnly:
                    //    templateLink = links.CashBackedOnly;
                    //    break;

                    //case (short)ProductClassEnum.FirstEdu:
                    //    templateLink = links.FirstEdu;
                    //    break;

                    //case (short)ProductClassEnum.FirstTrader:
                    //    templateLink = links.FirstTrader;
                    //    break;

                    //case (short)ProductClassEnum.ImportFinance:
                    //    templateLink = links.ImportFinance;
                    //    break;

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
                        return SaveFinalOfferLetter(1,model);
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

        public bool UpdateFinalOfferLetter(int loanApplicationId, OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    SaveFinalOfferLetter(loanApplicationId,model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return true;
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

        public IQueryable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters()
        {
            var data = (from a in context.TBL_LOAN_OFFER_LETTER
                        select new OfferLetterTemplateViewModel
                        {
                            isAccepted = (bool)a.ISACCEPTED,
                            isFinal = a.ISFINAL,
                            loanApplicationId = a.LOANAPPLICATIONID,


                        });

                return data;

        }

        public OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(int loanApplicationId)
        {
            var data = GetAllFinalOfferLetters().Where(x => x.loanApplicationId == loanApplicationId).FirstOrDefault();

            if (data != null)
            {
                return data;
            }

            return new OfferLetterTemplateViewModel { };
        }

        public bool SaveFinalOfferLetter(int loanApplicationId, OfferLetterTemplateViewModel model)
        {
            bool result = false;
            try
            {
                var exisitingDocument = context.TBL_LOAN_OFFER_LETTER.Where(x => x.LOANAPPLICATIONID == loanApplicationId).FirstOrDefault();

                if (exisitingDocument != null)
                {
                    exisitingDocument.ISFINAL = model.isFinal;
                    exisitingDocument.ISACCEPTED = model.isAccepted;

                    //if (!model.isAccepted)
                    //{
                    //    UpdateLoanApplicationStatus(model.applicationReferenceNumber, (short)LoanApplicationStatusEnum.ApplicationUnderReview);
                    //}
                }
                else
                {
                    if (model.isFinal) {
                        var detail = (from a in context.TBL_LOAN_APPLICATION
                                      join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                      where a.LOANAPPLICATIONID == loanApplicationId
                                      select new OfferLetterViewModel
                                      {
                                          customerName = a.CUSTOMERID != null ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault(),
                                          offerLetteracceptance = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERACCEPT").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                          offerLetterClauses = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                          customerId = b.CUSTOMERID,
                                          customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.ADDRESS).FirstOrDefault(),
                                          title = b.TITLE,
                                      }).FirstOrDefault();

                        var document = new TBL_LOAN_OFFER_LETTER
                        {
                            LOANAPPLICATIONID = loanApplicationId,
                            ISLMS = false,
                            OFFERLETTERCLAUSES = detail.offerLetterClauses,
                            OFFERLETTERACCEPTANCE = detail.offerLetteracceptance,
                            CREATEDBY = model.staffId,
                            DATETIMECREATED = DateTime.Now,
                            DELETED = false,
                            ISFINAL = model.isFinal,
                            ISACCEPTED = model.isAccepted
                        };

                        context.TBL_LOAN_OFFER_LETTER.Add(document);

                    }

                }


                if (model.isAccepted == false && model.saveOnly != true)
                {
                    var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == loanApplicationId);

                    if (appl == null)
                    {
                        result = false;
                       // throw new SecureException("Loan application with the given reference number not found!");
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

        [OperationBehavior(TransactionScopeRequired = true)]
        public int ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            int operationId = (int)OperationsEnum.LoanAvailment;// (int)OperationsEnum.LoanAvailment;
            var loanApplication = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID && x.STATUSID == (int)ApprovalStatusEnum.Approved);

            // VALIDATIONS
            PendingJobRequestCheck(loanApplicationDetails);
            // AvailmentChecklistValidation(loanApplication.LOANAPPLICATIONID, entity.staffId);
            //if (!checkListResult.isdone) throw new SecureException(checkListResult.messageStr);
            BeforeAvailmentValidationChecks(loanApplication.LOANAPPLICATIONID);


            var initiated = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == loanApplication.LOANAPPLICATIONID).Any();
            workflow.StaffId = entity.createdBy;
            //workflow.OperationId = operationId;
            workflow.OperationId = operationId;
            workflow.TargetId = loanApplication.LOANAPPLICATIONID;
            workflow.CompanyId = loanApplication.COMPANYID;
            //workflow.ProductClassId = loanApplication.PRODUCTCLASSID; // commented out to allow B&G approval fly
            workflow.StatusId = initiated == true ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Processing;
            workflow.Comment = entity.comment;
            workflow.Amount = entity.amount;
            workflow.DeferredExecution = true;
            workflow.LogActivity(); // ------------------- LOG ONCE

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

        // TO BE OPTIMISED LATER!!!
        private int BeforeAvailmentValidationChecks(int applicationId)
        {
            int result = 0;

            // CRMS VALIDATION
            var Record = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId && x.CRMSCOLLATERALTYPEID == null && x.STATUSID == (int)ApprovalStatusEnum.Approved).Count();
            if (Record > 0)
            {
                result = 1;
                throw new ConditionNotMetException("Kindly Ensure All Loan Details Have CRMS Collateral Type Attached");
            }

            // LIMIT VALIDATION

            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);
            LoanApplicationViewModel application = new LoanApplicationViewModel();

            application.branchId = appl.BRANCHID;
            application.customerId = appl.CUSTOMERID;

            var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => new LoanApplicationDetailViewModel
                {
                    customerId = x.CUSTOMERID,
                    sectorId = x.SUBSECTORID,
                    approvedAmount = x.APPROVEDAMOUNT
                })
                .ToList();

            application.LoanApplicationDetail = details;

            // ValidateLoanApplicationLimits(application);
            creditCommon.ValidateLoanApplicationLimits(
                details.Select(x => x.customerId).ToList(),
                details.Select(x => x.sectorId).ToList(),
                (int) application.branchId,
                details.Sum(x => x.approvedAmount),
                (int)OperationsEnum.LoanAvailment
            );

            // CHECKLIST VALIDATION

            ChecklistValidation(applicationId);

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

                //if ((record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability))
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
                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.JOBTYPEID != (short)JobTypeEnum.legal && (x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any())
                    throw new ConditionNotMetException("There are pending job request for this application.");

                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");

                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");

                if (item.TBL_LOAN_APPLICATION.PRODUCTCLASSID != (short)ProductClassEnum.BondAndGuarantees)
                {
                    if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.JOBTYPEID == (short)JobTypeEnum.legal && (x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any())
                        throw new ConditionNotMetException("There are unattended Legal job request which must be attended to.");
                }

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

        [OperationBehavior(TransactionScopeRequired = true)]
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
            workflow.ProductClassId = appl.PRODUCTCLASSID; //  model.productClassId; <--- reserved for product programs!
            workflow.ProductId = appl.PRODUCTID != null ? appl.PRODUCTID : appl.TBL_LOAN_APPLICATION_DETAIL.First().APPROVEDPRODUCTID; //model.productId;
            workflow.StatusId = model.approvalStatusId;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;

            // log
            workflow.LogActivity();

            if (appl.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress) appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress;
            if (appl.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress && model.approvalStatusId == (int)ApprovalStatusEnum.Referred)
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                //bool cleared = OfferLetterChecklistValidation(appl.LOANAPPLICATIONID, 1);
                //if (cleared == false) throw new SecureException("Checklist not cleared to go further!");
                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress; // TODO booking

                if (appl.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees) // Bonds and Guarantees adapter
                {
                    var bondAndGauranteeSent = false;
                    var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);

                    foreach (var item in detail)
                        if (PendingBondsAndGuaranteeJobRequest(item.LOANAPPLICATIONDETAILID) == true) bondAndGauranteeSent = true;

                    if (bondAndGauranteeSent == false)
                        throw new ConditionNotMetException("There is no Job Request sent to Legal for the B&G document. Please send one to proceed to availment!.");
                }

                /*int staffId = model.createdBy;
                int? productClassId = null; // appl.PRODUCTCLASSID <-------------- was this in fbn
                // receiverLevelId = GetFirstReceiverLevel(staffId, (int)OperationsEnum.LoanAvailment, productClassId, true); // appl.PRODUCTCLASSID
                // workflow.NextLevelId = receiverLevelId; // BREAKING!
                workflow.StaffId = staffId;
                workflow.OperationId = (int)OperationsEnum.LoanAvailment; // TODO BOOKING
                workflow.ProductClassId = productClassId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.Comment = "Offer letter approved";
                workflow.DeferredExecution = true;
                workflow.LogActivity(); // SECOND LOG!*/

                // adjustment to skip availment
                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                appl.AVAILMENTDATE = DateTime.Now;

                var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.STATUSID == (int)ApprovalStatusEnum.Approved)
                    .ToList();

                var systemdate = genSetup.GetApplicationDate();
                // FinTrakBankingContext ctx = new FinTrakBankingContext();
                foreach (var detail in loanApplicationDetails)
                {
                    //var currentLoanApplicationDetailRow = context.TBL_LOAN_APPLICATION_DETAIL.Find(detail.LOANAPPLICATIONDETAILID);
                    detail.EFFECTIVEDATE = systemdate;
                    detail.EXPIRYDATE = (systemdate.AddDays(detail.APPROVEDTENOR));
                    // ctx.SaveChanges();
                }

                workflow.Response.nextLevelName = "Drawdown";
                workflow.Response.nextOperationName = "Drawdown";

                var staffName = context.TBL_STAFF.Where(s => s.STAFFID == model.staffId).FirstOrDefault();
                var fullNames = staffName?.FIRSTNAME +" "+ staffName?.LASTNAME;
                context.SaveChanges();
                if (appl.PRODUCTID == 20)
                {
                   var sendOfferLetter = reportRoutes.GetProductSpecificTemplateCFL(null, appl.PRODUCTCLASSID, model.applicationReferenceNumber, "90", appl.APIREQUESTID, "14", model.comment, fullNames);
                   if(sendOfferLetter != "")
                    {
                        var successCashFlow = context.SaveChanges() > 0;
                        workflow.Response.success = successCashFlow;
                        return workflow.Response;
                    }
                }
            }

            
            var success = context.SaveChanges() > 0;
            workflow.Response.success = success;
            return workflow.Response;
        }


        private bool PendingBondsAndGuaranteeJobRequest(int applicationdetailId)
        {
            var requests = context.TBL_JOB_REQUEST
                .Where(x => x.TARGETID == applicationdetailId
                && x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval
                && x.JOBTYPEID == (short)JobTypeEnum.legal
            ).ToList();

            var test = requests;

            return requests.Count() > 0;
        }

        //private bool OfferLetterChecklistValidation(int id, int type)
        //{
        //    int count = 0;
        //    if (type == 1)
        //    {
        //        var detailids = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == id)
        //            .Select(x => x.LOANAPPLICATIONDETAILID)
        //            .ToList();

        //        count = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => detailids.Contains(x.LOANAPPLICATIONDETAILID)
        //                && x.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred
        //                && x.ISSUBSEQUENT == false
        //            )
        //            .Count();
        //    }

        //    return count == 0;
        //}

        private int? GetFirstReceiverLevel(int staffId, int operationId, int? productClassId, bool next = false)
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

            var nextLevelId = levels.Skip(1).Take(1).Select(x => x.levelId).FirstOrDefault();

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
            workflow.DestinationOperationId = model.destinationOperationId;
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
                workflow.NextProcess(appl.COMPANYID, model.createdBy, nextOperationId, appl.FLOWCHANGEID, appl.LOANAPPLICATIONID, null, "New application", true, true, false, model.isFlowTest, appl.TBL_CUSTOMER?.BUSINESSUNTID); // model.operationId must be used here!
                // PassApplicationToOperation(model.companyId, model.createdBy, (int)OperationsEnum.LoanAvailment, model.applicationId, "B&G application for availment...");
            }

            return context.SaveChanges() > 0;
        }

        #endregion Bonds and Guarantees


        //public bool OfferLetterRejectionOld(ForwardViewModel model)
        //{
        //    var operationId = model.operationId;
        //    var o = context.TBL_APPROVAL_TRAIL.Find(model.trailId); // here we try to get the staffid on the trail row
        //    var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);

        //    var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
        //        x.OPERATIONID == operationId
        //        && x.TARGETID == appl.LOANAPPLICATIONID
        //        && x.REQUESTSTAFFID == o.REQUESTSTAFFID
        //    );

        //    workflow.StaffId = model.createdBy;
        //    workflow.OperationId = operationId;
        //    workflow.TargetId = model.applicationId;
        //    workflow.CompanyId = appl.COMPANYID;
        //    workflow.ProductClassId = appl.PRODUCTCLASSID;
        //    workflow.ProductId = model.productId;
        //    workflow.NextLevelId = trail.FROMAPPROVALLEVELID;//
        //    workflow.ToStaffId = o.REQUESTSTAFFID;
        //    workflow.StatusId = (int)ApprovalStatusEnum.Referred;
        //    workflow.Comment = model.comment;
        //    workflow.DeferredExecution = true;
        //    workflow.ExternalInitialization = true;
        //    workflow.BusinessUnitId = appl.TBL_CUSTOMER?.BUSINESSUNTID;
        //    workflow.LogActivity();

        //    // Take out of offer letter screen
        //    var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
        //        x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
        //        && x.RESPONSESTAFFID == null
        //        && x.TARGETID == appl.LOANAPPLICATIONID
        //    );
        //    if (currentTrail != null)
        //    {
        //        currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
        //        currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
        //        currentTrail.COMMENT = model.comment;
        //        currentTrail.TOAPPROVALLEVELID = null;
        //        currentTrail.TOSTAFFID = null;
        //    }
        //    appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
        //    appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;

        //    return context.SaveChanges() > 0;
        //}

        public bool OfferLetterRejection(ForwardViewModel model)
        {
            var operationId = model.operationId;
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
            workflow.DestinationOperationId = (int)OperationsEnum.OfferLetterApproval;
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
                currentTrail.RESPONSESTAFFID = model.createdBy;
                currentTrail.RESPONSEDATE = DateTime.Now;
                //currentTrail.TOAPPROVALLEVELID = null;
                //currentTrail.TOSTAFFID = null;
            }
            appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;

            return context.SaveChanges() > 0;
        }


        //public bool OfferLetterReferBack(ApprovalViewModel model)
        //{
        //    int staffId = model.staffId;

        //    var staff = context.TBL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();

        //    var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId)
        //         .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
        //         .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
        //             mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
        //             {
        //                 groupPosition = mg.m.POSITION,
        //                 levelPosition = l.POSITION,
        //                 levelId = l.APPROVALLEVELID,
        //                 levelName = l.LEVELNAME,
        //                 staffRoleId = l.STAFFROLEID,
        //             })
        //             .OrderBy(x => x.groupPosition)
        //             .ThenBy(x => x.levelPosition)
        //             .ToList();

        //    var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
        //    var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
        //    var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

        //    int currentLevelIndex = levels.FindIndex(p => p.levelId == staffRoleLevelId);
        //    int nextLevelIndex = levels.FindIndex(p => p.levelId == model.approvalLevelId);


        //    if (nextLevelIndex > currentLevelIndex)
        //        throw new ConditionNotMetException("The referred level is higher than the current level.");

        //    workflow.StaffId = model.createdBy;
        //    workflow.OperationId = model.operationId;
        //    workflow.DestinationOperationId = model.destinationOperationId;
        //    workflow.TargetId = model.targetId;
        //    workflow.CompanyId = model.companyId;
        //    workflow.ProductClassId = model.productClassId;
        //    workflow.ProductId = model.productId;
        //    workflow.ToStaffId = model.toStaffId;
        //    workflow.NextLevelId = model.nextLevelId;


        //    workflow.LoopedStaffId = model.loopedStaffId;
        //    workflow.StatusId = (int)ApprovalStatusEnum.Referred;
        //    workflow.Comment = model.comment;
        //    workflow.DeferredExecution = true;

        //    workflow.LogActivity();


        //    return context.SaveChanges() > 0;

        //}

        public int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, int? productId, int? exclusiveFlowChangeId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var mappingsOnProducts = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && (x.PRODUCTID == productId && x.PRODUCTID != null)
                           )
                           .ToList();

            var mappingsOnProductClass = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && x.PRODUCTID == null
                           )
                           .ToList();

            var mappingsOnOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();

            List<TBL_APPROVAL_GROUP_MAPPING> mappingsOnExclusiveOperations = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (exclusiveFlowChangeId > 0)
            {
                var flowChangePartern = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(exclusiveFlowChangeId);

                if (flowChangePartern != null)
                {
                    mappingsOnExclusiveOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == flowChangePartern.OPERATIONID
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();
                }
            }

            List<TBL_APPROVAL_GROUP_MAPPING> mappings = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (mappingsOnOperations.Any()) mappings = mappingsOnOperations;

            if (mappingsOnProductClass.Any()) mappings = mappingsOnProductClass;

            if (mappingsOnProducts.Any()) mappings = mappingsOnProducts;

            if (mappingsOnExclusiveOperations.Any()) mappings = mappingsOnExclusiveOperations;

            if (mappingsOnProducts.Any() == false && mappingsOnProductClass.Any() == false && mappingsOnOperations.Any() == false && mappingsOnExclusiveOperations.Any() == false)
            {
                var operation = context.TBL_OPERATIONS.Find(operationId);
                if (operation == null) throw new SecureException("Operation ID didn't match");
                if (productClassId != null)
                {
                    var productClass = context.TBL_PRODUCT_CLASS.Find(productClassId);
                    throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME + ", PRODUCT CLASS: " + productClass.PRODUCTCLASSNAME);
                }
                throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME);
            }

            //var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId && x.PRODUCTID == productId)
            var levels = mappings.Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
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


            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID).ToList();
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId).ToList();
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
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
                staffId = appla.OWNEDBY;
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

        public bool ReferBackOneStep(LoanAvailmentApprovalViewModel model)
        {
            int? productClassId = 0;

            // int? currentLevelId = GetCurrentApprovalLevelId(model.companyId, model.operationId, model.targetId);
            // int? currentLevelId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == model.targetId && x.OPERATIONID == model.operationId)
            int? currentLevelId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == model.targetId && x.OPERATIONID == model.operationId
            && x.RESPONSESTAFFID == null && x.APPROVALSTATEID != 3)
                .OrderByDescending(x => x.APPROVALTRAILID)
                .FirstOrDefault()
                ?.TOAPPROVALLEVELID
                ;

            if (model.operationId == (int)OperationsEnum.LoanAvailment)
            {
                var appla = context.TBL_LOAN_APPLICATION.Find(model.targetId);
                productClassId = appla.PRODUCTCLASSID;
            }
            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAvailment ||
                model.operationId == (int)OperationsEnum.NPLoanReviewApprovalAppraisal ||
                model.operationId == (int)OperationsEnum.WrittenOffLoanReviewApprovalAppraisal)
            {
                var applb = context.TBL_LMSR_APPLICATION.Find(model.targetId);
                productClassId = null;
            }


            var levels = getLevels(model, (int)productClassId);

            int? nextId = null;
            foreach (var level in levels)
            {
                if (level.levelId == currentLevelId) break;
                nextId = level.levelId;
            }

            
            int? staffId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == model.targetId && x.OPERATIONID == model.operationId && x.TOAPPROVALLEVELID == nextId)
                .FirstOrDefault()
                ?.TOSTAFFID
                ;

            var from = context.TBL_STAFF.Where(x => x.STAFFID == model.staffId).FirstOrDefault();
            

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.targetId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = null;
            workflow.ProductId = null;
            workflow.NextLevelId = nextId ?? throw new SecureException("Unable to complete refer back. The destination approval level could not be resolved!");
            workflow.ToStaffId = staffId;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = "Referred back from " + from.FIRSTNAME + " " + from.MIDDLENAME + " " + from.LASTNAME;
            workflow.DeferredExecution = true;

            // log
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }

        private IEnumerable<ApprovalLevelGroup> getLevels(LoanAvailmentApprovalViewModel model, int productClassId)
        {
            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId && x.PRODUCTCLASSID == productClassId)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelGroup
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         staffRoleId = (int)l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList()
                     ;
            //if(levels == null)
            //{
            //    levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId && x.PRODUCTCLASSID == productClassId)
            //   .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
            //   .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
            //       mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelGroup
            //       {
            //           groupPosition = mg.m.POSITION,
            //           levelPosition = l.POSITION,
            //           levelId = l.APPROVALLEVELID,
            //           levelName = l.LEVELNAME,
            //           staffRoleId = (int)l.STAFFROLEID,
            //       })
            //       .OrderBy(x => x.groupPosition)
            //       .ThenBy(x => x.levelPosition)
            //       .ToList()
            //       ;
            //}


            if (levels.Count == 0)
            {
                 levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelGroup
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         staffRoleId = l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList();
            }

            return levels;
        }

        private int? GetCurrentApprovalLevelId(int companyId, int operationId, int targetId)
        {
            var trailLog = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == companyId
                                && x.OPERATIONID == operationId
                                && x.TARGETID == targetId
                                && x.RESPONSESTAFFID == null
                                && x.TOAPPROVALLEVELID != null // check in workflow
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                            ).ToList();

            var request = trailLog.OrderByDescending(x => x.APPROVALTRAILID).FirstOrDefault() ?? null;

            return request.TOAPPROVALLEVELID ?? null;
        }

        private void ChecklistValidation(int applicationId)
        {
            LoanApplicationUpdateMessage result = new LoanApplicationUpdateMessage();
            int targetId = 0;

            List<int> operations = new List<int>();
            operations.Add((int)OperationsEnum.LoanApplication);
            operations.Add((int)OperationsEnum.CreditAppraisal);
            operations.Add((int)OperationsEnum.LoanAvailment);

            var applicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId 
                    && x.STATUSID == (int)ApprovalStatusEnum.Approved 
                    && x.DELETED == false
                ).ToList();

            var types = from a in context.TBL_CHECKLIST_TYPE select a;

            // conditions
            var detailIds = applicationDetails.Select(x => x.LOANAPPLICATIONDETAILID);

            var conditionItems = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                                  where detailIds.Contains(c.LOANAPPLICATIONDETAILID) && c.ISSUBSEQUENT == false && (c.CHECKLISTVALIDATED == false || c.CHECKLISTVALIDATED == null) 
                                  select c).ToList();

            if (conditionItems.Any()) throw new SecureException($"One or more condition(s) is not validated. " + Environment.NewLine + " Please check your response to confirm. " + Environment.NewLine);

            // checklist
            foreach (var d in applicationDetails)
            {
                foreach (var item in types)
                {
                    targetId = item.ISPRODUCT_BASED ? d.LOANAPPLICATIONDETAILID : applicationId;

                    var checklistItems = (from a in context.TBL_CHECKLIST_DEFINITION
                                          join b in context.TBL_CHECKLIST_DETAIL on a.CHECKLISTDEFINITIONID equals b.CHECKLISTDEFINITIONID
                                          where b.TARGETID == targetId
                                                && b.TARGETTYPEID == (item.ISPRODUCT_BASED ? (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist : (short)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist)
                                                && a.CHECKLIST_TYPEID == item.CHECKLIST_TYPEID
                                                && operations.Contains(a.OPERATIONID)
                                          select b).ToList();

                    var omission = checklistItems.Where(c => c.CHECKLISTSTATUSID3 == false || c.CHECKLISTSTATUSID3 == null); //c.CHECKLISTSTATUSID2 == false ||

                    if (item.CHECKLIST_TYPEID == (short)(int)CheckListTypeEnum.CAPChecklist) return;

                    if (omission.Any()) throw new SecureException($"One or more {item.CHECKLIST_TYPE_NAME} item(s) is not validated. " + Environment.NewLine + " Please check your response to confirm. " + Environment.NewLine);

                }

            }
        }

        public void AddOfferLetterClauses(int applicationId, int staffId,bool isLMS, bool callSaveChanges)
        {
            //int? customerExist = null;
            var detail = new OfferLetterViewModel();

            var clause = "";
            var acceptance = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERACCEPT").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();

            if (isLMS) {

                var approvedProduct = context.TBL_LMSR_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == applicationId).Select(o => o.TBL_PRODUCT.PRODUCTTYPEID).FirstOrDefault();
                var customerExist = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == applicationId);
                var customer = customerExist != null ? context.TBL_CUSTOMER.Where(b => b.CUSTOMERID == customerExist.CUSTOMERID).Select(b => b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME).FirstOrDefault() : context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == customerExist.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault();
                acceptance = acceptance.Replace("{@DATE}", DateTime.Now.ToLongDateString());
                acceptance = acceptance.Replace("{@OBLIGUR}", customer);


                if (approvedProduct==(int)LoanProductTypeEnum.ContingentLiability)
                {
                    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE_BG").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                }
                else
                {
                    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                }

                detail = (from a in context.TBL_LMSR_APPLICATION
                              join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                              where a.LOANAPPLICATIONID == applicationId
                              select new OfferLetterViewModel
                              {
                                  customerName = customer,
                                  offerLetteracceptance = acceptance,
                                  offerLetterClauses = clause,
                                  customerId = b.CUSTOMERID,
                                  customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(o=>o.CUSTOMERID==b.CUSTOMERID).Select(o=>o.ADDRESS).FirstOrDefault(),
                                  title = b.TITLE,
                              }).FirstOrDefault();

            } else {

                var approvedProduct = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == applicationId).Select(o => o.APPROVEDPRODUCTID).FirstOrDefault();
                var customerExist = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == applicationId);
                var customer = customerExist != null ? context.TBL_CUSTOMER.Where(b => b.CUSTOMERID == customerExist.CUSTOMERID).Select(b => b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME).FirstOrDefault() : context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == customerExist.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault();
                acceptance = acceptance.Replace("{@DATE}", DateTime.Now.ToLongDateString());
                acceptance = acceptance.Replace("{@OBLIGUR}", customer);

                
                if (approvedProduct == (int)ProductClassEnum.AutoLoans )
                {
                    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTER_LEASE_FACILITY").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();

                }
                //else if (approvedProduct == (int)ProductClassEnum.ContingentFacilities)
                //{
                //    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE_BG").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                //}
                else if (approvedProduct == (int)ProductClassEnum.ImportFinanceFacilities)
                {
                    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTER_IMPORT_FINANCE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                }else
                {
                    clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                }

                detail = (from a in context.TBL_LOAN_APPLICATION
                              join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                              where a.LOANAPPLICATIONID == applicationId
                              select new OfferLetterViewModel
                              {
                                  customerName = customer,
                                  offerLetteracceptance = acceptance,
                                  offerLetterClauses = clause,
                                  customerId = b.CUSTOMERID,
                                  customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.ADDRESS).FirstOrDefault(),
                                  title = b.TITLE,

                              }).FirstOrDefault();
            }
           

            var offerLetterDoc = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == detail.customerId).Select(o => o).FirstOrDefault();
            if (offerLetterDoc!=null)
            {
                offerLetterDoc.OFFERLETTERSALUTATION = "The Managing Director, <br /><br /> " + detail.customerName + "<br /><br />" + detail.customerAddress + "<br /><br /> Attention: " + detail.title + " " + detail.customerName ;
               // offerLetterDoc.OFFERLETTERTITLE = "Dear Sir,";

                if (!context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId).Any())
                {

                    var loanOfferLetter = new TBL_LOAN_OFFER_LETTER
                    {
                        CREATEDBY = staffId,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false,
                        ISLMS = isLMS,
                        LOANAPPLICATIONID = applicationId,
                        OFFERLETTERACCEPTANCE = detail.offerLetteracceptance,
                        OFFERLETTERCLAUSES = detail.offerLetterClauses,
                        ISACCEPTED = true,
                        ISFINAL = false
                    };

                    context.TBL_LOAN_OFFER_LETTER.Add(loanOfferLetter);

                    if (callSaveChanges)
                        context.SaveChanges();
                }
            }
           
        }
      

        public bool EditOfferLetterTitle(int custimerId, string data, int staffId, int branchId)
        {
            var clause = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == custimerId).Select(o => o).FirstOrDefault();
            if (clause != null)
            {
                clause.OFFERLETTERTITLE = data;
                clause.LASTUPDATEDBY = staffId;
                clause.DATETIMEUPDATED = DateTime.Now;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.EditOfferLetterContent,
                    STAFFID = staffId,
                    BRANCHID = (short)branchId,
                    DETAIL =
                   $"Change offer letter title for a customer with custimerId " + custimerId,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = "",
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = custimerId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }

            if (context.SaveChanges() > 0)
                return true;

            return false;
        }

        public bool EditOfferLetterSalutation(int custimerId, string data, int staffId, int branchId)
        {
            var clause = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == custimerId).Select(o => o).FirstOrDefault();
            if (clause != null)
            {
                clause.OFFERLETTERSALUTATION = data;
                clause.LASTUPDATEDBY = staffId;
                clause.DATETIMEUPDATED = DateTime.Now;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.EditOfferLetterContent,
                    STAFFID = staffId,
                    BRANCHID = (short)branchId,
                    DETAIL =
                  $"Change offer letter saluatation for a customer with custimerId " + custimerId,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = "",
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = custimerId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }

          

            if (context.SaveChanges() > 0)
                return true;

            return false;
        }

        public bool EditOfferLetterAcceptance(int applicationId, string data, bool isLMS, int staffId, int branchId)
        {
            var clause = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId && o.ISLMS == isLMS).Select(o => o).FirstOrDefault();
            if (clause != null)
            {
                clause.OFFERLETTERACCEPTANCE = data;
                clause.LASTUPDATEDBY = staffId;
                clause.DATETIMEUPDATED = DateTime.Now;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.EditOfferLetterContent,
                    STAFFID = staffId,
                    BRANCHID = (short)branchId,
                    DETAIL =
                   $"Change offer letter acceptance discription for customer with db loan application Id " + applicationId,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = "",
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = applicationId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }

           

            if (context.SaveChanges() > 0)
                return true;

            return false;
        }

        public bool EditOfferLetterClause(int applicationId, string data, bool isLMS, int staffId, int branchId)
        {
            var clause = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId && o.ISLMS==isLMS).Select(o => o).FirstOrDefault();
            if (clause != null)
            {
                clause.OFFERLETTERCLAUSES = data;
                clause.LASTUPDATEDBY = staffId;
                clause.DATETIMEUPDATED = DateTime.Now;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.EditOfferLetterContent,
                    STAFFID = staffId,
                    BRANCHID = (short)branchId,
                    DETAIL =
                   $"Change offer letter clause for customer with db loan application Id " + applicationId,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),                
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = applicationId
                 
                   
                };

                this.auditTrail.AddAuditTrail(audit);
            }

            

            if (context.SaveChanges() > 0)
                return true;

            return false;
        }

        public OfferLetterViewModel GetOfferLetterTitle(int custimerId)
        {
            return context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == custimerId).Select( o => new OfferLetterViewModel {
                   offerLetterTitle = o.OFFERLETTERTITLE,
                   customerId = o.CUSTOMERID
            }).FirstOrDefault();
        }

        public OfferLetterViewModel GetOfferLetterSalutation(int custimerId)
        {
            return context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == custimerId).Select(o => new OfferLetterViewModel
            {
                offerLetterSalutation = o.OFFERLETTERSALUTATION,
                customerId = o.CUSTOMERID
            }).FirstOrDefault();
        }

        public OfferLetterViewModel GetOfferLetterAcceptance(int applicationId)
        {
            return context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId).Select(o => new OfferLetterViewModel
            {
                offerLetteracceptance = o.OFFERLETTERACCEPTANCE,
                loanApplicationId = o.LOANAPPLICATIONID
            }).FirstOrDefault();
        }

        public OfferLetterViewModel GetOfferLetterClause(int applicationId)
        {
            return context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId).Select(o => new OfferLetterViewModel
            {
                offerLetterClauses = o.OFFERLETTERCLAUSES,
                loanApplicationId = o.LOANAPPLICATIONID
            }).FirstOrDefault();
        }

        public bool IsOfferLetterGenerated(int templateId, int loanApplicationId, int staffId, int branchId)
        {
            // TBL_LOAN_OFFER_LETTER offerLetterExists = new TBL_LOAN_OFFER_LETTER();
            var offerLetterExists = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == loanApplicationId && o.ISLMS == false);

            if (offerLetterExists.Any())
            {
                return true;
            }
            return false;
        }

        public bool ApplyTemplateToOfferLetter(int templateId, int loanApplicationId, int staffId, int branchId)
        {
            AddOfferLetterClausesWithTemplate(loanApplicationId, templateId, staffId, false, false);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.EditOfferLetterContent,
                STAFFID = staffId,
                BRANCHID = (short)branchId,
                DETAIL =
               $"Offer Letter Template Has Been Applied For Loan Application ID  " + loanApplicationId,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loanApplicationId
            };

            this.auditTrail.AddAuditTrail(audit);


            if (context.SaveChanges() > 0) return true;

            return false;
        }

        public void AddOfferLetterClausesWithTemplate(int applicationId, int templateId, int staffId, bool isLMS, bool callSaveChanges)
        {
            int? customerExist = null;
            var detail = new OfferLetterViewModel();
            if (isLMS)
            {
                customerExist = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == applicationId).CUSTOMERID;
                var loanApp = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == applicationId);
                var ao = context.TBL_STAFF.Where(o => o.STAFFID == loanApp.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();
                var cause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATEID == templateId && o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                cause = cause != null ? cause.Replace("{@RelationshipOfficerName}", ao) : null;
                cause = cause.Replace("{@RelationshipManagerName}", "");
                cause = cause.Replace("{@BusinessManagerName}", "");


                detail = (from a in context.TBL_LMSR_APPLICATION
                          join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                          where a.LOANAPPLICATIONID == applicationId
                          select new OfferLetterViewModel
                          {
                              customerName = customerExist != null ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault(),
                              offerLetteracceptance = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATEID == templateId && o.TEMPLATESECTIONCODE == "OFFERLETTERACCEPT").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                              offerLetterClauses = cause, //context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                              customerId = b.CUSTOMERID,
                              customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.ADDRESS).FirstOrDefault(),
                              title = b.TITLE,
                          }).FirstOrDefault();
            }
            else
            {
                customerExist = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == applicationId).CUSTOMERID;
                //Get RM and BM for LOS

                var loanApp = context.TBL_LOAN_APPLICATION.Where(o => o.LOANAPPLICATIONID == applicationId).Select(o => o).FirstOrDefault();

                var ao = context.TBL_STAFF.Where(o => o.STAFFID == loanApp.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();
                var rm = context.TBL_STAFF.Where(o => o.STAFFID == loanApp.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();

                var rmId = loanApp.RELATIONSHIPMANAGERID;
                var bmId = context.TBL_STAFF.Where(o => o.STAFFID == rmId).Select(o => o.SUPERVISOR_STAFFID).FirstOrDefault();
                var bm = context.TBL_STAFF.Where(o => o.STAFFID == bmId).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();

                var clause = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATEID == templateId && o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault();
                clause = clause != null ? clause.Replace("{@RelationshipOfficerName}", ao) : null;
                clause = clause != null ? clause.Replace("{@RelationshipManagerName}", rm) : null;
                clause = clause != null ? clause.Replace("{@BusinessManagerName}", bm) : null;
                if (loanApp.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
                {
                    detail = (from a in context.TBL_LOAN_APPLICATION
                              join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                              where a.LOANAPPLICATIONID == applicationId
                              select new OfferLetterViewModel
                              {
                                  customerName = customerExist != null ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault(),
                                  offerLetteracceptance = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATEID == templateId && o.TEMPLATESECTIONCODE == "OFFERLETTERACCEPT").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                  // offerLetterClauses = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                  offerLetterClauses = clause,
                                  customerId = b.CUSTOMERID,
                                  customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.ADDRESS).FirstOrDefault(),
                                  title = b.TITLE,

                              }).FirstOrDefault();
                     }
                else
                {
                    detail = (from a in context.TBL_LOAN_APPLICATION
                              join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                              join b in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals b.CUSTOMERGROUPID
                              where a.LOANAPPLICATIONID == applicationId
                              select new OfferLetterViewModel
                              {
                                  customerName = context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPNAME).FirstOrDefault(),
                                  customerName2 = context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPCONTACTPERSON).FirstOrDefault(),
                                  offerLetteracceptance = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATEID == templateId && o.TEMPLATESECTIONCODE == "OFFERLETTERACCEPT").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                  // offerLetterClauses = context.TBL_DOC_TEMPLATE_SECTION.Where(o => o.TEMPLATESECTIONCODE == "OFFERLETTERCLAUSE").Select(o => o.TEMPLATEDOCUMENT).FirstOrDefault(),
                                  offerLetterClauses = clause,
                                  customerId = c.CUSTOMERID,
                                  customerAddress = context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(o => o.GROUPADDRESS).FirstOrDefault(),
                                  title = "",

                              }).FirstOrDefault();
                }
            }


            var offerLetterDoc = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == detail.customerId).Select(o => o).FirstOrDefault();
            if (offerLetterDoc != null)
            {
                offerLetterDoc.OFFERLETTERSALUTATION = "The Managing Director, <br /><br /> " + detail.customerName + "<br /><br />" + detail.customerAddress + "<br /><br /> Attention: " + detail.title + " " + detail.customerName2;

                detail.offerLetteracceptance = detail.offerLetteracceptance != null ? detail.offerLetteracceptance.Replace("{@customerName}", detail.customerName) : null;

                if (!context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId).Any())
                {

                    var loanOfferLetter = new TBL_LOAN_OFFER_LETTER
                    {
                        CREATEDBY = staffId,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false,
                        ISLMS = isLMS,
                        LOANAPPLICATIONID = applicationId,
                        OFFERLETTERACCEPTANCE = detail.offerLetteracceptance,
                        OFFERLETTERCLAUSES = detail.offerLetterClauses,
                        ISACCEPTED = true,
                        ISFINAL = false
                    };

                    context.TBL_LOAN_OFFER_LETTER.Add(loanOfferLetter);


                }
                else
                {
                    TBL_LOAN_OFFER_LETTER offerLetterExists = new TBL_LOAN_OFFER_LETTER();
                    //offerLetterExists = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId && o.ISLMS == isLMS).FirstOrDefault();
                    offerLetterExists = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == applicationId).FirstOrDefault();
                    offerLetterExists.OFFERLETTERACCEPTANCE = detail.offerLetteracceptance;
                    offerLetterExists.OFFERLETTERCLAUSES = detail.offerLetterClauses;
                    offerLetterExists.LASTUPDATEDBY = staffId;
                    offerLetterExists.ISACCEPTED = true;
                    offerLetterExists.ISFINAL = false;
                    offerLetterExists.DATETIMEUPDATED = DateTime.Now;

                }
                if (callSaveChanges)
                    context.SaveChanges();
            }

        }

        //public void ValidateLoanApplicationLimits(LoanApplicationViewModel application)
        //{
        //    var details = application.LoanApplicationDetail;
        //    int branchId = (int)application.branchId;
        //    // int customerId = (int)application.customerId;
        //    int? branchOverrideRequestId = null;
        //    int? sectorOverrideRequestId = null;

        //    var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL
        //        .Where(x => x.LOANAPPLICATIONID == application.loanApplicationId
        //            && x.DELETED == false && x.STATUSID == (int)ApprovalStatusEnum.Approved
        //        );

        //    foreach (var detail in loanApplicationDetails)
        //    {
        //        var branchOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == detail.CUSTOMERID)
        //            .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride && x.ISUSED == false),
        //                c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
        //            .Select(x => new { id = x.o.OVERRIDE_DETAILID })
        //            .FirstOrDefault();

        //        if (branchOverrideRequest != null) branchOverrideRequestId = branchOverrideRequest.id;

        //        var sectorOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == detail.CUSTOMERID)
        //            .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride && x.ISUSED == false),
        //                c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
        //            .Select(x => new { id = x.o.OVERRIDE_DETAILID })
        //            .FirstOrDefault();

        //        if (sectorOverrideRequest != null) sectorOverrideRequestId = sectorOverrideRequest.id;

        //        if (branchOverrideRequestId != null)
        //        {
        //            var request = context.TBL_OVERRIDE_DETAIL.Find(branchOverrideRequestId);
        //            request.ISUSED = true;
        //            context.Entry(request).State = System.Data.Entity.EntityState.Modified;
        //        }
        //        else
        //        {
        //            // branch limits
        //            var branchValidation = limitValidation.ValidateNPLByBranch((short)branchId);
        //            decimal branchNplAmount = (decimal)branchValidation.outstandingBalance;
        //            decimal applicationAmount = details.Sum(x => x.approvedAmount); // proposedAmount should be approvedAmount after application
        //            var branch = context.TBL_BRANCH.Find(branchId);
        //            if (branch.NPL_LIMIT > 0 && branch.NPL_LIMIT < (branchNplAmount + applicationAmount)) throw new SecureException("Branch NPL Limit exceeded!");
        //        }

        //        if (sectorOverrideRequestId != null)
        //        {
        //            var request = context.TBL_OVERRIDE_DETAIL.Find(sectorOverrideRequestId);
        //            request.ISUSED = true;
        //            context.Entry(request).State = System.Data.Entity.EntityState.Modified;
        //        }
        //        else
        //        {
        //            // sector limits
        //            // sectorId here is actually the subsectorId
        //            List<short> sectorIds = details.Select(x => x.sectorId).ToList();
        //            foreach (var sectorId in sectorIds)
        //            {
        //                var sectorValidation = limitValidation.ValidateNPLBySector(sectorId);
        //                decimal sectorAmount = (decimal)sectorValidation.outstandingBalance;
        //                //var sector = context.TBL_SECTOR.Find(sectorId);
        //                if (sectorValidation.maximumAllowedLimit > 0 && sectorValidation.maximumAllowedLimit <= sectorAmount) throw new SecureException("Sector Limit exceeded!");
        //            }
        //        }
        //    }
        //}



    } 
}

public class ApprovalLevelGroup
{
    public int groupPosition { get; set; }
    public int levelPosition { get; set; }
    public int levelId { get; set; }
    public string levelName { get; set; }
    public int? staffRoleId { get; set; }

                         
}
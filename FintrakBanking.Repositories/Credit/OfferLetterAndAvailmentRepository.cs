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
        private IApprovalLevelStaffRepository approvalLevel;
        private ILoanRepository loans;

        public OfferLetterAndAvailmentRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workflow,
            ILoanRepository _loans
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            approvalLevel = _approvallevel;
            workflow = _workflow;
            loans = _loans;
        }

        #region OfferLetter & Availment Process

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int companyId) // Control Generation
        {
            var exceptIds = context.TBL_LOAN_RATE_FEE_CONCESSION
                    .Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    .Select(x => (int)x.TBL_LOAN_APPLICATION_DETAIL.PROPOSEDPRODUCTID).ToList();

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OfferLetterApproval).ToList();
            IQueryable<CamProcessedLoanViewModel> data = null;

            data = context.TBL_LOAN_APPLICATION.Where(x => !exceptIds.Contains(x.LOANAPPLICATIONID))
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

            data = context.TBL_LOAN_APPLICATION.Where(x => x.BRANCHID == branchId && !exceptIds.Contains(x.LOANAPPLICATIONID))
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

            var data = context.TBL_LOAN_APPLICATION
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.STATUSID == (int)ApprovalStatusEnum.Approved),
                    a => a.LOANAPPLICATIONID, b => b.LOANAPPLICATIONID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.LoanAvailment && ids.Contains((int)x.TOAPPROVALLEVELID)
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
                    productClassProcessId = x.c.a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                    isFirstApprover = false,
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
                });

            data = data.Where(x =>
                    x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted
                    || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress
                )
                .GroupBy(c => c.loanApplicationId)
                .Select(y => y.FirstOrDefault())
                .OrderByDescending(c => c.loanApplicationId);

            return data;
        }


        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentCheckList(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_LOAN_CONDITION_PRECEDENT on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID

                        where a.COMPANYID == companyId && a.DELETED == false
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

            var targetAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber);

            if (targetAppl.PRODUCTCLASSID == null)
            {
                targetAppl.PRODUCTCLASSID = 1;
            }

            var productClassProcess = context.TBL_PRODUCT_CLASS.FirstOrDefault(x => x.PRODUCTCLASSID == targetAppl.PRODUCTCLASSID);

            var templateLink = GetProductSpecificTemplate(productClassProcess.PRODUCT_CLASS_PROCESSID, (short?)targetAppl.PRODUCTCLASSID ?? 1);


            var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
                                       join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == true
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
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower() &&
                                     b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   purpose = b.LOANPURPOSE,
                                   applicationDate = applDate,
                               }).ToList();

            var transactionDynamicsDetails = (from a in context.TBL_LOAN_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where c.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  dynamics = a.DYNAMICS,
                                              }).ToList();

            var loanCollaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                                   join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                   where y.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT
                                   }).ToList();


            var loanMonitoringTriggers = (from x in context.TBL_LOAN_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals y.LOANAPPLICATIONDETAILID
                                          join z in context.TBL_LOAN_APPLICATION on y.LOANAPPLICATIONID equals z.LOANAPPLICATIONID
                                          where z.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          select new MonitoringTriggersViewModel()
                                          {
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                          }).ToList();





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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
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
                    $"<table border='1' cellspacing='0'<tbody>" +
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
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.rateValue}</p></td>" +
                    $"</tr>";
            }

            noOfFees = 0;

            fee = fee + "</tbody></table><p> &nbsp;</p>";

            loanfee += fee;

            var feeData = $"{loanfee}";



            loanDetail = $" ";//<p><strong> Facility Deatils: </strong></p>

            loanDetail = loanDetail +
                    $"<table border='1' cellspacing='0' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
                    $"<tr>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Facility Type </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Purpose </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:49.5pt'><p> &nbsp;</p>" +
                    $"<strong> Tenor </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:119.8pt'><p> &nbsp;</p>" +
                    $"<strong> Interest </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Review Date </strong></td></tr>";



            foreach (var item in loanDetails)
            {
                loanDetail = loanDetail +
                    $"<tr>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.purpose}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.tenor}</p> Days </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.interestRate}</p> % p.a </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.applicationDate}</p></td>" +
                    $"</tr>";
            }

            noOfDetails = 0;

            loanDetail = loanDetail + "</tbody></table><p> &nbsp;</p>";

            detail += loanDetail;

            var loanDetailData = $"{detail}";


            loanCollateral = $" ";//<p><strong> Collateral: </strong></p>

            loanCollateral = loanCollateral +
                    $"<table border='1' cellspacing='0' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Type and description of security </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Value(<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Amount Stamped To Cover (<s>N</s>) </strong></td></tr>";

            foreach (var item in loanCollaterals)
            {
                loanCollateral = loanCollateral +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfCollaterals}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralDetail}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralValue}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.stapedToCoverAmount}</p> Days </td>" +
                    $"</tr>";
            }

            noOfCollaterals = 0;

            loanCollateral = loanCollateral + "</tbody></table><p> &nbsp;</p>";

            collateral += loanCollateral;

            var loanCollateralData = $"{collateral}";


            loanMonitoringTrigger = $"<p><strong> Monitoring Triggers: </strong></p>";

            loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<table border='1' cellspacing='0'<tbody>" +
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
                    $"<table border='1' cellspacing='0'<tbody>" +
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
            if(customerExist != null)
            {
                customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_CUSTOMER.FIRSTNAME;
            }
            else
            {
                customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_CUSTOMER_GROUP.GROUPNAME;
            }
            
            var branch = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_BRANCH.BRANCHNAME;
            //var info = data;

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData, templateLink, branch, customer, feeData, loanDetailData, currentDate, loanCollateralData, monitoringTriggerData, transactionDynamicsData);

            if (preparedTemplate != null)
            {
                return new Form3800ViewModel { documentTemplate = preparedTemplate };
            }

            return new Form3800ViewModel { };
        }

        public Form3800ViewModel GenerateForm3800TemplateLMS(string refNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            var currentDate = DateTime.Now;
            var loanApplId = 0;
            var facility = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == refNumber);
            if (facility == null)
            {
                var facility1 = context.TBL_LOAN_REVOLVING.Where(x => x.LOANREFERENCENUMBER == refNumber);
                loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility1.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;
            }

            loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;

            var targetAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == loanApplId);

            if (targetAppl.PRODUCTCLASSID == null)
            {
                targetAppl.PRODUCTCLASSID = 1;
            }

            var productClassProcess = context.TBL_PRODUCT_CLASS.FirstOrDefault(x => x.PRODUCTCLASSID == targetAppl.PRODUCTCLASSID);

            var templateLink = GetProductSpecificTemplate(productClassProcess.PRODUCT_CLASS_PROCESSID, (short?)targetAppl.PRODUCTCLASSID ?? 1);


            var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
                                       join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER && b.ISSUBSEQUENT == false
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER && b.ISSUBSEQUENT == true
                                        select new OfferLetterConditionPrecidentViewModel()
                                        {
                                            conditionPrecident = b.CONDITION,
                                            loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                            isExternal = b.ISEXTERNAL,
                                            productName = c.TBL_PRODUCT.PRODUCTNAME
                                        }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var products = (from a in context.TBL_LOAN_APPLICATION
                            join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                            where a.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
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
                        where d.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
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
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == targetAppl.APPLICATIONREFERENCENUMBER.ToLower() &&
                                     b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   purpose = b.LOANPURPOSE,
                                   applicationDate = applDate,
                               }).ToList();

            var transactionDynamicsDetails = (from a in context.TBL_LOAN_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  dynamics = a.DYNAMICS,
                                              }).ToList();

            var loanCollaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                                       //join y in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                   where x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT
                                   }).ToList();


            var loanMonitoringTriggers = (from x in context.TBL_LOAN_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals y.LOANAPPLICATIONDETAILID
                                          where y.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                                          select new MonitoringTriggersViewModel()
                                          {
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                          }).ToList();





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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
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
                    $"<table border='1' cellspacing='0'<tbody>" +
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
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.rateValue}</p></td>" +
                    $"</tr>";
            }

            noOfFees = 0;

            fee = fee + "</tbody></table><p> &nbsp;</p>";

            loanfee += fee;

            var feeData = $"{loanfee}";



            loanDetail = $" ";//<p><strong> Facility Deatils: </strong></p>

            loanDetail = loanDetail +
                    $"<table border='1' cellspacing='0' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
                    $"<tr>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Facility Type </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Purpose </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:49.5pt'><p> &nbsp;</p>" +
                    $"<strong> Tenor </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:119.8pt'><p> &nbsp;</p>" +
                    $"<strong> Interest </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Review Date </strong></td></tr>";



            foreach (var item in loanDetails)
            {
                loanDetail = loanDetail +
                    $"<tr>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.productName}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.purpose}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.tenor}</p> Days </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.interestRate}</p> % p.a </td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 150.05pt'><p>{item.applicationDate}</p></td>" +
                    $"</tr>";
            }

            noOfDetails = 0;

            loanDetail = loanDetail + "</tbody></table><p> &nbsp;</p>";

            detail += loanDetail;

            var loanDetailData = $"{detail}";


            loanCollateral = $" ";//<p><strong> Collateral: </strong></p>

            loanCollateral = loanCollateral +
                    $"<table border='1' cellspacing='0' style='width: 100%; overflow-x:auto; margin-bottom:5px'><tbody>" +
                    $"<tr>" +
                    $"<td style='height:31.0pt; vertical-align:top; width:40.45pt'>" +
                    $"<strong> S/No </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:1.25in'><p> &nbsp;</p>" +
                    $"<strong> Type and description of security </strong></p></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:130.5pt'><p> &nbsp;</p>" +
                    $"<strong> Value(<s>N</s>) </strong></td>" +
                    $"<td style='height:29.65pt; vertical-align:top; width:.75in'><p> &nbsp;</p>" +
                    $"<strong> Amount Stamped To Cover (<s>N</s>) </strong></td></tr>";

            foreach (var item in loanCollaterals)
            {
                loanCollateral = loanCollateral +
                    $"<tr>" +
                    $"<td style='height:18.4pt; vertical - align:top; width:40.45pt'>" + $"<ol><li>{++noOfCollaterals}</li></ol></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralDetail}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.collateralValue}</p></td>" +
                    $"<td style='height: 18.4pt; vertical - align:top; width: 225.05pt'><p>{item.stapedToCoverAmount}</p> Days </td>" +
                    $"</tr>";
            }

            noOfCollaterals = 0;

            loanCollateral = loanCollateral + "</tbody></table><p> &nbsp;</p>";

            collateral += loanCollateral;

            var loanCollateralData = $"{collateral}";


            loanMonitoringTrigger = $"<p><strong> Monitoring Triggers: </strong></p>";

            loanMonitoringTrigger = loanMonitoringTrigger +
                    $"<table border='1' cellspacing='0'<tbody>" +
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
                    $"<table border='1' cellspacing='0'<tbody>" +
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

            var customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).TBL_CUSTOMER.FIRSTNAME;
            var branch = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).TBL_BRANCH.BRANCHNAME;
            //var info = data;

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData, templateLink, branch, customer, feeData, loanDetailData, currentDate, loanCollateralData, monitoringTriggerData, transactionDynamicsData);

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

        private static string PopulateTemplatePlaceholders(DateTime applicationDate, string conditionPrecedent, string template, string branch, string customer, string feecondition, string facilitycondition, DateTime currentDate, string collateralcondition, string monitoringTrigger, string transactionDynamics)
        {
            string body;

            string templateLink = template;

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{@ApplicationDate}", applicationDate.ToLongDateString());
            body = body.Replace("{@ConditionPrecedents}", conditionPrecedent);
            body = body.Replace("{@Branch}", branch);
            body = body.Replace("{@Customer}", customer);
            body = body.Replace("{@Fees}", feecondition);
            body = body.Replace("{@facility}", facilitycondition);
            body = body.Replace("{@CurrentDate}", currentDate.ToLongDateString());
            body = body.Replace("{@Collateral}", collateralcondition);
            body = body.Replace("{@monitoringTrigger}", monitoringTrigger);
            body = body.Replace("{@transactionDynamics}", transactionDynamics);

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
                        throw new Exception("Loan application with the given reference number not found!");
                    }
                    else
                    {
                        result = true;
                    }

                    // appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterRejected;
                }

                context.SaveChanges();
                //if (result == true) return true;

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

        public bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            int operationId = (int)OperationsEnum.LoanAvailment;
            //int staffApprovalLevelId = 0;
            //var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(entity.staffId, entity.companyId, operationId);
            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == operationId).ToList();
            var loanApplication = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID);

            foreach (var item in loanApplicationDetails)
            {
                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.OPERATIONSID == (short)OperationsEnum.LoanApplication && x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");
                if (context.TBL_JOB_REQUEST.Where(x => x.TARGETID == item.LOANAPPLICATIONDETAILID && x.OPERATIONSID == (short)OperationsEnum.LoanApplication && x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending).Any())
                    throw new ConditionNotMetException("There are unattended middle office request which must be attended to.");
            }

            //if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var initiated = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == loanApplication.LOANAPPLICATIONID).Any();

            workflow.StaffId = entity.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = loanApplication.LOANAPPLICATIONID;
            workflow.CompanyId = loanApplication.COMPANYID;
            workflow.ProductClassId = loanApplication.PRODUCTCLASSID;
            workflow.ProductId = null;
            workflow.StatusId = initiated == true ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Processing;
            workflow.Comment = entity.comment;
            workflow.Amount = entity.amount;
            workflow.DeferredExecution = true;

            workflow.LogActivity(); // ------------------- LOG ONCE

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                loanApplication.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                loanApplication.AVAILMENTDATE = DateTime.Now;


                //CHECKING FOR COMMERCIAL LOANS IN LOOP
                foreach (var record in loanApplicationDetails)
                {
                    if (record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialPaper)
                    {
                        record.EFFECTIVEDATE = DateTime.Now;
                        record.EXPIRYDATE = (DateTime.Now.AddDays(record.APPROVEDTENOR));
                    }
                    else if (record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan || record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                    {
                        if (record.STATUSID == (short)ApprovalStatusEnum.Approved)
                        {
                            var request = new TBL_LOAN_BOOKING_REQUEST
                            {
                                AMOUNT_REQUESTED = record.APPROVEDAMOUNT,
                                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                                LOANAPPLICATIONDETAILID = record.LOANAPPLICATIONDETAILID,
                                DATETIMECREATED = DateTime.Now,
                                CREATEDBY = entity.staffId,
                            };
                            context.TBL_LOAN_BOOKING_REQUEST.Add(request);
                        }
                    }
                    else if (record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || record.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating)
                    {
                        if(loanApplication.PRODUCTCLASSID != 0 && loanApplication.PRODUCTCLASSID != null)
                        {
                            if (loanApplication.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.ProductBased)
                            {
                                if (record.STATUSID == (short)ApprovalStatusEnum.Approved)
                                {
                                    var request = new TBL_LOAN_BOOKING_REQUEST
                                    {
                                        AMOUNT_REQUESTED = record.APPROVEDAMOUNT,
                                        APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                                        LOANAPPLICATIONDETAILID = record.LOANAPPLICATIONDETAILID,
                                        DATETIMECREATED = DateTime.Now,
                                        CREATEDBY = entity.staffId,
                                    };
                                    context.TBL_LOAN_BOOKING_REQUEST.Add(request);
                                }
                            }
                        }
                    }
                };
            }

            context.SaveChanges();

            if (workflow.NewState == (int)ApprovalState.Ended)
                return true;
            else
                return false;


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

        public bool ApproveOfferLetterGeneration(LoanAvailmentApprovalViewModel model)
        {
            var operationId = (int)OperationsEnum.OfferLetterApproval;
            var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);
            if (appl == null) throw new Exception("Loan application with the given reference number not found!");

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
                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;

                if (appl.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees) // Bonds and Guarantees adapter
                {
                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress;
                    context.SaveChanges(); // save changes at this point

                    workflow.ProductClassId = appl.PRODUCTCLASSID;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.Comment = "Bonds and Guarantees document process started";
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();
                }
                else
                {
                    workflow.OperationId = (int)OperationsEnum.LoanAvailment;
                    workflow.ProductClassId = appl.PRODUCTCLASSID;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.Comment = "Offer letter approved";
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();
                }
            }

            return context.SaveChanges() > 0;
        }

        //public bool LogApplicationForApprovalDuringAvailment(LoanAvailmentApprovalViewModel model)
        //{
        //    try
        //    {
        //        var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);

        //        var entity = new ApprovalViewModel
        //        {
        //            staffId = model.createdBy,
        //            companyId = model.companyId,
        //            approvalStatusId = (int)ApprovalStatusEnum.Pending,
        //            targetId = target.LOANAPPLICATIONID,
        //            operationId = model.operationId,
        //            comment = model.comment,
        //            amount = model.amount,
        //            BranchId = model.BranchId,
        //            externalInitialization = false
        //        };

        //        return workflow.LogForApproval(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public IQueryable<CamProcessedLoanViewModel> GetApplicationsUnderForReview(int companyId)
        //{
        //    var data = GetCamProcessedLoanApplications(companyId).Where(x => x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationUnderReview);

        //    return data;
        //}

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
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.AvailmentInProgress;
                PassApplicationToOperation(model.companyId, model.createdBy, (int)OperationsEnum.LoanAvailment, model.applicationId, "B&G application for availment...");
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
                        where b.APPLICATIONREFERENCENUMBER == applicationRefNumber && a.OPERATIONID == (int)OperationsEnum.LoanAvailment
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
        
        private void PassApplicationToOperation(int companyId, int staffId, int operationId, int targetId, string comment)
        {
            workflow.StaffId = staffId;
            workflow.CompanyId = companyId;
            workflow.OperationId = operationId;
            workflow.TargetId = targetId;
            workflow.ProductClassId = null;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = comment;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
        }

    }
}
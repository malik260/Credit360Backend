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
            ILoanRepository _loans)
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            approvalLevel = _approvallevel;
            workflow = _workflow;
            loans = _loans;
        }

        #region OfferLetter & Availment Process

        private IQueryable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                        from c in cam.DefaultIfEmpty()
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCU on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                        from d in camDoc.DefaultIfEmpty()
                        join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                        from e in apprTrail.DefaultIfEmpty()
                        where a.COMPANYID == companyId && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            appraisalMemorandumId = c.APPRAISALMEMORANDUMID,
                            customerId = a.TBL_CUSTOMER.CUSTOMERID,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.LOANTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = a.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = c.CAMREF,
                            camDocumentation = d.CAMDOCUMENTATION,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            branchId = a.BRANCHID,
                            productClassId = b.TBL_PRODUCT.PRODUCTCLASSID,
                            productTypeId = b.TBL_PRODUCT.PRODUCTTYPEID,
                            productClassName = a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                            dateTimeCreated = a.DATETIMECREATED,
                            availmentlDate = a.AVAILMENTDATE,
                            approvalDate = a.APPROVEDDATE,
                            camDocuments = c.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Where(x => x.APPRAISALMEMORANDUMID == d.APPRAISALMEMORANDUMID)
                                .Select(camDoc => new CamDocumentViewModel
                                {
                                    appraisalMemorandumId = camDoc.APPRAISALMEMORANDUMID,
                                    approvalLevelId = camDoc.APPROVALLEVELID,
                                    approvalLevelName = camDoc.TBL_APPROVAL_LEVEL.LEVELNAME,
                                    camDocumentation = camDoc.CAMDOCUMENTATION
                                }
                            ).ToList(),
                            loanApplicationCollateral = (from e in context.TBL_LOAN_APPLICATION_COLLATERL.Where(s => s.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                                                         select new LoanApplicationCollateralViewModel
                                                         {
                                                             collateralValue = e.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                             collateralType = e.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                                             collateralCustomerId = e.COLLATERALCUSTOMERID,
                                                             collateralSubtype = e.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB
                                                             .Where(p => p.COLLATERALSUBTYPEID == e.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                                                             collateralReferenceNumber = e.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                                                             haircut = e.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                             valuationCycle = e.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                             allowSharing = e.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                                                             currencyCode = e.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE
                                                         }).ToList(),

                            operationId = e.OPERATIONID,
                            currentApprovalStateId = e.APPROVALSTATEID,
                            approvalStatusId = e.APPROVALSTATUSID,
                        });

            //var forDebugging = data.ToList();

            return data;
        }

        public bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId)
        {
            var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber);

            //if (target != null && target.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees)
            //{
            //    target.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.BondAndGuaranteesInProgress;
            //    return context.SaveChanges() > 0;
            //}
            //else
            //{ 
            switch (applicationStatusId)
            {
                case (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress:
                    if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                    {
                        target.APPLICATIONSTATUSID =
                            (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;

                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted:
                    if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted)
                    {
                        target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted;
                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.OfferLetterReviewInProgress:
                    if (target.APPLICATIONSTATUSID !=
                        (short)LoanApplicationStatusEnum.OfferLetterReviewInProgress)
                    {
                        target.APPLICATIONSTATUSID =
                            (short)LoanApplicationStatusEnum.OfferLetterReviewInProgress;
                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted:
                    if (target.APPLICATIONSTATUSID !=
                        (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted)
                    {
                        target.APPLICATIONSTATUSID =
                            (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted;
                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.AvailmentInProgress:
                    if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.AvailmentInProgress)
                    {
                        target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.AvailmentCompleted:
                    if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.AvailmentCompleted)
                    {
                        target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;

                        return context.SaveChanges() > 0;
                    }
                    return true;

                case (short)LoanApplicationStatusEnum.ApplicationUnderReview:
                    if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.ApplicationUnderReview)
                    {
                        target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationUnderReview;

                        return context.SaveChanges() > 0;
                    }
                    return true;

                default:
                    return false;
            }
            //}
            //else
            //{
            //switch (applicationStatusId)
            //{
            //    case (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress:
            //        if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress)
            //        {
            //            target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress;
            //            return context.SaveChanges() > 0;
            //        }
            //        return true;
            //}
            //}

            //return false;

            //public IEnumerable
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int staffId, int companyId)
        {
            var camProcessedData = GetCamProcessedLoanApplications(companyId)
                .Where(x =>
                    x.applicationStatusId == (short)LoanApplicationStatusEnum.CAMCompleted
                    || x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                )
                .GroupBy(c => c.loanApplicationId)
                .Select(y => y.FirstOrDefault())
                .OrderByDescending(b => b.loanApplicationId);

            //var test = camProcessedData.ToList();

            return camProcessedData;
        }

        //public IQueryable<CamProcessedLoanViewModel> GetApplicationsDueBondAndGuarantees(int staffId, int companyId)
        //{
        //    var camProcessedData = GetCamProcessedLoanApplications(companyId).Where(x =>
        //        x.applicationStatusId == (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress || x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
        //        .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());

        //    return camProcessedData;
        //}

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int staffId, int companyId)
        {
            //var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.OfferLetterApproval);
            //int staffApprovalLevelId = 0;
            //if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;
            //var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(companyId).Where(x => x.operationId == (int)OperationsEnum.OfferLetterApproval).ToList();

            var staffApprovalLevelIds =
                context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval)// && x.PRODUCTCLASSID == classId)
                .Select(x => x.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL.Where(l => l.ISACTIVE == true))
                .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF.Where(s => s.STAFFID == staffId))
                .Select(x => x.APPROVALLEVELID)
                .ToList();

            IQueryable<CamProcessedLoanViewModel> data;

            data = (from a in context.TBL_LOAN_APPLICATION
                    join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                    join c in context.TBL_CREDIT_APPRAISAL_MEMORANDM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                    from c in cam.DefaultIfEmpty()
                    join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCU on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                    from d in camDoc.DefaultIfEmpty()
                    join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                    from e in apprTrail.DefaultIfEmpty()
                    where a.COMPANYID == companyId && a.DELETED == false
                          && b.STATUSID == (int)ApprovalStatusEnum.Approved
                          && e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                          && e.RESPONSESTAFFID == null
                          && e.OPERATIONID == (int)OperationsEnum.OfferLetterApproval
                           //&& e.TOAPPROVALLEVELID == staffApprovalLevelId
                           && staffApprovalLevelIds.Contains((int)e.TOAPPROVALLEVELID)
                    select new CamProcessedLoanViewModel
                    {
                        loanApplicationId = a.LOANAPPLICATIONID,
                        applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                        customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = a.LOANTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                        customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                        customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                        relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                        relationshipManagerId = a.RELATIONSHIPMANAGERID,
                        loanTypeId = a.LOANTYPEID,
                        loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                        camReference = c.CAMREF,
                        camDocumentation = d.CAMDOCUMENTATION,
                        approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                        applicationDate = a.APPLICATIONDATE,
                        applicationStatusId = a.APPLICATIONSTATUSID,
                        subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                        //approvalLevelId = staffApprovalLevelId,
                        operationId = e.OPERATIONID,
                        currentApprovalStateId = e.APPROVALSTATEID,
                        productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                        productClassId = a.PRODUCTCLASSID,
                        camDocuments = c.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Where(x => x.APPRAISALMEMORANDUMID == d.APPRAISALMEMORANDUMID)
                                .Select(camDoc => new CamDocumentViewModel
                                {
                                    appraisalMemorandumId = camDoc.APPRAISALMEMORANDUMID,
                                    approvalLevelId = camDoc.APPROVALLEVELID,
                                    approvalLevelName = camDoc.TBL_APPROVAL_LEVEL.LEVELNAME,
                                    camDocumentation = camDoc.CAMDOCUMENTATION
                                }
                            ).ToList(),
                    });

            var applicationDueForReview = data
                //.Where(x =>
                //x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted 
                //|| x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress
                //)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault()).OrderByDescending(b => b.loanApplicationId); ;

            return applicationDueForReview;
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId)
        {
            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanAvailment);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(companyId).Where(x => x.operationId == (int)OperationsEnum.LoanAvailment).ToList();

            IQueryable<CamProcessedLoanViewModel> data;

            IQueryable<CamProcessedLoanViewModel> loanAvailmentData;

            var existOnApprovalTrail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.LoanAvailment).Select(x => x.TARGETID).ToList();

            // Check if the current staff is the first level
            if (staffApprovalLevelId == approvalLvlStaff[0].approvalLevelId)
            {
                // meaning it does not exist on the approval trail yet
                data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted 
                || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());

                loanAvailmentData = data.Where(x => !existOnApprovalTrail.Contains(x.loanApplicationId));

            }
            else
            {
                data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                        from c in cam.DefaultIfEmpty()
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCU on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                        from d in camDoc.DefaultIfEmpty()
                        join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                        from e in apprTrail.DefaultIfEmpty()
                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                              e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                               //e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending ||
                          && e.RESPONSESTAFFID == null
                          && e.OPERATIONID == (int)OperationsEnum.LoanAvailment && e.TOAPPROVALLEVELID == staffApprovalLevelId
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.LOANTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            productTypeId = b.TBL_PRODUCT.PRODUCTTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = c.CAMREF,
                            camDocumentation = d.CAMDOCUMENTATION,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            approvalLevelId = staffApprovalLevelId,
                            operationId = e.OPERATIONID,
                            currentApprovalStateId = e.APPROVALSTATEID,
                            productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                            camDocuments = c.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Where(x => x.APPRAISALMEMORANDUMID == d.APPRAISALMEMORANDUMID)
                                .Select(camDoc => new CamDocumentViewModel
                                {
                                    appraisalMemorandumId = camDoc.APPRAISALMEMORANDUMID,
                                    approvalLevelId = camDoc.APPROVALLEVELID,
                                    approvalLevelName = camDoc.TBL_APPROVAL_LEVEL.LEVELNAME,
                                    camDocumentation = camDoc.CAMDOCUMENTATION
                                }
                            ).ToList(),
                            loanApplicationCollateral = (from r in context.TBL_LOAN_APPLICATION_COLLATERL.Where(s => s.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
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

                loanAvailmentData = data.Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault()).OrderByDescending(b => b.loanApplicationId); ;
            }

            return loanAvailmentData;
        }
        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentCheckList(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_LOAN_CONDITION_PRECEDENT on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                      
                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                               (a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.OfferLetterReviewCompleted
                               || a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.LOANTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            productTypeId = b.TBL_PRODUCT.PRODUCTTYPEID,
                            productName = b.TBL_PRODUCT.PRODUCTNAME,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                        });

            return data.GroupBy(x => x.loanApplicationId).Select(y => y.FirstOrDefault()).ToList();
        }
        public Form3800ViewModel GenerateForm3800Template(string applicationRefNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

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

            //var data    = (from a in context.TBL_LOAN_APPLICATION
            //                join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
            //                join d in context.TBL_CUSTOMER on a.CUSTOMERID equals d.CUSTOMERID
            //                join e in context.TBL_BRANCH on a.BRANCHID equals e.BRANCHID
            //                where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
            //                select new CamProcessedLoanViewModel()
            //                {
            //                    productId = c.TBL_PRODUCT.PRODUCTID,
            //                    productName = c.TBL_PRODUCT.PRODUCTNAME,
            //                    productClassId = a.PRODUCTCLASSID,
            //                    productClassProcessId = productClassProcess.PRODUCT_CLASS_PROCESSID, //a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID
            //                    branchId = e.BRANCHID,
            //                    branchName = e.BRANCHNAME,
            //                    customerId = d.CUSTOMERID,
            //                    customerCode = d.CUSTOMERCODE,
            //                    customerName = d.LASTNAME + ' ' + d.MIDDLENAME + ' ' +  d.FIRSTNAME
            //                }).ToList();





            var conditions = string.Empty;

            var internalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == false).ToList();

            var externalConditionsPrecedents = conditionPrecedents.Where(x => x.isExternal == true).ToList();

            var internalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == false).ToList();

            var externalConditionsSubsequents = conditionSubsequents.Where(x => x.isExternal == true).ToList();

            int noOfInternalConditions = 0;

            int noOfExternalConditions = 0;

            var finalConditionPrecedents = string.Empty;

            var finalConditionSubsequents = string.Empty;

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

            var conditionPrecedentData = $"{finalConditionPrecedents} {finalConditionSubsequents}";

            var customer = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_CUSTOMER.FIRSTNAME ;
            var branch  = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).TBL_BRANCH.BRANCHNAME;
            //var info = data;

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData, templateLink, branch, customer);

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

        private static string PopulateTemplatePlaceholders(DateTime applicationDate, string conditionPrecedent, string template,string branch, string customer)
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
                    if (appl == null) throw new Exception("Loan application with the given reference number not found!");
                    //appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterRejected;
                }

                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            int operationId = (int)OperationsEnum.LoanAvailment;
            int staffApprovalLevelId = 0;
            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(entity.staffId, entity.companyId, operationId);
            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == operationId).ToList();
            var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);
            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            workflow.StaffId = entity.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.CompanyId = entity.companyId;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.ProductId = null;
            workflow.StatusId = entity.approvalStatusId;
            workflow.Comment = entity.comment;
            workflow.Amount = entity.amount;
            workflow.DeferredExecution = true;
            
            workflow.LogActivity(); // ------------------- LOG ONCE

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                appl.AVAILMENTDATE = DateTime.Now;

                var loanApplication = appl; // context.TBL_LOAN_APPLICATION.Find(entity.targetId);
<<<<<<< HEAD
                if(loanApplication.PRODUCTCLASSID != 0 || loanApplication.PRODUCTCLASSID != null)
                {
                    if(loanApplication.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.ProductBased)
=======
                if (loanApplication.PRODUCTCLASSID != 0 || loanApplication.PRODUCTCLASSID != null)
                {
                    if (loanApplication.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.ProductBased)
>>>>>>> a6eae90d708da213ea2a3df29405a32df3711c53
                    {
                        var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID);
                        foreach (var record in loanApplicationDetails)
                        {
                            var request = new TBL_LOAN_BOOKING_REQUEST
                            {
                                AMOUNT_REQUESTED = entity.amount,
                                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                                LOANAPPLICATIONDETAILID = record.LOANAPPLICATIONDETAILID,
                                DATETIMECREATED = DateTime.Now,
                                CREATEDBY = entity.staffId,
                            };
                            context.TBL_LOAN_BOOKING_REQUEST.Add(request);
                        };
<<<<<<< HEAD
                        
                    }
                }

            }

            //if (entity.amount > (long)LoanAvailmentApprovalFlowEnum.LevelOne && staffApprovalLevelId == approvalLvlStaff[0].approvalLevelId)
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
            //}
            //else
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
            //}

            //if (entity.amount > (long)LoanAvailmentApprovalFlowEnum.LevelTwo)
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
            //}
            //else
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
            //}

            //if(entity.amount > (long)LoanAvailmentApprovalFlowEnum.LevelThree)
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
            //}
            //else
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
            //}
            //if (entity.amount >= (long)LoanAvailmentApprovalFlowEnum.LevelFour)
            //{
            //    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
            //}
=======

                    }
                }
>>>>>>> a6eae90d708da213ea2a3df29405a32df3711c53

            }
            
            return context.SaveChanges() > 0;
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

                if (appl.PRODUCTCLASSID == 10) // Bonds and Guarantees adapter
                {
                    appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BondAndGuaranteesInProgress;
                    context.SaveChanges(); // save changes at this point

                    workflow.ProductClassId = appl.PRODUCTCLASSID;
                    workflow.ProductId = null;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.Comment = "Bonds and Guarantees document process started";
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();
                }
            }

            return context.SaveChanges() > 0;
        }

        public bool LogApplicationForApprovalDuringAvailment(LoanAvailmentApprovalViewModel model)
        {
            try
            {
                var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);

                var entity = new ApprovalViewModel
                {
                    staffId = model.createdBy,
                    companyId = model.companyId,
                    approvalStatusId = (int)ApprovalStatusEnum.Pending,
                    targetId = target.LOANAPPLICATIONID,
                    operationId = model.operationId,
                    comment = model.comment,
                    amount = model.amount,
                    BranchId = model.BranchId,
                    externalInitialization = false
                };

                return workflow.LogForApproval(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IQueryable<CamProcessedLoanViewModel> GetApplicationsUnderForReview(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x => x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationUnderReview);

            return data;
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
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.AvailmentInProgress;
            }

            return context.SaveChanges() > 0;
        }

        #endregion Bonds and Guarantees

    }
}
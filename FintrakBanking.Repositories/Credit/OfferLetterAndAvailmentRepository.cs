using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace FintrakBanking.Repositories.Credit
{
    public class OfferLetterAndAvailmentRepository : IOfferLetterAndAvailmentRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository approvalLevel;

        public OfferLetterAndAvailmentRepository(IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workFlow)
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            approvalLevel = _approvallevel;
            workFlow = _workFlow;
        }

        #region OfferLetter & Availment Process

        private IQueryable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                        from c in cam.DefaultIfEmpty()
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                        from d in camDoc.DefaultIfEmpty()
                        join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                        from e in apprTrail.DefaultIfEmpty()
                        where a.COMPANYID == companyId && a.DELETED == false && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            appraisalMemorandumId = c.APPRAISALMEMORANDUMID,
                            customerId = a.TBL_CUSTOMER.CUSTOMERID,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
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
                            camDocuments = c.TBL_CREDIT_APPRAISAL_MEMO_DOCUM.Where(x => x.APPRAISALMEMORANDUMID == d.APPRAISALMEMORANDUMID)
                                .Select(camDoc => new CamDocumentViewModel
                                {
                                    appraisalMemorandumId = camDoc.APPRAISALMEMORANDUMID,
                                    approvalLevelId = camDoc.APPROVALLEVELID,
                                    approvalLevelName = camDoc.TBL_APPROVAL_LEVEL.LEVELNAME,
                                    camDocumentation = camDoc.CAMDOCUMENTATION
                                }
                            ).ToList(),
                            operationId = e.OPERATIONID,
                            currentApprovalStateId = e.APPROVALSTATEID,
                            approvalStatusId = e.APPROVALSTATUSID,
                        });

            var forDebugging = data.ToList();

            return data;
        }

        public bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId)
        {
            var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER ==
                applicationRefNumber.ToString());

            if (target != null)
            {
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

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress:
                        if (target.APPLICATIONSTATUSID !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress)
                        {
                            target.APPLICATIONSTATUSID =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted:
                        if (target.APPLICATIONSTATUSID !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted)
                        {
                            target.APPLICATIONSTATUSID =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted;
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
            }

            return false;

            //public IEnumerable
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int staffId, int companyId)
        {
            var camProcessedData = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.CAMCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault()).ToList();
            return camProcessedData;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int staffId, int companyId)
        {
            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.OfferLetterApproval);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(companyId).Where(x => x.operationId == (int)OperationsEnum.OfferLetterApproval).ToList();

            IQueryable<CamProcessedLoanViewModel> data;

            data = (from a in context.TBL_LOAN_APPLICATION
                    join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                    join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                    from c in cam.DefaultIfEmpty()
                    join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                    from d in camDoc.DefaultIfEmpty()
                    join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                    from e in apprTrail.DefaultIfEmpty()
                    where a.COMPANYID == companyId && a.DELETED == false
                          && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                          e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            && e.RESPONSESTAFFID == null
                      && e.OPERATIONID == (int)OperationsEnum.OfferLetterApproval && e.TOAPPROVALLEVELID == staffApprovalLevelId
                    select new CamProcessedLoanViewModel
                    {
                        loanApplicationId = a.LOANAPPLICATIONID,
                        applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                        customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
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
                        approvalLevelId = staffApprovalLevelId,
                        operationId = e.OPERATIONID,
                        currentApprovalStateId = e.APPROVALSTATEID,
                        camDocuments = c.TBL_CREDIT_APPRAISAL_MEMO_DOCUM.Where(x => x.APPRAISALMEMORANDUMID == d.APPRAISALMEMORANDUMID)
                                .Select(camDoc => new CamDocumentViewModel
                                {
                                    appraisalMemorandumId = camDoc.APPRAISALMEMORANDUMID,
                                    approvalLevelId = camDoc.APPROVALLEVELID,
                                    approvalLevelName = camDoc.TBL_APPROVAL_LEVEL.LEVELNAME,
                                    camDocumentation = camDoc.CAMDOCUMENTATION
                                }
                            ).ToList(),
                    });

            var applicationDueForReview = data.Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault()).ToList();
            return applicationDueForReview;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId)
        {
            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanAvailment);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(companyId).Where(x => x.operationId == (int)OperationsEnum.LoanAvailment).ToList();

            IQueryable<CamProcessedLoanViewModel> data;

            IEnumerable<CamProcessedLoanViewModel> loanAvailmentData;

            var existOnApprovalTrail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == (int)OperationsEnum.LoanAvailment).Select(x => x.TARGETID).ToList();

            // Check if the current staff is the first level
            if (staffApprovalLevelId == approvalLvlStaff[0].approvalLevelId)
            {
                // meaning it does not exist on the approval trail yet
                data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());

                loanAvailmentData = data.Where(x => !existOnApprovalTrail.Contains(x.loanApplicationId)).ToList();
            }
            else
            {
                data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID into cam
                        from c in cam.DefaultIfEmpty()
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID into camDoc
                        from d in camDoc.DefaultIfEmpty()
                        join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                        from e in apprTrail.DefaultIfEmpty()
                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                              e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                && e.RESPONSESTAFFID == null
                          && e.OPERATIONID == (int)OperationsEnum.LoanAvailment && e.TOAPPROVALLEVELID == staffApprovalLevelId
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
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
                            currentApprovalStateId = e.APPROVALSTATEID
                        });

                loanAvailmentData = data.Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault()).ToList();
            }

            return loanAvailmentData;
        }

        public Form3800ViewModel GenerateForm3800Template(string applicationRefNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
                                       join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                       join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = b.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == true
                                        select new OfferLetterConditionPrecidentViewModel()
                                        {
                                            conditionPrecident = b.CONDITION,
                                            loanApplicationId = b.LOANAPPLICATIONID,
                                            isExternal = b.ISEXTERNAL,
                                            productName = c.TBL_PRODUCT.PRODUCTNAME
                                        }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var products = (from a in context.TBL_LOAN_APPLICATION
                            join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                            where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                            select new ProductViewModel()
                            {
                                productId = c.TBL_PRODUCT.PRODUCTID,
                                productName = c.TBL_PRODUCT.PRODUCTNAME
                            }).ToList();

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

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData);

            if (preparedTemplate != null)
            {
                return new Form3800ViewModel { documentTemplate = preparedTemplate };
            }

            return new Form3800ViewModel { };
        }

        public OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
                                       join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                       join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = b.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == true
                                        select new OfferLetterConditionPrecidentViewModel()
                                        {
                                            conditionPrecident = b.CONDITION,
                                            loanApplicationId = b.LOANAPPLICATIONID,
                                            isExternal = b.ISEXTERNAL,
                                            productName = c.TBL_PRODUCT.PRODUCTNAME
                                        }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var products = (from a in context.TBL_LOAN_APPLICATION
                            join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                            where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                            select new ProductViewModel()
                            {
                                productId = c.TBL_PRODUCT.PRODUCTID,
                                productName = c.TBL_PRODUCT.PRODUCTNAME
                            }).ToList();

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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100 %; overflow-x:auto; margin-bottom:5px'><tbody>" +
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
                        $"<table border='1' cellspacing='0' class='conditionsTable_OL' style='width: 100 %; overflow-x:auto; margin-bottom:5px'><tbody>" +
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

            var preparedTemplate = PopulateTemplatePlaceholders(applDate, conditionPrecedentData);

            if (preparedTemplate != null)
            {
                return new OfferLetterTemplateViewModel { documentTemplate = preparedTemplate };
            }

            return new OfferLetterTemplateViewModel { };
        }

        private static string PopulateTemplatePlaceholders(DateTime applicationDate, string conditionPrecedent)
        {
            string body;

            string templateLink = "~/EmailTemplates/FORM-3800B-Template.html";

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{@ApplicationDate}", applicationDate.ToLongDateString());
            body = body.Replace("{@ConditionPrecedents}", conditionPrecedent);

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
                            isAccepted = (bool)a.ISACCEPTED
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

                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.LoanAvailment;

            entity.externalInitialization = false;

            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(entity.staffId, entity.companyId, (int)OperationsEnum.LoanAvailment);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == (int)OperationsEnum.LoanAvailment).ToList();

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var targetLoanAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x =>
                        x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);

                    ForwardViewModel forward;

                    if (entity.amount >= (long)LoanAvailmentApprovalFlowEnum.LevelTwo && entity.amount <= (long)LoanAvailmentApprovalFlowEnum.LevelThree)
                    {
                        if (staffApprovalLevelId != approvalLvlStaff[2].approvalLevelId) // forward only if the approval level Id is not the third level
                        {
                            forward = new ForwardViewModel
                            {
                                createdBy = entity.createdBy,
                                amount = entity.amount,
                                companyId = entity.companyId,
                                receiverLevelId = approvalLvlStaff[2].approvalLevelId,
                                receiverStaffId = approvalLvlStaff[2].staffId,
                                comment = entity.comment,
                                operationId = entity.operationId,
                                applicationId = targetLoanAppl.LOANAPPLICATIONID,
                                forwardAction = entity.approvalStatusId
                            };

                            ForwardApplicationToNextLevel(forward);
                        }
                        else
                        {
                            // indicate an end to the process before logging on the trail
                            workFlow.KeepPending = false;
                            workFlow.ForcefullyEndProcess = true;

                            workFlow.LogForApproval(entity);
                        }
                    }
                    else if (entity.amount >= (long)LoanAvailmentApprovalFlowEnum.LevelThree)
                    {
                        if (staffApprovalLevelId != approvalLvlStaff[3].approvalLevelId)
                        {
                            forward = new ForwardViewModel
                            {
                                createdBy = entity.createdBy,
                                amount = entity.amount,
                                companyId = entity.companyId,
                                receiverLevelId = approvalLvlStaff[3].approvalLevelId,
                                receiverStaffId = approvalLvlStaff[3].staffId,
                                comment = entity.comment,
                                operationId = entity.operationId,
                                applicationId = targetLoanAppl.LOANAPPLICATIONID,
                                forwardAction = entity.approvalStatusId
                            };

                            ForwardApplicationToNextLevel(forward);
                        }
                        else
                        {
                            // indicate an end to the process before logging on the trail
                            workFlow.KeepPending = false;
                            workFlow.ForcefullyEndProcess = true;

                            workFlow.LogForApproval(entity);
                        }
                    }
                    else
                    {
                        // indicate an end to the process before logging on the trail
                        workFlow.KeepPending = false;
                        workFlow.ForcefullyEndProcess = true;

                        workFlow.LogForApproval(entity);
                    }

                    var b = workFlow.NextLevelId ?? 0;

                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        private bool ForwardApplicationToNextLevel(ForwardViewModel model)
        {
            workFlow.StaffId = model.createdBy;
            workFlow.OperationId = model.operationId;
            workFlow.TargetId = model.applicationId;
            workFlow.CompanyId = model.companyId;
            workFlow.Vote = model.vote;
            workFlow.ProductClassId = model.productClassId;
            workFlow.ProductId = model.productId;
            workFlow.NextLevelId = model.receiverLevelId;
            workFlow.ToStaffId = model.receiverStaffId;
            workFlow.StatusId = model.forwardAction;
            workFlow.Comment = model.comment;

            workFlow.Amount = model.amount;
            workFlow.InvestmentGrade = model.investmentGrade;
            workFlow.Tenor = model.applicationTenor;
            workFlow.PoliticallyExposed = model.politicallyExposed;

            return workFlow.LogActivity();
        }

        private bool ReferApplicationToSpecificLevel(LoanAvailmentApprovalViewModel model)
        {
            workFlow.StaffId = model.createdBy;
            workFlow.OperationId = model.operationId;
            workFlow.TargetId = model.targetId;
            workFlow.CompanyId = model.companyId;
            workFlow.NextLevelId = model.nextLevelId;
            workFlow.ToStaffId = model.toStaffId;
            workFlow.StatusId = model.approvalStatusId;
            workFlow.Comment = model.comment;

            workFlow.Amount = model.amount;

            return workFlow.LogActivity();
        }

        public bool ApproveOfferLetterGeneration(LoanAvailmentApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.OfferLetterApproval;

            entity.externalInitialization = false;

            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(entity.staffId, entity.companyId, (int)OperationsEnum.OfferLetterApproval);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == (int)OperationsEnum.OfferLetterApproval).ToList();

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var targetLoanAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x =>
                        x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);

                    var operationDetails = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.OfferLetterApproval);

                    LoanAvailmentApprovalViewModel referBack;

                    if (staffApprovalLevelId == approvalLvlStaff[1].approvalLevelId)
                    {
                        // For when offer letter has been rejected
                        if (entity.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationUnderReview)
                        {
                            UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                            entity.targetId = targetLoanAppl.LOANAPPLICATIONID;

                            entity.approvalStatusId = (short)ApprovalStatusEnum.Referred;

                            entity.keepPending = false;

                            workFlow.ForcefullyEndProcess = true;

                            workFlow.LogForApproval(entity);
                        }
                        // else If RM initiated 'Send For Availment then end the workflow process
                        else if (entity.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted)
                        {
                            // UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                            entity.targetId = targetLoanAppl.LOANAPPLICATIONID;

                            entity.keepPending = false;

                            workFlow.ForcefullyEndProcess = true;

                            workFlow.LogForApproval(entity);
                        }
                        else 
                        {
                            referBack = new LoanAvailmentApprovalViewModel()
                            {
                                nextLevelId = approvalLvlStaff[0].approvalLevelId,
                                toStaffId = approvalLvlStaff[0].staffId,
                                createdBy = entity.createdBy,
                                targetId = targetLoanAppl.LOANAPPLICATIONID,
                                amount = entity.amount,
                                companyId = entity.companyId,
                                comment = entity.comment,
                                operationId = entity.operationId,
                                approvalStatusId = entity.approvalStatusId
                            };

                            operationDetails.OPERATIONURL = "/credit/loan/offer-letter";

                            UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                            ReferApplicationToSpecificLevel(referBack);
                        }
                    }
                    else
                    {
                        operationDetails.OPERATIONURL = "/credit/loan/offer-letter-review";

                        UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                        entity.targetId = targetLoanAppl.LOANAPPLICATIONID;

                        workFlow.LogForApproval(entity);
                    }

                    var b = workFlow.NextLevelId ?? 0;

                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool LogApplicationForApprovalDuringAvailment(LoanAvailmentApprovalViewModel model)
        {
            try
            {
                var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x =>
                    x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);

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

                return workFlow.LogForApproval(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsUnderForReview(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x => x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationUnderReview).ToList();

            return data;
        }

        #endregion OfferLetter & Availment Process
    }
}
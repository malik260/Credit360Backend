using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Media
{
    public class OriginalDocumentApprovalRepository : IOriginalDocumentApprovalRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;


        public OriginalDocumentApprovalRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentApprovals(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OriginalDocumentApproval).ToList();

            return (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                   join a in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                    where x.DELETED == false && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                     && atrail.RESPONSESTAFFID == null
                     && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval

                    select new OriginalDocumentApprovalViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        approvalStatusId = x.APPROVALSTATUSID,
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = l.APPLICATIONDATE,
                        applicationAmount = l.APPLICATIONAMOUNT,
                        interestRate = l.INTERESTRATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        operationId = (int)OperationsEnum.OriginalDocumentApproval


                    })
                .ToList();
        }

        public OriginalDocumentApprovalViewModel GetOriginalDocumentApproval(int id)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.FirstOrDefault(x => x.ORIGINALDOCUMENTAPPROVALID == id && x.DELETED == false);

            return new OriginalDocumentApprovalViewModel
            {
                originalDocumentApprovalId = entity.ORIGINALDOCUMENTAPPROVALID,
                loanApplicationId = entity.LOANAPPLICATIONID,
                description = entity.DESCRIPTION,
                approvalStatusId = entity.APPROVALSTATUSID,
                applicationReferenceNumber = entity.APPLICATIONREFERNECENUMBER,
                referenceNumber = entity.REFERENCENUMBER,
                dateTimeCreated = entity.DATETIMECREATED
            };
        }
        public List<OriginalDocumentApprovalViewModel> GetOriginalDocumentByLoanApplicationId(int id)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(x => x.ORIGINALDOCUMENTAPPROVALID == id && x.DELETED == false && x.APPROVALSTATUSID==(int)ApprovalStatusEnum.Pending)
                .Select(x=> new OriginalDocumentApprovalViewModel
            {
                originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                loanApplicationId = x.LOANAPPLICATIONID,
                description = x.DESCRIPTION,
                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o=>o.APPROVALSTATUSID== x.APPROVALSTATUSID).Select(o=>o.APPROVALSTATUSNAME).FirstOrDefault(),
                applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                referenceNumber = x.REFERENCENUMBER,
                dateTimeCreated = x.DATETIMECREATED
            }).ToList();

            return entity;
        }
        public int AddOriginalDocumentApproval(OriginalDocumentApprovalViewModel model)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            var entity = new TBL_ORIGINAL_DOCUMENT_APPROVAL
            {
                LOANAPPLICATIONID = model.loanApplicationId,
                DESCRIPTION = model.description,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                DATETIMECREATED = general.GetApplicationDate(),
                APPLICATIONREFERNECENUMBER = model.applicationReferenceNumber,
                REFERENCENUMBER = referenceNumber,
                DELETED =false,
                CREATEDBY = model.createdBy,
                

            };

            context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------
            context.SaveChanges();

            return entity.ORIGINALDOCUMENTAPPROVALID;
        }

        public bool UpdateOriginalDocumentApproval(OriginalDocumentApprovalViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(id);
            entity.LOANAPPLICATIONID = model.loanApplicationId;
            entity.DESCRIPTION = model.description;
            entity.APPROVALSTATUSID = model.approvalStatusId;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ORIGINALDOCUMENTAPPROVALID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteOriginalDocumentApproval(int id, UserInfo user)
        {
            var entity = this.context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ORIGINALDOCUMENTAPPROVALID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            return (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    where x.APPLICATIONREFERENCENUMBER == searchString
                 || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                 || c.LASTNAME.ToLower().Contains(searchString.Trim())
                 || c.MIDDLENAME.ToLower().Contains(searchString.Trim())

                                select new LoanApplicationViewModel
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    customerId = c.CUSTOMERID,
                                    branchName = context.TBL_BRANCH.Where(o=>o.BRANCHID==c.BRANCHID).Select(o=>o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productName = context.TBL_PRODUCT.Where(o=>o.PRODUCTID==a.APPROVEDPRODUCTID).Select(o=>o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName =context.TBL_STAFF.Where(o=>o.STAFFID==x.RELATIONSHIPMANAGERID).Select(o=>o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval
                                }).ToList();
        }

        public bool GoForApproval(OriginalDocumentApprovalViewModel entity)
        {
            var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(entity.originalDocumentApprovalId);
            if(document != null)
            {
                document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = entity.originalDocumentApprovalId;
                workflow.Comment = "Request for Original document submission approval";
                workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

            return context.SaveChanges() != 0;

            
        }

        public bool SubmitApproval(OriginalDocumentApprovalViewModel model)
        {
            bool responce =false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.originalDocumentApprovalId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                       var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(o => o.ORIGINALDOCUMENTAPPROVALID == model.originalDocumentApprovalId).FirstOrDefault();
                        if (document!=null)
                        {
                            document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        }
                        
                    }

                    responce = context.SaveChanges() > 0;
                    transaction.Commit();

                    return responce;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }
    }
}


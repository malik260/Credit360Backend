using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class AppraisalMemorandumRepository : IAppraisalMemorandumRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

        public AppraisalMemorandumRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
        }
        
        public AppraisalMemorandumViewModel GetAppraisalMemorandumByLoanApplicationId(int id)
        {
            return this.GetAllAppraisalMemorandum().Where(x => x.loanApplicationId == id).FirstOrDefault();
        }

        public IEnumerable<AppraisalMemorandumViewModel> GetAllAppraisalMemorandum()
        {
            return this.context.tbl_Credit_Appraisal_Memorandum.Where(x => x.Deleted == false).Select(x => new AppraisalMemorandumViewModel
            {
                appraisalMemorandumId = x.AppraisalMemorandumId,
                loanApplicationId = x.LoanApplicationId,
                camRef = x.CAMRef,
                isCompleted = x.IsCompleted,
                riskRated = x.RiskRated,
                camDocumentation = x.CAMDocumentation,
                loanDetails = x.LoanDetails,
            });
        }

        public AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model)
        {
            var data = new tbl_Credit_Appraisal_Memorandum
            {
                CompanyId = model.companyId,
                LoanApplicationId = model.loanApplicationId,
                CAMRef = this.GetUniqueReferenceNumber(2),
                IsCompleted = model.isCompleted,
                RiskRated = model.riskRated,
                CAMDocumentation = model.camDocumentation,
                LoanDetails = "",
                CreatedBy = model.createdBy,
                DateTimeCreated = DateTime.Now
            };

            var memo = context.tbl_Credit_Appraisal_Memorandum.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added AppraisalMemorandum '{ model.camRef }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            this.FlagSubmittedForAppraisal(model.loanApplicationId);

            context.SaveChanges();

            return new AppraisalMemorandumViewModel
            {
                appraisalMemorandumId = memo.AppraisalMemorandumId,
                loanApplicationId = memo.LoanApplicationId,
                camRef = memo.CAMRef,
                isCompleted = memo.IsCompleted,
                riskRated = memo.RiskRated,
                camDocumentation = memo.CAMDocumentation,
                loanDetails = memo.LoanDetails,
            };
        }

        private bool FlagSubmittedForAppraisal(int id)
        {
            var application = context.tbl_Loan_Application.Find(id);
            if (application != null)
            {
                application.SubmittedForAppraisal = true;
                return true;
            }
            return false;
        }

        private string GetUniqueReferenceNumber(int type)
        {
            int size = 16;
            byte[] data = new byte[size];
            System.Security.Cryptography.RNGCryptoServiceProvider crypto = new System.Security.Cryptography.RNGCryptoServiceProvider();
            crypto.GetBytes(data);
            return BitConverter.ToString(data).Replace("-", String.Empty);
        }

        public bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId)
        {
            var data = this.context.tbl_Credit_Appraisal_Memorandum.Find(appraisalMemorandumId);
            if (data == null)
            {
                return false;
            }

            data.CAMDocumentation = model.camDocumentation;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated AppraisalMemorandum '{ model.camRef }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public async Task<bool> ForwardAppraisalMemorandum(ForwardViewModel model)
        {
            var operationId = (int)OperationsEnum.CAM;

            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = model.productClassId;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId; //?status eror if not provided & error if assign but used
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            await workflow.LogActivity();

            if (workflow.Saved)
            {
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                    StaffId = model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Forward AppraisalMemorandum '{ model.applicationId }' ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = general.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                var appl = context.tbl_Loan_Application.Find(model.applicationId);
                appl.ApprovalStatusId = workflow.StatusId;

                context.SaveChanges();
            }

            return workflow.Saved;
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId)
        {
            var allstaff = this.GetAllStaffNames();

            return this.context.tbl_Approval_Trail
                .Where(x => x.OperationId == (int)OperationsEnum.CAM && x.TargetId == applicationId)
                .Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.ApprovalTrailId,
                    targetId = x.TargetId,
                    arrivalDate = x.ArrivalDate,
                    systemArrivalDateTime = x.SystemArrivalDateTime,
                    responseDate = x.ResponseDate,
                    systemResponseDateTime = x.SystemResponseDateTime,
                    responseStaffId = x.ResponseStaffId,
                    requestStaffId = x.RequestStaffId,
                    fromApprovalLevelId = x.FromApprovalLevelId,
                    toApprovalLevelId = (int)x.ToApprovalLevelId,
                    approvalStateId = x.ApprovalStateId,
                    approvalStatusId = x.ApprovalStatusId,
                    comment = x.Comment,
                    staffName = allstaff.FirstOrDefault(s => s.id == x.RequestStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.RequestStaffId).name,
                }).OrderByDescending(x => x.approvalTrailId);
        }

        public PrivilegeViewModel GetUserPrivilege(int staffId, int applicationId)
        {
            var privilege = new PrivilegeViewModel();

            var application = this.context.tbl_Loan_Application.Find(applicationId);
            var grant = context.tbl_Approval_Group_Mapping
                                    .Where(x => x.OperationId == (int)OperationsEnum.CAM && x.ProductClassId == application.tbl_Product.ProductClassId)
                                .SelectMany(x => x.tbl_Approval_Level)
                                .SelectMany(x => x.tbl_Approval_Level_Staff)
                                    .Where(x => x.StaffId == staffId)
                                    .SingleOrDefault();
            if (grant != null)
            {
                return new PrivilegeViewModel
                {
                    viewCamDocument = grant.CanViewCAMDocument,
                    viewUploadedFiles = grant.CanViewUploadedFile,
                    viewApproval = grant.CanViewApproval,
                    canMakeChanges = grant.CanEdit,
                    canAppendTemplate = grant.CanEdit,
                    canApprove = grant.CanApprove,
                    canUploadFile = grant.CanUploadFile,
                    canSendRequest = grant.CanSendJobRequest,
                };
            }

            return privilege;
        }

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.tbl_Staff.Select(s => new OperationStaffViewModel
            {
                id = s.StaffId,
                name = s.LastName + " " + s.FirstName
            });
        }

        private bool RunningProcess(int operationId, int targetId)
        {
            var trail = context.tbl_Approval_Trail.FirstOrDefault(x => x.OperationId == operationId && x.TargetId == targetId);
            if (trail == null) { return false; }
            return true;
        }
    }
}

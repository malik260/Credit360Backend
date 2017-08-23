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

namespace FintrakBanking.Repositories.Credit
{
    public class AppraisalMemorandumRepository : IAppraisalMemorandumRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkFlowRepository workflow;

        public AppraisalMemorandumRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkFlowRepository workflow)
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

            var response = this.workflow.GoForApproval(new ApprovalViewModel
            {
                operationId = (int)Operations.CAM,
                targetId = model.loanApplicationId,
                myLevelId = 4, //model.levelId,
                approvalStatusId = 0,
                operationURL = model.applicationUrl,
                comment = "TEST CAM", //model.comment,
                amount = 1000000, //model.loanAmount,
                isPoliticalyExposed = false, //model.politicalyExposed,
            });

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

        public bool AppendTemplate(AppraisalMemorandumViewModel model, int appraisalMemorandumId, int userId)
        {
            var data = this.context.tbl_Credit_Appraisal_Memorandum.Find(appraisalMemorandumId);
            if (data == null)
            {
                return false;
            }

            data.CAMDocumentation = model.camDocumentation; // TODO
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumUpdated, // TODO
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

            //var levelStaff = context.tbl_Approval_Group_Mapping.Where(x=>x.
                
                //GetStaffLevel(model.lastUpdatedBy, model.companyId, (int)Operations.CAM);

            //var response = this.workflow.GoForApproval(new ApprovalViewModel
            //{
            //    operationId = (int)Operations.CAM,
            //    targetId = 2,
            //    myLevelId = levelId,
            //    nextLevelId = 2,
            //    approvalStatusId = 1,
            //    amount = 2000000,
            //    comment = "TEST CAM",
            //    operationURL = "",
            //    isPoliticalyExposed = false,
            //});

            return context.SaveChanges() != 0;
        }

        public ForwardViewModel ForwardAppraisalMemorandum(ForwardViewModel model)
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

            context.SaveChanges();

            return new ForwardViewModel
            {
                comment = string.Empty,
                applicationId = 1,
                receiverStaffId = 1
            };
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId)
        {
            var allstaff = this.GetAllStaffNames();

            return this.context.tbl_Approval_Trail
                .Where(x => x.OperationId == (int)Operations.CAM && x.TargetId == applicationId)
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
                    toApprovalLevelId = x.ToApprovalLevelId,
                    approvalStateId = x.ApprovalStateId,
                    approvalStatusId = x.ApprovalStatusId,
                    comment = x.Comment,
                    staffName = allstaff.FirstOrDefault(s => s.id == x.ResponseStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.ResponseStaffId).name,
                }).OrderByDescending(x => x.approvalTrailId);
        }
        
        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.tbl_Staff.Select(s => new OperationStaffViewModel
            {
                id = s.StaffId,
                name = s.LastName + " " + s.FirstName
            });
        }

        //public bool PushIntoWorkflowProcess(ApprovalViewModel entity)
        //{
        //    return true;
        //}

        //public bool PushToNext() {
        //    return true;
        //}


    }
}

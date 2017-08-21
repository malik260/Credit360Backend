using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class AppraisalMemorandumRepository : IAppraisalMemorandumRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public AppraisalMemorandumRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
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
                LoanDetails = "ujigfghjhg",
                CreatedBy = 1,
                DateTimeCreated = DateTime.Now
            };

            var memo = context.tbl_Credit_Appraisal_Memorandum.Add(data);

            // if (memo == null) { return null; }

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

            return context.SaveChanges() != 0;
        }



    }
}

using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class CreditTemplateRepository : ICreditTemplateRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public CreditTemplateRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddCreditTemplate(CreditTemplateViewModel model)
        {
            if (String.IsNullOrEmpty(model.templateDocument)) { throw new Exception("Document is blank. Cannot create a blank document!"); }

            var data = new tbl_Credit_Template
            {
                CompanyId = model.companyId,
                TemplateTitle = model.templateTitle,
                TemplateDocument = model.templateDocument,
                ApprovalLevelId = model.approvalLevelId,
                //ProductClassId = model.productClassId,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = general.GetApplicationDate()
            };

            context.tbl_Credit_Template.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CreditTemplateAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added CreditTemplate '{ model.templateTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateCreditTemplate(CreditTemplateViewModel model, int creditTemplateId)
        {
            var data = this.context.tbl_Credit_Template.Find(creditTemplateId);
            if (data == null)
            {
                return false;
            }

            data.CompanyId = model.companyId;
            data.TemplateTitle = model.templateTitle;
            data.TemplateDocument = model.templateDocument;
            data.ApprovalLevelId = model.approvalLevelId;
            //data.ProductClassId = model.productClassId;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CreditTemplateUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated CreditTemplate '{ model.templateTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplate()
        {
            return this.context.tbl_Credit_Template.Where(x => x.Deleted == false).Select(x => new CreditTemplateViewModel
            {
                creditTemplateId = x.CreditTemplateId,
                companyId = x.CompanyId,
                templateTitle = x.TemplateTitle,
                templateDocument = x.TemplateDocument,
                approvalLevelId = x.ApprovalLevelId,
                //productClassId = x.ProductClassId,
            });
        }

        public CreditTemplateViewModel GetCreditTemplate(int creditTemplateId)
        {
            var data = this.context.tbl_Credit_Template.Find(creditTemplateId);

            if (data == null)
            {
                return null;
            }

            return new CreditTemplateViewModel
            {
                creditTemplateId = data.CreditTemplateId,
                companyId = data.CompanyId,
                templateTitle = data.TemplateTitle,
                templateDocument = data.TemplateDocument,
                approvalLevelId = data.ApprovalLevelId,
                //productClassId = data.ProductClassId,
            };
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByLevelProduct(int levelId, int productId, int companyId)
        {
            return this.GetAllCreditTemplate().Where(x =>
                x.approvalLevelId == levelId
                && x.productClassId == productId
                && x.companyId == companyId
            );
        }

        public IEnumerable<CreditTemplateViewModel> GetCreditTemplateByLevelId(int levelId, int companyId)
        {
            return this.GetAllCreditTemplate().Where(x =>
                x.approvalLevelId == levelId
            );
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByProductClass(int productId, int staffId)
        {
            var staffApprovalLevelIds = context.tbl_Approval_Level_Staff.Where(x => x.StaffId == staffId).Select(x => x.ApprovalLevelId);
            return this.GetAllCreditTemplate().Where(x =>
                staffApprovalLevelIds.Contains(x.approvalLevelId)
                && x.productClassId == productId
            );
        }

    }
}


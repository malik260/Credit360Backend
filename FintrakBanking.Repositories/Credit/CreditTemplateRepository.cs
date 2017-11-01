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

            var data = new TBL_CREDIT_TEMPLATE
            {
                COMPANYID = model.companyId,
                TEMPLATETITLE = model.templateTitle,
                TEMPLATEDOCUMENT = model.templateDocument,
                APPROVALLEVELID = model.approvalLevelId,
                //ProductClassId = model.productClassId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_CREDIT_TEMPLATE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditTemplateAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added CreditTemplate '{ model.templateTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateCreditTemplate(CreditTemplateViewModel model, int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Find(creditTemplateId);
            if (data == null)
            {
                return false;
            }

            data.COMPANYID = model.companyId;
            data.TEMPLATETITLE = model.templateTitle;
            data.TEMPLATEDOCUMENT = model.templateDocument;
            data.APPROVALLEVELID = model.approvalLevelId;
            //data.ProductClassId = model.productClassId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditTemplateUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated CreditTemplate '{ model.templateTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplate()
        {
            return this.context.TBL_CREDIT_TEMPLATE.Where(x => x.DELETED == false).Select(x => new CreditTemplateViewModel
            {
                creditTemplateId = x.CREDITTEMPLATEID,
                companyId = x.COMPANYID,
                templateTitle = x.TEMPLATETITLE,
                templateDocument = x.TEMPLATEDOCUMENT,
                approvalLevelId = x.APPROVALLEVELID,
                //productClassId = x.ProductClassId,
            });
        }

        public CreditTemplateViewModel GetCreditTemplate(int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Find(creditTemplateId);

            if (data == null)
            {
                return null;
            }

            return new CreditTemplateViewModel
            {
                creditTemplateId = data.CREDITTEMPLATEID,
                companyId = data.COMPANYID,
                templateTitle = data.TEMPLATETITLE,
                templateDocument = data.TEMPLATEDOCUMENT,
                approvalLevelId = data.APPROVALLEVELID,
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
            var staffApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == staffId).Select(x => x.APPROVALLEVELID);
            return this.GetAllCreditTemplate().Where(x =>
                staffApprovalLevelIds.Contains(x.approvalLevelId)
                && x.productClassId == productId
            );
        }

    }
}


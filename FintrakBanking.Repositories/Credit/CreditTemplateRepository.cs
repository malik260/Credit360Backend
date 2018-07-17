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
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Repositories.Credit
{
    public class CreditTemplateRepository : ICreditTemplateRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private ILoanRepository loan;

        public CreditTemplateRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, ILoanRepository loan)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.loan = loan;
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

        public bool DeleteCreditTemplate(int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Find(creditTemplateId);
            if (data != null)
            {
                data.DELETED = true;
            }
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
            var data = this.context.TBL_CREDIT_TEMPLATE.Where(x=> x.DELETED == false && x.CREDITTEMPLATEID==creditTemplateId).FirstOrDefault();

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

        #region DOCUMENT TEMPLATE IMPL

        List<CustomerExposure> customerIds = null;
        String customerExposure = String.Empty;
        TBL_LOAN_APPLICATION loanAppllication = null;

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentSections(int staffId, int operationId, int targetId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null) {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }

            return this.context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    canEdit = x.CANEDIT, // system
                    editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    // templateDocument = x.TEMPLATEDOCUMENT,
                })
                .ToList();
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentation(int staffId, int operationId, int targetId)
        {
            return this.context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();
        }

        public bool LoadDocumentTemplate(DocumentTemplateViewModel entity)
        {
            var templateSections = context.TBL_DOC_TEMPLATE_SECTION
                .Where(x => x.TEMPLATEID == entity.templateId && x.ISDISABLED == false && x.DELETED == false);

            string content = String.Empty;
            foreach (var temp in templateSections)
            {
                content = ResolvePlaceHolders(temp.TEMPLATEDOCUMENT,entity.operationId,entity.targetId);// find placeholder find and replace
                context.TBL_DOC_TEMPLATE_DETAIL.Add(new TBL_DOC_TEMPLATE_DETAIL
                {
                    OPERATIONID = entity.operationId,
                    TARGETID = entity.targetId,
                    TEMPLATESECTIONID = temp.TEMPLATESECTIONID,
                    TITLE = temp.TITLE,
                    TEMPLATEDOCUMENT = content, //temp.TEMPLATEDOCUMENT, // one time placeholder find replace
                    POSITION = temp.POSITION,
                    CANEDIT = temp.CANEDIT,
                    CREATEDBY = entity.staffId,
                    DATETIMECREATED = DateTime.Now,
                });
            }

            return context.SaveChanges() > 0;
        }

        private string ResolvePlaceHolders(string template, int operationId, int targetId)
        {
            string content = String.Empty;

            if (operationId == (int)OperationsEnum.CAM)
            {
                if (loanAppllication == null)
                {
                    this.loanAppllication = context.TBL_LOAN_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select( x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = GenerateCustomerExposure(this.customerIds, loanAppllication.COMPANYID);
                }

                string customerName = String.Empty;
                if (loanAppllication.CUSTOMERGROUPID != null) customerName = loanAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (loanAppllication.CUSTOMERID != null) customerName = loanAppllication.TBL_CUSTOMER.FIRSTNAME + " " + loanAppllication.TBL_CUSTOMER.MIDDLENAME + " " + loanAppllication.TBL_CUSTOMER.LASTNAME;

                content = content.Replace("@{{CustomerName}}", customerName);
                content = content.Replace("@{{BranchName}}", loanAppllication.TBL_BRANCH.BRANCHNAME);
                content = content.Replace("@{{LocationName}}", loanAppllication.TBL_BRANCH.BRANCHNAME);
                content = content.Replace("@{{IsRelatedParty}}", loanAppllication.ISRELATEDPARTY == true ? "Yes" : "No");
                content = content.Replace("@{{RecommendedInterest}}", loanAppllication.INTERESTRATE.ToString());
                content = content.Replace("@{{DateCreated}}", loanAppllication.DATETIMECREATED.ToShortDateString());
                content = content.Replace("@{{CustomerExposure}}", customerExposure);
            }

            if (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal || operationId == (int)OperationsEnum.NPLoanReviewApprovalAppraisal)
            {

            }

            return content;
        }

        private string GenerateCustomerExposure(List<CustomerExposure> customerIds, int companyId)
        {
            var exposures = loan.GetCurrentCustomerExposure(customerIds, companyId);
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Facility Type</th>
                        <th>Existing Limit</th>
                        <th>Proposed Limit</th>
                        <th>Change</th>
                        <th>Outstandings</th>
                        <th>Past Due Obligations Principal</th>
                        <th>Past Due Obligations Interest</th>
                        <th>Review Date</th>
                    </tr>
                 ";
            foreach(var e in exposures)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{e.facilityType}</td>
                        <td>{String.Format("{0:n}", e.existingLimit)}</td>
                        <td>{String.Format("{0:n}", e.proposedLimit)}</td>
                        <td>{String.Format("{0:n}", e.change)}</td>
                        <td>{String.Format("{0:n}", e.outstandings)}</td>
                        <td>{String.Format("{0:n}", e.PastDueObligationsPrincipal)}</td>
                        <td>{String.Format("{0:n}", e.PastDueObligationsInterest)}</td>
                        <td>{e.reviewDate.ToShortDateString()}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;
        }

        public bool SaveLoadedDocumentSection(LoadedDocumentSectionViewModel entity)
        {
            var doc = context.TBL_DOC_TEMPLATE_DETAIL.Find(entity.sectionId);
            doc.TEMPLATEDOCUMENT = entity.templateDocument;
            doc.LASTUPDATEDBY = entity.staffId;
            doc.DATETIMEUPDATED = DateTime.Now;
            return context.SaveChanges() > 0;
        }

        public LoadedDocumentSectionViewModel GetDocumentSection(int staffId, int operationId, int sectionId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }

            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            if (doc == null) return new LoadedDocumentSectionViewModel();
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                templateDocument = doc.TEMPLATEDOCUMENT, // placeholder find replace
                canEdit = doc.CANEDIT,
                editable = sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }

        public List<DocumentTemplateViewModel> GetDocumentTemplates(int staffId, int operationId, int companyId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            int ownerId = staff == null ? 0 : staff.STAFFROLEID;

            return this.context.TBL_DOC_TEMPLATE
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.COMPANYID == companyId && x.STAFFROLEID == ownerId)
                .Select(x => new DocumentTemplateViewModel
                {
                    templateId = x.TEMPLATEID,
                    templateName = x.TEMPLATENAME
                })
                .ToList();
        }

        #endregion DOCUMENT TEMPLATE IMPL

    }
}


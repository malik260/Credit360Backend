using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICreditTemplateRepository
    {
        List<LoadedDocumentSectionViewModel> GetLoadedExceptionDocumentation(int staffId, int operationId, int targetId, UserInfo user);
        LoadedDocumentSectionViewModel GetExceptionDocumentSection(int staffId, int operationId, int targetId, int sectionId);
        List<LoadedDocumentSectionViewModel> getRecoveryAnalysisDocumentation(int staffId, int operationId, int targetId, string referenceId, UserInfo user, int templateId);
        LoadedDocumentSectionViewModel GetRecoveryAnalysisDocumentSection(int staffId, int operationId, int targetId, string referenceId, int sectionId);
        bool LoadDocumentTemplateLMS(DocumentTemplateViewModel entity);
        CreditTemplateViewModel GetCreditTemplate(int creditTemplateId);

        IEnumerable<CreditTemplateViewModel> GetAllCreditTemplate();

        IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByLevelProduct(int levelId, int productId, int companyId);

        IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByProductClass(int productClassId, int staffId);

        IEnumerable<CreditTemplateViewModel> GetCreditTemplateByLevelId(int approvalLevelId, int companyId);

        bool AddCreditTemplate(CreditTemplateViewModel model);

        bool UpdateCreditTemplate(CreditTemplateViewModel model, int creditTemplateId);
        bool DeleteCreditTemplate(int creditTemplateId);
        //form CAM setup
        IEnumerable<DocumentTemplateViewModel> GetAllDocumentTemplateSetup();
        bool AddDocumentTemplate(DocumentTemplateViewModel model);
        bool UpdateDocumentTemplate(DocumentTemplateViewModel model, int documentTemplateId);
        bool DeleteDocumentTemplate(int documentTemplateId);
        IEnumerable<DocumentTemplateSectionViewModel> GetAllDocumentTemplateSectionSetup(int templateId);
        IEnumerable<DocumentTemplateSectionRoleViewModel> GetAllDocumentTemplateSectionRoleSetup(int templateSectionId);
        bool AddDocumentTemplateSection(DocumentTemplateSectionViewModel model);
        bool UpdateDocumentTemplateSection(DocumentTemplateSectionViewModel model, int documentTemplateId);
        bool DeleteDocumentTemplateSection(int documentTemplateId, short userBranchId, int companyId, int lastUpdatedBy, string applicationUrl, string userIPAddress);
        bool AddDocumentTemplateSectionRole(DocumentTemplateSectionRoleViewModel model);
        bool UpdateDocumentTemplateSectionRole(DocumentTemplateSectionRoleViewModel model);
        bool DeleteDocumentTemplateSectionRole(int sectionRoleId, short userBranchId, int companyId, int lastUpdatedBy, string applicationUrl, string userIPAddress);

        // form CAM impl
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentSections(int staffId, int operationId, int targetId);
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentation(int staffId, int operationId, int targetId, UserInfo user,bool isThirdPartyFacility = false);
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentationGeneric(int staffId, int operationId, int targetId, int targetIdForWorkFlow, UserInfo user, int customerId);
        bool LoadDocumentTemplate(DocumentTemplateViewModel entity);
        bool SaveLoadedDocumentSection(LoadedDocumentSectionViewModel entity);
        LoadedDocumentSectionViewModel GetDocumentSection(int staffId, int operationId, int targetId, int sectionId, int customerId = 0, int targetIdForWorkFlow = 0, bool isGeneric = false);
        LoadedDocumentSectionViewModel GetThirdPartyLoanDocumentSection(int staffId, int operationId, int targetId, int sectionId);


        List<DocumentTemplateViewModel> GetDocumentTemplates(int staffId, int operationId, int companyId);
        dynamic GetIsLLLViolated(int operationId, int targetId);
        bool SaveApprovedDocumentation(int staffId, int operationId, int targetId);
        List<LoadedDocumentSectionViewModel> GetSavedDocumentation(int operationId, int targetId);
        LoadedDocumentSectionViewModel GetDocumentSectionBulkLiquidation(int staffId, int operationId, int targetId, int sectionId);
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentBulkLiquidation(int staffId, int operationId, int targetId, UserInfo user);
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentationBulkLiquidation(int staffId, int operationId, int targetId, UserInfo user);
        InsurancePolicy GetInsurancePolicyConfirmationStatus(int staffId, int appDetailId);
        InsurancePolicyRecordViewModel GetInsurancePolicyConfirmationStatusByAppDetailId(int staffId, int appDetailId);
    }
}

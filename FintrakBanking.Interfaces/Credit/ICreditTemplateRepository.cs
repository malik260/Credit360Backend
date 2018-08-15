using FintrakBanking.ViewModels.Setups.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICreditTemplateRepository
    {
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
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentation(int staffId, int operationId, int targetId);
        bool LoadDocumentTemplate(DocumentTemplateViewModel entity);
        bool SaveLoadedDocumentSection(LoadedDocumentSectionViewModel entity);
        LoadedDocumentSectionViewModel GetDocumentSection(int staffId, int operationId, int sectionId);
        List<DocumentTemplateViewModel> GetDocumentTemplates(int staffId, int operationId, int companyId);


    }
}

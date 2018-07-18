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

        // form CAM impl
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentSections(int staffId, int operationId, int targetId);
        List<LoadedDocumentSectionViewModel> GetLoadedDocumentation(int staffId, int operationId, int targetId);
        bool LoadDocumentTemplate(DocumentTemplateViewModel entity);
        bool SaveLoadedDocumentSection(LoadedDocumentSectionViewModel entity);
        LoadedDocumentSectionViewModel GetDocumentSection(int staffId, int operationId, int sectionId);
        List<DocumentTemplateViewModel> GetDocumentTemplates(int staffId, int operationId, int companyId);
    }
}

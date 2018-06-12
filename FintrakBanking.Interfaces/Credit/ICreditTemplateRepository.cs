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
    }
}

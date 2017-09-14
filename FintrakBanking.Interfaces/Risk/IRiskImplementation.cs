using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Risk
{
    public interface IRiskImplementation
    {
        IEnumerable<AssessmentResultViewModel> GetAllAssessmentResultByApplicationId(int companyId, int applicationId);

        IEnumerable<AssessmentFormViewModel> GetRiskFormElements(int companyId, int titleId, int applicationId);

        IEnumerable<AssessmentFormViewModel> SaveFormElements(AssessmentFormSaveViewModel entity);
    }
}

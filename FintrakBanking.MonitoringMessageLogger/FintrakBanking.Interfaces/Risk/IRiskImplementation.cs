using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Risk
{
    public interface IRiskImplementation
    {
        IEnumerable<AssessmentResultViewModel> GetAllAssessmentResult(int companyId);

        IEnumerable<AssessmentFormViewModel> GetRiskFormElements(int companyId, int titleId, int? targetId);

        IEnumerable<AssessmentFormViewModel> SaveFormElements(AssessmentFormSaveViewModel entity);
    }
}

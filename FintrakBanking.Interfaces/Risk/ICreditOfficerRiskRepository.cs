using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.Interfaces.Risk
{
    public interface ICreditOfficerRiskRepository
    {
        //CreditOfficerRiskViewModel GetCreditOfficerRisk(int id);

        //IEnumerable<CreditOfficerRiskViewModel> GetCreditOfficerRisks();

        //bool AddCreditOfficerRisk(CreditOfficerRiskViewModel model);

        //bool UpdateCreditOfficerRisk(CreditOfficerRiskViewModel model, int id);

        //bool DeleteCreditOfficerRisk(int id);
        MatrixGrid GetCreditOfficerRiskRating(string username);
    }
}

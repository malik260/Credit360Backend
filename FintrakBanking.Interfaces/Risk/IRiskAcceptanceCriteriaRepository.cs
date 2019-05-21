using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.Interfaces.Risk
{
    public interface IRiskAcceptanceCriteriaRepository
    {
        RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProduct(int productId);
        
    }
}

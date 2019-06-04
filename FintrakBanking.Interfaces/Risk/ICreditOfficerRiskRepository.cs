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
        MatrixGrid GetCreditOfficerRiskRating(string username);
    }
}

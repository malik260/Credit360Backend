using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Setups.Credit
{
     public interface IRepaymentTermsRepository
    {
        IEnumerable<RepaymentScheduleTermSetupViewModel> GetAllRepaymentTerms();

        //IEnumerable<RepaymentTermViewModel> GetRepaymentTerms(int id);

        RepaymentScheduleTermSetupViewModel GetRepaymentTerm(int id);

        bool AddRepaymentTerm(RepaymentScheduleTermSetupViewModel model);

        bool UpdateRepaymentTerm(RepaymentScheduleTermSetupViewModel model);

        bool DeleteRepaymentTerm(int id, UserInfo user);
    }
}

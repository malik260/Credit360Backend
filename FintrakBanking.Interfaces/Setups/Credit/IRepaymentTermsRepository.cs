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
        IEnumerable<RepaymentTermViewModel> GetAllRepaymentTerms();

        //IEnumerable<RepaymentTermViewModel> GetRepaymentTerms(int id);

        RepaymentTermViewModel GetRepaymentTerm(int id);

        bool AddRepaymentTerm(RepaymentTermViewModel model);

        bool UpdateRepaymentTerm(RepaymentTermViewModel model);

        bool DeleteRepaymentTerm(int id, UserInfo user);
    }
}

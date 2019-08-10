using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Setups.Credit;

namespace FintrakBanking.Interfaces.Setups.Credit
{
     public interface IRepaymentTermsRepository
    {
        IEnumerable<RepaymentTermViewModel> GetAllRepaymentTerms();

        //IEnumerable<RepaymentTermViewModel> GetAllRepaymentTerms();

        //IEnumerable<RepaymentTermViewModel> GetAllRepaymentTerms();

        //IEnumerable<RepaymentTermViewModel> GetAllRepaymentTerms();
    }
}

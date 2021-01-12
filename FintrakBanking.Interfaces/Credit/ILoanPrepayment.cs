using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPrepayment
    {
        bool GetLoanRepaymentToStaging();
        bool GetOverdraftRepaymentToStaging();
    }
}

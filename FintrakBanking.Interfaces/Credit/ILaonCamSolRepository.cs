using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
   public interface ILaonCamSolRepository
    {
        List<LoanCAMSOLViewModel> GetCamSol();//string customerName);

        LoanCAMSOLViewModel GetCamSol(int loancamsolid);
    }
}

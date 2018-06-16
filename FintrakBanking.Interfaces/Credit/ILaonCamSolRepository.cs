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
        List<LoanCAMSOLViewModel> GetCamSol();
        List<LoanCAMSOLViewModel> GetCamSolByType(int customerName);
        LoanCAMSOLViewModel ViewCamSolByType(int id);
        List<LoanCAMSOLViewModel> GetCamSol(string loancamsolid);

        List<LoanCAMSOLViewModel> GetCamSolType();
        List<LoanCAMSOLViewModel> GetCamSolByCustomerCode(string customerCode);
        bool ApproveCamsol(LoanCAMSOLViewModel option);



    }
}

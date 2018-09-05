using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
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
        string ApproveCamsol(LoanCAMSOLViewModel option);
        List<LoanCAMSOLViewModel> CamSolAwaitingApproval(int companyId, int staffId);
        LoanCAMSOLViewModel CamSolAwaitingApprovalById(int id);
        string goForApproval(LoanCAMSOLViewModel data);
        bool GoForBulkApproval(LoanCAMSOLViewModel data);

        camsolBulkFeedbackViewModel UploadCamsolData(CamsolDocumentViewModel model, byte[] file);
        //bool GoForBulkApproval(List<ApprovalViewModel> model, UserInfo userInfo);



    }
}

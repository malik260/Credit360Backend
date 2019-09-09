using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILetterGenerationRequestRepository
    {
        LetterGenerationRequestViewModel GetLetterGenerationRequest(int id);

        IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationRequests();

        IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationRequestsForApproval(int staffId);

        LetterGenerationRequestViewModel AddLetterGenerationRequest(LetterGenerationRequestViewModel model);

        LetterGenerationRequestViewModel UpdateLetterGenerationRequest(LetterGenerationRequestViewModel model, int id, UserInfo user);

        bool DeleteLetterGenerationRequest(int id, UserInfo user);
        List<CamsolLoanDocumentViewModel> GetCamsolLoansByCustomerCode(string customerName, string customerCode);
        string GetCamsolLoanDocument(int typeId, LetterGenerationRequestViewModel model);
        IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationCompleted();
    }
}

using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICreditAssessmentMemoRepository
    {
        Task<bool> AddCreditAssessmentMemo(int operationId,CreditAssessmentMemoViewModel entity);
        bool SubmitRequestForProcessing(ApprovalViewModel entity);
      //  IEnumerable<LoanApplicationViewModel> GetRequestForCreditAssessmentMemo(int companyId, int branchId );
        IQueryable<LoanApplicationViewModel> GetRequestOnCreditAssessmentMemo(int companyId, int branchId, int staffId, int operationId);
        Task<bool> AddAssessmentTempates(AssessmentTemplatesViewModel entity);
        Task<bool> UpdateAssessmentTempates(AssessmentTemplatesViewModel entity);        
        IEnumerable<AssessmentTemplatesViewModel> GetAssessmentTempates(int approvalLevelId, int productId, int companyId);
     }
}

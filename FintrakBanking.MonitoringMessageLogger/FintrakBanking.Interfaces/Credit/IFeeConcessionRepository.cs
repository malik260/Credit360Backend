using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IFeeConcessionRepository
    {
        IEnumerable<FeeConcessionTypeViewModel> GetConcessionFeeType();
        IEnumerable<LoanFeeChargesViewModel> GetAllLoanFeeChargeByDetailId(int loanApplicationDetailId);
        IEnumerable<FeeConcessionViewModel> GetAllConcessionFee(int loanApplicationDetailId);
        int AddUpdateFeeConcession(FeeConcessionViewModel model);
        int GoForApproval(ApprovalViewModel entity);
        bool ValidateFeeConcession(int loanApplicationDetailId, int? loanChargeFeeId);
        bool ValidateApprovedFeeConcession(int concessionId);
        IEnumerable<FeeConcessionViewModel> GetAllConcessionFeeAwaitingApproval(int staffId, int companyId);
    }
}

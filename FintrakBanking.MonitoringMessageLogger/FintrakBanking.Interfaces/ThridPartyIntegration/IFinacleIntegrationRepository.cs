using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.ThridPartyIntegration
{
  public  interface IFinacleIntegrationRepository
    {
        #region
        List<BatchPostingViewModel> GetBatchPostingDetail(DateTime startDate, DateTime endDate, string searchInfo);
        List<BatchPostingViewModel> GetBatchPostingMain(DateTime startDate, DateTime endDate, string searchInfo);
        List<BatchPostingViewModel> GetBatchPostingDetailSearch(DateTime startDate, DateTime endDate, string status);
        CRMSRecord GenerateExcell(DateTime date, string loanAcct);
        CRMSRecord GetEODErrorLogDetail(FinanceEndofdayViewModel model);


        #endregion
    }
}

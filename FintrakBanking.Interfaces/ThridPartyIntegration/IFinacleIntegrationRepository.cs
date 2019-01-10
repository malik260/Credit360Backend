using FintrakBanking.ViewModels.Credit;
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
        List<BatchPostingViewModel> GetBatchPostingDetail(DateTime startDate, DateTime endDate, string searchItem);
        List<BatchPostingViewModel> GetBatchPostingMain(DateTime startDate, DateTime endDate, string searchItem);
        #endregion
    }
}

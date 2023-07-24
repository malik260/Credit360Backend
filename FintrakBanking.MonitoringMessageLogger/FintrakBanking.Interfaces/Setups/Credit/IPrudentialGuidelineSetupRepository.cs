using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Setups.Credit
{
   public  interface IPrudentialGuidelineSetupRepository
    {
        IEnumerable<PrudentialGuidelineViewModel> GetAllGuidelines(int companyId);

        PrudentialGuidelineViewModel getGuideline(int prudentialGuidelineId);

        string UpdateGuideline(PrudentialGuidelineViewModel guideline,int prudentialGuidelineId);

        string DeleteGuideline(int prudentialGuidelineId);

        string AddGuideline(PrudentialGuidelineViewModel guideline);
        IEnumerable<PrudentialGuidelineViewModel> GetAllGuidelineTypes(int getCompanyId);
    }
}

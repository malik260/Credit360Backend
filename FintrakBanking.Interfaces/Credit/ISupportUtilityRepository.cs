using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.SupportUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{

    public interface ISupportUtilityRepository 
    {
      SupportUtilityViewModel GetSupportIssueType(int supportIssueTypeId);
    }

}

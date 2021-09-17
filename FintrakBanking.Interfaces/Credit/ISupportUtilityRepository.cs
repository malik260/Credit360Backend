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
      IEnumerable<SupportUtilityViewModel> GetAllSupportIssueType();
      IEnumerable<CustomerViewModels> GetCustomersIssuesByParams(string searchParam, short? IssueTypeId);
      List<WorkflowSupportUtilityViewModel> GetApprovalTrail(string searchString, int staffId);
    }

}

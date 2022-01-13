using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.SupportUtility;
using FintrakBanking.ViewModels.WorkFlow;
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
      IEnumerable<WorkflowSupportUtilityViewModel> GetApprovalTrail(string searchString);
      List<WorkflowSupportUtilityViewModel> GetDistinctOperations(string searchString);
      List<ExpectedWorkflowViewModel> GetExpectedWorkFlow(int searchString);
      BusinessRuleViewModel GetBusinessRule(int approvalLevelId);
      List<StaffSupportUtilityViewModel> GetStaff(string searchString);
      StaffInfoViewModel GetStaffCompairRecord(string searchString);
      List<CustomerViewModels> GetSingleCustomerGeneralInfo(string searchString);
      CustomerViewModels GetTempCustomerRecord(int customerId);
      ApprovalTrailViewModel GetSingleTrail(int approvalTrailId);
      bool UpdateCustomerRecord(int customerId, CustomerViewModels entity);
      bool UpdateStaffRecord(int staffId, string staffCode, StaffInfoViewModel staffModel);
      List<CasaViewModel> GetCasaCustomerRecord(int customerId);
    }

}

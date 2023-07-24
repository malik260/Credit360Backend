using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IStaffRoleRepository
    {

        bool DeleteBulkPrepayment(int bulkPrepaymentId, UserInfo user);

        IEnumerable<StaffRoleViewModel> GetStaffRole();
        StaffRoleViewModel GetStaffRoleByStaffId(int staffId);

        StaffRoleViewModel GetStaffRole(int rankId);

        IEnumerable<StaffRoleViewModel> GetStaffRoleByCompanyId(int companyId);

        IEnumerable<StaffRoleViewModel> GetStaffRoles();
        IEnumerable<ApprovalFlowTypeViewModel> GetAllApprovalFlowTypes();

        bool AddUpdateStaffRole(StaffRoleViewModel entity);

        bool ValidateStaffRole(string staffRoleCode, string staffRoleName);

        bool ValidateStaffRoleUpdate(int staffRoleId);

        bool GoForApproval(ApprovalViewModel entity); 

        IEnumerable<StaffRoleViewModel> GetStaffRoleAwaitingApproval(int staffId, int companyId);

        bool AddApprovalSetUp(ApprovalSetUpViewModel entity);
        IEnumerable<ApprovalSetUpViewModel> GetApprovalSetup();
        bool UpdateApprovalSetUp(ApprovalSetUpViewModel entity);
        IEnumerable<OperationPageOrderViewModel> GetAllOperationOrder();
        IEnumerable<OperationPageOrderViewModel> GetAllOperations();
        bool UpdateFlowOrder(OperationPageOrderViewModel entity);
        bool AddFlowOrder(OperationPageOrderViewModel entity);
    }
}
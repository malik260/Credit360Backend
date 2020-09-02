using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IEmployerRepository
    {
        IEnumerable<EmployerViewModel> getEmployer(int companyId);
        IEnumerable<EmployerViewModel> getAllPendingEmployers(int companyId);
        IEnumerable<EmployerViewModel> getAllApprovedEmployers(int companyId);
        String ResponseMessage(WorkflowResponse response, string itemHeading);
        WorkflowResponse ForwardRelatedEmployerForApproval(EmployerViewModel model);
        IEnumerable<EmployerViewModel> GetRelatedEmployersWaitingForApproval(int staffId);

        EmployerViewModel getEmployer(int employerId, int companyId);
        string addEmployer(EmployerViewModel employer);
        string updateEmployer(int employerId, EmployerViewModel employer);
        string deleteEmployer(int employerId, EmployerViewModel employer);
        IEnumerable<EmployerType> getEmployerType();
        IEnumerable<EmployerSubType> getEmployerSunType(int employerTypeId);
        IEnumerable<EmployerSubType> getAllEmployerSubTypes();


        string addEmployerType(EmployerViewModel employerType);
        string deleteEmployerType(int employerId, EmployerViewModel employerType);
        string updateEmployerType(int employerTypeId, EmployerViewModel employerType);
        IEnumerable<EmployerType> getEmployerType(int employerTypeId);
        string addEmployerSubType(EmployerViewModel employerSubType);
        string deleteEmployerSubType(int employerId, EmployerViewModel employerSubType);
        string updateEmployerSubType(int employerTypeId, EmployerViewModel employerSubType);
        IEnumerable<EmployerSubType> getEmployerSubType(int employerSubTypeId);
    }
}

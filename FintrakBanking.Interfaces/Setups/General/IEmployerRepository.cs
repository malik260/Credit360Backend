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
        EmployerViewModel getEmployer(int employerId, int companyId);
        string addEmployer(EmployerViewModel employer);
        string updateEmployer(int employerId, EmployerViewModel employer);
        string deleteEmployer(int employerId, EmployerViewModel employer);
        IEnumerable<EmployerType> getEmployerType();
        IEnumerable<EmployerSubType> getEmployerSunType(int employerTypeId);
    }
}

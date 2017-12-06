using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface ICompanyRepository
    {
        IEnumerable<CompanyViewModel> GetAllCompany();

        CompanyViewModel GetCompanyViewModel(int companyId);

        bool AddCompany(CompanyViewModel company);

        bool UpdateCompany(int companyId, CompanyViewModel company);

        //bool DeleteAccount(short accountId);
    }
}
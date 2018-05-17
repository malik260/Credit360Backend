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

        bool UpdateCompanies(int companyId, CompanyViewModel model);

        IEnumerable<CompanyViewModel> GetCompanies();


        IEnumerable<LanguageViewModel> GetLanguages();

        IEnumerable<NatureOfBusinessViewModel> GetNatureOfBusiness();
        //bool DeleteAccount(short accountId);

        byte[] GetCompanyLogoArray(int conpanyId);
        
    }
}
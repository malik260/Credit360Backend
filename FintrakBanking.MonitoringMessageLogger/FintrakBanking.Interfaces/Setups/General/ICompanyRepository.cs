using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface ICompanyRepository
    {
        IEnumerable<CompanyViewModel> GetAllCompany();

        CompanyViewModel GetCompanyViewModel(int companyId);

        //bool AddCompany(CompanyViewModel company);
        bool AddCompany(CompanyViewModel company, byte[] buffer);

        bool UpdateCompany(int companyId, CompanyViewModel company);

        bool UpdateCompanies(int companyId, CompanyViewModel model);

        IEnumerable<CompanyViewModel> GetCompanies();

        IEnumerable<LanguageViewModel> GetLanguages();

        IEnumerable<NatureOfBusinessViewModel> GetNatureOfBusiness();

        //bool DeleteAccount(short accountId);

        byte[] GetCompanyLogoArray(int conpanyId);

        #region Company Director
        IEnumerable<CompanyDirectorsViewModel> GetCompanyDirectors();
        IEnumerable<LookupViewModel> GetCompanyDirectorsByCompanyId(int companyId);
        bool AddUpdateCompanyDirector(CompanyDirectorsViewModel director);
        IEnumerable<CompanyDirectorsViewModel> GetCustomerCompanyDirectorsByCompanyId(int companyId);
        bool DeleteCompanyDirector(int companyDirectorId, UserInfo user);
        bool ValidateCompanyDirectorEmail(int companyId, string email);
        bool ValidateCompanyDirectorBVN(int companyId, string bvn);
        #endregion

        Task<List<CustomerTurnoverViewModel>> TestTurnover();
        Task<List<CustomerTurnoverViewModel>> TestTurnoverInterest();
        bool UpdateSingleObligorLimit(int companyId, CompanyViewModel model);

    }
}
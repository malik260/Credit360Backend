using FintrakBanking.ViewModels.External.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.External
{
    public interface IGeneralRepositoryExternal
    {
        Task<List<LookupForReturn>> GetAllCRMSLegalStatusAsync();
        Task<List<LookupForReturn>> GetAllCRMSRelationshipTypeAsync();
        Task<List<LookupForReturn>> GetAllCRMSCompanySizeAsync();
        Task<List<LookupForReturn>> GetAllCountries();
        Task<List<LookupForReturn>> GetStatesByCountryId(int countryId);
        Task<List<LookupForReturn>> GetLgasByStateId(int stateId);
        Task<List<LookupForReturn>> GetCitiesByLgaId(int lgaId);
        Task<List<LookupForReturn>> GetAllCompanies();
        Task<List<LookupForReturn>> GetCompanyDirectorsByCompanyId(int companyId);
        Task<List<LookupForReturn>> GetAddressTypes();
    }
}

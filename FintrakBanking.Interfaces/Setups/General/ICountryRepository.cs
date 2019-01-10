using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.ViewModels; 
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface ICountryRepository
    {
        IEnumerable<CityViewModel> GetCity();

        IEnumerable<CityViewModel> GetCity(int Id);

        IEnumerable<LookupViewModel> GetAllCityClass();

        CityViewModel GetCityById(int cityId);

        IEnumerable<CityViewModel> GetCityByStateId(int stateId);
        IEnumerable<CityViewModel> GetCityByLGAId(int lgaId);
        int GetLgaCity(int cityId);

        IEnumerable<LocalGovtViewModel> GetLGAByStateId(int stateId);

        IEnumerable<Object> GetAllCitiesByContryId(int countryId);

       bool AddCity(CityViewModel entity);

        bool UpdateCity(CityViewModel entity, int id);

        IEnumerable<StateViewModel> GetState();

        IEnumerable<StateViewModel> GetStateByCountryId(int countryId);

        IEnumerable<CountryViewModel> GetCountry(int countryId);

        IEnumerable<CountryViewModel> GetCountry();

        IEnumerable<StateViewModel> GetStateByCompanyId(int companyId);

        bool UpdateState(StateViewModel entity, int stateId);

        bool AddLocalGovt(LocalGovtViewModel entity);

        bool UpdateLocalGovt(LocalGovtViewModel entity, int id);

        IEnumerable<LocalGovtViewModel> GetLocalGovt();

        List<LocalGovtViewModel> GetLocalGovtByStateId(int stateId);

        LocalGovtViewModel GetLocalGovtById(int id);
    }
}
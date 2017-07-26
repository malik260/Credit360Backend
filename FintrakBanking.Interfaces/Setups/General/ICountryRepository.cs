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

        IEnumerable<LookupViewModel> GetAllCityClass();

        CityViewModel GetCityById(int cityId);

        IEnumerable<CityViewModel> GetCityByStateId(int stateId);

        IEnumerable<Object> GetAllCitiesByContryId(int countryId);

        Task<bool> AddCity(CityViewModel entity);        

        IEnumerable<StateViewModel> GetState();

        IEnumerable<StateViewModel> GetStateByCountryId(int countryId);

        IEnumerable<CountryViewModel> GetCountry(int countryId);

        IEnumerable<CountryViewModel> GetCountry();
         
    }
}
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using FintrakBanking.ViewModels;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(ICountryRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CountryRepository : ICountryRepository
    {
        private FinTrakBankingContext context;

        public CountryRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public IEnumerable<LookupViewModel> GetAllCityClass()
        {
            var data = (from a in context.tbl_City_Class
                              select new LookupViewModel
                              {
                                 lookupId = a.CityClassId,
                                 lookupName = a.CityClassName
                              });
            return data;
        }

        public async Task<bool> AddCity(CityViewModel entity)
        {
            var cityEntity = new tbl_City
            {
                CityName = entity.cityName,
                StateId = entity.stateId,
                CityClassId = entity.cityClassId,
                AllowedForCollateral = entity.allowedForCollateral

            };
            context.tbl_City.Add(cityEntity);
            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<CityViewModel> GetCity()
        {
            var cityEntity = (from a in context.tbl_City
                              select new CityViewModel
                              {
                                  cityId = a.CityId,
                                  cityName = a.CityName,
                                  stateId = a.StateId,
                                  stateName = a.tbl_State.StateName,
                                  cityClassId = a.CityClassId,
                                  cityClassName = a.tbl_City_Class.CityClassName,
                                  allowedForCollateral = a.AllowedForCollateral                                  
                              });
            return cityEntity;
        }


        public CityViewModel GetCityById(int cityId)
        {
            var cityEntity = (from a in context.tbl_City
                              where a.CityId == cityId
                              select new CityViewModel
                              {
                                  cityId = a.CityId,
                                  cityName = a.CityName,
                                  stateId = a.StateId,
                                  cityClassId = a.CityClassId,
                                  cityClassName = a.tbl_City_Class.CityClassName,
                                  allowedForCollateral = a.AllowedForCollateral
                              });
            return cityEntity.FirstOrDefault();
        }


        public IEnumerable<CityViewModel> GetCityByStateId(int stateId)
        {
            var stateEntity = (from a in context.tbl_City
                               where a.StateId == stateId
                               select new CityViewModel
                               {
                                   cityId = a.CityId,
                                   cityName = a.CityName,
                                   stateId = a.StateId,
                                   cityClassId = a.CityClassId,
                                   cityClassName = a.tbl_City_Class.CityClassName,
                                   allowedForCollateral = a.AllowedForCollateral
                               });
            return stateEntity;
        }

        public IEnumerable<Object> GetAllCitiesByContryId(int countryId)
        {
            return from c in context.tbl_Country
                   join st in context.tbl_State
                    on c.CountryId equals st.CountryId
                   join ct in context.tbl_City on st.StateId equals ct.StateId
                   where c.CountryId == countryId
                   select new
                   {
                       cityId = ct.CityId,
                       cityName = ct.CityName,
                       stateId = st.StateId,
                       stateName = st.StateName,
                       cityClassId = ct.CityClassId,
                       cityClassName = ct.tbl_City_Class.CityClassName,
                       allowedForCollateral = ct.AllowedForCollateral
                   };

        }

        public IEnumerable<StateViewModel> GetStateByCountryId(int countryId)
        {
            var stateEntity = (from a in context.tbl_State
                               where a.CountryId == countryId
                               select new StateViewModel
                               {
                                   CountryId = a.CountryId,
                                   StateName = a.StateName,
                                   StateId = a.StateId
                               });
            return stateEntity;
        }

        public IEnumerable<CountryViewModel> GetCountry(int countryId)
        {
            var countryEntity = (from a in context.tbl_Country where a.CountryId == countryId 

                                 select new CountryViewModel
                                 {
                                     CountryId = a.CountryId,
                                     CountryName = a.Name
                                 }).ToList();
            return countryEntity;
        }
        public IEnumerable<CountryViewModel> GetCountry()
        {
            var countryEntity = (from a in context.tbl_Country                              

                                 select new CountryViewModel
                                 {
                                     CountryId = a.CountryId,
                                     CountryName = a.Name
                                 }).ToList();
            return countryEntity;
        }
        public IEnumerable<StateViewModel> GetState()
        {
            var stateEntity = (from a in context.tbl_State

                               select new StateViewModel
                               {
                                   CountryId = a.CountryId,
                                   StateName = a.StateName,
                                   StateId = a.StateId
                               });
            return stateEntity.ToList();
        }

 


    }
}
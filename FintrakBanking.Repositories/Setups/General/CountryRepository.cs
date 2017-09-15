using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using FintrakBanking.ViewModels;
using System.ComponentModel.Composition;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Admin;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(ICountryRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CountryRepository : ICountryRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;

        public CountryRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository genSetup, FinTrakBankingContext _context)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this._genSetup = genSetup;
        }
        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
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

        public bool AddCity(CityViewModel entity)
        {
            var cityEntity = new tbl_City
            {
                CityName = entity.cityName,
                StateId = entity.stateId,
                CityClassId = entity.cityClassId,
                AllowedForCollateral = entity.allowedForCollateral

            };
            context.tbl_City.Add(cityEntity);
            return context.SaveChanges() != 0;
        }
        public bool UpdateCity(CityViewModel entity, int id)
        {
            var cityEntity = context.tbl_City.Find(id);
            {
                cityEntity.CityName = entity.cityName;
                cityEntity.StateId = entity.stateId;
                cityEntity.CityClassId = entity.cityClassId;
                cityEntity.AllowedForCollateral = entity.allowedForCollateral;

            }
          
            return context.SaveChanges() != 0;
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
        public IEnumerable<StateViewModel> GetStateByCompanyId(int companyId)
        {
            var countryId = context.tbl_Company.Find(companyId).CompanyId;
            if (countryId != 0)
            {
                var stateEntity = (from a in context.tbl_State
                                   where a.CountryId == countryId
                                   select new StateViewModel
                                   {
                                       CountryId = a.CountryId,
                                       StateName = a.StateName,
                                       StateId = a.StateId,
                                       CountryName = context.tbl_Country.FirstOrDefault(j => j.CountryId == a.CountryId).Name ?? string.Empty,
                                       CollateralSearchChargeAmount = a.CollateralSearchChargeAmount
                                       
                                   });
                return stateEntity;
            }
            return null;
        }
        public bool UpdateState(StateViewModel entity, int stateId)
        {
            var state = context.tbl_State.Find(stateId);
            if (state != null)
            {
                state.CollateralSearchChargeAmount = entity.CollateralSearchChargeAmount;
                state.StateName = entity.StateName;
            }
           
            // Audit Section ----------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.DepartmentUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated tbl_State with Id: {entity.StateId} ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
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
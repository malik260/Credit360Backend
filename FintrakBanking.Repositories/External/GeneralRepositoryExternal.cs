using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.External;
using FintrakBanking.ViewModels.External.General;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.External
{
    public class GeneralRepositoryExternal : IGeneralRepositoryExternal
    {
        public GeneralRepositoryExternal()
        {
        }

        public async Task<List<LookupForReturn>> GetAllCRMSLegalStatusAsync()
        {
            using (var context = new FinTrakBankingContext())
            { 
                var data = await context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.LegalStatusType).Select(x => new LookupForReturn()
                {
                    id = (short)x.CRMSREGULATORYID,
                    name = x.DESCRIPTION

                }).ToListAsync();

                return data;
            }
        }

        public async Task<List<LookupForReturn>> GetAllCRMSCompanySizeAsync()
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.CompanySize).Select(x => new LookupForReturn()
                {
                    id = (short)x.CRMSREGULATORYID,
                    name = x.DESCRIPTION

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetAllCRMSRelationshipTypeAsync()
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.RelationshipType).Select(x => new LookupForReturn()
                {
                    id = (short)x.CRMSREGULATORYID,
                    name = x.DESCRIPTION

                }).ToListAsync();
            }
        }


        public async Task<List<LookupForReturn>> GetAllCountries()
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_COUNTRY.Select(x => new LookupForReturn()
                {
                    id = x.COUNTRYID,
                    name = x.NAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetStatesByCountryId(int countryId)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_STATE.Where(s => s.COUNTRYID == countryId).Select(x => new LookupForReturn()
                {
                    id = x.STATEID,
                    name = x.STATENAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetLgasByStateId(int stateId)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_LOCALGOVERNMENT.Where(s => s.STATEID == stateId).Select(x => new LookupForReturn()
                {
                    id = x.LOCALGOVERNMENTID,
                    name = x.NAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetCitiesByLgaId(int lgaId)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_CITY.Where(s => s.LOCALGOVERNMENTID == lgaId).Select(x => new LookupForReturn()
                {
                    id = x.CITYID,
                    name = x.CITYNAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetAllCompanies()
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_COMPANY.Select(x => new LookupForReturn()
                {
                    id = x.COMPANYID,
                    name = x.NAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetCompanyDirectorsByCompanyId(int companyId)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_COMPANY_DIRECTOR.Where(s => s.COMPANYID == companyId && s.DELETED == false).Select(x => new LookupForReturn()
                {
                    id = (int)x.COMPANYDIRECTORID,
                    name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME

                }).ToListAsync();
            }
        }

        public async Task<List<LookupForReturn>> GetAddressTypes()
        {
            using (var context = new FinTrakBankingContext())
            {
                return await context.TBL_CUSTOMER_ADDRESS_TYPE.Select(x => new LookupForReturn()
                {
                    id = x.ADDRESSTYPEID,
                    name = x.ADDRESS_TYPE_NAME

                }).ToListAsync();
            }
        }







    }
}

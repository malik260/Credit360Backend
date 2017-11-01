using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(ICompanyRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompanyRepository : ICompanyRepository
    {
        private FinTrakBankingContext context;

        public CompanyRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public bool AddCompany(CompanyViewModel company)
        {
            var _company = new TBL_COMPANY()
            {
                NAME = company.companyName,
                ADDRESS = company.address,
                TELEPHONE = company.telephone,
                EMAIL = company.email,
                DATEOFINCORPORATION = company.dateOfIncorporation.Value,
                COUNTRYID = company.countryId,
                CURRENCYID = company.currencyId,
                NATUREOFBUSINESSID = company.natureOfBusinessId,
                NAMEOFSCHEME = company.nameOfScheme,
                FUNCTIONSREGISTERED = company.functionsRegistered,
                AUTHORISEDSHARECAPITAL = company.authorisedShareCapital,
                NAMEOFREGISTRAR = company.nameOfRegistrar,
                NAMEOFTRUSTEES = company.nameOfTrustees,
                FORMERMANAGERSTRUSTEES = company.formerManagersTrustees,
                DATEOFRENEWALOFREGISTRATION = company.dateOfRenewalOfRegistration,
                DATEOFCOMMENCEMENT = company.dateOfCommencement,
                INITIALFLOATATION = company.initialFloatation,
                INITIALSUBSCRIPTION = company.initialSubscription,
                REGISTEREDBY = company.registeredBy,
                PARENTID = company.parentId,
                TRUSTEESADDRESS = company.trusteesAddress,
                INVESTMENTOBJECTIVE = company.investmentObjective,

            };

            return true;
        }


        //join c in context.TblCountry
        //             on data.CountryId equals c.CountryId
        private IEnumerable<CompanyViewModel> tbl_Customer()
        {
            return from data in context.TBL_COMPANY
                   join c in context.TBL_COUNTRY
                     on data.COUNTRYID equals c.COUNTRYID
                   select new CompanyViewModel()
                   {
                       companyId = data.COMPANYID,
                       companyName = data.NAME,
                       address = data.ADDRESS,
                       telephone = data.TELEPHONE,
                       email = data.EMAIL,
                       dateOfIncorporation = data.DATEOFINCORPORATION ?? DateTime.Now,
                       //natureOfBusinessId = data.NatureOfBusinessId ?? 0,
                       nameOfScheme = data.NAMEOFSCHEME,
                       functionsRegistered = data.FUNCTIONSREGISTERED,
                       authorisedShareCapital = data.AUTHORISEDSHARECAPITAL ?? 0,
                       nameOfRegistrar = data.NAMEOFREGISTRAR,
                       nameOfTrustees = data.NAMEOFTRUSTEES,
                       formerManagersTrustees = data.FORMERMANAGERSTRUSTEES,
                       dateOfRenewalOfRegistration = data.DATEOFRENEWALOFREGISTRATION ?? DateTime.Now,
                       dateOfCommencement = data.DATEOFCOMMENCEMENT ?? DateTime.Now,
                       //initialFloatation = data.InitialFloatation ?? 0,
                       //initialSubscription = data.InitialSubscription ?? 0,
                       registeredBy = data.REGISTEREDBY,
                       trusteesAddress = data.TRUSTEESADDRESS,
                       investmentObjective = data.INVESTMENTOBJECTIVE,
                       website = data.WEBSITE,
                       countryId = data.COUNTRYID,
                       country = c.NAME,
                       //companyClassId = data.CompanyClassId ?? (short)1,
                       //companyTypeId = data.CompanyTypeId ?? (short)1,
                       //accountingStandardId = data.AccountingStandardId ?? (short)1,
                       //managementTypeId = data.ManagementTypeId ?? (short)1,
                       createdBy = data.CREATEDBY ?? 0,
                       lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                       CompanyLogo = data.COMPANYLOGO,
                       dateTimeCreated = data.DATETIMECREATED ?? DateTime.Now,
                       dateTimeUpdated = data.DATETIMEUPDATED ?? DateTime.Now
                   };
        }


        public IEnumerable<CompanyViewModel> GetAllCompany()
        {
            return tbl_Customer();
        }

        public CompanyViewModel GetCompanyViewModel(int companyId)
        {
            return tbl_Customer().Where(c => c.companyId == companyId).SingleOrDefault();
        }

        public bool UpdateCompany(CompanyViewModel company)
        {
            throw new NotImplementedException();
        }
    }
}
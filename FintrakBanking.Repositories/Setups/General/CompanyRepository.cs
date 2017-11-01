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
            var _company = new tbl_Company()
            {
                Name = company.companyName,
                Address = company.address,
                Telephone = company.telephone,
                Email = company.email,
                DateOfIncorporation = company.dateOfIncorporation.Value,
                CountryId = company.countryId,
                CurrencyId = company.currencyId,
                NatureOfBusinessId = company.natureOfBusinessId,
                NameOfScheme = company.nameOfScheme,
                FunctionsRegistered = company.functionsRegistered,
                AuthorisedShareCapital = company.authorisedShareCapital,
                NameOfRegistrar = company.nameOfRegistrar,
                NameOfTrustees = company.nameOfTrustees,
                FormerManagersTrustees = company.formerManagersTrustees,
                DateOfRenewalOfRegistration = company.dateOfRenewalOfRegistration,
                DateOfCommencement = company.dateOfCommencement,
                InitialFloatation = company.initialFloatation,
                InitialSubscription = company.initialSubscription,
                RegisteredBy = company.registeredBy,
                ParentId = company.parentId,
                TrusteesAddress = company.trusteesAddress,
                InvestmentObjective = company.investmentObjective,

            };

            return true;
        }


        //join c in context.TblCountry
        //             on data.CountryId equals c.CountryId
        private IEnumerable<CompanyViewModel> tbl_Customer()
        {
            return from data in context.TBL_COMPANY
                   join c in context.TBL_COUNTRY
                     on data.CountryId equals c.CountryId
                   select new CompanyViewModel()
                   {
                       companyId = data.CompanyId,
                       companyName = data.Name,
                       address = data.Address,
                       telephone = data.Telephone,
                       email = data.Email,
                       dateOfIncorporation = data.DateOfIncorporation ?? DateTime.Now,
                       //natureOfBusinessId = data.NatureOfBusinessId ?? 0,
                       nameOfScheme = data.NameOfScheme,
                       functionsRegistered = data.FunctionsRegistered,
                       authorisedShareCapital = data.AuthorisedShareCapital ?? 0,
                       nameOfRegistrar = data.NameOfRegistrar,
                       nameOfTrustees = data.NameOfTrustees,
                       formerManagersTrustees = data.FormerManagersTrustees,
                       dateOfRenewalOfRegistration = data.DateOfRenewalOfRegistration ?? DateTime.Now,
                       dateOfCommencement = data.DateOfCommencement ?? DateTime.Now,
                       //initialFloatation = data.InitialFloatation ?? 0,
                       //initialSubscription = data.InitialSubscription ?? 0,
                       registeredBy = data.RegisteredBy,
                       trusteesAddress = data.TrusteesAddress,
                       investmentObjective = data.InvestmentObjective,
                       website = data.Website,
                       countryId = data.CountryId,
                       country = c.Name,
                       //companyClassId = data.CompanyClassId ?? (short)1,
                       //companyTypeId = data.CompanyTypeId ?? (short)1,
                       //accountingStandardId = data.AccountingStandardId ?? (short)1,
                       //managementTypeId = data.ManagementTypeId ?? (short)1,
                       createdBy = data.CreatedBy ?? 0,
                       lastUpdatedBy = data.LastUpdatedBy ?? 0,
                       CompanyLogo = data.CompanyLogo,
                       dateTimeCreated = data.DateTimeCreated ?? DateTime.Now,
                       dateTimeUpdated = data.DateTimeUpdated ?? DateTime.Now
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
using FintrakBanking.Entities.DocumentModels;
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
        private FinTrakBankingDocumentsContext documentContext;

        public CompanyRepository(FinTrakBankingContext _context, FinTrakBankingDocumentsContext _documentContext)
        {
            this.context = _context;
            this.documentContext = _documentContext;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public bool AddCompany(CompanyViewModel company)
        {
            try
            {
                var _company = new TBL_COMPANY()
                {
                    NAME = company.companyName,
                    ADDRESS = company.address,
                    TELEPHONE = company.telephone,
                    EMAIL = company.email,
                    LANGUAGEID = company.languageId,
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
                    WEBSITE = company.website,
                    TRUSTEESADDRESS = company.trusteesAddress,
                    INVESTMENTOBJECTIVE = company.investmentObjective,

                };

                context.TBL_COMPANY.Add(_company);

                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IQueryable<CompanyViewModel> GetAllCompanies()
        {
            var companies = (from data in context.TBL_COMPANY
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
                                 natureOfBusinessId = data.NATUREOFBUSINESSID ?? 0,
                                 natureOfBusiness = data.TBL_NATURE_OF_BUSINESS.NAME,
                                 nameOfScheme = data.NAMEOFSCHEME,
                                 functionsRegistered = data.FUNCTIONSREGISTERED,
                                 authorisedShareCapital = data.AUTHORISEDSHARECAPITAL ?? 0,
                                 nameOfRegistrar = data.NAMEOFREGISTRAR,
                                 nameOfTrustees = data.NAMEOFTRUSTEES,
                                 formerManagersTrustees = data.FORMERMANAGERSTRUSTEES,
                                 dateOfRenewalOfRegistration = data.DATEOFRENEWALOFREGISTRATION ?? DateTime.Now,
                                 dateOfCommencement = data.DATEOFCOMMENCEMENT ?? DateTime.Now,
                                 initialFloatation = data.INITIALFLOATATION ?? 0,
                                 initialSubscription = data.INITIALSUBSCRIPTION ?? 0,
                                 registeredBy = data.REGISTEREDBY,
                                 trusteesAddress = data.TRUSTEESADDRESS,
                                 investmentObjective = data.INVESTMENTOBJECTIVE,
                                 website = data.WEBSITE,
                                 countryId = data.COUNTRYID,
                                 country = c.NAME,
                                 currencyId = data.CURRENCYID,
                                 languageId = data.LANGUAGEID,
                                 companyClassId = data.COMPANYCLASSID ?? 1,
                                 companyTypeId = data.COMPANYTYPEID ?? 1,
                                 accountingStandardId = data.ACCOUNTINGSTANDARDID ?? 1,
                                 managementTypeId = data.MANAGEMENTTYPEID ?? 1,
                                 createdBy = data.CREATEDBY ?? 0,
                                 lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                                 CompanyLogo = data.COMPANYLOGO,
                                 dateTimeCreated = data.DATETIMECREATED ?? DateTime.Now,
                                 dateTimeUpdated = data.DATETIMEUPDATED ?? DateTime.Now
                             });

            return companies;
        }


        public IEnumerable<CompanyViewModel> GetCompanies()
        {
            var companies = (from data in context.TBL_COMPANY join cont in context.TBL_COUNTRY on data.COUNTRYID equals cont.COUNTRYID
                             select new CompanyViewModel()
                             {
                                 companyId = data.COMPANYID,
                                 companyName = data.NAME,
                                 address = data.ADDRESS,
                                 telephone = data.TELEPHONE,
                                 languageId = data.LANGUAGEID,
                                 email = data.EMAIL,
                                 currencyId = data.CURRENCYID,
                                 dateOfIncorporation = data.DATEOFINCORPORATION ?? DateTime.Now,
                                 natureOfBusinessId = data.NATUREOFBUSINESSID ?? 0,
                                 natureOfBusiness = data.TBL_NATURE_OF_BUSINESS.NAME,
                                 nameOfScheme = data.NAMEOFSCHEME,
                                 functionsRegistered = data.FUNCTIONSREGISTERED,
                                 authorisedShareCapital = data.AUTHORISEDSHARECAPITAL ?? 0,
                                 nameOfRegistrar = data.NAMEOFREGISTRAR,
                                 nameOfTrustees = data.NAMEOFTRUSTEES,
                                 formerManagersTrustees = data.FORMERMANAGERSTRUSTEES,
                                 dateOfRenewalOfRegistration = data.DATEOFRENEWALOFREGISTRATION ?? DateTime.Now,
                                 dateOfCommencement = data.DATEOFCOMMENCEMENT ?? DateTime.Now,
                                 initialFloatation = data.INITIALFLOATATION ?? 0,
                                 initialSubscription = data.INITIALSUBSCRIPTION ?? 0,
                                 registeredBy = data.REGISTEREDBY,
                                 trusteesAddress = data.TRUSTEESADDRESS,
                                 investmentObjective = data.INVESTMENTOBJECTIVE,
                                 website = data.WEBSITE,
                                 countryId = data.COUNTRYID,
                                 country =cont.NAME ?? string.Empty,
                                 companyClassId = data.COMPANYCLASSID ?? 1,
                                 companyTypeId = data.COMPANYTYPEID ?? 1,
                                 accountingStandardId = data.ACCOUNTINGSTANDARDID ?? 1,
                                 managementTypeId = data.MANAGEMENTTYPEID ?? 1,
                                 createdBy = data.CREATEDBY ?? 0,
                                 lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                                 CompanyLogo = data.COMPANYLOGO,
                                 shareHoldersFund = data.SHAREHOLDERSFUND,
                                 dateTimeCreated = data.DATETIMECREATED ?? DateTime.Now,
                                 dateTimeUpdated = data.DATETIMEUPDATED ?? DateTime.Now
                             });

            return companies;
        }


        public IEnumerable<CompanyViewModel> GetAllCompany()
        {
            return GetAllCompanies().ToList();
        }

        public CompanyViewModel GetCompanyViewModel(int companyId)
        {
            return GetAllCompanies().Where(c => c.companyId == companyId).FirstOrDefault();
        }

        public bool UpdateCompany(int companyId, CompanyViewModel model)
        {
            var data = context.TBL_COMPANY.Find(companyId);

            try
            {
                if (data != null)
                {
                    data.COMPANYID = companyId;
                    data.NAME = model.companyName;
                    data.ADDRESS = model.address;
                    data.TELEPHONE = model.telephone;
                    data.LANGUAGEID = model.languageId;
                    data.EMAIL = model.email;
                    data.DATEOFINCORPORATION = model.dateOfIncorporation;
                    data.NATUREOFBUSINESSID = model.natureOfBusinessId;
                    data.NAMEOFSCHEME = model.nameOfScheme;
                    data.FUNCTIONSREGISTERED = model.functionsRegistered;
                    data.AUTHORISEDSHARECAPITAL = model.authorisedShareCapital;
                    data.NAMEOFREGISTRAR = model.nameOfRegistrar;
                    data.NAMEOFTRUSTEES = model.nameOfTrustees;
                    data.FORMERMANAGERSTRUSTEES = model.formerManagersTrustees;
                    data.DATEOFRENEWALOFREGISTRATION = model.dateOfRenewalOfRegistration;
                    data.DATEOFCOMMENCEMENT = model.dateOfCommencement;
                    data.INITIALFLOATATION = model.initialFloatation;
                    data.INITIALSUBSCRIPTION = model.initialSubscription;
                    data.REGISTEREDBY = model.registeredBy;
                    data.TRUSTEESADDRESS = model.trusteesAddress;
                    data.INVESTMENTOBJECTIVE = model.investmentObjective;
                    data.WEBSITE = model.website;
                    data.COUNTRYID = model.countryId;
                    data.COMPANYCLASSID = data.COMPANYCLASSID;
                    data.COMPANYTYPEID = data.COMPANYTYPEID;
                    data.ACCOUNTINGSTANDARDID = data.ACCOUNTINGSTANDARDID;
                    data.MANAGEMENTTYPEID = data.MANAGEMENTTYPEID;
                    data.CREATEDBY = model.createdBy;
                    data.LASTUPDATEDBY = model.lastUpdatedBy;
                    data.COMPANYLOGO = model.CompanyLogo;
                    data.SHAREHOLDERSFUND = model.shareHoldersFund;
                    data.DATETIMECREATED = model.dateTimeCreated;
                    data.DATETIMEUPDATED = model.dateTimeUpdated;

                    return context.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public bool UpdateCompanies(int companyId, CompanyViewModel model)
        {
            var data = context.TBL_COMPANY.Find(companyId);

            try
            {
                if (data != null)
                {
                    data.COMPANYID = companyId;
                    data.NAME = model.companyName;
                    data.SHAREHOLDERSFUND = model.shareHoldersFund;

                    return context.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IEnumerable<LanguageViewModel> GetLanguages()
        {
            var languages = (from data in context.TBL_LANGUAGE
                             select new LanguageViewModel()
                             {
                                 languageCode = data.LANGUAGECODE,
                                 language = data.LANGUAGENAME,
                                 languageId = data.LANGUAGEID
                             }).ToList();
            return languages;
        }
        public IEnumerable<NatureOfBusinessViewModel> GetNatureOfBusiness()
        {
            var languages = (from data in context.TBL_NATURE_OF_BUSINESS
                             select new NatureOfBusinessViewModel()
                             {
                                 natureOfBusinessId = data.NATUREOFBUSINESSID,
                                 natureOfBusiness = data.NAME
                             }).ToList();
            return languages;
        }

        public byte[] GetCompanyLogoArray(int companyId)
        {
            return documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(x => x.DOCUMENTID == 1).FirstOrDefault().FILEDATA;
        }
    }
}
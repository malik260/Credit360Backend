using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{
    public class CompanyRepository : ICompanyRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext documentContext;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;

        private CustomerDetails _customerIntegration; // test

        public CompanyRepository(FinTrakBankingContext _context, FinTrakBankingDocumentsContext _documentContext,
            IAuditTrailRepository _auditTrail, IGeneralSetupRepository _generalSetup,
            CustomerDetails _customerIntegration)
        {
            this.context = _context;
            this.documentContext = _documentContext;
            this.auditTrail = _auditTrail;
            this.generalSetup = _generalSetup;
            this._customerIntegration = _customerIntegration;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public bool AddCompany(CompanyViewModel company,  byte[] buffer)
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

                    FILENAME = company.fileName,
                    FILEEXTENSION = company.fileExtension.ToLower(),
                    FILEDATA = buffer,
                    IMAGEPATH = company.imagePath,

                };

                context.TBL_COMPANY.Add(_company);

                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /*public bool AddCompany(CompanyViewModel company)
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
        }*/

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
            var companies = (from data in context.TBL_COMPANY
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
                                 country = data.TBL_COUNTRY.NAME ?? string.Empty,
                                 companyClassId = data.COMPANYCLASSID ?? 1,
                                 companyTypeId = data.COMPANYTYPEID ?? 1,
                                 accountingStandardId = data.ACCOUNTINGSTANDARDID ?? 1,
                                 managementTypeId = data.MANAGEMENTTYPEID ?? 1,
                                 createdBy = data.CREATEDBY ?? 0,
                                 lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                                 CompanyLogo = data.COMPANYLOGO,
                                 companyLimit = data.COMPANYLIMIT,
                                 dateTimeCreated = data.DATETIMECREATED ?? DateTime.Now,
                                 dateTimeUpdated = data.DATETIMEUPDATED ?? DateTime.Now,
                                 shareHoldersFund = data.SHAREHOLDERSFUND,
                                 singleObligorLimit = data.SINGLEOBLIGORLIMIT,

                                 fileExtension = data.FILEEXTENSION,
                                 fileName = data.FILENAME,
                                 fileData = data.FILEDATA,
                                 imagePath = data.IMAGEPATH,
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
                    data.COMPANYLIMIT = model.companyLimit;
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
                    data.COMPANYLIMIT = model.companyLimit;

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
        public bool UpdateSingleObligorLimit(int companyId, CompanyViewModel model)
        {
            var data = context.TBL_COMPANY.Find(companyId);

            try
            {
                if (data != null)
                {
                    data.SINGLEOBLIGORLIMIT = model.singleObligorLimit;
                   
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
            return documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS
                .FirstOrDefault(x => x.DOCUMENTID == 1)
                ?.FILEDATA;
        }
        #region Company Director
        public IEnumerable<CompanyDirectorsViewModel> GetCompanyDirectors()
        {
            var directors = (from data in context.TBL_COMPANY_DIRECTOR
                             where data.DELETED == false
                             select new CompanyDirectorsViewModel()
                             {
                                 title = data.TITLE,
                                 companyDirectorId = data.COMPANYDIRECTORID,
                                 companyId = data.COMPANYID,
                                 firstName = data.FIRSTNAME,
                                 middleName = data.MIDDLENAME,
                                 lastName = data.LASTNAME,
                                 gender = data.GENDER,
                                 bvn = data.BVN,
                                 address = data.ADDRESS,
                                 email = data.EMAIL,
                                 shareHoldingPercentage = data.SHAREHOLDINGPERCENTAGE,
                                 phoneNumber = data.PHONENUMBER,
                                 isActive = data.ISACTIVE,
                             }).ToList();
            return directors;
        }

        public IEnumerable<LookupViewModel> GetCompanyDirectorsByCompanyId(int companyId)
        {
            var directors = (from data in context.TBL_COMPANY_DIRECTOR
                             where data.COMPANYID == companyId && data.DELETED == false
                             select new LookupViewModel()
                             {
                               lookupId = (short)data.COMPANYDIRECTORID,
                               lookupName = data.FIRSTNAME + " " + data.MIDDLENAME + " " + data.LASTNAME
                             }).ToList();
            return directors;
        }
        public IEnumerable<CompanyDirectorsViewModel> GetCustomerCompanyDirectorsByCompanyId(int companyId)
        {
            var directors = (from data in context.TBL_COMPANY_DIRECTOR
                             where data.COMPANYID == companyId && data.DELETED == false
                             select new CompanyDirectorsViewModel()
                             {
                                 title = data.TITLE,
                                 companyDirectorId = data.COMPANYDIRECTORID,
                                 companyId = data.COMPANYID,
                                 firstName = data.FIRSTNAME,
                                 middleName = data.MIDDLENAME,
                                 lastName = data.LASTNAME,
                                 gender = data.GENDER,
                                 bvn = data.BVN,
                                 address = data.ADDRESS,
                                 email = data.EMAIL,
                                 phoneNumber = data.PHONENUMBER,
                                 isActive = data.ISACTIVE,
                                 shareHoldingPercentage = data.SHAREHOLDINGPERCENTAGE,
                                 directorName = data.FIRSTNAME + " " + data.MIDDLENAME + " " + data.LASTNAME
                             }).ToList();

            return directors;
        }


        public bool AddUpdateCompanyDirector(CompanyDirectorsViewModel director)
        {
            if (director == null) return false;
            try
            {
                TBL_COMPANY_DIRECTOR comDirector = null;

                if (director.companyDirectorId > 0)
                {
                    comDirector = context.TBL_COMPANY_DIRECTOR.Find(director.companyDirectorId);
                    if (comDirector != null)
                    {
                        comDirector.TITLE = director.title;
                        comDirector.FIRSTNAME = director.firstName;
                        comDirector.MIDDLENAME = director.middleName == null ? " " : director.middleName;
                        comDirector.LASTNAME = director.lastName;
                        comDirector.GENDER = director.gender;
                        comDirector.BVN = director.bvn;
                        comDirector.ADDRESS = director.address;
                        comDirector.EMAIL = director.email;
                        comDirector.PHONENUMBER = director.phoneNumber;
                        comDirector.ISACTIVE = director.isActive;
                        comDirector.LASTUPDATEDBY = director.createdBy;
                        comDirector.DATETIMEUPDATED = DateTime.Now;
                        comDirector.SHAREHOLDINGPERCENTAGE = director.shareHoldingPercentage;
                    }
                }
                else
                {
                    comDirector = new TBL_COMPANY_DIRECTOR()
                    {
                        COMPANYDIRECTORID = director.companyDirectorId,
                        COMPANYID = director.companyId,
                        TITLE = director.title,
                        FIRSTNAME = director.firstName,
                        MIDDLENAME = director.middleName,
                        LASTNAME = director.lastName,
                        GENDER = director.gender,
                        BVN = director.bvn,
                        ADDRESS = director.address,
                        EMAIL = director.email,
                        PHONENUMBER = director.phoneNumber,
                        ISACTIVE = director.isActive,
                        CREATEDBY = director.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false,
                        SHAREHOLDINGPERCENTAGE = director.shareHoldingPercentage
                    };
                    context.TBL_COMPANY_DIRECTOR.Add(comDirector);
                }
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CompanyDirectorAddedUpdated,
                    STAFFID = director.createdBy,
                    BRANCHID = (short)director.userBranchId,
                    DETAIL = $"Added new director information {director.firstName} {director.lastName}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = director.applicationUrl,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this.auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool DeleteCompanyDirector(int companyDirectorId, UserInfo user)
        {
            if (companyDirectorId == 0) return false;

            var comDirector = context.TBL_COMPANY_DIRECTOR.Find(companyDirectorId);
            if (comDirector != null)
            {
                comDirector.DELETED = true;
                comDirector.DELETEDBY = user.staffId;
            }
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CompanyDirectorDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Deleted Company Director information with companyDirectorId: " + companyDirectorId,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() > 0;
        }
        public bool ValidateCompanyDirectorBVN(int companyId, string bvn)
        {
            var bvnExist = (from a in context.TBL_COMPANY_DIRECTOR where a.COMPANYID == companyId && a.BVN == bvn select a).ToList();
            if (bvnExist.Any())
            {
                return true;
            }
            return false;
        }
        public bool ValidateCompanyDirectorEmail(int companyId, string email)
        {
            var emailExist = (from a in context.TBL_COMPANY_DIRECTOR where a.COMPANYID == companyId && a.EMAIL == email select a).ToList();
            if (emailExist.Any())
            {
                return true;
            }
            return false;
        }

        #endregion



        #region test

        public async Task<List<CustomerTurnoverViewModel>> TestTurnover()
        {
            var data = new List<CustomerTurnoverViewModel>();
            //Task.Run(async () => { data = await _customerIntegration.GetCustomerTransactions("483008974", 48); }).GetAwaiter().GetResult();
            data = await _customerIntegration.GetCustomerTransactions("483008974", 48);
            return data;
        }

        public async Task<List<CustomerTurnoverViewModel>> TestTurnoverInterest()
        {
            var data = new List<CustomerTurnoverViewModel>();
            //Task.Run(async () => { data = await _customerIntegration.GetCustomerInterestTransactions("230009868", 48); }).GetAwaiter().GetResult();
            data = await _customerIntegration.GetCustomerInterestTransactions("230009868", 48);
            return data;
        }
        
        #endregion test

    }


}
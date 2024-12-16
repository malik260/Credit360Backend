using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.CreditBureau;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using FinTrakBanking.ThirdPartyIntegration;
using FintrakBanking.Common.CustomException;
using System.Text;
using Newtonsoft.Json.Linq;
using FintrakBanking.Interfaces.Setups.Finance;
using System.Text.RegularExpressions;
using FintrakBanking.Interfaces.CASA;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
using System.IO;
using System.Xml;
using FintrakBanking.ViewModels.Setups.Credit;

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCreditBureauRepository : ICustomerCreditBureauRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IFinanceTransactionRepository financeTransaction;
        private IntegrationWithFlexcube integration;
        private CreditBureauProcess _creditBureau;
        private IChartOfAccountRepository chartOfAccount;
        private ITwoFactorAuthIntegrationService twoFactoeAuth;
        private IAdminRepository admin;


        public CustomerCreditBureauRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingDocumentsContext _docContext,
            FinTrakBankingContext _context,
            IFinanceTransactionRepository _financials, IntegrationWithFlexcube integration,
            CreditBureauProcess creditBureau, IChartOfAccountRepository _chartOfAccount,
            ITwoFactorAuthIntegrationService _twoFactoeAuth,
            IAdminRepository _admin)
        {
            this.context = _context;
            docContext = _docContext;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            financeTransaction = _financials;
            this.integration = integration;
            _creditBureau = creditBureau;
            chartOfAccount = _chartOfAccount;
            this.twoFactoeAuth = _twoFactoeAuth;
            this.admin = _admin;
        }

        #region Credit Bureau 
        public CompanySetupViewModel GetLoanThirdPartyServiceChargeStatusDetails(int companyId)
        {
            var companyDetails = context.TBL_SETUP_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId);
            var loanExternalServiceChargeSetup = new CompanySetupViewModel()
            {
                requireCreditBureauModule = companyDetails.REQUIRECREDITBUREAUMODULE,
                creditBureauSearchTypeId = companyDetails.CREDITBUREAUCHARGETYPEID,
                collateralSearchTypeId = companyDetails.COLLATERALSEARCHCHARGETYPEID
            };

            return loanExternalServiceChargeSetup;
        }

        public IEnumerable<CustomerViewModels> GetCreditBureauCustomerDetailsByCustomerId(int customerId, bool isExternal)
        {
            var creditCheckReviewInterval = context.TBL_SETUP_COMPANY.Select(x=>x.CREDITCHECKREVIEWINTERVAL).FirstOrDefault();

            var data = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(x => x.CUSTOMERID == customerId && x.DELETED == false
                                                                                            && x.COMPANYDIRECTORID == null
                                                                                            && (DbFunctions.DiffDays(x.DATETIMECREATED, DateTime.Now).Value <= creditCheckReviewInterval)
                                                                                            );

            int creditBureauCount = data.Count();

            List<CustomerViewModels> allCorporate = new List<CustomerViewModels>();
            var customerInfo = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customerId).FirstOrDefault();
            var customerType = context.TBL_CUSTOMER.Find(customerId).TBL_CUSTOMER_TYPE.CUSTOMERTYPEID;
            var customerContact = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customerId && x.ACTIVE == true).FirstOrDefault();
            var phoneNumber = customerContact != null ? customerContact.PHONENUMBER : null;
            var customer = from a in context.TBL_CUSTOMER
                           where a.DELETED == false && a.CUSTOMERID == customerId
                           select
                           new CustomerViewModels
                           {
                               accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                               branchId = a.BRANCHID,
                               branchName = a.TBL_BRANCH.BRANCHNAME,
                               createdBy = a.CREATEDBY,
                               customerCode = a.CUSTOMERCODE,
                               customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                               customerTypeId = (short)a.CUSTOMERTYPEID,
                               dateOfBirth = (DateTime)a.DATEOFBIRTH,
                               phoneNumber = phoneNumber,
                               customerId = a.CUSTOMERID,
                               emailAddress = a.EMAILADDRESS,
                               firstName = a.FIRSTNAME,
                               gender = a.GENDER,
                               lastName = a.LASTNAME,
                               maidenName = a.MAIDENNAME,
                               maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                               title = a.TITLE,
                               middleName = a.MIDDLENAME,
                               customerTypeName = a.TBL_CUSTOMER_TYPE.NAME,
                               nationalityId = a.NATIONALITYID,
                               occupation = a.OCCUPATION,
                               placeOfBirth = a.PLACEOFBIRTH,
                               isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                               sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                               sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                               subSectorName = a.TBL_SUB_SECTOR.NAME,
                               taxNumber = a.TAXNUMBER,
                               riskRatingId = a.RISKRATINGID,
                               riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                               customerBVN = a.CUSTOMERBVN,
                               //isCreditBureauUploadCompleted = false,
                               companyDirectorId = null,
                               creditBureauCount = creditBureauCount
                           };

            foreach (var item in customer)
            {
                var casa = context.TBL_CASA.Where(x => x.CUSTOMERID == item.customerId).FirstOrDefault();
                if (casa != null) item.customerAccountNo = casa.PRODUCTACCOUNTNUMBER;

                var phoneContact = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == item.customerId).FirstOrDefault();
                if (phoneContact != null) item.phoneNumber = phoneContact.PHONENUMBER;

                if (customerInfo != null) item.rcNumber = customerInfo.REGISTRATIONNUMBER;

                allCorporate.Add(item);
            }


            if (customerType == (short)CustomerTypeEnum.Corporate)
            {

                var shareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == customerId && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember).ToList();
                foreach (var director in shareholders)
                {
                    var directorData = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(x => x.CUSTOMERID == x.CUSTOMERID && x.DELETED == false
                                                                                    && x.COMPANYDIRECTORID == director.COMPANYDIRECTORID
                                                                                    && (DbFunctions.DiffDays(x.DATETIMECREATED, DateTime.Now).Value <= creditCheckReviewInterval)
                                                                                     );
                    var b = directorData.ToList();
                    int directorCount = directorData.Count();
                    var typeCustomer = director.TBL_CUSTOMER_TYPE.TBL_CUSTOMER.FirstOrDefault();
                    CustomerViewModels shareholdersData = new CustomerViewModels
                    {
                        companyDirectorId = director.COMPANYDIRECTORID,
                        customerTypeId = director.CUSTOMERTYPEID,
                        customerTypeName = director.TBL_CUSTOMER_TYPE.NAME,
                        numberOfShares = director.SHAREHOLDINGPERCENTAGE,
                        isPoliticallyExposed = director.ISPOLITICALLYEXPOSED,
                        customerBVN = director.CUSTOMERBVN,
                        companyDirectorTypeId = director.COMPANYDIRECTORTYPEID,
                        companyDirectorTypeName = director.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                        address = director.ADDRESS,
                        phoneNumber = director.PHONENUMBER,
                        dateOfBirth = typeCustomer != null ? (DateTime)typeCustomer.DATEOFBIRTH : DateTime.Now,
                        customerId = customerId,
                        emailAddress = director.EMAILADDRESS,
                        firstName = director.FIRSTNAME,
                        lastName = director.SURNAME,
                        middleName = director.MIDDLENAME,
                        creditBureauCount = directorCount,
                    };
                    //if(directorCount >0)
                    allCorporate.Add(shareholdersData);
                }
            }
            //if (isExternal)
            //{
            //    var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            //    if (setup.USE_THIRD_PARTY_INTEGRATION)
            //    {
            //        foreach (var item in customer)
            //        {
            //            try
            //            {
            //                var i = integration.AddCustomerAccounts(item.customerCode);
            //            }
            //            catch (APIErrorException ex)
            //            {
            //                return allCorporate;
            //                //throw new APIErrorException(ex.Message);
            //            }
            //            catch (Exception ex)
            //            {
            //                return allCorporate;  //throw ex; // new SecureException(ex.Message);
            //            }
            //        }
            //    }
            //}

            return allCorporate;
        }

        public void AddCustomerAccounts(IEnumerable<CustomerViewModels> customers)
        {
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                foreach (var customer in customers)
                {
                    try
                    {
                        var i = integration.AddCustomerAccounts(customer.customerCode);
                    }
                    catch (APIErrorException ex)
                    {
                        throw new APIErrorException(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new ConditionNotMetException("An error occured while get customer account details");
                    }
                }
            }
        }

        public int AddCustomerCreditBureauCharge(LoanCreditBureauViewModel entity)
        {
            var customerId = entity.customerId;
            var companyDirectorId = entity.companyDirectorId;
            var previousSearch = this.GetCustomerCreditBureauReportLog(customerId, companyDirectorId);

            bool hascrms = false;
            foreach (var i in previousSearch)
            {
                if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
            };

            //if (previousSearch.Count() >= 2 && !hascrms && entity.creditBureauId != (short)CreditBureauEnum.CRMS)
            //    throw new SecureException("Only three search options allowed and must inlude CRMS.\n Please check CRMS");

            //if (previousSearch.Count() >= 3)
            //    throw new SecureException("You have reached that maximum credit bureau search for this customer");

            if (entity.companyDirectorId == 0) entity.companyDirectorId = null;

            var data = new Entities.Models.TBL_CUSTOMER_CREDIT_BUREAU()
            {
                COMPANYDIRECTORID = entity.companyDirectorId,
                CHARGEAMOUNT = entity.chargeAmount,
                CREDITBUREAUID = entity.creditBureauId,
                CUSTOMERID = entity.customerId,
                ISREPORTOKAY = entity.isReportOkay,
                USEDINTEGRATION = entity.usedIntegration,
                DATECOMPLETED = entity.dateCompleted,
                DATETIMECREATED = DateTime.Now,
                DEBITBUSINESS = entity.debitBusiness,
                ACCOUNTNUMBER = entity.accountNumber,
                BRANCHID = entity.userBranchId,
                CREATEDBY = entity.createdBy
            };
            context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);
            if (context.SaveChanges() > 0) return data.CUSTOMERCREDITBUREAUID;
            else return 0;
        }

        public int AddCustomerCreditBureauUpload(LoanCreditBureauViewModel entity, LoanDocumentViewModel docModel, byte[] file)
        {
            string[] chanelArray = new string[] { "docx", "pdf", "jpg", "jpeg", "png", "PNG", "txt", "xlsx", "xls", "doc", "xml", "PNG", "PDF", "JPG", "JPEG", "XML" };
            if (!chanelArray.Contains(docModel.fileExtension))
            {
                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + chanelArray);
            }

            var previousSearch = this.GetCustomerCreditBureauReportLog(entity.customerId, entity.companyDirectorId);
            bool hascrms = false;
            foreach (var i in previousSearch)
            {
                if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
            };

            //if (previousSearch.Count() >= 2 && !hascrms && entity.creditBureauId != (short)CreditBureauEnum.CRMS)
            //    throw new SecureException("Only three search options allowed and must inlude CRMS.\n Please check CRMS");

            //if (previousSearch.Count() >= 3)
            //    throw new SecureException("You have reached that maximum credit bureau search for this customer");

            if (entity.companyDirectorId == 0) entity.companyDirectorId = null;

            var creditCheckReviewInterval = context.TBL_SETUP_COMPANY.Select(x => x.CREDITCHECKREVIEWINTERVAL).FirstOrDefault();

            var existing = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(O => O.CUSTOMERID == entity.customerId && O.CREDITBUREAUID == entity.creditBureauId 
                                                                                && O.COMPANYDIRECTORID == entity.companyDirectorId && O.DELETED == false 
                                                                                && (DbFunctions.DiffDays(O.DATETIMECREATED, DateTime.Now).Value <= creditCheckReviewInterval));

            if (existing != null) {
                return existing.CUSTOMERCREDITBUREAUID;
            }

            var data = new Entities.Models.TBL_CUSTOMER_CREDIT_BUREAU()
            {
                COMPANYDIRECTORID = entity.companyDirectorId,
                CHARGEAMOUNT = entity.chargeAmount,
                CREDITBUREAUID = entity.creditBureauId,
                CUSTOMERID = entity.customerId,
                ISREPORTOKAY = entity.isReportOkay,
                USEDINTEGRATION = entity.usedIntegration,
                DATECOMPLETED = entity.dateCompleted,
                DATETIMECREATED = DateTime.Now,
                BRANCHID = entity.userBranchId,
                CREATEDBY = entity.createdBy
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditBureauReportDocumentAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Credit Bureau Report Document with title : '{ docModel.documentTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
         
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------
            context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);

            if (context.SaveChanges() > 0)
            {
                docModel.customerCreditBureauId = data.CUSTOMERCREDITBUREAUID;
                if (AddCreditBureauReportDocument(docModel, file))
                {
                    return data.CUSTOMERCREDITBUREAUID;
                }
                else return 0;
            }
            else return 0;
          
        }

        public bool AddCreditBureauReportDocument(LoanDocumentViewModel model, byte[] file)
        {
            string[] chanelArray = new string[] { "docx", "pdf", "jpg", "jpeg", "png","PNG", "txt", "xlsx", "xls", "doc", "xml", "PNG", "PDF", "JPG", "JPEG", "XML" };
            if (!chanelArray.Contains(model.fileExtension))
            {
                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + chanelArray);
            }
            try
            {
                var data = new Entities.DocumentModels.TBL_CUSTOMER_CREDIT_BUREAU
                {
                    FILEDATA = file,
                    CUSTOMERCREDITBUREAUID = model.customerCreditBureauId,
                    DOCUMENT_TITLE = model.documentTitle,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                docContext.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);

                return docContext.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBureauViewModel model)
        {
            var creditCheckReviewInterval = context.TBL_SETUP_COMPANY.Select(x => x.CREDITCHECKREVIEWINTERVAL).FirstOrDefault();
            var directorId = model.companyDirectorId > 0 ? model.companyDirectorId : null;
            var data = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(c => c.CREDITBUREAUID == model.creditBureauId
                                                                && c.CUSTOMERID == model.customerId
                                                                && c.COMPANYDIRECTORID == directorId
                                                                && c.DELETED == false
                                                                && (DbFunctions.DiffDays(c.DATETIMECREATED, DateTime.Now).Value <= creditCheckReviewInterval)).FirstOrDefault();

            if (data != null)
                data.ISREPORTOKAY = status;

            context.SaveChanges();

            return true;
        }

        public bool UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBureauViewModel> model)
        {
            foreach (var item in model)
            {
                if (UpdateCreditBureauCustomerReportStatus(status, item) == false) return false;
            }

            return true;
        }

        public IEnumerable<CreditBereauViewModel> GetCreditBureauInformation()
        {
            var creditBureauList = from a in context.TBL_CREDIT_BUREAU
                                   where a.INUSE
                                   select new CreditBereauViewModel
                                   {
                                       creditBureauId = a.CREDITBUREAUID,
                                       creditBureauName = a.CREDITBUREAUNAME,
                                       corporateChargeAmount = a.CORPORATE_CHARGEAMOUNT,
                                       retailChargeAmount = a.INDIVIDUAL_CHARGEAMOUNT,
                                       inUse = a.INUSE,
                                       isMandatory = a.ISMANDATORY,
                                       useIntegration = a.USEINTEGRATION,
                                       appliedSearchForLoan = false,
                                       hasFile = false,
                                       fileName = string.Empty,

                                   };

            return creditBureauList;

        }

        public IEnumerable<CRCBureauFacilityViewModel> GetCRCBureauFacilities()
        {
            var creditBureauList = from a in context.TBL_CUSTOM_CRCBUREAU_PRODUCT
                                   select new CRCBureauFacilityViewModel
                                   {
                                       productCode = a.PRODUCTCODE,
                                       productName = a.PRODUCTNAME
                                   };

            return creditBureauList;
        }

        public List<LoanCreditBureauViewModel> GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId)
        {
            var creditCheckReviewInterval = context.TBL_SETUP_COMPANY.Select(x => x.CREDITCHECKREVIEWINTERVAL).FirstOrDefault();
            var directorId = companyDirectorId > 0 ? companyDirectorId : null;
            var customerLoanCreditBureauData = (from a in context.TBL_CUSTOMER_CREDIT_BUREAU
                                                where a.CUSTOMERID == customerId && a.DELETED == false && a.COMPANYDIRECTORID == directorId
                                                 && (DbFunctions.DiffDays(a.DATETIMECREATED, DateTime.Now).Value <= creditCheckReviewInterval)
                                                select new LoanCreditBureauViewModel
                                                {
                                                    customerCreditBureauId = a.CUSTOMERCREDITBUREAUID,
                                                    companyDirectorId = a.COMPANYDIRECTORID,
                                                    companyDirectorName = a.TBL_CUSTOMER_COMPANY_DIRECTOR.FIRSTNAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.MIDDLENAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.SURNAME,
                                                    chargeAmount = a.CHARGEAMOUNT,
                                                    customerId = a.CUSTOMERID,
                                                    creditBureauId = a.CREDITBUREAUID,
                                                    isReportOkay = a.ISREPORTOKAY,
                                                    usedIntegration = a.USEDINTEGRATION,
                                                    dateCompleted = (DateTime)a.DATECOMPLETED,
                                                    dateTimeCreated = a.DATETIMECREATED,
                                                    searchCount = 0,
                                                    uploadCount = 0,
                                                    createdBy = a.CREATEDBY,
                                                    debitBusiness = a.DEBITBUSINESS,
                                                    dayAgo = DbFunctions.DiffDays(a.DATETIMECREATED, DateTime.Now).Value
                                                }).ToList();
            return customerLoanCreditBureauData;
        }

        public List<LoanCreditBureauViewModel> GetCustomerCreditBureauReportLogDeleted(int customerId, int? companyDirectorId)
        {
            var directorId = companyDirectorId > 0 ? companyDirectorId : null;
            var customerLoanCreditBureauData = (from a in context.TBL_CUSTOMER_CREDIT_BUREAU
                                                where a.CUSTOMERID == customerId && a.DELETED == true && a.COMPANYDIRECTORID == directorId
                                                select new LoanCreditBureauViewModel
                                                {
                                                    customerCreditBureauId = a.CUSTOMERCREDITBUREAUID,
                                                    companyDirectorId = a.COMPANYDIRECTORID,
                                                    companyDirectorName = a.TBL_CUSTOMER_COMPANY_DIRECTOR.FIRSTNAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.MIDDLENAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.SURNAME,
                                                    chargeAmount = a.CHARGEAMOUNT,
                                                    customerId = a.CUSTOMERID,
                                                    creditBureauId = a.CREDITBUREAUID,
                                                    isReportOkay = a.ISREPORTOKAY,
                                                    usedIntegration = a.USEDINTEGRATION,
                                                    dateCompleted = (DateTime)a.DATECOMPLETED,
                                                    dateTimeCreated = a.DATETIMECREATED,
                                                    searchCount = 0,
                                                    uploadCount = 0,
                                                    createdBy = a.CREATEDBY,
                                                    debitBusiness = a.DEBITBUSINESS,
                                                    dayAgo = DbFunctions.DiffDays(a.DATETIMECREATED, DateTime.Now).Value
                                                }).ToList();
            return customerLoanCreditBureauData;
        }

        public List<CreditBureauDocument> GetCreditBureauDocument(int customerCreditBureauId)
        {
            return (from d in docContext.TBL_CUSTOMER_CREDIT_BUREAU
                    where d.CUSTOMERCREDITBUREAUID == customerCreditBureauId
                    select new CreditBureauDocument
                    {
                        documentId = d.DOCUMENTID,
                        customerCreditBureauId = d.CUSTOMERCREDITBUREAUID,
                        documentTitle = d.DOCUMENT_TITLE,
                        fileName = d.FILENAME,
                        fileExtension = d.FILEEXTENSION,
                        fileData = d.FILEDATA,
                    }).ToList();
        }

        public bool VerifyCustomerValidCreditBureau(int customerId)
        {
            var customers = GetCreditBureauCustomerDetailsByCustomerId(customerId, false);
            var creditBureau = GetCreditBureauInformation();
            int creditCount = 0;
            foreach (var customer in customers)
            {
                var customerCreditBureauLog = GetCustomerCreditBureauReportLog(customer.customerId, customer.companyDirectorId);
                if (customerCreditBureauLog.Count() > 0)
                {
                    foreach (var cb in creditBureau.ToList())
                    {
                        if (customerCreditBureauLog.Where(x => x.creditBureauId == cb.creditBureauId).Any()) creditCount++;
                    }
                }
                if (creditCount < 3) return false;
                creditCount = 0;
            };

            return true;

        }
        #endregion

        #region Integration 
        public XDSSearchResult GetCustomerXDSCreditMatch(CreditBureauSearchViewModel searchInfoList)
        {
            XDSSearchResult resultData;

            if (searchInfoList.dateOfBirth != string.Empty && searchInfoList.dateOfBirth != null)
            {
                var dateOfBirth = Convert.ToDateTime(searchInfoList.dateOfBirth);
                searchInfoList.dateOfBirth = dateOfBirth.ToString("dd-MMM-yyyy", null);
            }


            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInfoList.creditBureauId);
            if (creditBureau != null)
            {
                searchInfoList.userName = creditBureau.USERNAME;
                searchInfoList.password = creditBureau.PASSWORD;
            }
            else throw new ConditionNotMetException("Could not resolve the selected Credit Bureau item. Contact admin.");



            List<string> searchResult = new List<string>();
            var feedBackString = string.Empty;
            feedBackString = _creditBureau.XDSSearchCreditBureau(searchInfoList);
            //var task = Task.Run(() => feedBackString = _creditBureau.XDSSearchCreditBureau(searchInfoList));
            //if (task.Wait(TimeSpan.FromSeconds(2000)))
            //{
            resultData = new XDSSearchResult()
            {
                searchResult = feedBackString,
                status = 0
            };
            JObject json = JObject.Parse(feedBackString);

            if (json.Count >= 1)
            {
                JObject jsonNoResult = json;
                Object CommercialID;
                if (json["CommercialMatching"] != null || json["ConsumerMtaching"] != null)
                {
                    if (searchInfoList.searchType == (short)CreditBureauTypeEnum.CommercialSearch)
                    {
                        try { CommercialID = json["CommercialMatching"]["MatchedCommercial"]["CommercialID"].ToString(); } catch { CommercialID = 1; }
                    }
                    else
                    {
                        try { CommercialID = json["ConsumerMtaching"]["MatchedConsumer"]["ConsumerID"].ToString(); } catch { CommercialID = 1; }
                    }

                    if (Convert.ToInt32(CommercialID) == 0)
                    {
                        resultData.status = 1;
                    }
                }
                else if (jsonNoResult["NoResult"] != null)
                {
                    string stringNoResult = "XDS API Response - No Result Found"; //+ feedBackString; // noResult.ToString();
                    resultData.errorMessage = stringNoResult;
                    resultData.errorOccured = true;
                    resultData.status = 2;
                }
                else
                {
                    resultData.errorOccured = true;
                    resultData.status = 3;
                }
            }
            else
            {
                resultData.errorOccured = true;
                resultData.status = 3;
            }
            //var task = Task.Run(() => binaryData = creditBureauProcess.GetXDSFullSearchResultInPDF(searchInput));

            return resultData;

            //try
            //{

            //}
            //catch (ConditionNotMetException ex)
            //{
            //    throw new ConditionNotMetException(ex.Message);
            //}
            //catch (APIErrorException ex)
            //{
            //    throw new APIErrorException(ex.Message);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        private CreditBereauViewModel getBuiltCRCChargeModel(CRCRequestViewModel searchInfo)
        {
            var chargeModel = new CreditBereauViewModel();
            chargeModel.createdBy = searchInfo.createdBy;
            chargeModel.userBranchId = searchInfo.userBranchId;
            chargeModel.companyId = searchInfo.companyId;
            chargeModel.casaAccountId = searchInfo.casaAccountId;
            chargeModel.username = searchInfo.username;
            chargeModel.passCode = searchInfo.passCode;

            return chargeModel;
        }

        private SearchInput getBuiltCRCSearchInputModel(CRCRequestViewModel searchInfo)
        {
            var creditBureauInputs = new SearchInput()
            {
                applicationUrl = searchInfo.applicationUrl,
                userBranchId = searchInfo.userBranchId,
                staffId = searchInfo.staffId,
                companyId = searchInfo.companyId,
                createdBy = searchInfo.createdBy,
                casaAccountId = searchInfo.casaAccountId,
                creditBureauId = searchInfo.creditBureauId,
                userName = searchInfo.userName,
                password = searchInfo.password,
                customerCreditBureauUploadDetails = new LoanCreditBureauViewModel
                {
                    creditBureauId = searchInfo.creditBureauId,
                    companyDirectorId = searchInfo.companyDirectorId,
                    isReportOkay = true,
                    customerId = searchInfo.customerId,
                    chargeAmount = searchInfo.amount,
                    usedIntegration = true,
                    dateCompleted = DateTime.Now,
                    userBranchId = searchInfo.userBranchId
                }
            };

            return creditBureauInputs;
        }

        public CRCSearchResult GetCustomerCRCCreditMatch(CRCRequestViewModel searchInfo)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
            var chargeModel = getBuiltCRCChargeModel(searchInfo);
            var companyExternalServiceChargeInfo = GetLoanThirdPartyServiceChargeStatusDetails(searchInfo.companyId);

            var twoFADetails = new TwoFactorAutheticationViewModel();
            if (companyExternalServiceChargeInfo.requireCreditBureauModule && companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.NoCharge)
            {
                twoFADetails.username = searchInfo.username;
                twoFADetails.passcode = searchInfo.passCode;

                if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
                {
                    twoFADetails.username = context.TBL_STAFF.Find(searchInfo.staffId).STAFFCODE;
                }

                if (twoFADetails != null && admin.TwoFactorAuthenticationEnabled())
                {
                    var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException(authenticated.message);
                }
            }

            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInfo.creditBureauId);
            searchInfo.userName = creditBureau.USERNAME;
            searchInfo.password = creditBureau.PASSWORD;

            var creditBureauInputs = getBuiltCRCSearchInputModel(searchInfo);
            var casa = context.TBL_CASA.Find(creditBureauInputs.casaAccountId);
            var chargeAmount = (decimal)0;

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.NoCharge)
            {
                if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomer || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && !searchInfo.debitBusiness))
                {
                    if (casa == null) { throw new SecureException("Norminated Account Does not Exist"); }
                    if (creditBureauInputs.casaAccountId == 0) { throw new ConditionNotMetException("Missing Charge Account! Specify charge account or contact admin."); }
                }

                var accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;
                if (chargeAmount > accountBalance)
                {
                    if (!searchInfo.debitBusiness)
                        throw new SecureException("The norminated customer account has insufficient fund to perform this transaction.");
                }
            }

            creditBureauInputs.customerCreditBureauUploadDetails.accountNumber = casa?.PRODUCTACCOUNTNUMBER;
            chargeAmount = creditBureauInputs.searchType == (short)CreditBureauTypeEnum.ConsumerSearch ? creditBureau.INDIVIDUAL_CHARGEAMOUNT : creditBureau.CORPORATE_CHARGEAMOUNT;
            if (casa != null) referenceNumber = casa?.PRODUCTACCOUNTNUMBER;

            //chargeModel.feeAmount = chargeAmount;
            //chargeModel.referenceNumber = referenceNumber;
            //chargeModel.casaAccountId = casa?.CASAACCOUNTID;
            //chargeModel.debitBusiness = searchInfo.debitBusiness;

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeBank || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && searchInfo.debitBusiness))
            {
                var bizAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (short)OtherOperationEnum.ChargeOnBank).FirstOrDefault();
                if (bizAccount == null) throw new ConditionNotMetException("No Account has been mapped for charges on business");

                chargeModel.glAccountId = bizAccount.GLACCOUNTID;
                chargeModel.casaAccountId = null;

                var customChartOfAccount = context.TBL_CUSTOM_CHART_OF_ACCOUNT.FirstOrDefault(c => c.CUSTOMACCOUNTID == chargeModel.glAccountId);
                chargeModel.referenceNumber = customChartOfAccount != null ? customChartOfAccount.ACCOUNTID : string.Empty;

            }

            CRCSearchResult searchResponse = null;
            using (var docTrans = docContext.Database.BeginTransaction())
            using (var trans = context.Database.BeginTransaction())
            {
                if (searchInfo.accountOrRegistrationNumber == null)
                    searchInfo.accountOrRegistrationNumber = string.Empty;

                if (searchInfo.identification == null)
                    searchInfo.identification = string.Empty;

                searchResponse = _creditBureau.CRCCreditBureauSearch(searchInfo);

                //var task = Task.Run(() => searchResponse = _creditBureau.CRCCreditBureauSearch(searchInfo));

                //if (task.Wait(TimeSpan.FromSeconds(3500)))
                //{
                if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchIncomplete)
                {
                    JObject json = JObject.Parse(searchResponse.SearchResult);
                    if (json["DATAPACKET"]["BODY"]["ERROR-LIST"] != null)
                    {
                        string errorCode = json["DATAPACKET"]["BODY"]["ERROR-LIST"]["ERROR-CODE"].ToString();
                        errorCode.Replace("{", string.Empty);
                        errorCode.Replace("}", string.Empty);
                        var errorLog = context.TBL_CUSTOM_CREDITBUREAU_ERROR.Where(x => x.ERRORCODE == errorCode && x.BUREAUTYPE == "CRC");
                        if (errorLog.Any())
                        {
                            searchResponse.SearchResult = errorLog.FirstOrDefault().DESCRIPTION + ". ERROR-CODE: " + errorCode;
                            searchResponse.SearchCompleted = (int)SearchCompletedStatusEnum.SearchError;
                            searchResponse.errorOccured = true;
                        }
                    }
                    return searchResponse;
                }
                else if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchCompleted)
                {
                    byte[] fileArray = Encoding.ASCII.GetBytes(searchResponse.SearchResult);

                    var customerCreditBureauId = AddCustomerCreditBureauCharge(creditBureauInputs.customerCreditBureauUploadDetails);
                    if (SaveCreditBureauReportFile(customerCreditBureauId, fileArray, creditBureauInputs))
                    {
                        searchResponse.fileSaved = true;
                        searchResponse.file = fileArray;
                    }
                    else
                    {
                        searchResponse.errorOccured = true;
                        throw new ConditionNotMetException("Search could not save the result file");
                    }

                    if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.NoCharge) { DebitCustomer(chargeModel); }

                    context.SaveChanges();
                    trans.Commit();
                    docTrans.Commit();
                    return searchResponse;
                }
                else
                {
                    trans.Rollback();
                    var errorCode = searchResponse.SearchResult.Split(new string[] { "<ERROR-CODE>" }, StringSplitOptions.None)[1].Split('<')[0];
                    var errorDescription = string.Empty;

                    if (errorCode.ToLower().Contains("password")) {
                        errorDescription = errorCode;
                        errorCode = "0";
                    }
                    else {
                        errorDescription = context.TBL_CUSTOM_CREDITBUREAU_ERROR.Where(O => O.ERRORCODE == errorCode).FirstOrDefault().DESCRIPTION;
                    }

                    throw new ConditionNotMetException($"Search Response - ERRORCODE: {errorCode} ERRORMESSAGE: {errorDescription}");
                }
                //}
                //else
                //{
                //    trans.Rollback();
                //    throw new ConditionNotMetException("Search result Timed out");
                //}

                //try
                //{

                //}
                //catch (ConditionNotMetException ex)
                //{
                //    trans.Rollback();
                //    throw new ConditionNotMetException(ex.Message);

                //}
                //catch (TimeoutException ex)
                //{
                //    trans.Rollback();
                //    throw new CustomTimeoutException(ex.Message);
                //}
                //catch (APIErrorException ex)
                //{
                //    trans.Rollback();
                //    throw new ConditionNotMetException(ex.Message);

                //}
                //catch (SecureException ex)
                //{
                //    trans.Rollback();
                //    throw new ConditionNotMetException(ex.Message);

                //}
                //catch (Exception ex)
                //{
                //    trans.Rollback();
                //    throw new BadLogicException(ex.Message);

                //}
            }
        }

        private SearchInput getBuiltCRCCreditBureauSearchInputs(MultiHitRequestViewModel request)
        {
            var creditBureauInputs = new SearchInput()
            {
                applicationUrl = request.applicationUrl,
                userBranchId = request.userBranchId,
                staffId = request.staffId,
                companyId = request.companyId,
                createdBy = request.createdBy,
                creditBureauId = request.creditBureauId,
                casaAccountId = request.casaAccountId,
                searchType = request.searchType,

                customerCreditBureauUploadDetails = new LoanCreditBureauViewModel
                {
                    creditBureauId = request.creditBureauId,
                    companyDirectorId = request.companyDirectorId,
                    isReportOkay = true,
                    customerId = request.customerId,
                    userBranchId = request.userBranchId,
                    usedIntegration = true,
                    dateCompleted = DateTime.Now,
                    debitBusiness = request.debitBusiness
                }
            };
            return creditBureauInputs;
        }

        public string GetCRCFullCreditMergeReport(MultiHitRequestViewModel request)
        {
            var companyExternalServiceChargeInfo = GetLoanThirdPartyServiceChargeStatusDetails(request.companyId);
            var creditBureau = context.TBL_CREDIT_BUREAU.Find(request.creditBureauId);
            var creditBureauInputs = getBuiltCRCCreditBureauSearchInputs(request);
            var casa = context.TBL_CASA.Find(creditBureauInputs.casaAccountId);
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            var chargeAmount = (decimal)0;

            if (creditBureau != null)
            {
                request.userName = creditBureau.USERNAME;
                request.password = creditBureau.PASSWORD;
            }
            else { throw new ConditionNotMetException("Could not resolve the selected Credit Bureau item. Contact admin."); }

            creditBureauInputs.customerCreditBureauUploadDetails.chargeAmount = request.companyDirectorId != 0 ? creditBureau.INDIVIDUAL_CHARGEAMOUNT : creditBureau.CORPORATE_CHARGEAMOUNT;
            chargeAmount = creditBureauInputs.searchType == (short)CreditBureauTypeEnum.ConsumerSearch ? creditBureau.INDIVIDUAL_CHARGEAMOUNT : creditBureau.CORPORATE_CHARGEAMOUNT;

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.NoCharge)
            {
                if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomer || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && !request.debitBusiness))
                {
                    if (casa == null) { throw new SecureException("Norminated Account Does not Exist"); }

                    if (creditBureauInputs.casaAccountId == 0) { throw new ConditionNotMetException("Missing Charge Account! Specify charge account or contact admin."); }

                    if (chargeAmount > financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance) throw new SecureException("The norminated customer account has insufficient fund to perform this transaction.");
                }

                creditBureauInputs.customerCreditBureauUploadDetails.accountNumber = casa != null ? casa.PRODUCTACCOUNTNUMBER : null;
            }

            if (casa != null) referenceNumber = casa.PRODUCTACCOUNTNUMBER;
            var chargeModel = new CreditBereauViewModel();
            chargeModel.feeAmount = chargeAmount;
            chargeModel.createdBy = request.createdBy;
            chargeModel.userBranchId = request.userBranchId;
            chargeModel.companyId = request.companyId;
            chargeModel.referenceNumber = referenceNumber;
            chargeModel.casaAccountId = casa?.CASAACCOUNTID;
            chargeModel.debitBusiness = request.debitBusiness;

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeBank || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && request.debitBusiness))
            {
                var bizAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (short)OtherOperationEnum.ChargeOnBank).FirstOrDefault();
                if (bizAccount == null) throw new ConditionNotMetException("No Account has been mapped for charges on business");

                chargeModel.glAccountId = bizAccount.GLACCOUNTID;
                chargeModel.casaAccountId = null;

                var customChartOfAccount = context.TBL_CUSTOM_CHART_OF_ACCOUNT.FirstOrDefault(c => c.CUSTOMACCOUNTID == chargeModel.glAccountId);
                chargeModel.referenceNumber = customChartOfAccount != null ? customChartOfAccount.ACCOUNTID : string.Empty;

                chargeModel.currencyId = context.TBL_CURRENCY.Where(x => x.CURRENCYCODE == customChartOfAccount.CURRENCYCODE).FirstOrDefault().CURRENCYID;
            }

            const string DATA_PACKET = "DATAPACKET";
            const string ERROR = "ERROR_LIST";

            using (var docTrans = docContext.Database.BeginTransaction())
            using (var trans = context.Database.BeginTransaction())
            {
                string dataResponse = null;
                byte[] fileArray = null;
                try
                {
                    var task = Task.Run(() => dataResponse = _creditBureau.CRCCreditBureauMerge(request));
                    if (task.Wait(TimeSpan.FromSeconds(3500)))
                    {
                        XmlDocument xdoc = new XmlDocument();
                        if (dataResponse.Contains(DATA_PACKET))
                        {
                            if (!dataResponse.Contains(ERROR))
                            {
                                xdoc.LoadXml(dataResponse);

                                var SearchResult = new CreditBureauHelp().ConvertXmlToJson(xdoc);
                                JObject json = JObject.Parse(SearchResult);
                                if (json.Count >= 1)
                                {
                                    if (json["DATAPACKET"]["BODY"]["ERROR-LIST"] != null)
                                    {
                                        string errorCode = json["DATAPACKET"]["BODY"]["ERROR-LIST"]["ERROR-CODE"].ToString();
                                        errorCode.Replace("{", string.Empty);
                                        errorCode.Replace("}", string.Empty);
                                        var errorLog = context.TBL_CUSTOM_CREDITBUREAU_ERROR.Where(x => x.ERRORCODE == errorCode && x.BUREAUTYPE == "CRC");
                                        if (errorLog.Any())
                                        {
                                            throw new APIErrorException("Credit Bureau API Error - " + errorLog.FirstOrDefault().DESCRIPTION + ". ERROR-CODE: " + errorCode);
                                        }
                                    }
                                }
                                else { throw new ConditionNotMetException("Fintrak Credit 360 Could not resolve the API response."); }
                            }
                        }

                        if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.ChargeBank) { DebitCustomer(chargeModel); }

                        fileArray = Encoding.ASCII.GetBytes(dataResponse);

                        var customerCreditBureauId = AddCustomerCreditBureauCharge(creditBureauInputs.customerCreditBureauUploadDetails);
                        if (SaveCreditBureauReportFile(customerCreditBureauId, fileArray, creditBureauInputs))

                            context.SaveChanges();
                        trans.Commit();
                        docTrans.Commit();
                        return dataResponse;
                    }
                    else throw new Exception("Application failed to fetch merge file");

                }
                catch (ConditionNotMetException ex)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ex.Message.ToString());
                }
                catch (APIErrorException ex)
                {
                    trans.Rollback();
                    throw new APIErrorException(ex.Message.ToString());
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new BadLogicException(ex.Message.ToString());
                }
            }
        }

        public bool saveCrcPdfFile(CRCRequestViewModel searchInfo, SearchInput creditBureauInputs)
        {
            CRCSearchResult searchResponse = null;// new CRCSearchResult();

            using (var docTrans = docContext.Database.BeginTransaction())
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var task = Task.Run(() => searchResponse = _creditBureau.CRCCreditBureauSearch(searchInfo));
                    var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInfo.creditBureauId);

                    if (creditBureau != null)
                    {
                        searchInfo.password = creditBureau.PASSWORD;
                        searchInfo.userName = creditBureau.USERNAME;
                    }

                    if (task.Wait(TimeSpan.FromSeconds(640)))
                    {
                        if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchIncomplete) return true;
                        else if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchCompleted)
                        {
                            {
                                byte file = Convert.ToByte(searchResponse.SearchResult);

                                byte[] fileArray = new byte[file];

                                var customerCreditBureauId = AddCustomerCreditBureauCharge(creditBureauInputs.customerCreditBureauUploadDetails);

                                if (!SaveCreditBureauReportFile(customerCreditBureauId, fileArray, creditBureauInputs))
                                {
                                    throw new SecureException("Could not save file");
                                }

                                context.SaveChanges();
                                trans.Commit();
                                docTrans.Commit();

                                return true;
                            }
                        }
                        else
                        {
                            throw new SecureException("An error occured");
                        }
                    }
                    else
                    {
                        throw new SecureException("Timed out");
                    }
                }
                catch (APIErrorException ex)
                {
                    throw new APIErrorException(ex.Message);
                }
                catch (TimeoutException ex)
                {
                    throw new TimeoutException(ex.Message);
                }
                catch (ConditionNotMetException ex)
                {
                    throw new ConditionNotMetException(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
        }

        public XDSSearchResult GetXDSFullSearchResultInPDF(SearchInput searchInput)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            var companyExternalServiceChargeInfo = GetLoanThirdPartyServiceChargeStatusDetails(searchInput.companyId);

            var casa = context.TBL_CASA.Find(searchInput.casaAccountId);
            var accountBalance = casa != null ? financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance : 0;

            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInput.creditBureauId);
            var chargeAmount = searchInput.searchType == (short)CreditBureauTypeEnum.ConsumerSearch ? creditBureau.INDIVIDUAL_CHARGEAMOUNT : creditBureau.CORPORATE_CHARGEAMOUNT;

            var twoFADetails = new TwoFactorAutheticationViewModel();

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.ChargeBank)
            {
                if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomer || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && !searchInput.debitBusiness))
                {
                    if (searchInput.casaAccountId == 0)
                        throw new ConditionNotMetException("Missing Charge Account! Specify charge account or contact admin.");

                    if (casa == null) throw new ConditionNotMetException("Norminated Account Does not Exist");

                    if (chargeAmount > accountBalance) throw new ConditionNotMetException("The norminated customer account has insufficient fund to perform this transaction.");
                    if (casa != null) referenceNumber = casa.PRODUCTACCOUNTNUMBER;

                    twoFADetails.username = searchInput.username;
                    twoFADetails.passcode = searchInput.passCode;

                    if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
                    {
                        twoFADetails.username = context.TBL_STAFF.Find(searchInput.staffId).STAFFCODE;
                    }


                    if (twoFADetails.username != null && admin.TwoFactorAuthenticationEnabled())
                    {
                        var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                        if (authenticated.authenticated == false)
                            throw new TwoFactorAuthenticationException(authenticated.message);
                    }
                }
            }

            var chargeModel = new CreditBereauViewModel();
            chargeModel.createdBy = searchInput.createdBy;
            chargeModel.userBranchId = searchInput.userBranchId;
            chargeModel.companyId = searchInput.companyId;
            chargeModel.casaAccountId = searchInput.casaAccountId;
            chargeModel.username = searchInput.username;
            chargeModel.passCode = searchInput.passCode;
            chargeModel.referenceNumber = referenceNumber;
            chargeModel.feeAmount = chargeAmount;
            chargeModel.casaAccountId = casa?.CASAACCOUNTID;

            searchInput.customerCreditBureauUploadDetails.accountNumber = casa != null ? casa?.PRODUCTACCOUNTNUMBER : null;
            searchInput.customerCreditBureauUploadDetails.userBranchId = searchInput.userBranchId;
            searchInput.userName = creditBureau.USERNAME;
            searchInput.password = creditBureau.PASSWORD;

            if (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeBank || (companyExternalServiceChargeInfo.creditBureauSearchTypeId == (short)ChargeTypeEnum.ChargeCustomerORBank && searchInput.debitBusiness))
            {
                var customChartOfAccount = context.TBL_CUSTOM_CHART_OF_ACCOUNT.FirstOrDefault(c => c.CUSTOMACCOUNTID == chargeModel.glAccountId);
                chargeModel.referenceNumber = customChartOfAccount != null ? customChartOfAccount.ACCOUNTID : string.Empty;
                chargeModel.casaAccountId = null;
            }

            byte[] binaryData = null;
            var response = new XDSSearchResult();
            var creditBureauProcess = new CreditBureauProcess();

            var task = Task.Run(() => binaryData = creditBureauProcess.GetXDSFullSearchResultInPDF(searchInput));
            if (task.Wait(TimeSpan.FromSeconds(2500)))
            {
                if (binaryData == null)
                    throw new SecureException("File report not found. Please try again.");

                using (var docTrans = docContext.Database.BeginTransaction())
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var customerCreditBureauId = AddCustomerCreditBureauCharge(searchInput.customerCreditBureauUploadDetails);
                        if (!SaveCreditBureauReportFile(customerCreditBureauId, binaryData, searchInput))
                        {
                            response.fileSaved = false;
                            response.errorOccured = true;
                            response.status = 1;
                            throw new SecureException("Could not save file");
                        }
                        else
                        {
                            response.file = binaryData;
                            response.searchResult = Encoding.ASCII.GetString(binaryData);
                            response.errorOccured = false;
                            response.status = 0;
                            response.fileSaved = true;
                        }

                        if (companyExternalServiceChargeInfo.creditBureauSearchTypeId != (short)ChargeTypeEnum.NoCharge) { DebitCustomer(chargeModel); }

                        context.SaveChanges();
                        trans.Commit();
                        docTrans.Commit();

                        return response;
                    }
                    catch (TimeoutException ex)
                    {
                        throw new ConditionNotMetException(ex.Message);
                    }
                    catch (APIErrorException ex)
                    {
                        throw new ConditionNotMetException(ex.Message);
                    }
                    catch (ConditionNotMetException ex)
                    {
                        throw new ConditionNotMetException(ex.Message);
                    }
                    catch (SecureException ex)
                    {
                        throw new SecureException(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                throw new ConditionNotMetException("Search result Timed out");
            }

        }

        private bool SaveCreditBureauReportFile(int customerCreditBureauId, byte[] file, SearchInput model)
        {
            try
            {
                var creditBureau = context.TBL_CREDIT_BUREAU.Find(model.creditBureauId);
                var b = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == model.customerCreditBureauUploadDetails.customerId).FirstOrDefault();
                var fileName = b != null ? b.CUSTOMERCODE + creditBureau.CREDITBUREAUNAME : "new" + creditBureau.CREDITBUREAUNAME;
                var data = new Entities.DocumentModels.TBL_CUSTOMER_CREDIT_BUREAU
                {
                    CUSTOMERCREDITBUREAUID = customerCreditBureauId,
                    DOCUMENT_TITLE = creditBureau.CREDITBUREAUNAME + " Report Document Upload",
                    FILEEXTENSION = "pdf",
                    FILENAME = fileName.Trim(), //context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == model.customerCreditBureauUploadDetails.customerId).FirstOrDefault().CUSTOMERCODE + creditBureau.CREDITBUREAUNAME,
                    FILEDATA = file,
                    SYSTEMDATETIME = genSetup.GetApplicationDate(),
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = model.createdBy
                };

                docContext.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);

                // Audit Section ---------------------------
                var creditBureauInfo = context.TBL_CREDIT_BUREAU.Find(model.creditBureauId);
                var mergeId = model.mergeList;
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Uploaded '{ creditBureauInfo.CREDITBUREAUNAME }' Credit Bureau Report generated for merge ID list : '{ model.mergeList }' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                     DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    
                };
                this.auditTrail.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                docContext.SaveChanges();
                return context.SaveChanges() != 0;
            }
            catch (Exception ex) { throw ex; }
        }

        private void DebitCustomer(CreditBereauViewModel entity)
        {
            if (entity.feeAmount <= 0)
                return;

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true,
                username = entity.username,
                passcode = entity.passCode
            };
            entity.debitRequest = true;
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.AddRange(BuildCreditBureauFeesPosting(entity));

            if (inputTransactions.Count > 0) financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);
        }

        private void ReverseCustomerDebit(CreditBereauViewModel entity)
        {
            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                username = entity.username,
                passcode = entity.passCode
            };
            entity.debitRequest = false;
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.AddRange(BuildCreditBureauFeesPosting(entity));

            if (inputTransactions.Count > 0) financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);
        }

        //private void DebitCustomer(TBL_CREDIT_BUREAU creditBureau, TBL_CASA casa, decimal chargeAmount, SearchInput creditBureauInputs)
        //{
        //    var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
        //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
        //    debit.operationId = (int)OperationsEnum.CreditBureauSearch;
        //    debit.description = creditBureau.CREDITBUREAUNAME + " search charge";
        //    debit.valueDate = genSetup.GetApplicationDate();
        //    debit.transactionDate = debit.valueDate;
        //    debit.currencyId = casa.CURRENCYID;
        //    debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, creditBureauInputs.companyId).sellingRate;
        //    debit.isApproved = true;
        //    debit.postedBy = creditBureauInputs.createdBy;
        //    debit.approvedBy = creditBureauInputs.createdBy;
        //    debit.approvedDate = debit.transactionDate;
        //    debit.approvedDateTime = DateTime.Now;
        //    debit.sourceApplicationId = creditBureauInputs.creditBureauId;
        //    debit.companyId = creditBureauInputs.companyId;
        //    debit.batchCode = transactionCode;
        //    debit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL; 
        //    debit.sourceReferenceNumber = transactionCode;
        //    debit.casaAccountId = casa.CASAACCOUNTID;
        //    debit.debitAmount = chargeAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = creditBureauInputs.userBranchId;
        //    debit.destinationBranchId = casa.BRANCHID;

        //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //    credit.operationId = (int)OperationsEnum.CreditBureauSearch;
        //    credit.description = creditBureau.CREDITBUREAUNAME + " search charge";
        //    credit.valueDate = genSetup.GetApplicationDate();
        //    credit.transactionDate = credit.valueDate;
        //    credit.currencyId = casa.CURRENCYID;
        //    credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, creditBureauInputs.companyId).sellingRate;
        //    credit.isApproved = true;
        //    credit.postedBy = creditBureauInputs.createdBy;
        //    credit.approvedBy = creditBureauInputs.createdBy;
        //    credit.approvedDate = credit.transactionDate;
        //    credit.approvedDateTime = DateTime.Now;
        //    credit.sourceApplicationId = creditBureauInputs.creditBureauId;
        //    credit.companyId = creditBureauInputs.companyId;
        //    credit.batchCode = transactionCode;
        //    credit.glAccountId = creditBureau.GLACCOUNTID;
        //    credit.sourceReferenceNumber = transactionCode;
        //    credit.casaAccountId = null;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = chargeAmount;
        //    credit.sourceBranchId = creditBureauInputs.userBranchId;
        //    credit.destinationBranchId = creditBureauInputs.userBranchId;

        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

        //    inputTransactions.Add(debit);
        //    inputTransactions.Add(credit);
        //    financeTransaction.PostTransaction(inputTransactions);
        //}

        //private void ReverseDebit(TBL_CREDIT_BUREAU creditBureau, TBL_CASA casa, decimal chargeAmount, SearchInput creditBureauInputs)
        //{
        //    var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

        //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
        //    debit.operationId = (int)OperationsEnum.CreditBureauSearch;
        //    debit.description = creditBureau.CREDITBUREAUNAME + " search charge reversal";
        //    debit.valueDate = genSetup.GetApplicationDate();
        //    debit.transactionDate = debit.valueDate;
        //    debit.currencyId = casa.CURRENCYID;
        //    debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, creditBureauInputs.companyId).sellingRate;
        //    debit.isApproved = true;
        //    debit.postedBy = creditBureauInputs.createdBy;
        //    debit.approvedBy = creditBureauInputs.createdBy;
        //    debit.approvedDate = debit.transactionDate;
        //    debit.approvedDateTime = DateTime.Now;
        //    debit.sourceApplicationId = creditBureauInputs.creditBureauId;
        //    debit.companyId = creditBureauInputs.companyId;
        //    debit.batchCode = transactionCode;
        //    debit.glAccountId = creditBureau.GLACCOUNTID;
        //    debit.sourceReferenceNumber = transactionCode;
        //    debit.casaAccountId = null;
        //    debit.debitAmount = chargeAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = creditBureauInputs.userBranchId;
        //    debit.destinationBranchId = casa.BRANCHID;

        //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //    credit.operationId = (int)OperationsEnum.CreditBureauSearch;
        //    credit.description = creditBureau.CREDITBUREAUNAME + " search charge";
        //    credit.valueDate = genSetup.GetApplicationDate();
        //    credit.transactionDate = credit.valueDate;
        //    credit.currencyId = casa.CURRENCYID;
        //    credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, creditBureauInputs.companyId).sellingRate;
        //    credit.isApproved = true;
        //    credit.postedBy = creditBureauInputs.createdBy;
        //    credit.approvedBy = creditBureauInputs.createdBy;
        //    credit.approvedDate = credit.transactionDate;
        //    credit.approvedDateTime = DateTime.Now;
        //    credit.sourceApplicationId = creditBureauInputs.creditBureauId;
        //    credit.companyId = creditBureauInputs.companyId;
        //    credit.batchCode = transactionCode;
        //    credit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
        //    credit.sourceReferenceNumber = transactionCode;
        //    credit.casaAccountId = casa.CASAACCOUNTID;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = chargeAmount;
        //    credit.sourceBranchId = creditBureauInputs.userBranchId;
        //    credit.destinationBranchId = creditBureauInputs.userBranchId;

        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
        //    inputTransactions.Add(debit);
        //    inputTransactions.Add(credit);
        //    financeTransaction.PostTransaction(inputTransactions);
        //}


        public List<FinanceTransactionViewModel> BuildCreditBureauFeesPosting(CreditBereauViewModel model)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            var searchCharges = context.TBL_CHARGE_FEE.Where(x => x.OPERATIONID == (short)OperationsEnum.CreditBureauSearch);
            if (searchCharges.Any())
            {
                var chargeFeeId = searchCharges.FirstOrDefault().CHARGEFEEID;
                var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

                foreach (var post in postingGroups)
                {
                    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    if (model.debitRequest)
                    {
                        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                        {
                            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                            decimal debitAmount = 0;
                            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                debitAmount = (decimal)model.feeAmount * (decimal)(debits.VALUE / 100.0);
                            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                debitAmount = (decimal)model.feeAmount;
                            var v = model.feeAmount;
                            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
                            debit.description = $"Fee charge on {debits.DESCRIPTION}";
                            debit.valueDate = genSetup.GetApplicationDate();
                            debit.transactionDate = debit.valueDate;
                            debit.currencyId = model.debitBusiness ? model.currencyId : casa.CURRENCYID;
                            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                            debit.isApproved = true;
                            debit.postedBy = model.createdBy;
                            debit.approvedBy = model.createdBy;
                            debit.approvedDate = debit.transactionDate;
                            debit.approvedDateTime = DateTime.Now;
                            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            debit.companyId = model.companyId;

                            debit.glAccountId = model.debitBusiness ? model.glAccountId : context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                            debit.sourceReferenceNumber = model.referenceNumber;
                            debit.batchCode = batchCode;

                            if (!model.debitBusiness) debit.casaAccountId = casa.CASAACCOUNTID;
                            debit.debitAmount = debitAmount;
                            debit.creditAmount = 0;
                            debit.sourceBranchId = model.userBranchId;
                            debit.destinationBranchId = model.debitBusiness ? model.userBranchId : casa.BRANCHID;
                            //debit.rateCode = "TTB";
                            //debit.rateUnit = string.Empty;
                            //debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(debit);
                        }

                        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                        {
                            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                            decimal creditAmount = 0;
                            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                creditAmount = (decimal)model.feeAmount * (decimal)(credits.VALUE / 100.0);
                            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                creditAmount = (decimal)model.feeAmount;


                            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
                            credit.description = $"Fee charge on {credits.DESCRIPTION}";
                            credit.valueDate = genSetup.GetApplicationDate();
                            credit.transactionDate = credit.valueDate;
                            credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID;//(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId); //casa.CURRENCYID;
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                            credit.isApproved = true;
                            credit.postedBy = model.createdBy;
                            credit.approvedBy = model.createdBy;
                            credit.approvedDate = credit.transactionDate;
                            credit.approvedDateTime = DateTime.Now;
                            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            credit.companyId = model.companyId;
                            credit.glAccountId = (int)credits.GLACCOUNTID1;
                            credit.sourceReferenceNumber = model.referenceNumber;
                            credit.batchCode = batchCode;
                            credit.casaAccountId = null;
                            credit.debitAmount = 0;
                            credit.creditAmount = creditAmount;
                            credit.sourceBranchId = model.userBranchId;
                            credit.destinationBranchId = model.userBranchId;
                            //credit.rateCode = "TTB";
                            //credit.rateUnit = string.Empty;
                            //credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(credit);
                        }
                    }
                    else
                    {
                        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                        {
                            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                            decimal creditAmount = 0;
                            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                creditAmount = (decimal)model.feeAmount * (decimal)(debits.VALUE / 100.0);
                            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                creditAmount = (decimal)model.feeAmount;

                            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
                            debit.description = $"Fee charge reversal on {debits.DESCRIPTION}";
                            debit.valueDate = genSetup.GetApplicationDate();
                            debit.transactionDate = debit.valueDate;
                            debit.currencyId = casa.CURRENCYID;
                            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                            debit.isApproved = true;
                            debit.postedBy = model.createdBy;
                            debit.approvedBy = model.createdBy;
                            debit.approvedDate = debit.transactionDate;
                            debit.approvedDateTime = DateTime.Now;
                            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            debit.companyId = model.companyId;

                            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                            debit.sourceReferenceNumber = model.referenceNumber;
                            debit.batchCode = batchCode;
                            debit.casaAccountId = casa.CASAACCOUNTID;
                            debit.debitAmount = 0;
                            debit.creditAmount = creditAmount;
                            debit.sourceBranchId = model.userBranchId;
                            debit.destinationBranchId = casa.BRANCHID;
                            //debit.rateCode = "TTB";
                            //debit.rateUnit = string.Empty;
                            //debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(debit);
                        }

                        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                        {
                            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                            decimal debitAmount = 0;
                            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                debitAmount = (decimal)model.feeAmount * (decimal)(credits.VALUE / 100.0);
                            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                debitAmount = (decimal)model.feeAmount;


                            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
                            credit.description = $"Fee charge reversal on {credits.DESCRIPTION}";
                            credit.valueDate = genSetup.GetApplicationDate();
                            credit.transactionDate = credit.valueDate;
                            credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID; //(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId); //casa.CURRENCYID;
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                            credit.isApproved = true;
                            credit.postedBy = model.createdBy;
                            credit.approvedBy = model.createdBy;
                            credit.approvedDate = credit.transactionDate;
                            credit.approvedDateTime = DateTime.Now;
                            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            credit.companyId = model.companyId;
                            credit.glAccountId = (int)credits.GLACCOUNTID1;
                            credit.sourceReferenceNumber = model.referenceNumber;
                            credit.batchCode = batchCode;
                            credit.casaAccountId = null;
                            credit.debitAmount = debitAmount;
                            credit.creditAmount = 0;
                            credit.sourceBranchId = model.userBranchId;
                            credit.destinationBranchId = model.userBranchId;
                            //credit.rateCode = "TTB";
                            //credit.rateUnit = string.Empty;
                            //credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(credit);
                        }
                    }
                }
            }

            return inputTransactions;
        }


        #endregion
    }
}

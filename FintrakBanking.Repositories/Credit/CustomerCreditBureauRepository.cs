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

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCreditBureauRepository : ICustomerCreditBureauRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IFinanceTransactionRepository financeTransaction;
        private IntegrationWithFinacle integration;
        private CreditBureauProcess _creditBureau;

        public CustomerCreditBureauRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingDocumentsContext _docContext,
            FinTrakBankingContext _context,
            IFinanceTransactionRepository _financials, IntegrationWithFinacle integration,
            CreditBureauProcess creditBureau)
        {
            this.context = _context;
            docContext = _docContext;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            financeTransaction = _financials;
            this.integration = integration;
            _creditBureau = creditBureau;
        }

        #region Credit Bureau 
        public IEnumerable<CustomerViewModels> GetCreditBureauCustomerDetailsByCustomerId(int customerId, bool isExternal)
        {
            var data = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(x => x.CUSTOMERID == customerId && x.DELETED == false
                                                                                            && x.COMPANYDIRECTORID == null
                                                                                            && (DbFunctions.DiffDays(x.DATETIMECREATED, DateTime.Now).Value <= 30));

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
                               maritalStatus = a.MARITALSTATUS.Value,
                               title = a.TITLE,
                               middleName = a.MIDDLENAME,
                               customerTypeName = a.TBL_CUSTOMER_TYPE.NAME,
                               nationality = a.NATIONALITY,
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
                                                                                     && (DbFunctions.DiffDays(x.DATETIMECREATED, DateTime.Now).Value <= 30));
                    int directorCount = directorData.Count();
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
                        customerId = customerId,
                        emailAddress = director.EMAILADDRESS,
                        firstName = director.FIRSTNAME,
                        lastName = director.SURNAME,
                        middleName = director.MIDDLENAME,
                        creditBureauCount = directorCount,
                    };
                    allCorporate.Add(shareholdersData);
                }
            }
            if (isExternal)
            {
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    foreach (var item in customer)
                    {
                        try
                        {
                            integration.AddCustomerAccounts(item.customerCode);
                        }
                        catch(APIErrorException ex)
                        {
                            return allCorporate;
                            //throw new APIErrorException(ex.Message);
                        }
                        catch(Exception ex)
                        {
                            throw ex; // new Exception(ex.Message);
                        }
                    }
                }
            }

            return allCorporate;
        }


        public int AddCustomerCreditBureauCharge(LoanCreditBereauViewModel entity)
        {
            var previousSearch = this.GetCustomerCreditBureauReportLog(entity.customerId, entity.companyDirectorId);
            bool hascrms = false;
            foreach (var i in previousSearch)
            {
                if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
            };
            if (previousSearch.Count() >= 2 && !hascrms && entity.creditBureauId != (short)CreditBureauEnum.CRMS)
                throw new Exception("Only three search options allowed and must inlude CRMS.\n Please check CRMS");

            if (previousSearch.Count() >= 3)
                throw new Exception("You have reached that maximum credit bureau search for this customer");

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
                CREATEDBY = entity.createdBy
            };
            context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);
            if (context.SaveChanges() > 0) return data.CUSTOMERCREDITBUREAUID;
            else return 0;
        }

        public int AddCustomerCreditBureauUpload(LoanCreditBereauViewModel entity, LoanDocumentViewModel docModel, byte[] file)
        {
            var previousSearch = this.GetCustomerCreditBureauReportLog(entity.customerId, entity.companyDirectorId);
            bool hascrms = false;
            foreach (var i in previousSearch)
            {
                if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
            };
            if (previousSearch.Count() >= 2 && !hascrms && entity.creditBureauId != (short)CreditBureauEnum.CRMS)
                throw new Exception("Only three search options allowed and must inlude CRMS.\n Please check CRMS");

            if (previousSearch.Count() >= 3)
                throw new Exception("You have reached that maximum credit bureau search for this customer");

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
                CREATEDBY = entity.createdBy
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditBureauReportDocumentAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Credit Bureau Report Document with title : '{ docModel.documentTitle }' ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

        public bool UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBereauViewModel model)
        {
            var directorId = model.companyDirectorId > 0 ? model.companyDirectorId : null;
            var data = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(c => c.CREDITBUREAUID == model.creditBureauId
                                                                && c.CUSTOMERID == model.customerId
                                                                && c.COMPANYDIRECTORID == directorId).FirstOrDefault();

            if (data != null)
                data.ISREPORTOKAY = status;

            return context.SaveChanges() > 0;
        }

        public bool UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBereauViewModel> model)
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

        public List<LoanCreditBereauViewModel> GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId)
        {
            var directorId = companyDirectorId > 0 ? companyDirectorId : null;
            var customerLoanCreditBureauData = (from a in context.TBL_CUSTOMER_CREDIT_BUREAU
                                                where a.CUSTOMERID == customerId && a.DELETED == false && a.COMPANYDIRECTORID == directorId
                                                 && (DbFunctions.DiffDays(a.DATETIMECREATED, DateTime.Now).Value <= 30)
                                                //&& (DbFunctions.DiffDays(DbFunctions.TruncateTime(a.DATETIMECREATED), DbFunctions.TruncateTime(DateTime.Now)).Value <= 30 ) 
                                                select new LoanCreditBereauViewModel
                                                {
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
                                                    dayAgo = DbFunctions.DiffDays(a.DATETIMECREATED, DateTime.Now).Value
                                                }).ToList();
            return customerLoanCreditBureauData;
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
                    foreach (var cb in creditBureau)
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
        public List<string> GetCustomerXDSCreditMatch(CreditBureauSearchViewModel searchInfoList)
        {

            var dateOfBirth = Convert.ToDateTime(searchInfoList.dateOfBirth);
            searchInfoList.dateOfBirth = dateOfBirth.ToString("dd-MMM-yyyy", null);
            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInfoList.creditBureauId);
            if (creditBureau != null)
            {
                searchInfoList.userName = creditBureau.USERNAME;
                searchInfoList.password = creditBureau.PASSWORD;
            }
            else
                throw new ConditionNotMetException("Could not resolve the selected Credit Bureau entity. Contact admin.");

            List<string> searchResult = new List<string>();
            try
            {
                var task = Task.Run(() => searchResult.Add(_creditBureau.XDSSearchCreditBureau(searchInfoList)));
                if (task.Wait(TimeSpan.FromSeconds(600)))
                {
                    return searchResult;
                }
                else
                {
                    throw new APIErrorException("Credit Bureau search time out");
                }
            }
            catch (ConditionNotMetException ex)
            {
                throw new ConditionNotMetException(ex.Message);
            }
            catch (BadLogicException ex)
            {
                throw new ConditionNotMetException(ex.Message);
            }
            catch (APIErrorException ex)
            {
                throw new ConditionNotMetException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CRCSearchResult GetCustomerCRCCreditMatch(CRCRequestViewModel searchInfo)
        {
            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInfo.creditBureauId);
            searchInfo.userName = creditBureau.USERNAME;
            searchInfo.password = creditBureau.PASSWORD;

            //var dateOfBirth = Convert.ToDateTime(searchInfo.dateOfBirth);
           // searchInfo.dateOfBirth = dateOfBirth.ToString("dd-MMM-yyyy", null);
            //var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == searchInfo.currencyCode);

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
                
            };

            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var casa = context.TBL_CASA.Find(creditBureauInputs.casaAccountId);

            if (casa == null) throw new Exception("Norminated Account Does not Exist");

            //var accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;

            //var chargeAmount = creditBureauInputs.searchType == (short)CreditBureauTypeEnum.ConsumerSearch ? creditBureau.INDIVIDUAL_CHARGEAMOUNT
            //    : creditBureau.CORPORATE_CHARGEAMOUNT;

            //if (chargeAmount > accountBalance)
            //{
            //    //throw new Exception("The norminated customer account has insufficient fund to perform this transaction.");
            //}
            //else
            //{
            //    //DebitCustomer(creditBureau, casa, chargeAmount, creditBureauInputs);
            //}

            CRCSearchResult searchResponse = null;

            using (var docTrans = docContext.Database.BeginTransaction())
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var task = Task.Run(() => searchResponse = _creditBureau.CRCCreditBureauSearch(searchInfo));

                    if (task.Wait(TimeSpan.FromSeconds(640)))
                    {
                        if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchIncomplete) return searchResponse;

                        else if (searchResponse.SearchCompleted == (int)SearchCompletedStatusEnum.SearchCompleted)
                        {
                            //byte file = System.Text.Encoding.ASCII.GetByteCount(searchResult.SearchResult)
                            byte file = Convert.ToByte(searchResponse.SearchResult);
                            byte[] fileArray = new byte[file];
                            var customerCreditBureauId = AddCustomerCreditBureauCharge(creditBureauInputs.customerCreditBureauUploadDetails);
                            if (!SaveCreditBureauReportFile(customerCreditBureauId, fileArray, creditBureauInputs))
                            {
                                throw new ConditionNotMetException("Search could not save the result file");
                            }

                            context.SaveChanges();
                            trans.Commit();
                            docTrans.Commit();
                            return searchResponse;
                        }
                        else
                        {
                            //ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
                            throw new ConditionNotMetException("Search Response -  error occured during search");
                        }
                    }
                    else
                    {
                       // ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
                        throw new ConditionNotMetException("Search result Timed out");
                    }
                }
                catch (Exception ex)
                {
                    //ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
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
                            {   //System.Text.Encoding.ASCII.GetByteCount(searchResult.SearchResult)
                                byte file = Convert.ToByte(searchResponse.SearchResult);

                                byte[] fileArray = new byte[file];

                                var customerCreditBureauId = AddCustomerCreditBureauCharge(creditBureauInputs.customerCreditBureauUploadDetails);

                                if (!SaveCreditBureauReportFile(customerCreditBureauId, fileArray, creditBureauInputs))
                                {
                                    throw new Exception("Could not save file");
                                }

                                context.SaveChanges();
                                trans.Commit();
                                docTrans.Commit();

                                return true;
                            }
                        }
                        else
                        {
                            //ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
                            throw new Exception("An error occured");
                        }
                    }
                    else
                    {
                        //ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
                        throw new Exception("Timed out");
                    }
                }
                catch (Exception ex)
                {
                    //ReverseDebit(creditBureau, casa, chargeAmount, creditBureauInputs);
                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        private void DebitCustomer(TBL_CREDIT_BUREAU creditBureau, TBL_CASA casa, decimal chargeAmount, SearchInput creditBureauInputs)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
            debit.description = creditBureau.CREDITBUREAUNAME + " search charge";
            debit.valueDate = genSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, creditBureauInputs.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = creditBureauInputs.createdBy;
            debit.approvedBy = creditBureauInputs.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = creditBureauInputs.creditBureauId;
            debit.companyId = creditBureauInputs.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL; 
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = creditBureauInputs.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
            credit.description = creditBureau.CREDITBUREAUNAME + " search charge";
            credit.valueDate = genSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, creditBureauInputs.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = creditBureauInputs.createdBy;
            credit.approvedBy = creditBureauInputs.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = creditBureauInputs.creditBureauId;
            credit.companyId = creditBureauInputs.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = creditBureau.GLACCOUNTID;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = creditBureauInputs.userBranchId;
            credit.destinationBranchId = creditBureauInputs.userBranchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }

        private void ReverseDebit(TBL_CREDIT_BUREAU creditBureau, TBL_CASA casa, decimal chargeAmount, SearchInput creditBureauInputs)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
            debit.description = creditBureau.CREDITBUREAUNAME + " search charge reversal";
            debit.valueDate = genSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, creditBureauInputs.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = creditBureauInputs.createdBy;
            debit.approvedBy = creditBureauInputs.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = creditBureauInputs.creditBureauId;
            debit.companyId = creditBureauInputs.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = creditBureau.GLACCOUNTID;
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = null;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = creditBureauInputs.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
            credit.description = creditBureau.CREDITBUREAUNAME + " search charge";
            credit.valueDate = genSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, creditBureauInputs.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = creditBureauInputs.createdBy;
            credit.approvedBy = creditBureauInputs.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = creditBureauInputs.creditBureauId;
            credit.companyId = creditBureauInputs.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = creditBureauInputs.userBranchId;
            credit.destinationBranchId = creditBureauInputs.userBranchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }

        public byte[] GetFullSearchResultInPDF(SearchInput searchInput)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
            var casa = context.TBL_CASA.Find(searchInput.casaAccountId);
            if (casa == null) throw new Exception("Norminated Account Does not Exist");

            var accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;
            var creditBureau = context.TBL_CREDIT_BUREAU.Find(searchInput.creditBureauId);

            searchInput.userName = creditBureau.USERNAME;
            searchInput.password = creditBureau.PASSWORD;

            var chargeAmount = searchInput.searchType == (short)CreditBureauTypeEnum.ConsumerSearch ? creditBureau.INDIVIDUAL_CHARGEAMOUNT
                : creditBureau.CORPORATE_CHARGEAMOUNT;


            if (chargeAmount > accountBalance)
                throw new Exception("The norminated customer account has insufficient fund to perform this transaction.");
            else
            {
                DebitCustomer(creditBureau, casa, chargeAmount, searchInput);
            }

            byte[] binaryData;
            var creditBureauProcess = new CreditBureauProcess();
            try
            {
                binaryData = creditBureauProcess.GetFullSearchResultInPDF(searchInput);
                using (var docTrans = docContext.Database.BeginTransaction())
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var customerCreditBureauId = AddCustomerCreditBureauCharge(searchInput.customerCreditBureauUploadDetails);
                        if (!SaveCreditBureauReportFile(customerCreditBureauId, binaryData, searchInput))
                        {
                            throw new Exception("Could not save file");
                        }
                        context.SaveChanges();
                        trans.Commit();
                        docTrans.Commit();
                        return binaryData;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message.ToString());
                    }
                }
            }
            catch
            {
                ReverseDebit(creditBureau, casa, chargeAmount, searchInput);
                throw new Exception("Download failed. This may have been cause by slow or no internet connection");
            }
        }

        private bool SaveCreditBureauReportFile(int customerCreditBureauId, byte[] file, SearchInput model)
        {
            try
            {
                var creditBureau = context.TBL_CREDIT_BUREAU.Find(model.creditBureauId);
                var data = new Entities.DocumentModels.TBL_CUSTOMER_CREDIT_BUREAU
                {
                    CUSTOMERCREDITBUREAUID = customerCreditBureauId,
                    DOCUMENT_TITLE = creditBureau.CREDITBUREAUNAME + " Report Document Upload",
                    FILEEXTENSION = "pdf",
                    // FILENAME = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == model.customerCreditBureauUploadDetails.customerId).FirstOrDefault().CUSTOMERCODE + creditBureau.CREDITBUREAUNAME,
                    FILEDATA = file,
                    SYSTEMDATETIME = genSetup.GetApplicationDate(),
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = model.createdBy
                };

                docContext.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);

                // Audit Section ---------------------------
                var creditBureauInfo = context.TBL_CREDIT_BUREAU.Find(model.creditBureauId);
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Uploaded '{ creditBureauInfo.CREDITBUREAUNAME }' Credit Bureau Report generated for merge ID list : '{ model.mergeList }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                docContext.SaveChanges();
                return context.SaveChanges() != 0;
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion
    }
}

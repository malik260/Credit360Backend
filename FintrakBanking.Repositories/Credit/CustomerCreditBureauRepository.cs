using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.CreditBureau;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCreditBureauRepository : ICustomerCreditBureauRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;

        public CustomerCreditBureauRepository(
                IAuditTrailRepository _auditTrail,
                IGeneralSetupRepository _genSetup,
                FinTrakBankingDocumentsContext _docContext,
                FinTrakBankingContext _context
            )
        {
            this.context = _context;
            docContext = _docContext;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        #region Credit Bureau 
        public IEnumerable<CustomerViewModels> GetCreditBureauCustomerDetailsByCustomerId(int customerId)
        {
            List<CustomerViewModels> allCorporate = new List<CustomerViewModels>();
            var customerInfo = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customerId);
            var customerType = context.TBL_CUSTOMER.Find(customerId).TBL_CUSTOMER_TYPE.CUSTOMERTYPEID;
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
                               customerId = a.CUSTOMERID,
                               emailAddress = a.EMAILADDRESS,
                               phoneNumber = a.TBL_CUSTOMER_PHONECONTACT.Any() ? a.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault().PHONE : null,
                               firstName = a.FIRSTNAME,
                               gender = a.GENDER,
                               lastName = a.LASTNAME,
                               maidenName = a.MAIDENNAME,
                               maritalStatus = a.MARITALSTATUS.Value,
                               title = a.TITLE,
                               middleName = a.MIDDLENAME,
                               customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                               customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                               nationality = a.NATIONALITY,
                               occupation = a.OCCUPATION,
                               placeOfBirth = a.PLACEOFBIRTH,
                               isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                               sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                               sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                               subSectorId = (short)a.SUBSECTORID,
                               subSectorName = a.TBL_SUB_SECTOR.NAME,
                               taxNumber = a.TAXNUMBER,
                               riskRatingId = a.RISKRATINGID,
                               riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                               customerBVN = a.CUSTOMERBVN,
                               rcNumber = customerInfo.Any() ? customerInfo.FirstOrDefault().REGISTRATIONNUMBER : null,
                               isCreditBureauUploadCompleted = false,
                               companyDirectorId = null,
                               creditBureauCount = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(x => x.CUSTOMERID == a.CUSTOMERID && x.DELETED == false
                                                                                            && x.COMPANYDIRECTORID == null).Count(),
                   };

            foreach (var item in customer)
            {
                allCorporate.Add(item);
            }

            if (customerType == (short)CustomerTypeEnum.Corporate)
            {
                var shareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == customerId && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember).ToList();
                foreach (var director in shareholders)
                {
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
                        creditBureauCount = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(x => x.CUSTOMERID == x.CUSTOMERID && x.DELETED == false
                                                                                    && x.COMPANYDIRECTORID == director.COMPANYDIRECTORID).Count()
                    };
                    allCorporate.Add(shareholdersData);
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

        public List<LoanCreditBereauViewModel> GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId)
        {
            var directorId = companyDirectorId > 0 ? companyDirectorId : null;
            var customerLoanCreditBureauData = from a in context.TBL_CUSTOMER_CREDIT_BUREAU
                                               where a.CUSTOMERID == customerId && a.DELETED == false
                                              && a.COMPANYDIRECTORID == directorId
                                               //where a.DATETIMECREATED.Day <= ((DateTime.Now - a.DATETIMECREATED).TotalDays - 30)
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
                                                   createdBy = a.CREATEDBY
                                               };
            return customerLoanCreditBureauData.ToList();
        }
        #endregion

        #region Integration 
        public List<string> GetCustomerCreditMatch(CreditBureauSearchViewModel searchInfoList)
        {
            var creditBureau = new CreditBureauProcess();
            List<string> searchResult = new List<string>();


            var task = Task.Run(() => searchResult.Add(creditBureau.XDSSearchCreditBureau(searchInfoList)));
            if (task.Wait(TimeSpan.FromSeconds(40)))
                return searchResult;
            else
                throw new Exception("Timed out");

            
            
        }

        public byte[] GetFullSearchResultInPDF(SearchInput searchInput)
        {
            var creditBureau = new CreditBureauProcess();
            var binaryData = creditBureau.GetFullSearchResultInPDF(searchInput);

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var customerCreditBureauId = AddCustomerCreditBureauCharge(searchInput.customerCreditBureauUploadDetails);
                    if (!saveCreditBureauReportFile(customerCreditBureauId, binaryData, searchInput))
                    {
                        throw new Exception("Could not save file");
                    }
                    trans.Commit();
                    return binaryData;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message.ToString());
                }
            }
            
        }
        
        private bool saveCreditBureauReportFile(int customerCreditBureauId, byte[] file, SearchInput model)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_CUSTOMER_CREDIT_BUREAU
                {
                    CUSTOMERCREDITBUREAUID = customerCreditBureauId,
                    DOCUMENT_TITLE = "",
                    FILEEXTENSION = "pdf",
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

                return context.SaveChanges() != 0;
            }
            catch (Exception ex) { throw ex; }
                        
  
        }
        #endregion
    }
}

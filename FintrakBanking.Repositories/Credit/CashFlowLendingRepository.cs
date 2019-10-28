using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CashFlowLendingRepository :ICashFlowLendingRepository
    {
        private FinTrakBankingContext context;
        private ICustomerRepository customer;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private CustomerDetails customerRequest;
        private ICreditLimitValidationsRepository limitValidation;
        private ICasaRepository casa;


        public CashFlowLendingRepository(FinTrakBankingContext _context, 
                                        ICustomerRepository _customer, 
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                         CustomerDetails _customerRequest,
                                         ICreditLimitValidationsRepository _limitValidation,
                                         ICasaRepository _casa
                                         )
        {
            this.context = _context;
            this.customer = _customer;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.customerRequest = _customerRequest;
            limitValidation = _limitValidation;
            casa = _casa;
        }

        public APIResponse AddCustomer(IncomingCustomerViewModels model)
        {
           APIResponse response = new APIResponse();
           if (model.customerType == "I")
           {
               if (model.individualCustomerInformation.customerCode == string.Empty) { return fireResponse("Missing Customer Number", "99"); }

               return AddIndividualCustomer(model);
           }
           else if (model.customerType == "C")
           {
               if (model.corporateCustomerInformation.customerCode == string.Empty) { return fireResponse("Missing Customer Number", "99"); }

               return AddCorporateCustomer(model);
           }
           else { return fireResponse("Uknown Customer Type","99"); }
        }

        private APIResponse AddIndividualCustomer(IncomingCustomerViewModels model)
        {
            APIResponse response = new APIResponse();

            if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99"); }

            if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99"); }

            if (model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99"); }

            List<string> creditBureautype = model.creditBureauReport.Select(x => x.creditBureauType).ToList();
            if (creditBureautype.Contains(CreditBureauEnum.CRCCreditBureau.ToString()) == false) { return fireResponse("Missing CRC credit bureau", "99"); }

            DateTime dateTime12;
            if (!DateTime.TryParse(model.individualCustomerInformation.dateOfBirth, out dateTime12)) { return fireResponse("Date of birth not in the right format", "99"); }

            List<string> mStatus = new List<string> { "single", "married", "divorced", "widowed" };
            if (model.individualCustomerInformation.maritalStatus.ToLower() == "single") { model.individualCustomerInformation.maritalStatus = "1"; }
            if (model.individualCustomerInformation.maritalStatus.ToLower() == "married") { model.individualCustomerInformation.maritalStatus = "2"; }
            if (model.individualCustomerInformation.maritalStatus.ToLower() == "divorced") { model.individualCustomerInformation.maritalStatus = "3"; }
            if (model.individualCustomerInformation.maritalStatus.ToLower() == "widowed") { model.individualCustomerInformation.maritalStatus = "4"; }
            if (model.individualCustomerInformation.maritalStatus == "0") { fireResponse("Zero value is not a recognized marital status.","99"); }
            if (!mStatus.Contains(model.individualCustomerInformation.maritalStatus)) { fireResponse($"Value, '{model.individualCustomerInformation.maritalStatus}' is not a valid marital status.", "99"); }

            if (saveIndividualCustomerInformation(model))
            {
                fireResponse("Success","00");
            }
            else { fireResponse("Unresolved error: could not save customer information","99"); }

            return response;
        }

        private APIResponse AddCorporateCustomer(IncomingCustomerViewModels model)
        {
            APIResponse response = new APIResponse();

            if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99"); }

            if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99"); }

            if (model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99"); }

            List<string> creditBureautype = model.creditBureauReport.Select(x => x.creditBureauType).ToList();
            if (creditBureautype.Contains(CreditBureauEnum.CRCCreditBureau.ToString()) == false)
                                    { return fireResponse("Missing CRC credit bureau","99"); }

            if (saveCorporateCustomerInformation(model))
            {
                fireResponse("Success", "00");
            }
            else { fireResponse("Unresolved error: could not save customer information", "99"); }

            return response;
        }

        private bool saveIndividualCustomerInformation(IncomingCustomerViewModels entity)
        {
            var model = entity.individualCustomerInformation;
            

            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
                BRANCHID = 1, //entity.userBranchId,
                COMPANYID = 1, //entity.companyId,
                CREATEDBY = 1, //(int)entity.createdBy,
                CREATIONMAILSENT = true, //entity.creationMailSent,
                CUSTOMERCODE = model.customerCode,
                CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
                CUSTOMERTYPEID = (short)CustomerTypeEnum.Individual,
                DATEOFBIRTH = Convert.ToDateTime(model.dateOfBirth),
                DATETIMECREATED = DateTime.Now,
                EMAILADDRESS = model.emailAddress,
                FIRSTNAME = model.firstName,
                GENDER = model.gender,
                LASTNAME = model.lastName,
                //MAIDENNAME = model.maidenName,
                MARITALSTATUS = Convert.ToInt16(model.maritalStatus),
                TITLE = model.title,
                MIDDLENAME = model.middleName,
                //MISCODE = model.misCode,
                //MISSTAFF = model.misStaff,
                NATIONALITYID = context.TBL_COUNTRY.Where(X => X.NAME == model.countryOfOrigin).FirstOrDefault()?.COUNTRYID,
                OCCUPATION = model.occupation,
                PLACEOFBIRTH = model.PlaceOfBirth,
                ISPOLITICALLYEXPOSED = model.politicallyExposed == "1" ? true : false,
                //ISINVESTMENTGRADE = model,
                ISREALATEDPARTY = entity.insiderRelatedParties.Count > 0 ?  true : false,
                ///RELATIONSHIPOFFICERID = model.relationshipManagerCode,
                SPOUSE = model.spouse,
                //SUBSECTORID = model.,
                //RISKRATINGID = model.riskRatingId,
                CUSTOMERBVN = model.customerBvn,
                //PROSPECTCUSTOMERCODE = model.prospectCustomerCode,
                ISPROSPECT = false,
                CRMSCOMPANYSIZEID = Convert.ToInt32(model.crmsCompanySize),
                //CRMSLEGALSTATUSID = model.crmsLegalStatus,
                CRMSRELATIONSHIPTYPEID = context.TBL_CRMS_REGULATORY.Where(x=>x.CODE == (model.crmsRelationship)).FirstOrDefault()?.CRMSREGULATORYID,
                COUNTRYOFRESIDENTID = context.TBL_COUNTRY.Where(X => X.NAME == model.countryOfResidence).FirstOrDefault()?.COUNTRYID ,
                NUMBEROFDEPENDENTS = model.children.Count(),
                //NUMBEROFLOANSTAKEN = model.numberOfLoansTaken,
                //MONTHLYLOANREPAYMENT = model.loanMonthlyRepaymentFromOtherBanks,
                //DATEOFRELATIONSHIPWITHBANK = model.dateOfRelationshipWithBank,
                //RELATIONSHIPTYPEID = model.relationshipTypeCode,
                TEAMLDR = model.teamLdr,
                TEAMNPL = model.teamNpl,
                //CORR = model.corr,
                PASTDUEOBLIGATIONS = Convert.ToDecimal(model.pastDueObligation),
                APIREQUESTID = entity.requestId,
                //BUSINESSUNTID = model.businessUnitId

            };

            context.TBL_CUSTOMER.Add(customer);


            return context.SaveChanges() > 0;
        }


        private bool saveCorporateCustomerInformation(IncomingCustomerViewModels model)
        {
            ApiCustomerBusinessDetailsViewModel corporateDetails = model.corporateCustomerInformation;


            return false; var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
                BRANCHID = 1, //entity.userBranchId,
                COMPANYID = 1, //entity.companyId,
                CREATEDBY = 1, //(int)entity.createdBy,
                CREATIONMAILSENT = true, //entity.creationMailSent,
                CUSTOMERCODE = corporateDetails.customerCode,
                CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
                CUSTOMERTYPEID = (short)CustomerTypeEnum.Corporate,
                DATEOFBIRTH = Convert.ToDateTime(corporateDetails.dateOfIncorporation),
                DATETIMECREATED = DateTime.Now,
                EMAILADDRESS = corporateDetails.emailAddress,
                FIRSTNAME = corporateDetails.corporateName,
                //MISCODE = model.misCode,
                //MISSTAFF = model.misStaff,
                //NATIONALITYID = context.TBL_COUNTRY.Where(X => X.NAME == corporateDetails.countryOfOrigin).FirstOrDefault()?.COUNTRYID,

                ISPOLITICALLYEXPOSED = corporateDetails.politicallyExposed == "1" ? true : false,
                //ISINVESTMENTGRADE = model,

                //SUBSECTORID = model.,
                //RISKRATINGID = model.riskRatingId,
                //CUSTOMERBVN = corporateDetails.customerBvn,
                //PROSPECTCUSTOMERCODE = model.prospectCustomerCode,
                ISPROSPECT = false,
                CRMSCOMPANYSIZEID = Convert.ToInt32(corporateDetails.crmsCompanySize),
                //CRMSLEGALSTATUSID = model.crmsLegalStatus,
                CRMSRELATIONSHIPTYPEID = context.TBL_CRMS_REGULATORY.Where(x => x.CODE == (corporateDetails.crmsRelationship)).FirstOrDefault()?.CRMSREGULATORYID,
               // COUNTRYOFRESIDENTID = context.TBL_COUNTRY.Where(X => X.NAME == corporateDetails.countryOfResidence).FirstOrDefault()?.COUNTRYID,

                //NUMBEROFLOANSTAKEN = model.numberOfLoansTaken,
                //MONTHLYLOANREPAYMENT = model.loanMonthlyRepaymentFromOtherBanks,
                //DATEOFRELATIONSHIPWITHBANK = model.dateOfRelationshipWithBank,
                //RELATIONSHIPTYPEID = model.relationshipTypeCode,
                TEAMLDR = corporateDetails.teamLdr,
                TEAMNPL = corporateDetails.teamNpl,
                APIREQUESTID = model.requestId,
                //CORR = model.corr,
                //PASTDUEOBLIGATIONS = Convert.ToDecimal(corporateDetails.pastDueObligation),
                //BUSINESSUNTID = model.businessUnitId

            };

            context.TBL_CUSTOMER.Add(customer);


            return context.SaveChanges() > 0;
        }

        
        //public string AddCustomer(IncomingCustomerViewModels model)
        //{
        //    validateCustomerDetails(model);
        //    var entity = getCustomerViewModel(model);

        //    int? maritalStatus = null;
        //    if (entity.customerTypeId == (short)CustomerTypeEnum.Individual)
        //    {
        //        maritalStatus = Convert.ToInt32(entity.maritalStatus);
        //    }
        //    var customer = new TBL_CUSTOMER
        //    {
        //        ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
        //        BRANCHID = 1, //entity.userBranchId,
        //        COMPANYID = 1, //entity.companyId,
        //        CREATEDBY = 1, //(int)entity.createdBy,
        //        CREATIONMAILSENT = false, //entity.creationMailSent,
        //        CUSTOMERCODE = model.customerNumber,
        //        CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
        //        CUSTOMERTYPEID = entity.customerTypeId,
        //        DATEOFBIRTH = entity.dateOfBirth,
        //        DATETIMECREATED = DateTime.Now,
        //        EMAILADDRESS = entity.emailAddress,
        //        FIRSTNAME = entity.firstName,
        //        GENDER = entity.gender,
        //        LASTNAME = entity.lastName,
        //        MAIDENNAME = entity.maidenName,
        //        MARITALSTATUS = maritalStatus,
        //        TITLE = entity.title,
        //        MIDDLENAME = entity.middleName,
        //        MISCODE = entity.misCode,
        //        MISSTAFF = entity.misStaff,
        //        NATIONALITYID = entity.nationalityId,
        //        OCCUPATION = entity.occupation,
        //        PLACEOFBIRTH = entity.placeOfBirth,
        //        ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed,
        //        ISINVESTMENTGRADE = entity.isInvestmentGrade,
        //        ISREALATEDPARTY = entity.isRealatedParty,
        //        RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
        //        SPOUSE = entity.spouse,
        //        SUBSECTORID = entity.subSectorId,
        //        TAXNUMBER = entity.taxNumber,
        //        RISKRATINGID = entity.riskRatingId,
        //        CUSTOMERBVN = entity.customerBVN,
        //        PROSPECTCUSTOMERCODE = entity.prospectCustomerCode,
        //        ISPROSPECT = entity.isProspect,
        //        CRMSCOMPANYSIZEID = entity.crmsCompanySizeId,
        //        CRMSLEGALSTATUSID = entity.crmsLegalStatusId,
        //        CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId,
        //        COUNTRYOFRESIDENTID = entity.countryOfResidentId,
        //        NUMBEROFDEPENDENTS = entity.numberOfDependents,
        //        NUMBEROFLOANSTAKEN = entity.numberOfLoansTaken,
        //        MONTHLYLOANREPAYMENT = entity.loanMonthlyRepaymentFromOtherBanks,
        //        DATEOFRELATIONSHIPWITHBANK = entity.dateOfRelationshipWithBank,
        //        RELATIONSHIPTYPEID = entity.relationshipTypeId,
        //        TEAMLDR = entity.teamLDP,
        //        TEAMNPL = entity.teamNPL,
        //        CORR = entity.corr,
        //        PASTDUEOBLIGATIONS = entity.pastDueObligations,
        //        BUSINESSUNTID = entity.businessUnitId
        //    };
        //    context.TBL_CUSTOMER.Add(customer);

        //    //var audit = new TBL_AUDIT
        //    //{
        //    //    AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
        //    //    STAFFID = entity.createdBy,
        //    //    BRANCHID = (short)entity.userBranchId,
        //    //    DETAIL = $"Added Customer  '{entity.customerName}' with Code: {entity.customerCode}",
        //    //    IPADDRESS = entity.userIPAddress,
        //    //    URL = entity.applicationUrl,
        //    //    APPLICATIONDATE = genSetup.GetApplicationDate(),
        //    //    SYSTEMDATETIME = DateTime.Now
        //    //};

        //    //auditTrail.AddAuditTrail(audit);

        //    try
        //    {
        //        var output = context.SaveChanges() > 0;
        //        var result = entity.isProspect == true ? entity.prospectCustomerCode : entity.customerCode;
        //        if (output == true)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        string errorMessages = string.Join("; ",
        //            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
        //        throw new DbEntityValidationException(errorMessages);
        //    }
        //}

        public CustomerViewModels getCustomerViewModel(ApiIndividualClientViewModel entity)
        {
            CustomerViewModels model = new CustomerViewModels();
           // model.accountCreationComplete = "";
            //model.userBranchId = "";
            //model.companyId = "";
            //model.createdBy = "";
            //model.creationMailSent = "";
           // model.customerCode = entity.customerNumber;
           // //model.customerSensitivityLevelId = "";
           // model.dateOfBirth = Convert.ToDateTime(entity.dateOfBirth).Date;
           // model.emailAddress = entity.emailAddress;
           // model.firstName = entity.firstName;
           // model.gender = entity.gender;
           // model.lastName = entity.lastName;
           // model.maidenName = entity.maidenName;
           // model.maritalStatus = entity.maritalStatus;
           // model.title = entity.title;
           // model.middleName = entity.middleName;
           // model.misCode = entity.misCode;
           // model.misStaff = entity.misStaff;
           //// model.nationalityId = entity.nationality;
           // model.occupation = entity.occupation;
           // model.placeOfBirth = entity.placeOfBirth;
           // model.isPoliticallyExposed = Convert.ToBoolean(entity.isPoliticallyExposed);
           // model.isInvestmentGrade = Convert.ToBoolean(entity.isInvestmentGrade);
           // model.isRealatedParty = Convert.ToBoolean(entity.isRealatedParty); 
           //// model.relationshipOfficerId = ;
           // model.spouse = entity.spouse;
           // model.subSectorId = Convert.ToInt16(entity.subSectorId);
           // model.taxNumber = entity.taxNumber;
           // model.riskRatingId = Convert.ToInt16(entity.riskRatingId);
           // model.customerBVN = entity.customerBVN;
           // model.relationshipTypeId = Convert.ToInt16(entity.relationshipTypeId);


            return model;
        }

        private APIResponse fireResponse(string message, string statusCode)
        {
            APIResponse response = new APIResponse();
            response.StatusCode = statusCode;
            response.Message = message;

            return response;
        }
        public bool ValidateCustomerCode(string customerCode)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER where a.CUSTOMERCODE == customerCode select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public APIResponse submitRequest(CflLoanApplication model)
        {
            APIResponse response = new APIResponse();
            var product = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == model.productCode).FirstOrDefault();

            if (product == null) { return fireResponse("Product Code does not exist", "99"); }
            if (model.requestId == null) { return fireResponse("Missing application unique indentifier", "99"); }

            var subSector = context.TBL_SUB_SECTOR.Where(x => x.CODE == model.subSectorCode).FirstOrDefault();
            if (subSector == null) { return fireResponse("Missing sub sector code", "99"); }

            var sector = context.TBL_SECTOR.Where(x => x.CODE == subSector.CODE).FirstOrDefault();

            var currency = context.TBL_CURRENCY.Where(x => x.CURRENCYCODE == model.currencyCode || x.CURRENCYCODE =="NGN").FirstOrDefault();
            if (currency == null) fireResponse("Missing currency code", "99");

            var accountOfficerr = context.TBL_STAFF.Where(x => x.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();

            LoanApplicationViewModel loanApp = new LoanApplicationViewModel();

            loanApp.proposedTenor = Convert.ToInt16(model.tenor);
            loanApp.tenorModeId = (short)TenorModeEnum.Days;
            loanApp.proposedAmount = Convert.ToInt16(model.loanAmount);
            loanApp.productId = product.PRODUCTID;
            loanApp.productClassId = product.PRODUCTCLASSID;
            loanApp.productClassProcessId = product.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID;
            loanApp.loanPurpose = model.purpose;
            loanApp.sectorId = (int) sector?.SECTORID;
            loanApp.subSectorId = (short)subSector?.SUBSECTORID;
            loanApp.exchangeRate = Convert.ToDouble(model.exchangeRate);
            loanApp.currencyCode = currency.CURRENCYCODE;
            loanApp.interestRate = Convert.ToDouble(model.interestRate);
            loanApp.editMode = model.callStatusCode == "01" ?  true : false;
            loanApp.relationshipOfficerId = accountOfficerr.STAFFID;
            //loanApp.casaAccountId = model.settlementAccount;

            response.applicationReferenceNumber = AddLoanApplication(loanApp,model.requestId);
            response.StatusCode = "00";
            response.Message = "Success";
            if (response.applicationReferenceNumber == null)
            {
                response.StatusCode = "99";
                response.Message = "failed!";
            }
            return response;
        }

        public string AddLoanApplication(LoanApplicationViewModel loan, string apiRequestId)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{

            ValidateLoanApplicationLimits(loan);
            var additionalAmount = loan.LoanApplicationDetail.Sum(x => x.exchangeAmount);
            var savedDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId && c.DELETED == false).ToList();

            decimal cumulativeSum = 0;
            foreach (var s in savedDetails) { cumulativeSum = cumulativeSum + (s.PROPOSEDAMOUNT * (decimal)s.EXCHANGERATE); }

            if (loan.relationshipOfficerId != 0)
            {
                var validation = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId);
                if (validation.maximumAllowedLimit > 0) if ((cumulativeSum + additionalAmount) > (decimal)validation.limit) fireResponse($"RM Limit Exceeded. The limit of this RM is {validation.limit}","99");
            }

            loan.applicationAmount = cumulativeSum + additionalAmount;

            if (loan.editMode == true )
            {
               return UpdateLoanApplicationDetail(loan);
            }

            var loanData = context.TBL_LOAN_APPLICATION.FirstOrDefault(l => l.APPLICATIONREFERENCENUMBER == loan.applicationReferenceNumber && l.DELETED == false);


            if (savedDetails.Count() == 0 || loan.isNewApplication)
            {
                if (loanData != null)
                {
                    loanData.APPLICATIONAMOUNT = loan.applicationAmount;
                    loanData.TOTALEXPOSUREAMOUNT = cumulativeSum + additionalAmount + GetCustomerTotalOutstandingBalance((int)loan.customerId);
                    loanData.ISADHOCAPPLICATION = loan.isadhocapplication;
                    loanData.LOANAPPROVEDLIMITID = loan.loanApprovedLimitId;

                }

                if (loanData == null) 
                {
                    if (string.IsNullOrEmpty(loan.applicationReferenceNumber)) loan.applicationReferenceNumber = GetRefrenceNumber();
                    AddloanApplicationSub(loan, apiRequestId);
                }

                if (loan.LoanApplicationDetail.Count > 0)
                {

                    if (AddLoanApplicationDetail(loan)) return loan.applicationReferenceNumber;

                }
                else return null;
            }
            else
            {
                var limit = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId).limit;
                if ((limit != 0 && loan.applicationAmount != 0 && loan.applicationAmount > (decimal)limit)) throw new SecureException($"RM Limit Exceeded. The limit of this RM is {limit}");
                UpdateLoanApplication(loan);
            }


            return null;



        }

        private bool AddLoanApplicationDetail(LoanApplicationViewModel loan)//List<LoanApplicationDetailViewModel> entity, int createdBy)
        {
            var createdBy = loan.createdBy;
            //foreach (var a in entity)
            //{
            var a = loan.LoanApplicationDetail.FirstOrDefault();

            if (a.repaymentScheduleId <= 0)
            {
                fireResponse("Please select a repayment pattern","99");
            }

            if (a.proposedTenor == 0)
            {
                fireResponse("Tenor can not be ZERO (0)","99");
            }

            var data = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDAMOUNT = a.proposedAmount,
                APPROVEDINTERESTRATE = (double)a.proposedInterestRate,
                APPROVEDPRODUCTID = a.proposedProductId,
                APPROVEDTENOR = loan.proposedTenor,

                EXCHANGERATE = a.exchangeRate,
                CURRENCYID = a.currencyId,
                CUSTOMERID = a.customerId,
                LOANAPPLICATIONID = loan.loanApplicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,

                EQUITYCASAACCOUNTID = a.equityCasaAccountId,
                EQUITYAMOUNT = a.equityAmount,

                PROPOSEDAMOUNT = a.proposedAmount,
                PROPOSEDINTERESTRATE = (int)a.proposedInterestRate,
                PROPOSEDPRODUCTID = a.proposedProductId,
                PROPOSEDTENOR = loan.proposedTenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),
                DELETED = false,
                SUBSECTORID = a.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = a.loanPurpose,
                CASAACCOUNTID = a.casaAccountId,
                OPERATINGCASAACCOUNTID = a.operatingCasaAccountId,
                REPAYMENTSCHEDULEID = a.repaymentScheduleId,
                REPAYMENTTERMS = a.repaymentTerm,
                CRMSFUNDINGSOURCEID = a.crmsFundingSourceId,
                CRMSREPAYMENTSOURCEID = a.crmsPaymentSourceId,
                CRMSFUNDINGSOURCECATEGORY = a.crmsFundingSourceCategory,
                CRMS_ECCI_NUMBER = a.crms_ECCI_Number,
                FIELD1 = a.fieldOne,
                FIELD2 = a.fieldTwo,
                FIELD3 = a.fieldThree,
                PRODUCTPRICEINDEXID = a.productPriceIndexId,
                PRODUCTPRICEINDEXRATE = a.productPriceIndexRate,
                TENORFREQUENCYTYPEID = a.tenorModeId,
                CRMSVALIDATED = false,
                ISTAKEOVERAPPLICATION = a.isTakeOverApplication,
                //LOANAPPLICATIONDETAILID = a.loanApplicationDetailId
            };

            var loanExist = context.TBL_LOAN_APPLICATION_DETAIL.Any(o => o.APPROVEDAMOUNT == data.APPROVEDAMOUNT
                    && o.APPROVEDINTERESTRATE == data.APPROVEDINTERESTRATE && o.APPROVEDTENOR == data.APPROVEDTENOR && o.CURRENCYID == data.CURRENCYID && o.CUSTOMERID == data.CUSTOMERID
                    && o.SUBSECTORID == data.SUBSECTORID && o.CREATEDBY == data.CREATEDBY && o.DELETED != true);

            if (loanExist == true) throw new SecureException("This loan application has already been saved!");

            var appl = context.TBL_LOAN_APPLICATION_DETAIL.Add(data);

            if (a.productFees.Count > 0)
            {
                ProductFees(a.productFees, a.loanApplicationDetailId, createdBy);
            }

           return context.SaveChanges() > 0;

        }

        private void AddloanApplicationSub(LoanApplicationViewModel loan, string apiRequestId)
        {
            short productClassProcessId = 0;
            short? productClassId = null;
            var isGroupLoan = false;
            var response = 0;
            int loanId = 0;
            var proposedProductId = loan.LoanApplicationDetail.FirstOrDefault().proposedProductId;

            // ValidateLoanApplicationLimits(loan); // init only
            var workflowProductId = GetWorkflowProductId(proposedProductId);

            if (loan.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                isGroupLoan = true;
            }

            int? casaAccountId = null;
            string refNumber = GenerateLoanReference(loan.customerId.Value);
            if (loan.customerAccount != "N/A")
            {
                casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);
            }

            var dat = context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == loan.productClassId).FirstOrDefault();
            if (dat != null)
            {
                productClassId = loan.productClassId;
                productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;

            }
            decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + (loan.LoanApplicationDetail.Sum(x => x.exchangeAmount));
            var loanStatusId = (short)LoanStatusEnum.Inactive;

            if (loan.flowchangeId != null && loan.flowchangeId > 0)
            {
                var newWorkflowBaseRecord = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(loan.flowchangeId);
                if (newWorkflowBaseRecord != null)
                {
                    loan.exclusiveOperationId = newWorkflowBaseRecord.OPERATIONID;
                }
            }

            var loanData = new TBL_LOAN_APPLICATION
            {
                REQUIRECOLLATERAL = loan.requireCollateral,
                TOTALEXPOSUREAMOUNT = totalAmount,
                PRODUCTCLASSID = productClassId,
                APPLICATIONREFERENCENUMBER = loan.applicationReferenceNumber,
                PRODUCT_CLASS_PROCESSID = productClassProcessId,
                COMPANYID = loan.companyId,
                BRANCHID = (short)loan.branchId,
                RELATIONSHIPOFFICERID = loan.createdBy,
                RELATIONSHIPMANAGERID = loan.createdBy,
                MISCODE = loan.misCode,
                TEAMMISCODE = loan.teamMisCode,
                INTERESTRATE = loan.interestRate,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                LOANINFORMATION = loan.loanInformation,
                ISRELATEDPARTY = loan.isRelatedParty,
                ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed,
                CREATEDBY = (int)loan.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                CUSTOMERGROUPID = loan.customerGroupId,
                CASAACCOUNTID = loan.casaAccountId,
                APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONAMOUNT = loan.applicationAmount,
                APPLICATIONTENOR = loan.proposedTenor,
                ISINVESTMENTGRADE = loan.isInvestmentGrade,
                CAPREGIONID = loan.regionId,
                REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                LOANTERMSHEETID = loan.loanTermSheetId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                FLOWCHANGEID = loan.flowchangeId,
                OPERATIONID = ((loan.exclusiveOperationId == null) || (loan.exclusiveOperationId == 0)) ? (int)OperationsEnum.CreditAppraisal : (int)loan.exclusiveOperationId,
                LOANAPPLICATIONTYPEID = loan.loanTypeId,
                COLLATERALDETAIL = loan.collateralDetail,
                ISADHOCAPPLICATION = loan.isadhocapplication,
                LOANSWITHOTHERS = loan.loansWithOthers,
                OWNERSHIPSTRUCTURE = loan.ownershipStructure,
                LOANAPPROVEDLIMITID = loan.loanApprovedLimitId,
                PRODUCTID = workflowProductId,
                APIREQUESTID = apiRequestId

            };

            if (isGroupLoan)
            {
                loanData.CUSTOMERGROUPID = loan.customerGroupId;
                loanData.CUSTOMERID = null;
                loanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerGroupId);
                loanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerGroupId);
            }
            else
            {
                loanData.CUSTOMERID = loan.customerId;
                loanData.CUSTOMERGROUPID = null;
                loanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerId);
                loanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerId);
            }

            if (loan.loanPreliminaryEvaluationId != null && loan.loanPreliminaryEvaluationId != 0)
            {
                var pen = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loan.loanPreliminaryEvaluationId);
                pen.SENTFORLOANAPPLICATION = true;
            }


            context.TBL_LOAN_APPLICATION.Add(loanData);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = loan.createdBy,
                BRANCHID = (short)loan.userBranchId,
                DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = loan.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loan.loanApplicationId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);
        }

        private string UpdateLoanApplicationDetail(LoanApplicationViewModel loan)
        {
            UpdateLoanApplication(loan); // update main

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
            var update = loan.LoanApplicationDetail.SingleOrDefault();
            if (update == null) fireResponse("Sequence contain not single! " + loan.LoanApplicationDetail.Count(), "99");

            if (update.repaymentScheduleId <= 0 && (detail.TBL_PRODUCT1.PRODUCTCLASSID != (int)ProductClassEnum.BondAndGuarantees))
            {
                fireResponse("Please select a repayment pattern for the product " + update.productName, "99");
            }

            // LEFT TO RIGHT MAPPING
            detail.SUBSECTORID = update.subSectorId;
            detail.PROPOSEDAMOUNT = update.proposedAmount;
            detail.PROPOSEDINTERESTRATE = (double)update.proposedInterestRate;
            detail.PROPOSEDPRODUCTID = update.proposedProductId;
            detail.PROPOSEDTENOR = update.proposedTenor;
            detail.REPAYMENTSCHEDULEID = update.repaymentScheduleId;
            detail.REPAYMENTTERMS = update.repaymentTerm;
            detail.LOANPURPOSE = update.loanPurpose;
            detail.PRODUCTPRICEINDEXID = update.productPriceIndexId;
            detail.PRODUCTPRICEINDEXRATE = update.productPriceIndexRate;
            detail.CASAACCOUNTID = update.casaAccountId;
            detail.OPERATINGCASAACCOUNTID = update.operatingCasaAccountId;
            detail.EQUITYCASAACCOUNTID = update.equityCasaAccountId;
            detail.CURRENCYID = update.currencyId;
            detail.TENORFREQUENCYTYPEID = update.tenorModeId;
            detail.ISTAKEOVERAPPLICATION = update.isTakeOverApplication;

            var productClassId = detail.TBL_PRODUCT1.PRODUCTCLASSID;

            if (context.SaveChanges() == 0) return detail.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
            else return null;

        }

        private void UpdateLoanApplication(LoanApplicationViewModel loan)
        {
            var application = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loan.loanApplicationId).ToList();

            decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + application.Sum(a => a.PROPOSEDAMOUNT);

            decimal totalApplicationAmount = 0; //loan.applicationAmount;
            foreach (var item in application)
            {
                var exchangeValue = ((decimal)item.PROPOSEDAMOUNT * (decimal)item.EXCHANGERATE);
                totalApplicationAmount = totalApplicationAmount + exchangeValue;
            }

            var loanData = context.TBL_LOAN_APPLICATION.Find(loan.loanApplicationId);
            loanData.REQUIRECOLLATERAL = loan.requireCollateral;
            loanData.TOTALEXPOSUREAMOUNT = totalAmount;
            loanData.INTERESTRATE = loan.interestRate;
            loanData.APPLICATIONDATE = genSetup.GetApplicationDate();
            loanData.LOANINFORMATION = loan.loanInformation;
            loanData.ISRELATEDPARTY = loan.isRelatedParty;
            loanData.ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed;
            loanData.CREATEDBY = (int)loan.createdBy;
            loanData.DATETIMECREATED = genSetup.GetApplicationDate();
            loanData.SYSTEMDATETIME = DateTime.Now;
            loanData.CASAACCOUNTID = loan.casaAccountId;
            loanData.APPLICATIONAMOUNT = totalApplicationAmount;
            loanData.APPLICATIONTENOR = application.Max(c => c.PROPOSEDTENOR);
            loanData.COLLATERALDETAIL = loan.collateralDetail;
            loanData.CAPREGIONID = loan.regionId;
            loanData.REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId;
            loanData.LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId;
            loanData.LOANTERMSHEETID = loan.loanTermSheetId;
            loanData.ISADHOCAPPLICATION = loan.isadhocapplication;
            loanData.LOANAPPROVEDLIMITID = loan.loanApprovedLimitId;
            loanData.LOANSWITHOTHERS = loan.loansWithOthers;
            loanData.OWNERSHIPSTRUCTURE = loan.ownershipStructure;
        }


        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            decimal loanBalance = 0;
            decimal overdraftBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN
                               where a.CUSTOMERID == customerId
                               select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }
            else
            {
                loanBalance = 0;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING
                               where a.CUSTOMERID == customerId
                               select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }
            else
            {
                overdraftBalance = 0;
            }

            decimal totalBalance = loanBalance + overdraftBalance;

            return totalBalance;
        }

        private void ValidateLoanApplicationLimits(LoanApplicationViewModel application)
        {
            var details = application.LoanApplicationDetail;
            int branchId = (int)application.branchId;
            int customerId = (int)application.customerId;
            int productId = application.productId;
            decimal applicationAmount = details.Sum(x => x.proposedAmount); // proposedAmount should be approvedAmount after application

            var branchOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride && x.ISUSED == false),
                    c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                .FirstOrDefault();

            var sectorOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride && x.ISUSED == false),
                    c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                .FirstOrDefault();

            // if productoverride is to be used
            //var productOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
            //    .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.productLimitOverride && x.ISUSED == false),
            //        c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
            //    .Select(x => new { id = x.o.OVERRIDE_DETAILID })
            //    .FirstOrDefault();

            if (branchOverrideRequest != null)
            {
                //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                //request.ISUSED = true;
            }
            else
            {
                // branch limits
                var branchValidation = limitValidation.ValidateNPLByBranch((short)branchId);
                decimal branchNplAmount = (decimal)branchValidation.outstandingBalance;
                var branch = context.TBL_BRANCH.Find(branchId);
                if (branch.NPL_LIMIT > 0 && branch.NPL_LIMIT < (branchNplAmount + applicationAmount)) throw new SecureException("Branch NPL Limit exceeded!");
            }

            if (sectorOverrideRequest != null)
            {
                //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                //request.ISUSED = true;
            }
            else
            {
                foreach (var facility in details)
                {
                    // sector limits
                    // sectorId here is actually the subsectorId
                    //List<short> sectorIds = details.Select(x => x.subSectorId).ToList();
                    //foreach (var sectorId in sectorIds)
                    //{
                    var sectorValidation = limitValidation.ValidateNPLBySector(facility.subSectorId);
                    decimal sectorAmount = (decimal)sectorValidation.outstandingBalance + (facility.proposedAmount * (decimal)facility.exchangeRate);
                    //var sector = context.TBL_SECTOR.Find(sectorId);
                    if (sectorValidation.maximumAllowedLimit > 0 && sectorValidation.maximumAllowedLimit <= sectorAmount) throw new SecureException("Sector Limit for sector, " + facility.sectorName + " exceeded!");
                    //}
                }

            }
            try
            {
                if (limitValidation.ProductLimitExceeded(productId, application.proposedAmount))
                {
                    fireResponse("Product Limit exceeded!","99");
                }
            }
            catch (Exception ex) { throw ex; }

            var exposure = GetCurrentCompanyExposure();
            var proposedExposure = exposure.proposedLimit + applicationAmount;
            var company = context.TBL_COMPANY.Find(application.companyId);
            if (proposedExposure >= company.SHAREHOLDERSFUND)
            {
                fireResponse("Company Limit Exceeded","99");
            }

            var insiderLimit = limitValidation.ValidateNPLByInsiderCustomer();
            var insiderExposure = insiderLimit.outstandingBalance + (double)applicationAmount;
            if (insiderExposure >= (double)insiderLimit.maximumAllowedLimit)
            {
                fireResponse("Insider Limit Exceeded", "99");
            }

            if (limitValidation.IsDirectorRelatedGroup(application.customerGroupId) || limitValidation.CustomerIsDirector(application.customerId))
            {
                var directorLimit = limitValidation.ValidateNPLByDirectors();
                var directorExposure = (double)applicationAmount;
                if (directorExposure >= (double)directorLimit.maximumAllowedLimit)
                {
                    fireResponse("Director Limit Exceeded","99");
                }

            }

        }

        public string GetRefrenceNumber()
        {
            var millisecond = DateTime.Now.Millisecond;
            string refnumber = CommonHelpers.GetLoanReferanceNumber().ToString()
                + "" + CommonHelpers.AppendZeroString(millisecond, 3);
            return refnumber.ToString();
        }

        public CurrentCustomerExposure GetCurrentCompanyExposure()
        {
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            CurrentCustomerExposure totalExposures = new CurrentCustomerExposure();

            //if (operationId == (int)OperationsEnum.CreditAppraisal)
            //    details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.APPROVEDPRODUCTID }).ToList();
            //else
            //    details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.PRODUCTID }).ToList();

            //foreach (var detail in details)
            //{
            exposure = context.TBL_LOAN
                    .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
                        proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

            if (exposure.Count() > 0) exposures.AddRange(exposure);

            // Same for revolving and contegent facility ...

            exposure = context.TBL_LOAN_REVOLVING
                .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                .Select(g => new CurrentCustomerExposure
                {
                    facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                    existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                    proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                    recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                    PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                    PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                    reviewDate = DateTime.Now,
                    prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                    loanStatus = "Running"
                });

            if (exposure.Count() > 0) exposures.AddRange(exposure);


            exposure = context.TBL_LOAN_CONTINGENT
                .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                .Select(g => new CurrentCustomerExposure
                {
                    facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                    existingLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    proposedLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                    reviewDate = DateTime.Now,
                    loanStatus = "Running"
                });

            if (exposure.Count() > 0) exposures.AddRange(exposure);


            totalExposures = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                PastDueObligationsPrincipal = exposures.Sum(t => t.PastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
                prudentialGuideline = String.Empty,
                loanStatus = String.Empty,
            };

            return totalExposures;
        }

        private int? GetWorkflowProductId(short productId)
        {
            if (context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                && x.OPERATIONID == (int)OperationsEnum.CreditAppraisal //&& x.PRODUCTCLASSID == model.productClassId
                && x.PRODUCTID == productId).Any()) return productId;
            return null;
        }

        private bool GetCustomerIsRelatedParty(int customerId)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);
            var customerGroup = new TBL_CUSTOMER_GROUP();
            if (customer == null)
            {
                customerGroup = context.TBL_CUSTOMER_GROUP.Find(customerId);
                if (customerGroup != null)
                {
                    var mappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(m => m.DELETED != true && m.CUSTOMERGROUPID == customerGroup.CUSTOMERGROUPID);
                    var customers = new List<TBL_CUSTOMER>();
                    foreach (var map in mappings)
                    {
                        customers.Add(map.TBL_CUSTOMER);
                    }

                    if (customers.Exists(c => c.ISREALATEDPARTY == true))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return customer.ISREALATEDPARTY;
        }

        private bool GetCustomerIsPoliticallyExposed(int customerId)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);
            var customerGroup = new TBL_CUSTOMER_GROUP();
            if (customer == null)
            {
                customerGroup = context.TBL_CUSTOMER_GROUP.Find(customerId);
                if (customerGroup != null)
                {
                    var mappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(m => m.DELETED != true && m.CUSTOMERGROUPID == customerGroup.CUSTOMERGROUPID);
                    var customers = new List<TBL_CUSTOMER>();
                    foreach (var map in mappings)
                    {
                        customers.Add(map.TBL_CUSTOMER);
                    }

                    if (customers.Exists(c => c.ISPOLITICALLYEXPOSED == true))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return customer.ISPOLITICALLYEXPOSED;
        }


        private void ProductFees(List<ProductFeesViewModel> fees, int loanApplicationId, int createdBy)
        {
            var data = fees.Select(c => new TBL_LOAN_APPLICATION_DETL_FEE()
            {
                CHARGEFEEID = c.feeId,
                RECOMMENDED_FEERATEVALUE = c.rate,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                HASCONSESSION = false,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                LOANAPPLICATIONDETAILID = c.loanApplicationDetailId,
                DEFAULT_FEERATEVALUE = c.rate
            });

            context.TBL_LOAN_APPLICATION_DETL_FEE.AddRange(data);

        }

        private string GenerateLoanReference(int customerId)
        {
            string code = "";
            int data = 0;
            if (customerId > 2)
            {
                var grp = this.context.TBL_CUSTOMER_GROUP.Where(x => x.CUSTOMERGROUPID == customerId);
                if (grp.Any())
                {
                    code = grp.First().GROUPCODE;
                }
                data = ((this.context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
            }
            else
            {
                var cust = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId);
                if (cust.Any())
                {
                    code = cust.First().CUSTOMERCODE;
                }
                data = ((context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
            }

            return $"{code}{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }

        private void validateCustomerDetails(IncomingCustomerViewModels entity)
        {
            
        }


    }
}

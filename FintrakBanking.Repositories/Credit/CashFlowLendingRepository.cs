using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
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

        public CashFlowLendingRepository(FinTrakBankingContext _context, 
                                        ICustomerRepository _customer, 
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                         CustomerDetails _customerRequest)
        {
            this.context = _context;
            this.customer = _customer;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.customerRequest = _customerRequest;
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

                return AddIndividualCustomer(model);
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
            if (creditBureautype.Contains(CreditBureauEnum.CRCCreditBureau.ToString()) == false) { return fireResponse("Missing CRC credit bureau","99"); }

            if (saveIndividualCustomerInformation(model))
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
                //BUSINESSUNTID = model.businessUnitId
      
            };
            return false;
        }

        private bool saveCorporateCustomerInformation(ApiCustomerBusinessDetailsViewModel model)
        {
            return false;
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


            return response;
        }

        private void validateCustomerDetails(IncomingCustomerViewModels entity)
        {
            
        }
    }
}

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

        public string AddCustomer(IncomingCustomerViewModels model)
        {
            validateCustomerDetails(model);
            var entity = getCustomerViewModel(model);

            int? maritalStatus = null;
            if (entity.customerTypeId == (short)CustomerTypeEnum.Individual)
            {
                maritalStatus = Convert.ToInt32(entity.maritalStatus);
            }
            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
                BRANCHID = 1, //entity.userBranchId,
                COMPANYID = 1, //entity.companyId,
                CREATEDBY = 1, //(int)entity.createdBy,
                CREATIONMAILSENT = false, //entity.creationMailSent,
                CUSTOMERCODE =  model.customerNumber,
                CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
                CUSTOMERTYPEID = entity.customerTypeId,
                DATEOFBIRTH = entity.dateOfBirth,
                DATETIMECREATED = DateTime.Now,
                EMAILADDRESS = entity.emailAddress,
                FIRSTNAME = entity.firstName,
                GENDER = entity.gender,
                LASTNAME = entity.lastName,
                MAIDENNAME = entity.maidenName,
                MARITALSTATUS = maritalStatus,
                TITLE = entity.title,
                MIDDLENAME = entity.middleName,
                MISCODE = entity.misCode,
                MISSTAFF = entity.misStaff,
                NATIONALITYID = entity.nationalityId,
                OCCUPATION = entity.occupation,
                PLACEOFBIRTH = entity.placeOfBirth,
                ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed,
                ISINVESTMENTGRADE = entity.isInvestmentGrade,
                ISREALATEDPARTY = entity.isRealatedParty,
                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                SPOUSE = entity.spouse,
                SUBSECTORID = entity.subSectorId,
                TAXNUMBER = entity.taxNumber,
                RISKRATINGID = entity.riskRatingId,
                CUSTOMERBVN = entity.customerBVN,
                PROSPECTCUSTOMERCODE = entity.prospectCustomerCode,
                ISPROSPECT = entity.isProspect,
                CRMSCOMPANYSIZEID = entity.crmsCompanySizeId,
                CRMSLEGALSTATUSID = entity.crmsLegalStatusId,
                CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId,
                COUNTRYOFRESIDENTID = entity.countryOfResidentId,
                NUMBEROFDEPENDENTS = entity.numberOfDependents,
                NUMBEROFLOANSTAKEN = entity.numberOfLoansTaken,
                MONTHLYLOANREPAYMENT = entity.loanMonthlyRepaymentFromOtherBanks,
                DATEOFRELATIONSHIPWITHBANK = entity.dateOfRelationshipWithBank,
                RELATIONSHIPTYPEID = entity.relationshipTypeId,
                TEAMLDR = entity.teamLDP,
                TEAMNPL = entity.teamNPL,
                CORR = entity.corr,
                PASTDUEOBLIGATIONS = entity.pastDueObligations,
                BUSINESSUNTID = entity.businessUnitId
            };
            context.TBL_CUSTOMER.Add(customer);

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
            //    STAFFID = entity.createdBy,
            //    BRANCHID = (short)entity.userBranchId,
            //    DETAIL = $"Added Customer  '{entity.customerName}' with Code: {entity.customerCode}",
            //    IPADDRESS = entity.userIPAddress,
            //    URL = entity.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //auditTrail.AddAuditTrail(audit);

            try
            {
                var output = context.SaveChanges() > 0;
                var result = entity.isProspect == true ? entity.prospectCustomerCode : entity.customerCode;
                if (output == true)
                {
                    return result;
                }
                else
                {
                    return null;
                }

            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ",
                    ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }

        public CustomerViewModels getCustomerViewModel(IncomingCustomerViewModels entity)
        {
            CustomerViewModels model = new CustomerViewModels();
           // model.accountCreationComplete = "";
            //model.userBranchId = "";
            //model.companyId = "";
            //model.createdBy = "";
            //model.creationMailSent = "";
            model.customerCode = entity.customerNumber;
            //model.customerSensitivityLevelId = "";
            model.customerTypeId = Convert.ToInt16(entity.customerTypeId);
            model.dateOfBirth = Convert.ToDateTime(entity.dateOfBirth).Date;
            model.emailAddress = entity.emailAddress;
            model.firstName = entity.firstName;
            model.gender = entity.gender;
            model.lastName = entity.lastName;
            model.maidenName = entity.maidenName;
            model.maritalStatus = entity.maritalStatus;
            model.title = entity.title;
            model.middleName = entity.middleName;
            model.misCode = entity.misCode;
            model.misStaff = entity.misStaff;
           // model.nationalityId = entity.nationality;
            model.occupation = entity.occupation;
            model.placeOfBirth = entity.placeOfBirth;
            model.isPoliticallyExposed = Convert.ToBoolean(entity.isPoliticallyExposed);
            model.isInvestmentGrade = Convert.ToBoolean(entity.isInvestmentGrade);
            model.isRealatedParty = Convert.ToBoolean(entity.isRealatedParty); 
           // model.relationshipOfficerId = ;
            model.spouse = entity.spouse;
            model.subSectorId = Convert.ToInt16(entity.subSectorId);
            model.taxNumber = entity.taxNumber;
            model.riskRatingId = Convert.ToInt16(entity.riskRatingId);
            model.customerBVN = entity.customerBVN;
            model.relationshipTypeId = Convert.ToInt16(entity.relationshipTypeId);

            // model.prospectCustomerCode = ;
            //model.isProspect = false;
            //model.crmsCompanySizeId = "";
            //model.crmsLegalStatusId = "";
            //model.crmsRelationshipTypeId = "";
            //model.countryOfResidentId = "";
            //model.numberOfDependents = "";
            //model.numberOfLoansTaken = "";
            //model.loanMonthlyRepaymentFromOtherBanks = "";
            //model.dateOfRelationshipWithBank = "";
            //model.teamLDP = "";
            //model.teamNPL = "";
            //model.corr = "";
            //model.pastDueObligations = "";
            //model.businessUnitId = "";

            return model;
        }

        private void fireException(string message)
        {
            throw new ConditionNotMetException(message);
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

        private void validateCustomerDetails(IncomingCustomerViewModels entity)
        {
            if(entity.customerNumber == string.Empty || entity.customerNumber == null)
            {
                fireException($"Missing customer Number is required");
            }

            if (ValidateCustomerCode(entity.customerNumber))
            {
               fireException($"Customer with customer code {entity.customerNumber} already exist");
            }

            //if (ValidateModifiedCustomerRecord(entity.customerId))
            //{
            //    throw new ConditionNotMetException("Customer General Information is already undergoing approval.");
            //}

            //if (entity.isProspect == true)
            //{
            //    //string code = CommonHelpers.GenerateUniqueIntergers(7).ToString();
            //    //entity.prospectCustomerCode = "PROS-" + code;
            //    ////entity.customerCode = "PROS-" + code;
            //}
            //else
            //{
            //    //if (USE_THIRD_PARTY_INTEGRATION)
            //    //    entity.isPoliticallyExposed = finacle.GetExposePersonStatus(entity.customerCode);
            //}
        }

    }
}

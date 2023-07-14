using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using FinTrakBanking.ThirdPartyIntegration;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Setups.Credit;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using FintrakBanking.ViewModels.Audit;
using FintrakBanking.ViewModels.Credit;
using GemBox.Spreadsheet;
using System.IO;
using System.Transactions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Runtime.Remoting.Channels;

namespace FintrakBanking.Repositories.Customer
{
    public class CustomerRepository : ICustomerRepository
    {
        private ICustomerCreditBureauRepository bureau;
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private IWorkflow workflow;
        private IApprovalLevelStaffRepository level;
        private IIntegrationWithFinacle finacle;
        private IIntegrationWithFinacle integration;

        private int customerId;
        int status = 0;
        bool USE_THIRD_PARTY_INTEGRATION;

        public CustomerRepository(IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup,
            IWorkflow _workFlow,
            IApprovalLevelStaffRepository _level,
            FinTrakBankingContext _context,
            ICustomerCreditBureauRepository bureau, IIntegrationWithFinacle finacle, IIntegrationWithFinacle _integration)
        {
            context = _context;
            workflow = _workFlow;
            auditTrail = _auditTrail;
            _genSetup = genSetup;
            level = _level;
            this.integration = _integration;
            this.finacle = finacle;
            this.bureau = bureau;
            var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (global != null) this.USE_THIRD_PARTY_INTEGRATION = global.USE_THIRD_PARTY_INTEGRATION;
        }



        public dynamic GetCustomerRating(int custormerId)
        {

            var data = (from c in context.TBL_CUSTOMER  
                        join r in context.TBL_CUSTOMER_RISK_RATING on c.RISKRATINGID equals r.RISKRATINGID
                        where c.CUSTOMERID == custormerId
                        select new
                        {                           
                            isInvestment = r.ISINVESTMENTGRADE, // c.TBL_CUSTOMER_RISK_RATING.ISINVESTMENTGRADE,
                            rating = r.RISKRATING // c.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                        }).FirstOrDefault();
            return data;
        }

        public string AddCustomer(CustomerViewModels entity)
        {
            if (ValidateCustomerCode(entity.customerCode))
            {
                throw new ConditionNotMetException($"Customer with customer code {entity.customerCode} already exist");
            }
            if (ValidateModifiedCustomerRecord(entity.customerId))
            {
                throw new ConditionNotMetException("Customer General Information is already undergoing approval.");
            }

            if (entity.isProspect == true)
            {
                string code = CommonHelpers.GenerateUniqueIntergers(7).ToString();
                entity.prospectCustomerCode = "PROS-" + code;
                //entity.customerCode = "PROS-" + code;
            }
            else
            {
                //if (USE_THIRD_PARTY_INTEGRATION)
                //    entity.isPoliticallyExposed = finacle.GetExposePersonStatus(entity.customerCode);
            }

            int? maritalStatus = null;
            if (entity.customerTypeId == (short)CustomerTypeEnum.Individual)
            {
                if (entity.maritalStatus == null)
                {
                    maritalStatus = 0;
                }
                else if (entity.maritalStatus.ToLower() == "s")
                {
                    maritalStatus = 1;
                }
                else if(entity.maritalStatus.ToLower() == "m")
                {
                    maritalStatus = 2;
                }
                else if(entity.maritalStatus.ToLower() == "d")
                {
                    maritalStatus = 3;
                }
                else if(entity.maritalStatus.ToLower() == "w")
                {
                    maritalStatus = 4;
                }
                else
                {
                    maritalStatus = 0;
                }
                //maritalStatus = Convert.ToInt32(entity.maritalStatus) ;
            }

            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete,
                BRANCHID = entity.userBranchId,
                COMPANYID = entity.companyId,
                CREATEDBY = (int)entity.createdBy,
                CREATIONMAILSENT = entity.creationMailSent,
                CUSTOMERCODE = entity.isProspect == true ? entity.prospectCustomerCode : entity.customerCode,
                CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId,
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
                BUSINESSUNTID = entity.businessUnitId,
                OWNERSHIP = entity.ownership,
                NAMEOFSIGNATORY = entity.nameofSignatories,
                ADDRESSOFSIGNATORY = entity.addressofSignatories,
                PHONENUMBEROFSIGNATORY = entity.phoneNumberofSignatories,
                EMAILOFSIGNATORY = entity.emailofSignatories,
            };
            context.TBL_CUSTOMER.Add(customer);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer  '{entity.customerName}' with Code: {entity.prospectCustomerCode}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            auditTrail.AddAuditTrail(audit);
            var result = entity.isProspect == true ? entity.prospectCustomerCode : entity.customerCode;


            try
            {
                var output = context.SaveChanges() > 0;
                //var result = entity.isProspect == true ? entity.prospectCustomerCode : entity.customerCode;
                if (output == true)
                {
                    UpdateCustomerCollateralId(customer.CUSTOMERCODE);
                    fetchCustomerAccountBalance(customer);
                    return result;
                }
                else
                {
                    return null;
                }

            }
            catch (APIErrorException ex) {
                if (ex.Message.Contains("This customer does not have an account linked")) {
                    return result;
                }

                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            //catch (DbEntityValidationException ex)
            //{
            //    string errorMessages = string.Join("; ",
            //        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
            //    throw new DbEntityValidationException(errorMessages);
            //}
        }

        public void UpdateCustomerCollateralId(string customerCode)
        {
            customerCode = customerCode.Trim();
            //var customer = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERCODE.Contains(customerCode) || customerCode.Contains(c.CUSTOMERCODE.Trim()) && c.DELETED == false);
            var customer = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERCODE == customerCode && c.DELETED == false);
            var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.CUSTOMERCODE == customerCode).ToList();
            if (collaterals != null)
            {
                foreach (var c in collaterals)
                {
                    c.CUSTOMERID = customer?.CUSTOMERID;
                }
            }
            var saved = context.SaveChanges() > 0;
        }

        //public void UpdateCustomerCollateralId(string customerCode)
        //{
        //    customerCode = customerCode.Trim();
        //    var customer = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERCODE.Contains(customerCode) || customerCode.Contains(c.CUSTOMERCODE.Trim()) && c.DELETED == false);
        //    var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.CUSTOMERCODE.Contains(customerCode)).ToList();
        //    foreach(var c in collaterals)
        //    {
        //        c.CUSTOMERID = customer.CUSTOMERID;
        //    }
        //    var saved = context.SaveChanges() > 0;
        //}

        public bool refreshCustomerAccount(int customerId)
        {
            bool result = false;

            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId).FirstOrDefault();

            if (customer != null)
            {
                result = fetchCustomerAccountBalance(customer);
            }

            return result;    
        }

        private bool fetchCustomerAccountBalance(TBL_CUSTOMER data)
        {
            bool result = false;
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                result = integration.AddCustomerAccounts(data.CUSTOMERCODE);
            }
            return result;
        }

        public bool GetPoliticallyExposedPerson(string customerCode)
        {
            bool isPoliticallyExposed = false;
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                isPoliticallyExposed = finacle.GetExposePersonStatus(customerCode);
            }
            return isPoliticallyExposed;
        }

        private void AddCustomerAddresses(List<CustomerAddressViewModels> entity,
            int status)
        {
            var address = new TBL_CUSTOMER_ADDRESS();
            foreach (var ent in entity)
            {
                address.ACTIVE = ent.active;
                address.ADDRESS = ent.address;
                address.ADDRESSTYPEID = (short)ent.addressTypeId;
                address.CITYID = ent.cityId;
                address.CUSTOMERID = ent.customerId;
                address.STATEID = ent.stateId;
                address.HOMETOWN = ent.homeTown;
                address.POBOX = ent.pobox;
                address.STATEID = ent.stateId;
                address.LOCALGOVERNMENTID = ent.localGovernmentId;

                context.TBL_CUSTOMER_ADDRESS.Add(address);

            }
        }

        public bool UpdateCustomerInformation(string customerCode, string accountNumber, int createdBy)
        {
            var currentCustomer = context.TBL_CUSTOMER.Where(O => O.CUSTOMERCODE == customerCode).FirstOrDefault();

            if (currentCustomer != null) {
                var result = integration.GetCustomerByAccountsNumber(accountNumber).FirstOrDefault();

                if (result != null) {
                    integration.AddCustomerAccounts(customerCode);

                    context.TBL_CUSTOMER_ARCHIVE.Add(new TBL_CUSTOMER_ARCHIVE()
                    {
                        CUSTOMERID = currentCustomer.CUSTOMERID,
                        ACCOUNTCREATIONCOMPLETE = currentCustomer.ACCOUNTCREATIONCOMPLETE,
                        BRANCHID = currentCustomer.BRANCHID,
                        COMPANYID = currentCustomer.COMPANYID,
                        CREATEDBY = currentCustomer.CREATEDBY,
                        CREATIONMAILSENT = currentCustomer.CREATIONMAILSENT,
                        CUSTOMERCODE = currentCustomer.CUSTOMERCODE,
                        CUSTOMERSENSITIVITYLEVELID = currentCustomer.CUSTOMERSENSITIVITYLEVELID,
                        CUSTOMERTYPEID = currentCustomer.CUSTOMERTYPEID,
                        DATEOFBIRTH = currentCustomer.DATEOFBIRTH,
                        DATETIMECREATED = currentCustomer.DATETIMECREATED,
                        EMAILADDRESS = currentCustomer.EMAILADDRESS,
                        FIRSTNAME = currentCustomer.FIRSTNAME,
                        GENDER = currentCustomer.GENDER,
                        LASTNAME = currentCustomer.LASTNAME,
                        MAIDENNAME = currentCustomer.MAIDENNAME,
                        MARITALSTATUS = currentCustomer.MARITALSTATUS,
                        TITLE = currentCustomer.TITLE,
                        MIDDLENAME = currentCustomer.MIDDLENAME,
                        MISCODE = currentCustomer.MISCODE,
                        MISSTAFF = currentCustomer.MISSTAFF,
                        NATIONALITYID = currentCustomer.NATIONALITYID,
                        OCCUPATION = currentCustomer.OCCUPATION,
                        PLACEOFBIRTH = currentCustomer.PLACEOFBIRTH,
                        ISPOLITICALLYEXPOSED = currentCustomer.ISPOLITICALLYEXPOSED,
                        ISINVESTMENTGRADE = currentCustomer.ISINVESTMENTGRADE,
                        ISREALATEDPARTY = currentCustomer.ISREALATEDPARTY,
                        RELATIONSHIPOFFICERID = currentCustomer.RELATIONSHIPOFFICERID,
                        SPOUSE = currentCustomer.SPOUSE,
                        SUBSECTORID = currentCustomer.SUBSECTORID,
                        TAXNUMBER = currentCustomer.TAXNUMBER,
                        RISKRATINGID = currentCustomer.RISKRATINGID,
                        CUSTOMERBVN = currentCustomer.CUSTOMERBVN,
                        PROSPECTCUSTOMERCODE = currentCustomer.PROSPECTCUSTOMERCODE,
                        ISPROSPECT = currentCustomer.ISPROSPECT,
                        CRMSCOMPANYSIZEID = currentCustomer.CRMSCOMPANYSIZEID,
                        CRMSLEGALSTATUSID = currentCustomer.CRMSLEGALSTATUSID,
                        CRMSRELATIONSHIPTYPEID = currentCustomer.CRMSRELATIONSHIPTYPEID,
                        COUNTRYOFRESIDENTID = currentCustomer.COUNTRYOFRESIDENTID,
                        NUMBEROFDEPENDENTS = currentCustomer.NUMBEROFDEPENDENTS,
                        NUMBEROFLOANSTAKEN = currentCustomer.NUMBEROFLOANSTAKEN,
                        MONTHLYLOANREPAYMENT = currentCustomer.MONTHLYLOANREPAYMENT,
                        DATEOFRELATIONSHIPWITHBANK = currentCustomer.DATEOFRELATIONSHIPWITHBANK,
                        RELATIONSHIPTYPEID = currentCustomer.RELATIONSHIPTYPEID,
                        TEAMLDR = currentCustomer.TEAMLDR,
                        TEAMNPL = currentCustomer.TEAMNPL,
                        CORR = currentCustomer.CORR,
                        PASTDUEOBLIGATIONS = currentCustomer.PASTDUEOBLIGATIONS,
                        BUSINESSUNTID = currentCustomer.BUSINESSUNTID,
                        OWNERSHIP = currentCustomer.OWNERSHIP,
                        NAMEOFSIGNATORY = currentCustomer.NAMEOFSIGNATORY,
                        ADDRESSOFSIGNATORY = currentCustomer.ADDRESSOFSIGNATORY,
                        PHONENUMBEROFSIGNATORY = currentCustomer.PHONENUMBEROFSIGNATORY,
                        EMAILOFSIGNATORY = currentCustomer.EMAILOFSIGNATORY,
                    });

                    currentCustomer.FIRSTNAME = result.firstName;
                    currentCustomer.LASTNAME = result.lastName;
                    currentCustomer.MIDDLENAME = result.middleName;
                    currentCustomer.SPOUSE = result.spouse;
                    //currentCustomer.MARITALSTATUS  = result.maritalStatus;

                    currentCustomer.OCCUPATION = result.occupation;
                    //currentCustomer.EMAILADDRESS = result.emailAddress;
                    //currentCustomer.TBL_CUSTOMER_PHONECONTACT. = result.emailAddress;
                    currentCustomer.EMAILADDRESS = result.emailAddress;
                    currentCustomer.DATETIMEUPDATED = DateTime.Now;
                    currentCustomer.LASTUPDATEDBY = createdBy;

                    return context.SaveChanges() > 0;

                }

                return false;
            }

            return false;
        }

        public bool AddCustomerAddresses(CustomerAddressViewModels entity)
        {
            var auditDetail = string.Empty;
            short auditType = 0;

            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_ADDRESS address;
                    // if (entity.addressId != 0 || entity.addressId < 0)  //Check if record is new or modified record
                    // {
                    address = context.TBL_CUSTOMER_ADDRESS.Where(x=>x.ADDRESSID==entity.addressId).FirstOrDefault();
                    var customer = context.TBL_CUSTOMER.Find(entity.customerId);
                    var accountCompleted = customer.ACCOUNTCREATIONCOMPLETE;
                    
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false and entity.canModified equal true, record insert directly to the main table 
                    if (address != null && accountCompleted == false)
                    {
                        var cityName = context.TBL_CITY.Where(c => c.CITYID == address.CITYID).FirstOrDefault()?.CITYNAME;
                        var existingAddrss = address?.ADDRESS + " " + cityName;
                        entity.homeTown = context.TBL_CITY.Where(x => x.CITYID == entity.cityId).Select(m => m.CITYNAME).FirstOrDefault();

                        address.ACTIVE = entity.active;
                        address.ADDRESS = entity.address;
                        address.ADDRESSTYPEID = (short)entity.addressTypeId;
                        address.CITYID = entity.cityId;
                        address.STATEID = entity.stateId;
                        address.HOMETOWN = entity.homeTown;
                        address.POBOX = entity.pobox;
                        address.STATEID = entity.stateId;
                        address.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                        address.NEARESTLANDMARK = entity.nearestLandmark;
                        address.LOCALGOVERNMENTID = entity.localGovernmentId;

                        auditDetail = $"Updated new Customer Address for customer: {customer.CUSTOMERCODE}. from {existingAddrss} to {entity.address}";
                        auditType = (short)AuditTypeEnum.CustomerAddressUpdated;
                    }
                    else if (address == null && accountCompleted == false)
                    {
                        address = new TBL_CUSTOMER_ADDRESS();
                        address.ACTIVE = entity.active;
                        address.ADDRESS = entity.address;
                        address.ADDRESSTYPEID = (short)entity.addressTypeId;
                        address.CITYID = entity.cityId;
                        address.CUSTOMERID = entity.customerId;
                        address.STATEID = entity.stateId;
                        address.HOMETOWN = entity.homeTown;
                        address.POBOX = entity.pobox;
                        address.STATEID = entity.stateId;
                        address.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                        address.NEARESTLANDMARK = entity.nearestLandmark;
                        address.LOCALGOVERNMENTID = entity.localGovernmentId;

                        context.TBL_CUSTOMER_ADDRESS.Add(address);

                        auditDetail = "Added new Customer Address for customer ID: + (" + entity.customerId + ") ";
                        auditType = (short)AuditTypeEnum.CustomerAddressAdded;
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer address information has existing record being modified and approved
                        var existingTempAddress = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x =>
                            x.ADDRESSID == entity.addressId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.addressId != 0 || entity.addressId > 0)
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Address_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Address_Addition;
                        }



                        TBL_TEMP_CUSTOMER_ADDRESS temp = null;
                        if (existingTempAddress != null && entity.addressId > 0
                        ) //if customer address information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempAddress;

                            temp.ACTIVE = entity.active;
                            temp.ADDRESS = entity.address;
                            temp.ADDRESSTYPEID = (short)entity.addressTypeId;
                            temp.CITYID = entity.cityId;
                            temp.STATEID = entity.stateId;
                            temp.HOMETOWN = entity.homeTown;
                            temp.POBOX = entity.pobox;
                            temp.STATEID = entity.stateId;
                            temp.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                            temp.NEARESTLANDMARK = entity.nearestLandmark;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.LOCALGOVERNMENTID = entity.localGovernmentId;
                            modifiedTargetId = temp.TEMPADDRESSID;

                            auditDetail = "Added new Customer Address for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerAddressAdded;

                        }
                        else //if customer address information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMER_ADDRESS();

                            temp.ADDRESSID = entity.addressId;
                            temp.CUSTOMERID = entity.customerId;
                            temp.ACTIVE = entity.active;
                            temp.ADDRESS = entity.address;
                            temp.ADDRESSTYPEID = (short)entity.addressTypeId;
                            temp.CITYID = entity.cityId;
                            temp.STATEID = entity.stateId;
                            temp.HOMETOWN = entity.homeTown;
                            temp.POBOX = entity.pobox;
                            temp.STATEID = entity.stateId;
                            temp.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                            temp.NEARESTLANDMARK = entity.nearestLandmark;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATETIMECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.LOCALGOVERNMENTID = entity.localGovernmentId;
                            context.TBL_TEMP_CUSTOMER_ADDRESS.Add(temp);

                            auditDetail = "Added new Customer Address for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerAddressAdded;
                        }

                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {

                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPADDRESSID;

                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = auditType,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        private void AddCustomerBvn(List<CustomerBvnViewModels> entity, int status)
        {
            List<CustomerBvnViewModels> bvnlist = new List<CustomerBvnViewModels>();
            var customerBvn = new TBL_CUSTOMER_BVN();
            foreach (var bvn in entity)
            {
                customerBvn.BANKVERIFICATIONNUMBER = bvn.bankVerificationNumber;
                customerBvn.CUSTOMERID = bvn.companyId;
                customerBvn.CREATEDBY = bvn.createdBy;
                customerBvn.CUSTOMERID = bvn.customerId;
                customerBvn.FIRSTNAME = bvn.firstname;
                customerBvn.SURNAME = bvn.surname;
                customerBvn.ISVALIDBVN = bvn.isValidBvn;
                customerBvn.DATETIMECREATED = bvn.dateTimeCreated;

                context.TBL_CUSTOMER_BVN.Add(customerBvn);
            }
        }

        public bool AddCustomerBvn(CustomerBvnViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_BVN customerBvn;
                    if (entity.customerBvnid != 0 || entity.customerBvnid < 0)
                    {
                        customerBvn = context.TBL_CUSTOMER_BVN.Find(entity.customerBvnid);

                        if (customerBvn != null)
                        {
                            customerBvn.BANKVERIFICATIONNUMBER = entity.bankVerificationNumber;
                            customerBvn.CUSTOMERID = entity.customerId;
                            customerBvn.CREATEDBY = entity.createdBy;
                            customerBvn.FIRSTNAME = entity.firstname;
                            customerBvn.SURNAME = entity.surname;
                            customerBvn.ISVALIDBVN = entity.isValidBvn;
                            customerBvn.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                        }
                    }
                    else
                    {
                        customerBvn = new TBL_CUSTOMER_BVN
                        {
                            BANKVERIFICATIONNUMBER = entity.bankVerificationNumber,
                            CUSTOMERID = entity.customerId,
                            CREATEDBY = entity.createdBy,
                            FIRSTNAME = entity.firstname,
                            SURNAME = entity.surname,
                            ISVALIDBVN = entity.isValidBvn,
                            DATETIMECREATED = DateTime.Now,
                            ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed
                        };

                        context.TBL_CUSTOMER_BVN.Add(customerBvn);
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new Customer BVN for customer ID: + (" + entity.customerId + ") ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;

                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerChildren(List<CustomerChildrenViewModel> models, int staffId, short BranchId)
        {
            if (models != null)
            {
                try
                {
                    TBL_CUSTOMER_CHILDREN child;
                    List<TBL_CUSTOMER_CHILDREN> custChildren = new List<TBL_CUSTOMER_CHILDREN>();
                    foreach (CustomerChildrenViewModel entity in models)
                    {
                        if (entity.customerChildrenId != 0 || entity.customerChildrenId < 0)
                        {
                            child = context.TBL_CUSTOMER_CHILDREN.Find(entity.customerChildrenId);
                            if (child != null)
                            {
                                child.CHILDNAME = entity.childName;
                                child.CHILDDATEOFBIRTH = entity.childDateOfBirth;
                            }
                        }
                        else
                        {
                            child = new TBL_CUSTOMER_CHILDREN()
                            {
                                CUSTOMERID = entity.customerId,
                                CHILDNAME = entity.childName,
                                CHILDDATEOFBIRTH = entity.childDateOfBirth
                            };
                            custChildren.Add(child);
                        }

                        // Audit Section ----------------------------
                        var audit = new TBL_AUDIT
                        {
                            AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                            STAFFID = staffId,
                            BRANCHID = BranchId,
                            DETAIL = "Added new TBL_CUSTOMER_CHILDREN for customer ID: + (" + entity.customerId + ") ",
                            IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                            URL = entity.applicationUrl,
                            APPLICATIONDATE = _genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now,
                            DEVICENAME = CommonHelpers.GetDeviceName(),
                            OSNAME = CommonHelpers.FriendlyName()
                        };

                        this.auditTrail.AddAuditTrail(audit);
                    }

                    context.TBL_CUSTOMER_CHILDREN.AddRange(custChildren);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        private void AddCustomerPhoneContact(IEnumerable<CustomerPhoneContactViewModels> entity)
        {
            var phone = new TBL_CUSTOMER_PHONECONTACT();
            foreach (var ent in entity)
            {
                phone.ACTIVE = ent.active;
                phone.CUSTOMERID = customerId;
                phone.PHONE = ent.phone;
                phone.PHONENUMBER = ent.phoneNumber;
                context.TBL_CUSTOMER_PHONECONTACT.Add(phone);
            }
        }

        public bool AddCustomerPhoneContact(CustomerPhoneContactViewModels entity)
        {
            if (entity != null)
            {
                var auditDetail = string.Empty;
                short auditType = 0;
                try
                {

                    TBL_CUSTOMER_PHONECONTACT phone;

                    phone = context.TBL_CUSTOMER_PHONECONTACT.Find(entity.phoneContactId);
          
                    //get customer record by id
                    var customer = context.TBL_CUSTOMER.Find(entity.customerId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;

                    //previous phone number
                    var previousPhoneNumber = phone?.PHONENUMBER;

                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (phone != null && accountCompleted == false)
                    {
                        phone.ACTIVE = entity.active;
                        phone.PHONE = entity.phone;
                        phone.PHONENUMBER = entity.phoneNumber;

                        //auditDetail = "Updated Customer Phone Number for customer ID: + (" + entity.customerId + ") ";
                        auditDetail = $"Updated new Customer Phone Number for customer: {customer.CUSTOMERCODE}. from {previousPhoneNumber} to {entity.phoneNumber}";
                        auditType = (short)AuditTypeEnum.CustomerContactUpdated;
                    }
                    else if (phone == null && accountCompleted == false)
                    {
                        phone = new TBL_CUSTOMER_PHONECONTACT
                        {
                            ACTIVE = entity.active,
                            CUSTOMERID = entity.customerId,
                            PHONE = entity.phone,
                            PHONENUMBER = entity.phoneNumber
                        };
                        context.TBL_CUSTOMER_PHONECONTACT.Add(phone);
                        auditDetail = "Added new Customer Phone Number for customer ID: + (" + entity.customerId + ") ";
                        auditType = (short)AuditTypeEnum.CustomerContactAdded;
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer phone contact information has existing record being modified and approved
                        var existingTempPhone = context.TBL_TEMP_CUSTOMER_PHONCONTACT.FirstOrDefault(x =>
                            x.TEMPPHONECONTACTID == entity.phoneContactId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.phoneContactId != 0 || entity.phoneContactId > 0)
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Phone_Number_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Phone_Number_Addition;
                        }

                        TBL_TEMP_CUSTOMER_PHONCONTACT temp = null;
                        if (existingTempPhone != null && entity.phoneContactId > 0
                        ) //if customer phone contact information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempPhone;
                            temp.ACTIVE = entity.active;
                            temp.PHONECONTACTID = entity.phoneContactId;
                            temp.CUSTOMERID = entity.customerId;
                            temp.PHONE = entity.phone;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPPHONECONTACTID;

                            auditDetail = "Added new Customer Phone Number for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerContactAdded;
                        }
                        else //if customer phoneContact information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMER_PHONCONTACT();

                            temp.ACTIVE = entity.active;
                            temp.CUSTOMERID = entity.customerId;
                            temp.PHONECONTACTID = entity.phoneContactId;
                            temp.PHONE = entity.phone;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATETIMECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUSTOMER_PHONCONTACT.Add(temp);

                            auditDetail = "Added new Customer Phone Number for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerContactAdded;

                        }

                        //Insert new row to TBL_CUSTOMER_MODIFICATION 
                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {

                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPPHONECONTACTID;

                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId


                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = auditType,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = entity.userIPAddress,
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerNextOfKin(CustomerNextOfKinViewModels entity)
        {
            if (entity != null)
            {
                var auditDetail = string.Empty;
                short auditType = 0;
                try
                {
                    TBL_CUSTOMER_NEXTOFKIN next;
                    //get next of kin primary key
                    next = context.TBL_CUSTOMER_NEXTOFKIN.Find(entity.nextOfKinId);

                    //get customer record by id
                    var customer = context.TBL_CUSTOMER.Find(entity.customerId);

                    //get previous next of kin
                    var previousNextOfKin = next?.FIRSTNAME; 

                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (next != null && accountCompleted == false)
                    {
                        next.CUSTOMERID = entity.customerId;
                        next.FIRSTNAME = entity.firstName;
                        next.LASTNAME = entity.lastName;
                        next.PHONENUMBER = entity.phoneNumber;
                        next.RELATIONSHIP = entity.relationship;
                        next.DATEOFBIRTH = entity.dateOfBirth;
                        next.EMAIL = entity.email;
                        next.NEAREST_LANDMARK = entity.nearestLandmark;
                        next.GENDER = entity.gender;
                        next.ADDRESS = entity.address;
                        next.CITYID = entity.cityId;
                        next.ACTIVE = entity.active;

                        //auditDetail = "Updated Customer's Next Of Kin for customer ID: + (" + entity.customerId + ") ";
                        auditDetail = $"Updated new Customer Next Of Kin for customer: {customer.CUSTOMERCODE}. from {previousNextOfKin} to {entity.firstName}";
                        auditType = (short)AuditTypeEnum.CustomerDetailUpdated;

                    }
                    else if (next == null && accountCompleted == false)
                    {
                        next = new TBL_CUSTOMER_NEXTOFKIN();
                        next.CUSTOMERID = entity.customerId;
                        next.FIRSTNAME = entity.firstName;
                        next.LASTNAME = entity.lastName;
                        next.PHONENUMBER = entity.phoneNumber;
                        next.RELATIONSHIP = entity.relationship;
                        next.DATEOFBIRTH = entity.dateOfBirth;
                        next.EMAIL = entity.email;
                        next.NEAREST_LANDMARK = entity.nearestLandmark;
                        next.GENDER = entity.gender;
                        next.ADDRESS = entity.address;
                        next.CITYID = entity.cityId;
                        next.ACTIVE = entity.active;
                        context.TBL_CUSTOMER_NEXTOFKIN.Add(next);

                        auditDetail = "Added Customer's Next Of Kin for customer ID: + (" + entity.customerId + ") ";
                        auditType = (short)AuditTypeEnum.CustomerDetailAdded;

                    }

                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer next of kin information has existing record being modified and approved
                        var existingTempNext = context.TBL_TEMP_CUSTOMER_NEXTOFKIN.FirstOrDefault(x =>
                            x.NEXTOFKINID == entity.nextOfKinId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.nextOfKinId != 0 || entity.nextOfKinId > 0)
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Next_of_Kin_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Next_of_Kin_Addition;
                        }

                        TBL_TEMP_CUSTOMER_NEXTOFKIN temp = null;
                        if (existingTempNext != null && entity.nextOfKinId > 0
                        ) //if customer phone contact information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempNext;
                            temp.CUSTOMERID = entity.customerId;
                            temp.FIRSTNAME = entity.firstName;
                            temp.LASTNAME = entity.lastName;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.RELATIONSHIP = entity.relationship;
                            temp.DATEOFBIRTH = entity.dateOfBirth;
                            temp.EMAIL = entity.email;
                            temp.NEAREST_LANDMARK = entity.nearestLandmark;
                            temp.GENDER = entity.gender;
                            temp.ADDRESS = entity.address;
                            temp.CITYID = entity.cityId;
                            temp.ACTIVE = entity.active;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;

                            auditDetail = "Added Customer's Next Of Kin for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerDetailAdded;
                        }
                        else //if customer phoneContact information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMER_NEXTOFKIN();
                            temp.CUSTOMERID = entity.customerId;
                            temp.FIRSTNAME = entity.firstName;
                            temp.LASTNAME = entity.lastName;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.RELATIONSHIP = entity.relationship;
                            temp.DATEOFBIRTH = entity.dateOfBirth;
                            temp.EMAIL = entity.email;
                            temp.NEAREST_LANDMARK = entity.nearestLandmark;
                            temp.GENDER = entity.gender;
                            temp.ADDRESS = entity.address;
                            temp.CITYID = entity.cityId;
                            temp.ACTIVE = entity.active;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.NEXTOFKINID = entity.nextOfKinId;
                            context.TBL_TEMP_CUSTOMER_NEXTOFKIN.Add(temp);
                            //  var res = context.SaveChanges() > 0;

                            auditDetail = "Added Customer's Next Of Kin for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerDetailAdded;

                        }

                        //  modifiedTargetId = entity.nextOfKinId;

                        //Insert new row to TBL_CUSTOMER_MODIFICATION 
                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPNEXTOFKINID;
                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = auditType,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerCompanyInfomation(CustomerCompanyInfomationViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_COMPANYINFOMATION company;
                    if (entity.companyInfomationId != 0 || entity.companyInfomationId > 0
                    ) //Check if record is new or modified record
                    {
                        company = context.TBL_CUSTOMER_COMPANYINFOMATION.Find(entity.companyInfomationId);
                        //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false and entity.canModified equal true, record insert directly to the main table 
                        if (company != null && company.TBL_CUSTOMER.ACCOUNTCREATIONCOMPLETE == false &&
                            entity.canModified == true)
                        {
                            company.ANNUALTURNOVER = entity.annualTurnOver;
                            company.COMPANYEMAIL = entity.companyEmail;
                            company.COMPANYNAME = entity.companyName;
                            company.COMPANYWEBSITE = entity.companyWebsite;
                            company.CORPORATEBUSINESSCATEGORY = entity.corporateBusinessCategory;
                            company.REGISTEREDOFFICE = entity.registeredOffice;
                            company.REGISTRATIONNUMBER = entity.registrationNumber;
                            company.PAIDUPCAPITAL = entity.paidUpCapital;
                            company.AUTHORISEDCAPITAL = entity.authorizedCapital;
                            company.SHAREHOLDER_FUND = entity.shareholderFund;
                            company.NUMBEROFEMPLOYEES = entity.numberOfEmployees;
                            company.COUNTRYOFPARENTCOMPANYID = entity.countryOfParentCompanyId;
                            company.COMPANYSTRUCTURE = entity.companyStructure;
                            company.NOOFFEMALEEMPLOYEES = entity.noOfFemaleEmployees;
                            company.ISSTARTUP = entity.isStartUp;
                            company.ISFIRSTTIMECREDIT = entity.isFirstTimeCredit;
                            company.TOTALASSETS = entity.totalAssets;
                            company.CORPORATECUSTOMERTYPEID = entity.corporateCustomerTypeId;
                        }
                        else //If customer main table AccountCreationCompleted equals true then save record in temp table
                        {
                            //Check if customer company information has existing record being modified and approved
                            var existingTempCompany = context.TBL_TEMP_CUSTOMER_COMPANYINFO.FirstOrDefault(x =>
                                x.CUSTOMERID == company.CUSTOMERID && x.ISCURRENT == false &&
                                x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                            TBL_TEMP_CUSTOMER_COMPANYINFO temp = null;

                            if (existingTempCompany != null
                            ) //if customer company information has existing record being modified and approved, update it with the new change
                            {
                                temp = existingTempCompany;
                                temp.ANNUALTURNOVER = entity.annualTurnOver;
                                temp.COMPANYEMAIL = entity.companyEmail;
                                temp.COMPANYNAME = entity.companyName;
                                temp.COMPANYWEBSITE = entity.companyWebsite;
                                temp.CORPORATEBUSINESSCATEGORY = entity.corporateBusinessCategory;
                                temp.REGISTEREDOFFICE = entity.registeredOffice;
                                temp.REGISTRATIONNUMBER = entity.registrationNumber;
                                temp.PAIDUPCAPITAL = entity.paidUpCapital;
                                temp.AUTHORISEDCAPITAL = entity.authorizedCapital;
                                temp.SHAREHOLDER_FUND = entity.shareholderFund;
                                temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                                temp.ISCURRENT = true;
                                temp.NUMBEROFEMPLOYEES = entity.numberOfEmployees;
                                temp.COUNTRYOFPARENTCOMPANYID = entity.countryOfParentCompanyId;
                                temp.COMPANYSTRUCTURE = entity.companyStructure;
                                temp.NOOFFEMALEEMPLOYEES = entity.noOfFemaleEmployees;
                                temp.ISSTARTUP = entity.isStartUp;
                                temp.ISFIRSTTIMECREDIT = entity.isFirstTimeCredit;
                                temp.TOTALASSETS = entity.totalAssets;
                                temp.CORPORATECUSTOMERTYPEID = entity.corporateCustomerTypeId;
                            }
                            else //if customer company information has no existing record being modified and approved, insert new row
                            {
                                temp = new TBL_TEMP_CUSTOMER_COMPANYINFO();
                                temp.CUSTOMERID = entity.customerId;
                                temp.ANNUALTURNOVER = entity.annualTurnOver;
                                temp.COMPANYEMAIL = entity.companyEmail;
                                temp.COMPANYNAME = entity.companyName;
                                temp.COMPANYWEBSITE = entity.companyWebsite;
                                temp.CORPORATEBUSINESSCATEGORY = entity.corporateBusinessCategory;
                                temp.REGISTEREDOFFICE = entity.registeredOffice;
                                temp.REGISTRATIONNUMBER = entity.registrationNumber;
                                temp.PAIDUPCAPITAL = entity.paidUpCapital;
                                temp.AUTHORISEDCAPITAL = entity.authorizedCapital;
                                temp.SHAREHOLDER_FUND = entity.shareholderFund;
                                temp.CREATEDBY = entity.createdBy;
                                temp.DATETIMECREATED = DateTime.Now;
                                temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                                temp.ISCURRENT = true;
                                temp.NUMBEROFEMPLOYEES = entity.numberOfEmployees;
                                temp.COUNTRYOFPARENTCOMPANYID = entity.countryOfParentCompanyId;
                                temp.COMPANYSTRUCTURE = entity.companyStructure;
                                temp.NOOFFEMALEEMPLOYEES = entity.noOfFemaleEmployees;
                                temp.ISSTARTUP = entity.isStartUp;
                                temp.ISFIRSTTIMECREDIT = entity.isFirstTimeCredit;
                                temp.TOTALASSETS = entity.totalAssets;
                                temp.CORPORATECUSTOMERTYPEID = entity.corporateCustomerTypeId;

                                context.TBL_TEMP_CUSTOMER_COMPANYINFO.Add(temp);
                            }

                            //Insert new row to TBL_CUSTOMER_MODIFICATION 
                            var modified = new TBL_CUSTOMER_MODIFICATION
                            {
                                CUSTOMERID = entity.customerId,
                                TARGETID = entity.customerId,
                                MODIFICATIONTYPEID = (int)CustomerInformationTrackerEnum.Corporate_Information,
                                CREATEDBY = entity.createdBy,
                                DATETIMECREATED = DateTime.Now
                            };
                            //Log to the approval workflow 
                            using (var trans = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                    var output = context.SaveChanges() > 0;
                                    var targetId =
                                        modified
                                            .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                    workflow.StaffId = entity.staffId;
                                    workflow.CompanyId = entity.companyId;
                                    workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                    workflow.TargetId = targetId;
                                    workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                    workflow.ExternalInitialization = true;

                                    var returnVal = workflow.LogActivity();

                                    if (returnVal)
                                    {
                                        trans.Commit();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback();
                                    throw new SecureException(ex.Message);
                                }
                            }
                        }
                    }
                    else //If this is a new record insert a new row in TBL_CUSTOMER_COMPANYINFOMATION
                    {
                        company = new TBL_CUSTOMER_COMPANYINFOMATION();
                        company.ANNUALTURNOVER = entity.annualTurnOver;
                        company.COMPANYEMAIL = entity.companyEmail;
                        company.COMPANYNAME = entity.companyName;
                        company.COMPANYWEBSITE = entity.companyWebsite;
                        company.CORPORATEBUSINESSCATEGORY = entity.corporateBusinessCategory;
                        company.CUSTOMERID = entity.customerId;
                        company.REGISTEREDOFFICE = entity.registeredOffice;
                        company.REGISTRATIONNUMBER = entity.registrationNumber;
                        company.PAIDUPCAPITAL = entity.paidUpCapital;
                        company.AUTHORISEDCAPITAL = entity.authorizedCapital;
                        company.SHAREHOLDER_FUND = entity.shareholderFund;
                        company.NUMBEROFEMPLOYEES = entity.numberOfEmployees;
                        company.COUNTRYOFPARENTCOMPANYID = entity.countryOfParentCompanyId;
                        company.COMPANYSTRUCTURE = entity.companyStructure;
                        company.NOOFFEMALEEMPLOYEES = entity.noOfFemaleEmployees;
                        company.ISSTARTUP = entity.isStartUp;
                        company.ISFIRSTTIMECREDIT = entity.isFirstTimeCredit;
                        company.TOTALASSETS = entity.totalAssets;
                        company.CORPORATECUSTOMERTYPEID = entity.corporateCustomerTypeId;
                        context.TBL_CUSTOMER_COMPANYINFOMATION.Add(company);

                    }

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added Customer's Company Information with CustomerId : " + entity.customerId,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }


        private void AddCustomerCompanyInfomation(List<CustomerCompanyInfomationViewModels> entity,
            int status)
        {
            var info = new TBL_CUSTOMER_COMPANYINFOMATION();
            foreach (var ent in entity)
            {
                info.ANNUALTURNOVER = ent.annualTurnOver;
                info.COMPANYEMAIL = ent.companyEmail;
                info.COMPANYNAME = ent.companyName;
                info.COMPANYWEBSITE = ent.companyWebsite;
                info.CORPORATEBUSINESSCATEGORY = ent.corporateBusinessCategory;
                info.CUSTOMERID = customerId;
                info.REGISTEREDOFFICE = ent.registeredOffice;
                info.REGISTRATIONNUMBER = ent.registrationNumber;
                // info.paidUpCapital = ent.PaidUpCapital;
                //info.authorizedCapital = ent.AuthorisedCapital;
                info.NUMBEROFEMPLOYEES = ent.numberOfEmployees;
                info.COUNTRYOFPARENTCOMPANYID = ent.countryOfParentCompanyId;
                info.COMPANYSTRUCTURE = ent.companyStructure;
                info.NOOFFEMALEEMPLOYEES = ent.noOfFemaleEmployees;
                info.ISSTARTUP = ent.isStartUp;
                info.ISFIRSTTIMECREDIT = ent.isFirstTimeCredit;
                info.TOTALASSETS = ent.totalAssets;
                info.CORPORATECUSTOMERTYPEID = ent.corporateCustomerTypeId;
                context.TBL_CUSTOMER_COMPANYINFOMATION.Add(info);

                // Audit Section ---------------------------
                var customer = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == info.CUSTOMERID);
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                    STAFFID = ent.createdBy,
                    BRANCHID = (short)ent.userBranchId,
                    DETAIL = "Added Customer's Customer Info for: " + customer.FIRSTNAME + " " + customer.LASTNAME +
                             " with Id: " + info.CUSTOMERID + " to company" + " (" + info.COMPANYNAME + ") " + " on " +
                             info.COMPANYINFOMATIONID,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = ent.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
            }

        }

        public void UpdateCustomerCompanyInfomation(List<CustomerCompanyInfomationViewModels> entity)
        {

            foreach (var ent in entity)
            {
                if (ent.companyInfomationId != 0 && ent.customerId != 0)
                {
                    var info = context.TBL_CUSTOMER_COMPANYINFOMATION.Find(ent.companyInfomationId);
                    info.ANNUALTURNOVER = ent.annualTurnOver;
                    info.COMPANYEMAIL = ent.companyEmail;
                    info.COMPANYNAME = ent.companyName;
                    info.COMPANYWEBSITE = ent.companyWebsite;
                    info.CORPORATEBUSINESSCATEGORY = ent.corporateBusinessCategory;
                    info.CUSTOMERID = ent.customerId;
                    info.REGISTEREDOFFICE = ent.registeredOffice;
                    info.REGISTRATIONNUMBER = ent.registrationNumber;
                    info.NUMBEROFEMPLOYEES = ent.numberOfEmployees;
                    info.COUNTRYOFPARENTCOMPANYID = ent.countryOfParentCompanyId;
                    info.COMPANYSTRUCTURE = ent.companyStructure;
                    info.NOOFFEMALEEMPLOYEES = ent.noOfFemaleEmployees;
                    info.ISSTARTUP = ent.isStartUp;
                    info.ISFIRSTTIMECREDIT = ent.isFirstTimeCredit;
                    info.TOTALASSETS = ent.totalAssets;
                    info.CORPORATECUSTOMERTYPEID = ent.corporateCustomerTypeId;
                }
            }

        }

        private void AddCustomerIdentification(List<CustomerIdentificationViewModels> entity)
        {
            var identity = new TBL_CUSTOMER_IDENTIFICATION();
            foreach (var ent in entity)
            {
                identity.CUSTOMERID = customerId;
                identity.IDENTIFICATIONMODEID = ent.identificationModeId;
                identity.IDENTIFICATIONNO = ent.identificationNo;
                identity.ISSUEAUTHORITY = ent.issueAuthority;
                identity.ISSUEPLACE = ent.issuePlace;
                context.TBL_CUSTOMER_IDENTIFICATION.Add(identity);
            }

        }

        public bool AddCustomerCompanyDirector(CustomerCompanyDirectorsViewModels entity)
        {
            if (entity != null)
            {
                if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.BoardMember)
                {
                    var promoterExistsForCompany = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(p => p.CUSTOMERID == entity.customerId && p.ISTHEPROMOTER == true).ToList();
                    if (promoterExistsForCompany.Count() == 0 && !entity.isThePromoter)
                    {
                        throw new SecureException("A Director must be profiled as a promoter for this company!");
                    }
                    if (promoterExistsForCompany.Count() == 1 && !entity.isThePromoter)
                    {
                        throw new SecureException("Only this Director is profiled as a promoter for this company");
                    }
                }
                try
                {
                    TBL_CUSTOMER_COMPANY_DIRECTOR directors;
                    directors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Find(entity.companyDirectorId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;


                    List<TBL_CUSTOMER_COMPANY_BENEFICIA> beneficialList = new List<TBL_CUSTOMER_COMPANY_BENEFICIA>();
                    List<TBL_TEMP_CUSTOMER_COMP_BENEFIC> tempBeneficialList = new List<TBL_TEMP_CUSTOMER_COMP_BENEFIC>();

                    if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Shareholder)
                    {

                        if (entity.customerCompanyBeneficial != null)
                        {
                            if (accountCompleted == false)
                            {
                                foreach (var item in entity.customerCompanyBeneficial)
                                {
                                    var beneficial = new TBL_CUSTOMER_COMPANY_BENEFICIA()
                                    {
                                        SURNAME = item.surname,
                                        FIRSTNAME = item.firstname,
                                        MIDDLENAME = item.middlename,
                                        CUSTOMERNIN = item.customerNIN,
                                        CUSTOMERBVN = item.bankVerificationNumber,
                                        NUMBEROFSHARES = item.numberOfShares,
                                        ISPOLITICALLYEXPOSED = item.isPoliticallyExposed,
                                        ADDRESS = item.address,
                                        PHONENUMBER = item.phoneNumber,
                                        EMAILADDRESS = item.email,
                                        CREATEDBY = entity.createdBy,
                                        DATECREATED = DateTime.Now,
                                        DELETED = false
                                        //ISTHEPROMOTER = entity.isThePromoter
                                        
                                    };
                                    beneficialList.Add(beneficial);
                                }
                            }
                            else
                            {
                                foreach (var item in entity.customerCompanyBeneficial)
                                {
                                    var tempBeneficial = new TBL_TEMP_CUSTOMER_COMP_BENEFIC()
                                    {
                                        COMPANY_BENEFICIARYID = item.companyBeneficiaryId,
                                        SURNAME = item.surname,
                                        FIRSTNAME = item.firstname,
                                        MIDDLENAME = item.middlename,
                                        CUSTOMERNIN = item.customerNIN,
                                        CUSTOMERBVN = item.bankVerificationNumber,
                                        NUMBEROFSHARES = item.numberOfShares,
                                        ISPOLITICALLYEXPOSED = item.isPoliticallyExposed,
                                        ADDRESS = item.address,
                                        PHONENUMBER = item.phoneNumber,
                                        EMAILADDRESS = item.email,
                                        CREATEDBY = entity.createdBy,
                                        DATECREATED = DateTime.Now,
                                        DELETED = false,
                                        ISCURRENT = true,
                                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                                        //ISTHEPROMOTER = item.isThePromoter
                                    };
                                    tempBeneficialList.Add(tempBeneficial);
                                }
                            }

                        }

                    }
                    else
                    {
                        entity.customerTypeId = (int)CustomerTypeEnum.Individual;
                    }
                    var CustomerRec = context.TBL_CUSTOMER.Find(entity.customerId);
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (directors != null && accountCompleted == false)
                    {
                        directors.CUSTOMERID = entity.customerId;
                        directors.SURNAME = entity.surname;
                        directors.FIRSTNAME = entity.firstname;
                        directors.MIDDLENAME = entity.middlename;
                        directors.CUSTOMERNIN = entity.customerNIN;
                        directors.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                        directors.CUSTOMERBVN = entity.bankVerificationNumber;
                        directors.SHAREHOLDINGPERCENTAGE = entity.numberOfShares;
                        directors.REGISTRATION_NUMBER = entity.rcNumber;
                        directors.TAX_NUMBER = entity.taxNumber;
                        directors.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                        directors.GENDER = entity.gender;
                        directors.MARITALSTATUSID = entity.maritalStatusId;
                        directors.DATEOFBIRTH = entity.dateOfBirth;
                        directors.ISTHEPROMOTER = entity.isThePromoter;
                        if (entity.isPoliticallyExposed == true)
                        {
                            if (CustomerRec.ISPOLITICALLYEXPOSED == false)
                            {
                                CustomerRec.ISPOLITICALLYEXPOSED = true;
                            }
                        }


                        directors.ADDRESS = entity.address;
                        directors.PHONENUMBER = entity.phoneNumber;
                        directors.EMAILADDRESS = entity.email;
                        directors.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                    }
                    else if (directors == null && accountCompleted == false)
                    {
                        directors = new TBL_CUSTOMER_COMPANY_DIRECTOR();

                        directors.CUSTOMERID = entity.customerId;
                        directors.SURNAME = entity.surname;
                        directors.FIRSTNAME = entity.firstname;
                        directors.MIDDLENAME = entity.middlename;
                        directors.CUSTOMERNIN = entity.customerNIN;
                        directors.CUSTOMERTYPEID = entity.customerTypeId;
                        directors.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                        directors.CUSTOMERBVN = entity.bankVerificationNumber;
                        directors.REGISTRATION_NUMBER = entity.rcNumber;
                        directors.TAX_NUMBER = entity.taxNumber;
                        directors.SHAREHOLDINGPERCENTAGE = entity.numberOfShares;
                        directors.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                        directors.GENDER = entity.gender;
                        directors.MARITALSTATUSID = entity.maritalStatusId;
                        directors.DATEOFBIRTH = entity.dateOfBirth;
                        directors.ISTHEPROMOTER = entity.isThePromoter;

                        if (entity.isPoliticallyExposed == true)
                        {
                            if (CustomerRec.ISPOLITICALLYEXPOSED == false)
                            {
                                CustomerRec.ISPOLITICALLYEXPOSED = true;
                            }
                        }
                        directors.ADDRESS = entity.address;
                        directors.PHONENUMBER = entity.phoneNumber;
                        directors.EMAILADDRESS = entity.email;
                        directors.CREATEDBY = entity.createdBy;
                        directors.DATECREATED = DateTime.Now;
                        directors.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                        context.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(directors);
                        directors.ISTHEPROMOTER = entity.isThePromoter;
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer company director information has existing record being modified and approved
                        var existingTempDirector = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x =>
                            x.COMPANYDIRECTORID == entity.companyDirectorId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.companyDirectorId != 0 || entity.companyDirectorId > 0)
                        {
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Shareholder)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Shareholder_Modification;
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Account_Signatory)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Signatory_Modification;
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.BoardMember)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Director_Modification;
                        }
                        else
                        {
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Shareholder)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Shareholder_Addition;
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Account_Signatory)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Signatory_Adition;
                            if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.BoardMember)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Director_Addition;
                        }

                        TBL_TEMP_CUSTOMER_DIRECTOR temp = null;
                        if (existingTempDirector != null && entity.companyDirectorId > 0
                        ) //if customer phone contact information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempDirector;
                            temp.CUSTOMERID = entity.customerId;
                            temp.SURNAME = entity.surname;
                            temp.FIRSTNAME = entity.firstname;
                            temp.MIDDLENAME = entity.middlename;
                            temp.CUSTOMERNIN = entity.customerNIN;
                            temp.CUSTOMERTYPEID = entity.customerTypeId;
                            temp.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                            temp.CUSTOMERBVN = entity.bankVerificationNumber;
                            temp.REGISTRATION_NUMBER = entity.rcNumber;
                            temp.TAX_NUMBER = entity.taxNumber;
                            temp.SHAREHOLDINGPERCENTAGE = entity.numberOfShares;
                            temp.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                            temp.ADDRESS = entity.address;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.EMAILADDRESS = entity.email;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATECREATED = DateTime.Now;
                            temp.GENDER = entity.gender;
                            //temp.MARITALSTATUSID = entity.maritalStatusId;
                            temp.DATEOFBIRTH = entity.dateOfBirth;
                            // temp.TBL_TEMP_COMPANY_BENEFICIA = beneficialList;

                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.ISTHEPROMOTER = entity.isThePromoter;
                        }
                        else //if customer phoneContact information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMER_DIRECTOR();

                            temp.CUSTOMERID = entity.customerId;
                            temp.COMPANYDIRECTORID = entity.companyDirectorId;
                            temp.SURNAME = entity.surname;
                            temp.FIRSTNAME = entity.firstname;
                            temp.MIDDLENAME = entity.middlename;
                            temp.CUSTOMERNIN = entity.customerNIN;
                            temp.CUSTOMERTYPEID = entity.customerTypeId;
                            temp.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                            temp.CUSTOMERBVN = entity.bankVerificationNumber;
                            temp.REGISTRATION_NUMBER = entity.rcNumber;
                            temp.TAX_NUMBER = entity.taxNumber;
                            temp.SHAREHOLDINGPERCENTAGE = entity.numberOfShares;
                            temp.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                            temp.ADDRESS = entity.address;
                            temp.PHONENUMBER = entity.phoneNumber;
                            temp.EMAILADDRESS = entity.email;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.ISTHEPROMOTER = entity.isThePromoter;
                            temp.GENDER = entity.gender;
                            //temp.MARITALSTATUSID = entity.maritalStatusId;
                            temp.DATEOFBIRTH = entity.dateOfBirth;
                            context.TBL_TEMP_CUSTOMER_DIRECTOR.Add(temp);

                        }

                        //Insert new row to TBL_CUSTOMER_MODIFICATION 
                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPCOMPANYDIRECTORID;
                                if (tempBeneficialList.Count > 0)
                                {
                                    foreach (var item in tempBeneficialList)
                                    {
                                        item.TEMPCOMPANYDIRECTORID = temp.TEMPCOMPANYDIRECTORID;
                                    }
                                    context.TBL_TEMP_CUSTOMER_COMP_BENEFIC.AddRange(tempBeneficialList);
                                }
                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Processing;     //Formerly Pending (int)ApprovalStatusEnum.Pending; 
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL =
                            "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    //  var response = context.SaveChanges() != 0;
                    try
                    {
                        return context.SaveChanges() != 0;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }

                    // return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerIdentification(CustomerIdentificationViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_IDENTIFICATION identity;
                    if (entity.identificationId != 0 || entity.identificationId < 0)
                    {
                        identity = context.TBL_CUSTOMER_IDENTIFICATION.Find(entity.identificationId);
                        if (identity != null)
                        {
                            identity.CUSTOMERID = entity.customerId;
                            identity.IDENTIFICATIONMODEID = entity.identificationModeId;
                            identity.IDENTIFICATIONNO = entity.identificationNo;
                            identity.ISSUEAUTHORITY = entity.issueAuthority;
                            identity.ISSUEPLACE = entity.issuePlace;
                        }
                    }
                    else
                    {
                        identity = new TBL_CUSTOMER_IDENTIFICATION();
                        identity.CUSTOMERID = entity.customerId;
                        identity.IDENTIFICATIONMODEID = entity.identificationModeId;
                        identity.IDENTIFICATIONNO = entity.identificationNo;
                        identity.ISSUEAUTHORITY = entity.issueAuthority;
                        identity.ISSUEPLACE = entity.issuePlace;

                        context.TBL_CUSTOMER_IDENTIFICATION.Add(identity);
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL =
                            "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        private void AddCustomerEmploymentHistory(List<CustomerEmploymentHistoryViewModels> entity)
        {
            var history = new TBL_CUSTOMER_EMPLOYMENTHISTORY();
            foreach (var ent in entity)
            {
                history.ACTIVE = ent.active;
                history.CUSTOMERID = customerId;
                history.EMPLOYDATE = ent.employDate;
                history.EMPLOYERADDRESS = ent.employerAddress;
                history.EMPLOYERCOUNTRYID = ent.employerCountryId;
                history.EMPLOYERSTATEID = ent.employerStateId;
                history.EMPLOYERNAME = ent.employerName;
                history.OFFICEPHONE = ent.officePhone;
                history.PREVIOUSEMPLOYER = ent.previousEmployer;
                history.YEAROFEMPLOYMENT = ent.yearOfEmployment;
                history.TOTALWORKINGEXPERIENCE = ent.totalWorkingExperience;
                history.YEARSOFCURRENTEMPLOYMENT = ent.yearsOfCurrentEmployment;
                history.TERMINALBENEFITS = ent.terminalBenefits;
                history.ANNUALINCOME = ent.annualIncome;
                history.MONTHLYINCOME = ent.monthlyIncome;
                history.EXPENDITURE = ent.expenditure;
                context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Add(history);
            }
        }

        public bool AddCustomerClientSupplier(CustomerClientOrSupplierViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_CLIENT_SUPPLIER clientSupplier;

                    clientSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Find(entity.client_SupplierId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (clientSupplier != null && accountCompleted == false)
                    {
                        clientSupplier.CUSTOMERID = entity.customerId;
                        clientSupplier.CUSTOMERTYPEID = entity.customerTypeId;
                        clientSupplier.FIRSTNAME = entity.firstName;
                        clientSupplier.MIDDLENAME = entity.middleName;
                        clientSupplier.LASTNAME = entity.lastName;
                        clientSupplier.TAX_NUMBER = entity.taxNumber;
                        clientSupplier.REGISTRATION_NUMBER = entity.rcNumber;
                        clientSupplier.HAS_CASA_ACCOUNT = entity.hasCASAAccount;
                        clientSupplier.CASA_ACCOUNTNO = entity.casaAccountNumber;
                        clientSupplier.BANKNAME = entity.bankName;
                        clientSupplier.NATURE_OF_BUSINESS = entity.natureOfBusiness;
                        clientSupplier.CONTACT_PERSON = entity.contactPerson;
                        clientSupplier.ADDRESS = entity.client_SupplierAddress;
                        clientSupplier.PHONENUMBER = entity.client_SupplierPhoneNumber;
                        clientSupplier.EMAILADDRESS = entity.client_SupplierEmail;
                        clientSupplier.CLIENT_SUPPLIERTYPEID = entity.client_SupplierTypeId;
                        clientSupplier.CREATEDBY = entity.createdBy;
                    }
                    else if (clientSupplier == null && accountCompleted == false)
                    {
                        clientSupplier = new TBL_CUSTOMER_CLIENT_SUPPLIER();

                        clientSupplier.CUSTOMERID = entity.customerId;
                        clientSupplier.CUSTOMERTYPEID = entity.customerTypeId;
                        clientSupplier.FIRSTNAME = entity.firstName;
                        clientSupplier.MIDDLENAME = entity.middleName;
                        clientSupplier.LASTNAME = entity.lastName;
                        clientSupplier.ADDRESS = entity.client_SupplierAddress;
                        clientSupplier.PHONENUMBER = entity.client_SupplierPhoneNumber;
                        clientSupplier.EMAILADDRESS = entity.client_SupplierEmail;
                        clientSupplier.TAX_NUMBER = entity.taxNumber;
                        clientSupplier.REGISTRATION_NUMBER = entity.rcNumber;
                        clientSupplier.BANKNAME = entity.bankName;
                        clientSupplier.HAS_CASA_ACCOUNT = entity.hasCASAAccount;
                        clientSupplier.CASA_ACCOUNTNO = entity.casaAccountNumber;
                        clientSupplier.NATURE_OF_BUSINESS = entity.natureOfBusiness;
                        clientSupplier.CONTACT_PERSON = entity.contactPerson;
                        clientSupplier.CLIENT_SUPPLIERTYPEID = entity.client_SupplierTypeId;
                        clientSupplier.CREATEDBY = entity.createdBy;
                        clientSupplier.DATECREATED = DateTime.Now;
                        context.TBL_CUSTOMER_CLIENT_SUPPLIER.Add(clientSupplier);
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer client supplier information has existing record being modified and approved
                        var existingTempCliSup = context.TBL_TEMP_CUST_CLIENT_SUPPLIER.FirstOrDefault(x =>
                            x.CLIENT_SUPPLIERID == entity.client_SupplierId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.client_SupplierId != 0 || entity.client_SupplierId > 0)
                        {
                            if (entity.client_SupplierTypeId == (int)CompanyClientOrSupplierTypeEnum.Client)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Client_Modification;
                            else
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Suplier_Modification;
                        }
                        else
                        {
                            if (entity.client_SupplierTypeId == (int)CompanyClientOrSupplierTypeEnum.Client)
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Client_Addition;
                            else
                                modificationTypeId = (int)CustomerInformationTrackerEnum.Supplier_Addition;
                        }

                        TBL_TEMP_CUST_CLIENT_SUPPLIER temp = null;
                        if (existingTempCliSup != null && entity.client_SupplierId > 0
                        ) //if customer phone contact information has existing record being modified and approved, update it with the new change
                        {

                            temp = existingTempCliSup;
                            temp.CUSTOMERID = entity.customerId;
                            temp.CUSTOMERTYPEID = entity.customerTypeId;
                            temp.FIRSTNAME = entity.firstName;
                            temp.MIDDLENAME = entity.middleName;
                            temp.LASTNAME = entity.lastName;
                            temp.ADDRESS = entity.client_SupplierAddress;
                            temp.PHONENUMBER = entity.client_SupplierPhoneNumber;
                            temp.EMAILADDRESS = entity.client_SupplierEmail;
                            temp.TAX_NUMBER = entity.taxNumber;
                            temp.REGISTRATION_NUMBER = entity.rcNumber;
                            temp.BANKNAME = entity.bankName;
                            temp.HAS_CASA_ACCOUNT = entity.hasCASAAccount;
                            temp.CASA_ACCOUNTNO = entity.casaAccountNumber;
                            temp.NATURE_OF_BUSINESS = entity.natureOfBusiness;
                            temp.CONTACT_PERSON = entity.contactPerson;
                            temp.CLIENT_SUPPLIERTYPEID = entity.client_SupplierTypeId;
                            clientSupplier.CREATEDBY = entity.createdBy;
                            temp.DATECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPCLIENT_SUPPLIERID;
                        }
                        else //if customer phoneContact information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUST_CLIENT_SUPPLIER();
                            temp.CUSTOMERID = entity.customerId;
                            temp.CLIENT_SUPPLIERID = entity.client_SupplierId;
                            temp.CUSTOMERTYPEID = entity.customerTypeId;
                            temp.FIRSTNAME = entity.firstName;
                            temp.MIDDLENAME = entity.middleName;
                            temp.LASTNAME = entity.lastName;
                            temp.ADDRESS = entity.client_SupplierAddress;
                            temp.PHONENUMBER = entity.client_SupplierPhoneNumber;
                            temp.EMAILADDRESS = entity.client_SupplierEmail;
                            temp.TAX_NUMBER = entity.taxNumber;
                            temp.REGISTRATION_NUMBER = entity.rcNumber;
                            temp.BANKNAME = entity.bankName;
                            temp.HAS_CASA_ACCOUNT = entity.hasCASAAccount;
                            temp.CASA_ACCOUNTNO = entity.casaAccountNumber;
                            temp.NATURE_OF_BUSINESS = entity.natureOfBusiness;
                            temp.CONTACT_PERSON = entity.contactPerson;
                            temp.CLIENT_SUPPLIERTYPEID = entity.client_SupplierTypeId;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUST_CLIENT_SUPPLIER.Add(temp);
                        }

                        // modifiedTargetId = entity.client_SupplierId;
                        //Insert new row to TBL_CUSTOMER_MODIFICATION 
                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPCLIENT_SUPPLIERID;

                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_CLIENT_SUPPLIER for customer ID: + (" + entity.customerId +
                                 ") ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.auditTrail.AddAuditTrail(audit);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerEmploymentHistory(CustomerEmploymentHistoryViewModels entity)
        {
            if (entity != null)
            {
                var auditDetail = string.Empty;
                short auditType = 0;
                try
                {
                    TBL_CUSTOMER_EMPLOYMENTHISTORY history;

                    history = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Find(entity.placeOfWorkId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (history != null && accountCompleted == false)
                    {
                        history.ACTIVE = entity.active;
                        history.CUSTOMERID = entity.customerId;
                        history.EMPLOYDATE = entity.employDate;
                        history.EMPLOYERADDRESS = entity.employerAddress;
                        history.EMPLOYERCOUNTRYID = entity.employerCountryId;
                        if(entity.employerStateId > 0)
                        {
                            history.EMPLOYERSTATEID = entity.employerStateId;
                            history.EMPLOYERSTATE = context.TBL_STATE.Where(x=>x.STATEID == entity.employerStateId).Select(m=>m.STATENAME).FirstOrDefault();
                        }
                        else
                        {
                            history.EMPLOYERSTATE = entity.employerState;
                        }
                        history.EMPLOYERNAME = entity.employerName;
                        history.OFFICEPHONE = entity.officePhone;
                        history.PREVIOUSEMPLOYER = entity.previousEmployer;
                        history.ISEMPLOYERRELATED = entity.isEmployerRelated;
                        history.APPROVEDEMPLOYERID = entity.approvedEmployerId;

                        auditDetail = "Updated Customer Employment History for customer ID: + (" + entity.customerId + ") ";
                        auditType = (short)AuditTypeEnum.CustomerDetailUpdated;
                    }
                    else if (history == null && accountCompleted == false)
                    {
                        history = new TBL_CUSTOMER_EMPLOYMENTHISTORY();

                        history.ACTIVE = entity.active;
                        history.CUSTOMERID = entity.customerId;
                        history.EMPLOYDATE = entity.employDate;
                        history.EMPLOYERADDRESS = entity.employerAddress;
                        history.EMPLOYERCOUNTRYID = entity.employerCountryId;
                        if (entity.employerStateId > 0)
                        {
                            history.EMPLOYERSTATEID = entity.employerStateId;
                            history.EMPLOYERSTATE = context.TBL_STATE.Where(x => x.STATEID == entity.employerStateId).Select(m => m.STATENAME).FirstOrDefault();
                        }
                        else
                        {
                            history.EMPLOYERSTATE = entity.employerState;
                        }
                        history.EMPLOYERNAME = entity.employerName;
                        history.OFFICEPHONE = entity.officePhone;
                        history.PREVIOUSEMPLOYER = entity.previousEmployer;
                        history.YEAROFEMPLOYMENT = entity.yearOfEmployment;
                        history.TOTALWORKINGEXPERIENCE = entity.totalWorkingExperience;
                        history.YEARSOFCURRENTEMPLOYMENT = entity.yearsOfCurrentEmployment;
                        history.TERMINALBENEFITS = entity.terminalBenefits;
                        history.ANNUALINCOME = entity.annualIncome;
                        history.MONTHLYINCOME = entity.monthlyIncome;
                        history.EXPENDITURE = entity.expenditure;
                        history.ISEMPLOYERRELATED = entity.isEmployerRelated;
                        history.APPROVEDEMPLOYERID = entity.approvedEmployerId;
                        context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Add(history);
                        auditDetail = "Added Customer Employment History for customer ID: + (" + entity.customerId + ") ";
                        auditType = (short)AuditTypeEnum.CustomerDetailAdded;

                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer employment information has existing record being modified and approved
                        var existingTempAddress = context.TBL_TEMP_CUSTOMEREMPLOYMENT.FirstOrDefault(x =>
                            x.PLACEOFWORKID == entity.placeOfWorkId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.placeOfWorkId != 0 || entity.placeOfWorkId > 0)
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Employment_History_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int)CustomerInformationTrackerEnum.Employement_History_Addition;
                        }

                        TBL_TEMP_CUSTOMEREMPLOYMENT temp = null;
                        if (existingTempAddress != null && entity.placeOfWorkId > 0
                        ) //if customer employment information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempAddress;

                            temp.ACTIVE = entity.active;
                            temp.CUSTOMERID = entity.customerId;
                            temp.EMPLOYDATE = entity.employDate;
                            temp.EMPLOYERADDRESS = entity.employerAddress;
                            temp.EMPLOYERCOUNTRYID = entity.employerCountryId;
                            if (entity.employerStateId > 0)
                            {
                                temp.EMPLOYERSTATEID = entity.employerStateId;
                                temp.EMPLOYERSTATE = context.TBL_STATE.Where(x => x.STATEID == entity.employerStateId).Select(m => m.STATENAME).FirstOrDefault();
                            }
                            else
                            {
                                temp.EMPLOYERSTATE = entity.employerState;
                            }
                            temp.EMPLOYERNAME = entity.employerName;
                            temp.OFFICEPHONE = entity.officePhone;
                            temp.PREVIOUSEMPLOYER = entity.previousEmployer;
                            temp.ISEMPLOYERRELATED = entity.isEmployerRelated;
                            temp.APPROVEDEMPLOYERID = entity.approvedEmployerId;
                            existingTempAddress.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            existingTempAddress.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPPLACEOFWORKID;

                            auditDetail = "Added Customer Employment History for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerDetailAdded;
                        }
                        else //if customer employment information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMEREMPLOYMENT();
                            temp.ACTIVE = entity.active;
                            temp.CUSTOMERID = entity.customerId;
                            temp.EMPLOYDATE = entity.employDate;
                            temp.EMPLOYERADDRESS = entity.employerAddress;
                            temp.EMPLOYERCOUNTRYID = entity.employerCountryId;
                            if (entity.employerStateId > 0)
                            {
                                temp.EMPLOYERSTATEID = entity.employerStateId;
                                temp.EMPLOYERSTATE = context.TBL_STATE.Where(x => x.STATEID == entity.employerStateId).Select(m => m.STATENAME).FirstOrDefault();
                            }
                            else
                            {
                                temp.EMPLOYERSTATE = entity.employerState;
                            }
                            temp.EMPLOYERNAME = entity.employerName;
                            temp.OFFICEPHONE = entity.officePhone;
                            temp.PREVIOUSEMPLOYER = entity.previousEmployer;
                            temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            temp.YEAROFEMPLOYMENT = entity.yearOfEmployment;
                            temp.TOTALWORKINGEXPERIENCE = entity.totalWorkingExperience;
                            temp.YEARSOFCURRENTEMPLOYMENT = entity.yearsOfCurrentEmployment;
                            temp.TERMINALBENEFITS = entity.terminalBenefits;
                            temp.ANNUALINCOME = entity.annualIncome;
                            temp.MONTHLYINCOME = entity.monthlyIncome;
                            temp.EXPENDITURE = entity.expenditure;
                            temp.PLACEOFWORKID = entity.placeOfWorkId;
                            temp.ISEMPLOYERRELATED = entity.isEmployerRelated;
                            temp.APPROVEDEMPLOYERID = entity.approvedEmployerId;
                            context.TBL_TEMP_CUSTOMEREMPLOYMENT.Add(temp);

                            auditDetail = "Added Customer Employment History for customer ID: + (" + entity.customerId + ") ";
                            auditType = (short)AuditTypeEnum.CustomerDetailAdded;
                        }

                        // modifiedTargetId = entity.placeOfWorkId;

                        //Insert new row to TBL_CUSTOMER_MODIFICATION 
                        var modified = new TBL_CUSTOMER_MODIFICATION
                        {
                            CUSTOMERID = entity.customerId,
                            TARGETID = modifiedTargetId,
                            MODIFICATIONTYPEID = modificationTypeId,
                            CREATEDBY = entity.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };
                        //Log to the approval workflow 
                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var res = context.SaveChanges() > 0;
                                modified.TARGETID = temp.TEMPPLACEOFWORKID;

                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                                workflow.ExternalInitialization = true;

                                var returnVal = workflow.LogActivity();

                                if (returnVal)
                                {
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }

                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = auditType,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }

            }

            return false;
        }

        public bool DeleteChild(int childId, UserInfo user)
        {
            var child = context.TBL_CUSTOMER_CHILDREN.Find(childId);

            if (child != null)
            {
                context.TBL_CUSTOMER_CHILDREN.Remove(child);

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Child: " + child.CHILDNAME + " of customer with  Code : " + child.TBL_CUSTOMER.CUSTOMERCODE,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }

            return false;
        }

        


        public async Task<bool> DeleteCustomer(int customerId, UserInfo user)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);

            if (customer != null)
            {
                customer.DATETIMEDELETED = DateTime.Now;
                customer.DELETED = true;
                customer.DELETEDBY = user.staffId;


                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer: " + customer.LASTNAME + " with code: " + customer.CUSTOMERCODE,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
            }
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        public bool DeleteUltimateBeneficial(int companyBeneficialId, UserInfo user)
        {
            var ultimate = context.TBL_CUSTOMER_COMPANY_BENEFICIA.Find(companyBeneficialId);

            if (ultimate != null)
            {
                ultimate.DATETIMEDELETED = DateTime.Now;
                ultimate.DELETED = true;
                ultimate.DELETEDBY = user.staffId;
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Company ultimate beneficial: " + ultimate.SURNAME + " " + ultimate.SURNAME +" for: " + ultimate.TBL_CUSTOMER_COMPANY_DIRECTOR.FIRSTNAME,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                auditTrail.AddAuditTrail(audit);
            }
            //end of Audit section -------------------------------

            return  context.SaveChanges() != 0;
        }

        public CustomerViewModels GetCustomer(int custormerId)
        {
            var data = from a in context.TBL_CUSTOMER
                       join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                       where a.DELETED == false
                       select new CustomerViewModels
                       {
                           crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                           crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                           crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                           accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                           branchId = a.BRANCHID,
                           branchName = a.TBL_BRANCH.BRANCHNAME,
                           companyMainId = a.COMPANYID,
                           createdBy = a.CREATEDBY,
                           creationMailSent = a.CREATIONMAILSENT,
                           customerCode = a.CUSTOMERCODE,
                           customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                           customerTypeId = (short)a.CUSTOMERTYPEID,
                           dateOfBirth = (DateTime)a.DATEOFBIRTH,
                           customerId = a.CUSTOMERID,
                           emailAddress = a.EMAILADDRESS,
                           firstName = a.FIRSTNAME,
                           gender = a.GENDER,
                           lastName = a.LASTNAME,
                           maidenName = a.MAIDENNAME,
                           maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                           title = a.TITLE,
                           middleName = a.MIDDLENAME,
                           misCode = a.MISCODE,
                           misStaff = a.MISSTAFF,
                           nationalityId = a.NATIONALITYID,
                           occupation = a.OCCUPATION,
                           placeOfBirth = a.PLACEOFBIRTH,
                           isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                           relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                           relationshipOfficerName = st.FIRSTNAME + " " + st.LASTNAME,
                           spouse = a.SPOUSE,
                           sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                           subSectorId = (short)a.SUBSECTORID,
                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                           taxNumber = a.TAXNUMBER,
                           riskRatingId = a.RISKRATINGID,
                           ownership =a.OWNERSHIP,
                           //   riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                           customerBVN = a.CUSTOMERBVN,
                           isProspect = a.ISPROSPECT
                       };

            var cust = data.FirstOrDefault();
            if (cust != null)
            {
                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    cust.isPoliticallyExposed = finacle.GetExposePersonStatus(cust.customerCode);
                }


                //cust.CustomerCompanyShareholder
                //cust.CustomerCompanyInfomation
                cust.CustomerPhoneContact = GetSingleCustomerPhoneContactInfo(cust.customerId).ToList();
                cust.CustomerAddresses = GetSingleCustomerAddressInfo(cust.customerId).ToList();
                // cust.
                //cust.CustomerClientOrSupplier
                //cust.CustomerEmploymentHistory
                //cust.CustomerSupplier
                //cust.
                cust.CustomerChildren = GetSingleCustomerChildrenInfo(cust.customerId).ToList();
                cust.CustomerCompanyDirectors =
                    GetSingleCustomerDirectorInfo(cust.customerId, (int)CompanyDirectorTypeEnum.BoardMember).ToList();
                // cust.CustomerCompanyAccountSignatory = GetSingleCustomerDirectorInfo(cust.customerId, (int)CompanyDirectorTypeEnum.Account_Signatory).ToList();
                // cust.CustomerCompanyShareholder = GetSingleCustomerShareholderInfo(cust.customerId, (int)CustomerTypeEnum.Corporate).ToList();
            }


            return cust;
        }

        IQueryable<CustomerViewModels> GetCustomers()
        {
            return from a in context.TBL_CUSTOMER
                   join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                   where a.DELETED == false
                   select

                       new CustomerViewModels
                       {
                           crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                           crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                           crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                           accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                           branchId = a.BRANCHID,
                           branchName = a.TBL_BRANCH.BRANCHNAME,
                           companyMainId = a.COMPANYID,
                           createdBy = a.CREATEDBY,
                           creationMailSent = a.CREATIONMAILSENT,
                           customerCode = a.CUSTOMERCODE,
                           customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                           customerTypeId = (short)a.CUSTOMERTYPEID,
                           dateOfBirth = (DateTime)a.DATEOFBIRTH,
                           customerId = a.CUSTOMERID,
                           emailAddress = a.EMAILADDRESS,
                           firstName = a.FIRSTNAME,
                           gender = a.GENDER,
                           lastName = a.LASTNAME,
                           maidenName = a.MAIDENNAME,
                           maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                           title = a.TITLE,
                           middleName = a.MIDDLENAME,
                           misCode = a.MISCODE,
                           misStaff = a.MISSTAFF,
                           nationalityId = a.NATIONALITYID,
                           occupation = a.OCCUPATION,
                           placeOfBirth = a.PLACEOFBIRTH,
                           isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                           relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                           relationshipOfficerName = st.FIRSTNAME + " " + st.LASTNAME,
                           spouse = a.SPOUSE,
                           sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                           subSectorId = (short)a.SUBSECTORID,
                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                           taxNumber = a.TAXNUMBER,
                           riskRatingId = a.RISKRATINGID,
                           ownership = a.OWNERSHIP,
                           customerBVN = a.CUSTOMERBVN,


                           //   riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                           //customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                           // customerTypeName =  context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                           //CustomerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == a.CUSTOMERID).Select(x => new CustomerAddressViewModels()
                           //{
                           //    address = x.ADDRESS,
                           //    addressTypeId = x.ADDRESSTYPEID,
                           //    cityId = x.CITYID,
                           //    customerId = x.CUSTOMERID,
                           //    homeTown = x.HOMETOWN,
                           //    nearestLandmark = x.NEARESTLANDMARK,
                           //    electricMeterNumber = x.ELECTRICMETERNUMBER,
                           //    pobox = x.POBOX,
                           //    stateId = x.STATEID,
                           //    addressId = x.ADDRESSID
                           //}).ToList(),
                           //CustomerPhoneContact = context.TBL_CUSTOMER_PHONECONTACT.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => new CustomerPhoneContactViewModels
                           //{
                           //    active = c.ACTIVE,
                           //    customerId = c.CUSTOMERID,
                           //    phone = c.PHONE,
                           //    phoneContactId = c.PHONECONTACTID,
                           //    phoneNumber = c.PHONENUMBER
                           //}).ToList(),
                           //CustomerCompanyInfomation = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(d => d.CUSTOMERID == a.CUSTOMERID).Select(d => new CustomerCompanyInfomationViewModels()
                           //{
                           //    annualTurnOver = d.ANNUALTURNOVER,
                           //    companyEmail = d.COMPANYEMAIL,
                           //    companyId = d.CUSTOMERID,
                           //    companyName = d.COMPANYNAME,
                           //    companyWebsite = d.COMPANYWEBSITE,
                           //    companyInfomationId = d.COMPANYINFOMATIONID,
                           //    corporateBusinessCategory = d.CORPORATEBUSINESSCATEGORY,
                           //    createdBy = a.CREATEDBY,
                           //    registeredOffice = d.REGISTEREDOFFICE,
                           //    registrationNumber = d.REGISTRATIONNUMBER,
                           //    paidUpCapital = d.PAIDUPCAPITAL,
                           //    authorizedCapital = d.AUTHORISEDCAPITAL,
                           //    shareholderFund = d.SHAREHOLDER_FUND
                           //}).ToList(),
                           //CustomerEmploymentHistory = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(s => s.CUSTOMERID == a.CUSTOMERID).Select(s => new CustomerEmploymentHistoryViewModels()
                           //{
                           //    active = s.ACTIVE,
                           //    previousEmployer = s.PREVIOUSEMPLOYER,
                           //    customerId = s.CUSTOMERID,
                           //    employDate = s.EMPLOYDATE,
                           //    placeOfWorkId = s.PLACEOFWORKID,
                           //    employerAddress = s.EMPLOYERADDRESS,
                           //    employerCountryId = s.EMPLOYERCOUNTRYID,
                           //    employerName = s.EMPLOYERNAME,
                           //    officePhone = s.OFFICEPHONE,
                           //    employerStateId = s.EMPLOYERSTATEID
                           //}).ToList(),
                           //CustomerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                           //.Select(s => new CustomerCompanyDirectorsViewModels()
                           //{
                           //    companyDirectorId = s.COMPANYDIRECTORID,
                           //    surname = s.SURNAME,
                           //    firstname = s.FIRSTNAME,
                           //    middlename = s.MIDDLENAME,
                           //    customerNIN = s.CUSTOMERNIN,
                           //    numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                           //    isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           //    bankVerificationNumber = s.CUSTOMERBVN,
                           //    companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           //    companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           //    customerId = s.CUSTOMERID,
                           //    customerName = s.FIRSTNAME + " " + s.SURNAME,
                           //    address = s.ADDRESS,
                           //    phoneNumber = s.PHONENUMBER,
                           //    email = s.EMAILADDRESS
                           //}).ToList(),
                           //CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                           //.Select(s => new CustomerCompanyShareholderViewModels()
                           //{
                           //    companyDirectorId = s.COMPANYDIRECTORID,
                           //    surname = s.SURNAME,
                           //    firstname = s.FIRSTNAME,
                           //    middlename = s.MIDDLENAME,
                           //    customerNIN = s.CUSTOMERNIN,
                           //    numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                           //    isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           //    bankVerificationNumber = s.CUSTOMERBVN,
                           //    companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           //    companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           //    customerId = s.CUSTOMERID,
                           //    customerName = s.FIRSTNAME + " " + s.SURNAME,
                           //    address = s.ADDRESS,
                           //    phoneNumber = s.PHONENUMBER,
                           //    email = s.EMAILADDRESS
                           //}).ToList(),
                           //CustomerCompanyAccountSignatory = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory)
                           //.Select(s => new CustomerCompanyAccountSignatoryViewModels()
                           //{
                           //    companyDirectorId = s.COMPANYDIRECTORID,
                           //    surname = s.SURNAME,
                           //    firstname = s.FIRSTNAME,
                           //    middlename = s.MIDDLENAME,
                           //    customerNIN = s.CUSTOMERNIN,
                           //    numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                           //    isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           //    bankVerificationNumber = s.CUSTOMERBVN,
                           //    companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           //    companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           //    customerId = s.CUSTOMERID,
                           //    customerName = s.FIRSTNAME + " " + s.SURNAME,
                           //    address = s.ADDRESS,
                           //    phoneNumber = s.PHONENUMBER,
                           //    email = s.EMAILADDRESS
                           //}).ToList(),
                           //CustomerClientOrSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == a.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                           //.Select(cs => new CustomerClientOrSupplierViewModels()
                           //{
                           //    client_SupplierId = cs.CLIENT_SUPPLIERID,
                           //    clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                           //    firstName = cs.FIRSTNAME,
                           //    middleName = cs.MIDDLENAME,
                           //    lastName = cs.LASTNAME,
                           //    taxNumber = cs.TAX_NUMBER,
                           //    rcNumber = cs.REGISTRATION_NUMBER,
                           //    hasCASAAccount = (bool)cs.HAS_CASA_ACCOUNT,
                           //    bankName = cs.BANKNAME,
                           //    casaAccountNumber = cs.CASA_ACCOUNTNO,
                           //    natureOfBusiness = cs.NATURE_OF_BUSINESS,
                           //    contactPerson = cs.CONTACT_PERSON,
                           //    client_SupplierAddress = cs.ADDRESS,
                           //    client_SupplierPhoneNumber = cs.PHONENUMBER,
                           //    client_SupplierEmail = cs.EMAILADDRESS,
                           //    client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           //    client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                           //}).ToList(),
                           //CustomerSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == a.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                           //.Select(cs => new CustomerSupplierViewModels()
                           //{
                           //    client_SupplierId = cs.CLIENT_SUPPLIERID,
                           //    clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                           //    firstName = cs.FIRSTNAME,
                           //    middleName = cs.MIDDLENAME,
                           //    lastName = cs.LASTNAME,
                           //    taxNumber = cs.TAX_NUMBER,
                           //    rcNumber = cs.REGISTRATION_NUMBER,
                           //    hasCASAAccount = (bool)cs.HAS_CASA_ACCOUNT,
                           //    bankName = cs.BANKNAME,
                           //    casaAccountNumber = cs.CASA_ACCOUNTNO,
                           //    contactPerson = cs.CONTACT_PERSON,
                           //    natureOfBusiness = cs.NATURE_OF_BUSINESS,
                           //    client_SupplierAddress = cs.ADDRESS,
                           //    client_SupplierPhoneNumber = cs.PHONENUMBER,
                           //    client_SupplierEmail = cs.EMAILADDRESS,
                           //    client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           //    client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                           //}).ToList(),
                           //CustomerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID)
                           //.Select(x => new CollateralViewModel()
                           //{
                           //    collateralId = x.COLLATERALCUSTOMERID,
                           //    collateralTypeId = x.COLLATERALTYPEID,
                           //    collateralSubTypeId = x.COLLATERALSUBTYPEID,
                           //    customerId = x.CUSTOMERID,
                           //    currencyId = x.CURRENCYID,
                           //    currency = x.TBL_CURRENCY.CURRENCYNAME,
                           //    collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                           //    collateralCode = x.COLLATERALCODE,
                           //    camRefNumber = x.CAMREFNUMBER,
                           //    allowSharing = x.ALLOWSHARING,
                           //    isLocationBased = (bool)x.ISLOCATIONBASED,
                           //    valuationCycle = x.VALUATIONCYCLE,
                           //    collateralValue = x.COLLATERALVALUE,
                           //    haircut = x.HAIRCUT,
                           //    approvalStatus = x.APPROVALSTATUS,
                           //}).ToList(),
                           //CustomerChildren = context.TBL_CUSTOMER_CHILDREN.Where(chd => chd.CUSTOMERID == a.CUSTOMERID)
                           //.Select(kk => new CustomerChildrenViewModel()
                           //{
                           //    customerChildrenId = kk.CUSTOMERCHILDRENID,
                           //    customerId = kk.CUSTOMERID,
                           //    childName = kk.CHILDNAME,
                           //    childDateOfBirth = kk.CHILDDATEOFBIRTH
                           //}).ToList(),
                       };


        }


        public IEnumerable<CustomerViewModels> GetSimpleCustomerDetailsByCustomerId(int customerId)
        {
            return from a in context.TBL_CUSTOMER
                   where a.DELETED == false && a.CUSTOMERID == customerId
                   select
                       new CustomerViewModels
                       {
                           crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                           crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                           crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                           accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                           branchId = a.BRANCHID,
                           branchName = a.TBL_BRANCH.BRANCHNAME,
                           companyMainId = a.COMPANYID,
                           createdBy = a.CREATEDBY,
                           creationMailSent = a.CREATIONMAILSENT,
                           customerCode = a.CUSTOMERCODE,
                           customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                           customerTypeId = (short)a.CUSTOMERTYPEID,
                           dateOfBirth = (DateTime)a.DATEOFBIRTH,
                           customerId = a.CUSTOMERID,
                           emailAddress = a.EMAILADDRESS,
                           firstName = a.FIRSTNAME,
                           gender = a.GENDER,
                           lastName = a.LASTNAME,
                           maidenName = a.MAIDENNAME,
                           maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                           title = a.TITLE,
                           middleName = a.MIDDLENAME,
                           //customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                           customerTypeName = context.TBL_CUSTOMER_TYPE
                               .FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                           misCode = a.MISCODE,
                           misStaff = a.MISSTAFF,
                           nationalityId = a.NATIONALITYID,
                           occupation = a.OCCUPATION,
                           placeOfBirth = a.PLACEOFBIRTH,
                           isPoliticallyExposed = a.ISPOLITICALLYEXPOSED == false && (USE_THIRD_PARTY_INTEGRATION)
                               ? finacle.GetExposePersonStatus(a.CUSTOMERCODE)
                               : a.ISPOLITICALLYEXPOSED,
                           relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                           relationshipOfficerName =
                               context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                                                                                                                     + context
                                                                                                                         .TBL_STAFF
                                                                                                                         .FirstOrDefault(
                                                                                                                             f =>
                                                                                                                                 f.STAFFID ==
                                                                                                                                 a.RELATIONSHIPOFFICERID)
                                                                                                                         .LASTNAME,
                           spouse = a.SPOUSE,
                           sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                           subSectorId = (short)a.SUBSECTORID,
                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                           taxNumber = a.TAXNUMBER,
                           riskRatingId = a.RISKRATINGID,
                           riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                           customerBVN = a.CUSTOMERBVN,
                           isCreditBureauUploadCompleted = false,
                           creditBureauCount = context.TBL_CUSTOMER_CREDIT_BUREAU
                               .Where(x => x.CUSTOMERID == a.CUSTOMERID && x.DELETED == false).Count(),
                           CustomerPhoneContact = context.TBL_CUSTOMER_PHONECONTACT
                               .Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => new CustomerPhoneContactViewModels
                               {
                                   active = c.ACTIVE,
                                   customerId = c.CUSTOMERID,
                                   phone = c.PHONE,
                                   phoneContactId = c.PHONECONTACTID,
                                   phoneNumber = c.PHONENUMBER
                               }).ToList(),
                           CustomerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s =>
                                   s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID ==
                                   (short)CompanyDirectorTypeEnum.BoardMember)
                               .Select(s => new CustomerCompanyDirectorsViewModels()
                               {
                                   companyDirectorId = s.COMPANYDIRECTORID,
                                   surname = s.SURNAME,
                                   firstname = s.FIRSTNAME,
                                   numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                   isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                   bankVerificationNumber = s.CUSTOMERBVN,
                                   companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                   companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                   customerId = s.CUSTOMERID,
                                   customerName = s.FIRSTNAME + " " + s.SURNAME,
                                   address = s.ADDRESS,
                                   phoneNumber = s.PHONENUMBER,
                                   email = s.EMAILADDRESS,
                                   dateOfBirth = s.DATEOFBIRTH,
                                   gender = s.GENDER,
                                   maritalStatusId = s.MARITALSTATUSID
                               }).ToList(),
                           CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s =>
                                   s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID ==
                                   (short)CompanyDirectorTypeEnum.Shareholder)
                               .Select(s => new CustomerCompanyShareholderViewModels()
                               {
                                   companyDirectorId = s.COMPANYDIRECTORID,
                                   surname = s.SURNAME,
                                   firstname = s.FIRSTNAME,
                                   numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                   isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                   bankVerificationNumber = s.CUSTOMERBVN,
                                   companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                   companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                   customerId = s.CUSTOMERID,
                                   customerName = s.FIRSTNAME + " " + s.SURNAME,
                                   address = s.ADDRESS,
                                   phoneNumber = s.PHONENUMBER,
                                   email = s.EMAILADDRESS
                               }).ToList(),
                       };

        }

        IQueryable<CustomerViewModels> GetCustomersLite()
        {
            return context.TBL_CUSTOMER.Where(x => x.DELETED == false).Select(a => new CustomerViewModels
            {
                accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                branchId = a.BRANCHID,
                branchName = a.TBL_BRANCH.BRANCHNAME,
                companyMainId = a.COMPANYID,
                createdBy = a.CREATEDBY,
                creationMailSent = a.CREATIONMAILSENT,
                customerCode = a.CUSTOMERCODE,
                customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                customerTypeId = a.CUSTOMERTYPEID,
                dateOfBirth = a.DATEOFBIRTH,
                customerId = a.CUSTOMERID,
                emailAddress = a.EMAILADDRESS,
                firstName = a.FIRSTNAME,
                gender = a.GENDER,
                lastName = a.LASTNAME,
                maidenName = a.MAIDENNAME,
                maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : a.MARITALSTATUS.Value == 2 ? "F" : null,
                title = a.TITLE,
                middleName = a.MIDDLENAME,
                customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID) != null ? context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER : null,
                customerTypeName = a.TBL_CUSTOMER_TYPE.NAME, // context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                misCode = a.MISCODE,
                misStaff = a.MISSTAFF,
                nationalityId = a.NATIONALITYID,
                occupation = a.OCCUPATION,
                placeOfBirth = a.PLACEOFBIRTH,
                isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                //relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                //         + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                spouse = a.SPOUSE,
                sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                subSectorId = a.SUBSECTORID,
                subSectorName = a.TBL_SUB_SECTOR.NAME,
                taxNumber = a.TAXNUMBER,
                riskRatingId = a.RISKRATINGID,
                crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                // riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                customerBVN = a.CUSTOMERBVN,
                isProspect = a.ISPROSPECT,
                pastDueObligations = a.PASTDUEOBLIGATIONS,
                countryOfResidentId = a.COUNTRYOFRESIDENTID,
                numberOfDependents = a.NUMBEROFDEPENDENTS,
                numberOfLoansTaken = a.NUMBEROFLOANSTAKEN,
                loanMonthlyRepaymentFromOtherBanks = a.MONTHLYLOANREPAYMENT,
                dateOfRelationshipWithBank = a.DATEOFRELATIONSHIPWITHBANK,
                relationshipTypeId = a.RELATIONSHIPTYPEID,
                teamLDP = a.TEAMLDR,
                teamNPL = a.TEAMNPL,
                businessUnitId = a.BUSINESSUNTID,
                corr = a.CORR,
                ownership = a.OWNERSHIP,

            });
        }

        public IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId)
        {
            var data = (from cs in context.TBL_CUSTOMER_GROUP_MAPPING
                        where cs.CUSTOMERGROUPID == groupId
                        select new CustomerViewModels()
                        {

                            customerId = cs.CUSTOMERID,
                            fullName = cs.TBL_CUSTOMER.LASTNAME + " " + cs.TBL_CUSTOMER.FIRSTNAME + "(" +
                                       cs.TBL_CUSTOMER.CUSTOMERCODE + ")",
                            firstName = cs.TBL_CUSTOMER.FIRSTNAME,
                            lastName = cs.TBL_CUSTOMER.LASTNAME,
                            customerCode = cs.TBL_CUSTOMER.CUSTOMERCODE,
                        });

            return data;
        }




        public IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId)
        {
            return GetCustomers().Where(a => a.branchId == branchId);
        }

        public IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId)
        {
            return GetCustomers().Where(a => a.companyMainId == companyId);
        }

        public IEnumerable<CustomerViewModels> GetCustomerByTypeId(int customerTypeId)
        {
            return GetCustomers().Where(a => a.customerTypeId == customerTypeId);
        }

        public IEnumerable<CustomerTypeViewModels> GetCustomerType()
        {
            var type = from a in context.TBL_CUSTOMER_TYPE
                       select new CustomerTypeViewModels
                       {
                           isHybrid = a.ISHYBRID,
                           name = a.NAME,
                           customerTypeId = a.CUSTOMERTYPEID
                       };
            return type.Where(x => x.isHybrid == false).ToList();
        }

        public IEnumerable<CorporateCustomerTypeViewModels> GetCorporateCustomerType()
        {
            var type = from a in context.TBL_CORPORATE_CUSTOMER_TYPE
                       select new CorporateCustomerTypeViewModels
                       {
                           corporateCustomerTypeId = a.CORPORATECUSTOMERTYPEID,
                           corporateCustomerTypeName = a.CORPORATECUSTOMERTYPENAME
                       };
            return type.ToList();
        }
        public IEnumerable<CustomerTypeViewModels> GetCustomerTypeWithHybrid()
        {
            var type = from a in context.TBL_CUSTOMER_TYPE
                       select new CustomerTypeViewModels
                       {
                           name = a.NAME,
                           customerTypeId = a.CUSTOMERTYPEID
                       };
            return type.ToList();
        }

        public IEnumerable<CustomerAddressTypeViewModels> GetCustomerAddressType()
        {
            var type = from a in context.TBL_CUSTOMER_ADDRESS_TYPE
                       where a.ADDRESSTYPEID != (int)CustomerAddressTypeEnum.Corporate
                       select new CustomerAddressTypeViewModels
                       {
                           addressTypeName = a.ADDRESS_TYPE_NAME,
                           addressTypeId = a.ADDRESSTYPEID
                       };
            return type;
        }

        public IEnumerable<CustomerRiskRatingViewModels> GetCustomerRiskRating()
        {
            var type = from a in context.TBL_CUSTOMER_RISK_RATING
                       select new CustomerRiskRatingViewModels
                       {
                           riskRating = a.RISKRATING,
                           riskRatingId = a.RISKRATINGID
                       };
            return type;
        }

        public IEnumerable<KYCDocumentTypeViewModel> GetKYCDocumentType()
        {
            var type = from a in context.TBL_KYC_DOCUMENTTYPE
                       select new KYCDocumentTypeViewModel
                       {
                           documentTypeName = a.DOCUMENTTYPENAME,
                           documentTypeId = a.DOCUMENTTYPEID
                       };
            return type.ToList();
        }

        public IEnumerable<CustomerSupplierTypeViewModels> GetClientSupplierType()
        {
            var type = from a in context.TBL_CUSTOMER_CLIENT_SUPPLR_TYP
                       select new CustomerSupplierTypeViewModels
                       {
                           name = a.CLIENT_SUPPLIERTYPENAME,
                           client_SupplierTypeId = a.CLIENT_SUPPLIERTYPEID
                       };
            return type;
        }

        public IEnumerable<CustomerIdentificationModeTypeViewModels> GetIdentificationMode()
        {
            var type = from a in context.TBL_CUSTOMER_IDENTI_MODE_TYPE
                       select new CustomerIdentificationModeTypeViewModels
                       {
                           name = a.IDENTIFICATIONMODE,
                           identificationModeId = a.IDENTIFICATIONMODEID
                       };
            return type;
        }

        public IEnumerable<CompanyDirectorTypeViewModels> GetDirectorsTypes()
        {
            var type = from a in context.TBL_CUSTOMER_COMPANY_DIREC_TYP
                       select new CompanyDirectorTypeViewModels
                       {
                           name = a.COMPANYDIRECTORYTYPENAME,
                           companyDirectorTypeId = a.COMPANYDIRECTORYTYPEID
                       };
            return type;
        }

        public bool UpdateCustomer(int customerId, CustomerViewModels entity)
        {
  
            var detail = string.Empty;

            //get customer record by id
            var customerMain = context.TBL_CUSTOMER.Find(customerId);

            //get previous customer name
            var previousCustomerName = customerMain?.FIRSTNAME;

            if (customerMain != null && customerMain.ACCOUNTCREATIONCOMPLETE == false)// && entity.canModified == true)
            {
                customerMain.CRMSCOMPANYSIZEID = entity.crmsCompanySizeId;
                customerMain.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                customerMain.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                customerMain.BRANCHID = entity.userBranchId;
                customerMain.COMPANYID = entity.companyId;
                customerMain.CUSTOMERCODE = entity.customerCode;
                customerMain.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
                customerMain.CUSTOMERTYPEID = entity.customerTypeId;
                customerMain.DATEOFBIRTH = entity.dateOfBirth;
                customerMain.EMAILADDRESS = entity.emailAddress;
                customerMain.FIRSTNAME = entity.firstName;
                customerMain.GENDER = entity.gender;
                customerMain.LASTNAME = entity.lastName;
                customerMain.MAIDENNAME = entity.maidenName;
                customerMain.MARITALSTATUS = entity.maritalStatus == "M" ? 1 : entity.maritalStatus == "F" ?  2 : Convert.ToInt32(entity.maritalStatus);
                customerMain.TITLE = entity.title;
                customerMain.MIDDLENAME = entity.middleName;
                customerMain.MISCODE = entity.misCode;
                customerMain.MISSTAFF = entity.misStaff;
                customerMain.NATIONALITYID = entity.nationalityId;
                customerMain.OCCUPATION = entity.occupation;
                customerMain.PLACEOFBIRTH = entity.placeOfBirth;
                customerMain.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                customerMain.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                customerMain.ISREALATEDPARTY = entity.isRealatedParty;
                customerMain.RELATIONSHIPOFFICERID = entity.relationshipOfficerId;
                customerMain.SPOUSE = entity.spouse;
                customerMain.SUBSECTORID = entity.subSectorId;
                customerMain.TAXNUMBER = entity.taxNumber;
                customerMain.RISKRATINGID = entity.riskRatingId;
                customerMain.CUSTOMERBVN = entity.customerBVN;
                customerMain.OWNERSHIP = entity.ownership;
                customerMain.DATETIMEUPDATED = DateTime.Now;
                customerMain.LASTUPDATEDBY = entity.deletedBy;

                var saved = context.SaveChanges() != 0;

                UpdateCustomerCollateralId(customerMain.CUSTOMERCODE);

                return saved;
            }
            else
            {
                TBL_TEMP_CUSTOMER customer;
                var existingTempCustomer = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x =>
                    x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower() && x.COMPANYID == entity.companyId &&
                    x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER.Where(x =>
                    x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                    x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower());
                //////if (unApprovedCustomerUpdate.Any())
                //////{
                //////    throw new SecureException("Customer is already undergoing approval");
                //////}

                if (existingTempCustomer != null)
                {
                    customer = existingTempCustomer;
                    customer.CRMSCOMPANYSIZEID = entity.crmsCompanySizeId;
                    customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                    customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                    customer.BRANCHID = entity.userBranchId;
                    customer.COMPANYID = entity.companyId;
                    customer.CUSTOMERCODE = entity.customerCode;
                    customer.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
                    customer.CUSTOMERTYPEID = entity.customerTypeId;
                    customer.DATEOFBIRTH = entity.dateOfBirth;
                    customer.EMAILADDRESS = entity.emailAddress;
                    customer.FIRSTNAME = entity.firstName;
                    customer.GENDER = entity.gender;
                    customer.LASTNAME = entity.lastName;
                    customer.MAIDENNAME = entity.maidenName;
                    customer.MARITALSTATUS = entity.maritalStatus == "F" ? 2 : entity.maritalStatus == "M" ? 1 :  Convert.ToInt16(entity.maritalStatus);  //Convert.ToInt16(entity.maritalStatus);
                    customer.TITLE = entity.title;
                    customer.MIDDLENAME = entity.middleName;
                    customer.MISCODE = entity.misCode;
                    customer.MISSTAFF = entity.misStaff;
                    customer.NATIONALITYID = entity.nationalityId;
                    customer.OCCUPATION = entity.occupation;
                    customer.PLACEOFBIRTH = entity.placeOfBirth;
                    customer.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                    customer.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                    customer.ISREALATEDPARTY = entity.isRealatedParty;
                    customer.RELATIONSHIPOFFICERID = entity.relationshipOfficerId;
                    customer.SPOUSE = entity.spouse;
                    customer.SUBSECTORID = entity.subSectorId;
                    customer.TAXNUMBER = entity.taxNumber;
                    customer.RISKRATINGID = entity.riskRatingId;
                    customer.CUSTOMERBVN = entity.customerBVN;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                    customer.COUNTRYOFRESIDENTID = entity.countryOfResidentId;
                    customer.NUMBEROFDEPENDENTS = entity.numberOfDependents;
                    customer.NUMBEROFLOANSTAKEN = entity.numberOfLoansTaken;
                    customer.MONTHLYLOANREPAYMENT = entity.loanMonthlyRepaymentFromOtherBanks;
                    customer.DATEOFRELATIONSHIPWITHBANK = entity.dateOfRelationshipWithBank;
                    customer.RELATIONSHIPTYPEID = entity.relationshipTypeId;
                    customer.TEAMLDR = entity.teamLDP;
                    customer.TEAMNPL = entity.teamNPL;
                    customer.CORR = entity.corr;
                    customer.BUSINESSUNTID = entity.businessUnitId;
                    customer.PASTDUEOBLIGATIONS = entity.pastDueObligations;
                    customer.OWNERSHIP = entity.ownership;


                }
                else
                {
                    customer = new TBL_TEMP_CUSTOMER();
                    customer.CRMSCOMPANYSIZEID = entity.crmsCompanySizeId;
                    customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                    customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                    customer.CUSTOMERID = entity.customerId;
                    customer.BRANCHID = entity.userBranchId;
                    customer.COMPANYID = entity.companyId;
                    customer.CUSTOMERCODE = entity.customerCode;
                    customer.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
                    customer.CUSTOMERTYPEID = entity.customerTypeId;
                    customer.DATEOFBIRTH = entity.dateOfBirth;
                    customer.EMAILADDRESS = entity.emailAddress;
                    customer.FIRSTNAME = entity.firstName;
                    customer.GENDER = entity.gender;
                    customer.LASTNAME = entity.lastName;
                    customer.MAIDENNAME = entity.maidenName;
                    //customer.MARITALSTATUS = Convert.ToInt16(entity.maritalStatus);
                    customer.MARITALSTATUS = entity.maritalStatus == "F" ? 2 : entity.maritalStatus == "M" ? 1 : Convert.ToInt16(entity.maritalStatus);
                    customer.TITLE = entity.title;
                    customer.MIDDLENAME = entity.middleName;
                    customer.MISCODE = entity.misCode;
                    customer.MISSTAFF = entity.misStaff;
                    customer.NATIONALITYID = entity.nationalityId;
                    customer.OCCUPATION = entity.occupation;
                    customer.PLACEOFBIRTH = entity.placeOfBirth;
                    customer.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                    customer.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                    customer.ISREALATEDPARTY = entity.isRealatedParty;
                    customer.RELATIONSHIPOFFICERID = entity.relationshipOfficerId;
                    customer.SPOUSE = entity.spouse;
                    customer.SUBSECTORID = entity.subSectorId;
                    customer.TAXNUMBER = entity.taxNumber;
                    customer.RISKRATINGID = entity.riskRatingId;
                    customer.CUSTOMERBVN = entity.customerBVN;
                    customer.CREATEDBY = entity.createdBy;
                    customer.DATETIMECREATED = DateTime.Now;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                    customer.COUNTRYOFRESIDENTID = entity.countryOfResidentId;
                    customer.NUMBEROFDEPENDENTS = entity.numberOfDependents;
                    customer.NUMBEROFLOANSTAKEN = entity.numberOfLoansTaken;
                    customer.MONTHLYLOANREPAYMENT = entity.loanMonthlyRepaymentFromOtherBanks;
                    customer.DATEOFRELATIONSHIPWITHBANK = entity.dateOfRelationshipWithBank;
                    customer.RELATIONSHIPTYPEID = entity.relationshipTypeId;
                    customer.TEAMLDR = entity.teamLDP;
                    customer.TEAMNPL = entity.teamNPL;
                    customer.CORR = entity.corr;
                    customer.BUSINESSUNTID = entity.businessUnitId;
                    customer.PASTDUEOBLIGATIONS = entity.pastDueObligations;
                    customer.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                    customer.OWNERSHIP = entity.ownership;

                    context.TBL_TEMP_CUSTOMER.Add(customer);

           
                }

                var modified = new TBL_CUSTOMER_MODIFICATION
                {
                    CUSTOMERID = entity.customerId,
                    TARGETID = entity.customerId,
                    MODIFICATIONTYPEID = (int)CustomerInformationTrackerEnum.General_Information,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"The Customer First Name for '{ entity.customerName }' with code: '{entity.customerCode}' " +
                    $"on  ({ entity.customerId })  Has Been Updated from '{previousCustomerName}' to '{entity.firstName}' ",
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                        this.auditTrail.AddAuditTrail(audit);
                        //end of Audit section -------------------------------

                        var output = context.SaveChanges() > 0;

                        var targetId = modified.CUSTOMERMODIFICATIONID;


                        workflow.StaffId = entity.createdBy;
                        workflow.CompanyId = entity.companyId;
                        workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                        workflow.TargetId = targetId;
                        workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                        workflow.ExternalInitialization = true;

                        var response = workflow.LogActivity();

                        //var model = new ApprovalViewModel
                        //{
                        //    staffId = entity.createdBy,
                        //    companyId = entity.companyId,
                        //    approvalStatusId = (int) ApprovalStatusEnum.Pending,
                        //    targetId = targetId,
                        //    operationId = (int) OperationsEnum.CustomerInformationApproval,
                        //    BranchId = entity.userBranchId,
                        //    externalInitialization = true
                        //};
                        //var response = workflow.LogForApproval(model);

                        if (response)
                        {
                            trans.Commit();

                            return output;
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }




                List<ChangeTrackingViewModel> kk = new List<ChangeTrackingViewModel>();
                auditTrail.AddAuditTrail(audit);

                var currentData = context.Entry(customer).CurrentValues;
                var originalData = context.Entry(customer).OriginalValues;
                foreach (string propertyName in originalData.PropertyNames)
                {
                    var original = originalData[propertyName];
                    var current = currentData[propertyName];

                    if (!Equals(original, current))
                    {
                        kk.Add(new ChangeTrackingViewModel()
                        {
                            customerId = customer.CUSTOMERID,
                            propertyName = propertyName,
                            propertyOriginalValue = original.ToString(),
                            propertyCurrentValue = current.ToString(),
                        });
                    }
                }

                var diff = kk.ToList();
                return false;

            }
        }

        public IEnumerable<CustomerViewModels> CustomerSearch(int companyId, string search)
        {
            var customers = GetCustomersLite();
            if (!string.IsNullOrWhiteSpace(search))
            {
                customers = customers.Where(x =>
                    x.firstName.ToLower().Contains(search.ToLower())
                    || x.lastName.ToLower().Contains(search.ToLower())
                    || x.middleName.ToLower().Contains(search.ToLower())
                    || x.customerCode.Contains(search.ToLower())
                    || x.customerId.ToString().Contains(search)
                );
            }

            return customers;
        }

        public IQueryable<CustomerSearchItemViewModels> CustomerSearchRealTime(int companyId, string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            IQueryable<CustomerSearchItemViewModels> allCustomers = null;


            if (!string.IsNullOrWhiteSpace(searchQuery?.Trim()))
            {
                allCustomers = GetCustomers().Where(c => c.companyId == companyId)
                    .Where(x => x.firstName.ToLower().Contains(searchQuery)

                                || x.lastName.ToLower().Contains(searchQuery)
                                || x.middleName.ToLower().Contains(searchQuery)
                                || x.customerCode.Contains(searchQuery)
                    ).Select(c => new CustomerSearchItemViewModels
                    {
                        customerId = c.customerId,
                        branchId = c.branchId,
                        branchName = c.branchName,
                        customerName = c.firstName + " " + c.lastName,
                        customerTypeId = c.customerTypeId,
                        customerTypeName = c.customerTypeName,
                        customerCode = c.customerCode,
                        customerSectorId = c.sectorId,
                        customerSectorName = c.sectorName,
                        subSectorId = c.subSectorId,
                        subSectorName = c.subSectorName,
                        relationshipOfficerId = c.relationshipOfficerId.Value

                    });
            }

            return allCustomers;
        }

        public IEnumerable<CustomerViewModels> CustomerSearch(int companyId, CustomerSearchItemViewModels search)
        {
            var customers = GetCustomersLite();

            if (search.branchId != null)
            {
                customers = customers.Where(x => x.branchId == search.branchId);
            }

            if (search.customerTypeId != null)
            {
                customers = customers.Where(x => x.customerTypeId == search.customerTypeId);
            }

            if (!String.IsNullOrEmpty(search.phoneNumber))
            {
                customers = customers.Where(x =>
                    x.CustomerPhoneContact.Where(o => o.phoneNumber == search.phoneNumber).Count() >= 1);
            }

            if (!String.IsNullOrEmpty(search.customerName))
            {
                customers = customers.Where(x =>
                    x.firstName.ToLower().Contains(search.customerName.ToLower())
                    || x.lastName.ToLower().Contains(search.customerName.ToLower())
                    || x.middleName.ToLower().Contains(search.customerName.ToLower())
                    || x.customerCode.Contains(search.customerName)
                );
            }

            return customers;
        }

        public CustomerEligibilityViewModels EligibilitySearch(int companyId, CustomerEligibilityViewModels search)
        {
            var customerEligibility = new CustomerEligibilityViewModels();
            search.account_number = search.accountNumber;
            search.phone_number = search.phoneNumber;
            if (search.account_number != null && search.phone_number != null)
            {
                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    customerEligibility = finacle.GetCustomerEligibility(search.phone_number, search.account_number);
                    
                }
            }

            var eligibilityExist = context.TBL_CUSTOMER_IBL_ELIGIBILITY.Where(e => e.CUSTOMERID == search.customerId).FirstOrDefault();
            if(eligibilityExist != null)
            {
                context.TBL_CUSTOMER_IBL_ELIGIBILITY.Remove(eligibilityExist);
            }
            //if(customerEligibility.account_number != null)
            {
                var eligibility = new TBL_CUSTOMER_IBL_ELIGIBILITY();
                eligibility.CUSTOMERID = search.customerId;
                eligibility.RESPONSEDESCRIPTION = customerEligibility.response_descr;
                eligibility.MAXIMUMAMOUNT = customerEligibility.MaximumAmount;
                eligibility.MINIMUMAMOUNT = customerEligibility.MinimumAmount;
                eligibility.ISELIGIBLE = customerEligibility.IsEligible;
                eligibility.FULLDESCRIPTION = customerEligibility.full_description;
                eligibility.ACCOUNTNUMBER = search.account_number;
                eligibility.AMOUNT = customerEligibility.amount;
                eligibility.ISIBLREQUEST = search.isIblRequest;
                eligibility.PHONENUMBER = search.phone_number;

                context.TBL_CUSTOMER_IBL_ELIGIBILITY.Add(eligibility);
                context.SaveChanges();
            }


            return customerEligibility;

        }

        public IEnumerable<CustomerEligibilityViewModels> GetCustomerIBLEligibility(int customerId)
        {
            var data = (from e in context.TBL_CUSTOMER_IBL_ELIGIBILITY
                        where e.CUSTOMERID == customerId
                        select new CustomerEligibilityViewModels()
                        {
                            eliigibilityId = e.ELIGIBILITYID,
                            customerId = e.CUSTOMERID,
                            IsEligible = e.ISELIGIBLE ? true : false,
                            MinimumAmount = e.MINIMUMAMOUNT,
                            MaximumAmount = e.MAXIMUMAMOUNT,
                            full_description = e.FULLDESCRIPTION,
                            account_number = e.ACCOUNTNUMBER,
                            customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == e.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                            amount = e.AMOUNT,
                            isIblRequest = e.ISIBLREQUEST ? true : false,
                            phone_number = e.PHONENUMBER,
                        }); 

            return data;
        }

        public  bool updateIBLEligibility(int iblEligibilityId)
        {
            try
            {
                var iblEligibility = context.TBL_CUSTOMER_IBL_ELIGIBILITY.Find(iblEligibilityId);
                if (iblEligibility != null)
                {
                    iblEligibility.ISIBLREQUEST = false;
                }
                context.SaveChanges();

               
                return true;
               
            }
            catch(Exception e)
            {
                throw e;
            }
           
        }

        public IEnumerable<CustomerSectorViewModel> GetCustomerSectors()
        {
            var data = (from cs in context.TBL_SECTOR
                        select new CustomerSectorViewModel()
                        {
                            sectorId = cs.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                        });

            return data;
        }

        public IEnumerable<CustomerSectorViewModel> GetCustomerSectorBySubSectorId(short ssId)
        {
            var data = (from s in context.TBL_SUB_SECTOR
                        where s.SUBSECTORID == ssId
                        select new CustomerSectorViewModel()
                        {
                            subSectorId = s.SUBSECTORID,
                            sectorId = s.TBL_SECTOR.SECTORID,
                            sectorName = s.NAME,
                            sectorCode = s.CODE
                        });

            return data;
        }

        public IEnumerable<CustomerViewModels> SearchRandomCustomerBySearchQuery(string searchQuery)
        {
            var customers = (from x in GetCustomersLite()
                             where x.firstName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.lastName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.middleName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.customerCode.StartsWith(searchQuery)
                                   || x.branchName.StartsWith(searchQuery)
                                   || x.customerId.ToString().StartsWith(searchQuery)
                             select x).ToList();

            var customerInfo = customers.ToList();

            if (customerInfo.Count > 0)
            {
                return customerInfo;
            }

            return null;
        }

        public IEnumerable<CustomerViewModels> SearchRandomCustomersBySearchQuery(string searchQuery)
        {
            var singleCustomers = SearchRandomSingleCustomersBySearchQuery(searchQuery);
            var corporateCustomers = SearchRandomSingleCorporateCustomersBySearchQuery(searchQuery);

            var customers = singleCustomers.Union(corporateCustomers);
            return customers;
        }
        public IEnumerable<CustomerViewModels> SearchRandomSingleCustomersBySearchQuery(string searchQuery)
        {
            var customerGroup = (from m in context.TBL_CUSTOMER_GROUP_MAPPING
                                 select m).ToList();
            var customers = (from x in GetCustomersLite()
                             where (x.firstName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.lastName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.middleName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.customerCode.StartsWith(searchQuery))
                                   //|| x.branchName.StartsWith(searchQuery)
                                   //|| x.customerId.ToString().StartsWith(searchQuery))
                                   && (x.customerTypeId == 1) 
                             select x).ToList();

            //var customerInfo = new List<CustomerViewModels>();
            //foreach (var customer in customers)
            //{
            //    if (!customerGroup.Exists(c => c.CUSTOMERID == customer.customerId))
            //    {
            //        customerInfo.Add(customer);
            //    }
            //}
            return customers.OrderBy(O => O.firstName).ToList();
            //if (customerInfo.Count > 0)
            //{
            //    return customerInfo;
            //}

            //return null;
        }

        public IEnumerable<CustomerViewModels> SearchRandomSingleCorporateCustomersBySearchQuery(string searchQuery)
        {
            //var customerGroup = (from m in context.TBL_CUSTOMER_GROUP_MAPPING
            //                     select m).ToList();
            var customers = (from x in GetCustomersLite()
                             where (x.firstName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.lastName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.middleName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.customerCode.StartsWith(searchQuery))
                                   //|| x.branchName.StartsWith(searchQuery)
                                   //|| x.customerId.ToString().StartsWith(searchQuery))
                                   && (x.customerTypeId == 2)
                             select x);

            //var customerInfo = new List<CustomerViewModels>();
            //foreach (var customer in customers)
            //{
            //    if (customerGroup.Exists(c => c.CUSTOMERID == customer.customerId))
            //    {
            //        customerInfo.Add(customer);
            //    }
            //}

            //if (customerInfo.Count > 0)
            //{
            //    return customerInfo;
            //}
           return customers.OrderBy(O => O.firstName).ToList();
        }

        public IEnumerable<CustomerViewModels> SearchRandomGroupCustomersBySearchQuery(string searchQuery)
        {
            var customers = (from x in GetCustomersLite()
                             where x.firstName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.lastName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.middleName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.customerCode.StartsWith(searchQuery)
                                   //|| x.branchName.StartsWith(searchQuery)
                                   || x.customerId.ToString().StartsWith(searchQuery)
                                   && context.TBL_CUSTOMER_GROUP_MAPPING.Find(x.customerId) != null
                             select x);

            var customerInfo = customers.ToList();

            if (customerInfo.Count > 0)
            {
                return customerInfo;
            }

            return null;
        }

        public IEnumerable<CustomerViewModels> SearchGroupCustomersBySearchQuery(string searchQuery, int groupId)
        {
            var customers = (from x in GetCustomersLite()
                             join g in context.TBL_CUSTOMER_GROUP_MAPPING on x.customerId equals g.CUSTOMERID
                             where x.firstName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.lastName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.middleName.ToLower().StartsWith(searchQuery.ToLower())
                                   || x.customerCode.StartsWith(searchQuery)
                                   || x.branchName.StartsWith(searchQuery)
                                   || x.customerId.ToString().StartsWith(searchQuery)
                                   && g.CUSTOMERGROUPID == groupId
                             select x);
            var customerInfo = customers.ToList();

            if (customerInfo.Count > 0)
            {
                return customerInfo;
            }


            return null;
        }

        #region Single Customer Information By CustomerID

        public IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLoanId(int loanApplicationId)
        {
            var loanCust = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                            where a.LOANAPPLICATIONID == loanApplicationId
                            select a.CUSTOMERID).ToList();
            var customers = GetCustomers().Where(x => loanCust.Contains(x.customerId))?.ToList();
            if (loanCust.Any())
            {
                customers = customers.Where(x => loanCust.Contains(x.customerId)).ToList();
            }

            return customers;
        }

        public CustomerViewModels GetSingleCustomerGeneralInfo(string customerCode)
        {
            //int? maritalStatus = null;
            
            var data = (from a in context.TBL_CUSTOMER
                        where a.DELETED == false && a.CUSTOMERCODE == customerCode
                        select new CustomerViewModels
                        {
                            crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                            crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                            crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                            accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                            branchId = a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            companyMainId = a.COMPANYID,
                            createdBy = a.CREATEDBY,
                            creationMailSent = a.CREATIONMAILSENT,
                            customerCode = a.CUSTOMERCODE,
                            customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                            customerTypeId = (short)a.CUSTOMERTYPEID,
                            dateOfBirth = (DateTime)a.DATEOFBIRTH,
                            customerId = a.CUSTOMERID,
                            emailAddress = a.EMAILADDRESS,
                            firstName = a.FIRSTNAME,
                            gender = a.GENDER,
                            lastName = a.LASTNAME,
                            maidenName = a.MAIDENNAME,
                            maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : a.MARITALSTATUS.Value == 2 ? "F" : null,
                            title = a.TITLE,
                            middleName = a.MIDDLENAME,
                            customerTypeName = a.TBL_CUSTOMER_TYPE.NAME,
                            misCode = a.MISCODE,
                            misStaff = a.MISSTAFF,
                            nationalityId = a.NATIONALITYID,
                            occupation = a.OCCUPATION,
                            placeOfBirth = a.PLACEOFBIRTH,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                            spouse = a.SPOUSE,
                            sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            subSectorId = (short)a.SUBSECTORID,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            taxNumber = a.TAXNUMBER,
                            customerRating = a.CUSTOMERRATING,
                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                            businessUnitId = a.BUSINESSUNTID,
                            ownership = a.OWNERSHIP,
                            relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID)
                                .Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                            riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                            customerBVN = a.CUSTOMERBVN,
                            nameofSignatories = a.NAMEOFSIGNATORY,
                            addressofSignatories = a.ADDRESSOFSIGNATORY,
                            phoneNumberofSignatories = a.PHONENUMBEROFSIGNATORY,
                            emailofSignatories = a.EMAILOFSIGNATORY,
                            bvnNumberofSignatories = a.BVNNUMBEROFSIGNATORY,
                        }).FirstOrDefault();
            if (USE_THIRD_PARTY_INTEGRATION)
                data.isPoliticallyExposed = finacle.GetExposePersonStatus(data.customerCode);

            return data;
        }

        public CustomerViewModels GetSingleCustomerGeneralInfoByCustomerId(int customerId)
        {
            var data = (from a in context.TBL_CUSTOMER
                        where a.DELETED == false && a.CUSTOMERID == customerId
                        select new CustomerViewModels
                        {
                            crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                            crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                            crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                            accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                            branchId = a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            companyMainId = a.COMPANYID,
                            createdBy = a.CREATEDBY,
                            creationMailSent = a.CREATIONMAILSENT,
                            customerCode = a.CUSTOMERCODE,
                            // customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                            customerTypeId = a.CUSTOMERTYPEID,
                            dateOfBirth = a.DATEOFBIRTH,
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
                            misCode = a.MISCODE,
                            misStaff = a.MISSTAFF,
                            nationalityId = a.NATIONALITYID,
                            occupation = a.OCCUPATION,
                            placeOfBirth = a.PLACEOFBIRTH,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            spouse = a.SPOUSE,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            sectorId = (short)(from e in context.TBL_SECTOR where e.SECTORID == (short)a.TBL_SUB_SECTOR.SECTORID select e.SECTORID).FirstOrDefault(),  //a.TBL_SUB_SECTOR.TBL_SECTOR?.SECTORID == null ?? 0,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            subSectorId = a.SUBSECTORID,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            taxNumber = a.TAXNUMBER,
                            customerRating = a.CUSTOMERRATING,
                            ownership = a.OWNERSHIP,
                            relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID.Value)
                                .Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                            riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                            customerBVN = a.CUSTOMERBVN,
                            nameofSignatories = a.NAMEOFSIGNATORY,
                            addressofSignatories = a.ADDRESSOFSIGNATORY,
                            phoneNumberofSignatories = a.PHONENUMBEROFSIGNATORY,
                            emailofSignatories = a.EMAILOFSIGNATORY,
                            bvnNumberofSignatories = a.BVNNUMBEROFSIGNATORY,
                        }).FirstOrDefault();
            if (USE_THIRD_PARTY_INTEGRATION)
                data.isPoliticallyExposed = finacle.GetExposePersonStatus(data.customerCode);
            return data;
        }

        public CustomerViewModels GetSingleCustomerGeneralInfoByCustomerId(int customerId, int targetId)
        {
            var data = (from a in context.TBL_TEMP_CUSTOMER
                        where a.CUSTOMERID == customerId //&& a.TEMPCUSTOMERID == targetId
                        select new CustomerViewModels
                        {
                            accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                            branchId = a.BRANCHID,
                            branchName = context.TBL_BRANCH.FirstOrDefault(u => u.BRANCHID == a.BRANCHID).BRANCHNAME,
                            companyMainId = a.COMPANYID,
                            createdBy = a.CREATEDBY,
                            creationMailSent = a.CREATIONMAILSENT,
                            customerCode = a.CUSTOMERCODE,
                            customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                            customerTypeId = (short)a.CUSTOMERTYPEID,
                            dateOfBirth = (DateTime)a.DATEOFBIRTH,
                            customerId = a.CUSTOMERID,
                            emailAddress = a.EMAILADDRESS,
                            firstName = a.FIRSTNAME,
                            gender = a.GENDER,
                            lastName = a.LASTNAME,
                            maidenName = a.MAIDENNAME,
                            maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                            title = a.TITLE,
                            middleName = a.MIDDLENAME,
                            customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                            misCode = a.MISCODE,
                            misStaff = a.MISSTAFF,
                            nationalityId = a.NATIONALITYID,
                            occupation = a.OCCUPATION,
                            placeOfBirth = a.PLACEOFBIRTH,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                            spouse = a.SPOUSE,
                            crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                            crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                            crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                            crmsCompanySizeName = context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSCOMPANYSIZEID).CODE + "-" + context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSCOMPANYSIZEID).DESCRIPTION,
                            crmsLegalStatusName = context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSLEGALSTATUSID).CODE + "-" + context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSLEGALSTATUSID).DESCRIPTION,
                            crmsRelationshipTypeName = context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSRELATIONSHIPTYPEID).CODE + "-" + context.TBL_CRMS_REGULATORY.FirstOrDefault(x => x.CRMSREGULATORYID == a.CRMSRELATIONSHIPTYPEID).DESCRIPTION,


                            //  sectorId = context.TBL_SUB_SECTOR.FirstOrDefault(c=>c.SUBSECTORID == (short)a.SUBSECTORID).SECTORID,
                            // sectorName = context.TBL_SECTOR.FirstOrDefault(d=>d.TBL_SUB_SECTOR.FirstOrDefault(l=>l.SUBSECTORID==a.SUBSECTORID).NAME,
                            subSectorId = (short)a.SUBSECTORID,
                            subSectorName = context.TBL_SUB_SECTOR.FirstOrDefault(r => r.SUBSECTORID == a.SUBSECTORID).NAME,
                            taxNumber = a.TAXNUMBER,
                            ownership = a.OWNERSHIP,
                            relationshipOfficerName =
                                context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                                                                                                                      + context
                                                                                                                          .TBL_STAFF
                                                                                                                          .FirstOrDefault(
                                                                                                                              f =>
                                                                                                                                  f.STAFFID ==
                                                                                                                                  a.RELATIONSHIPOFFICERID)
                                                                                                                          .LASTNAME,
                            customerBVN = a.CUSTOMERBVN,
                        }).FirstOrDefault();

            return data;
        }

        public CustomerCompanyInfomationViewModels GetSingleCustomerCompanyInfo(int customerId)
        {
            var comany = (from d in context.TBL_CUSTOMER_COMPANYINFOMATION
                          where d.CUSTOMERID == customerId
                          select new CustomerCompanyInfomationViewModels()
                          {

                              annualTurnOver = d.ANNUALTURNOVER,
                              companyEmail = d.COMPANYEMAIL,
                              companyId = d.CUSTOMERID,
                              companyName = d.COMPANYNAME,
                              companyWebsite = d.COMPANYWEBSITE,
                              companyInfomationId = d.COMPANYINFOMATIONID,
                              corporateBusinessCategory = d.CORPORATEBUSINESSCATEGORY,
                              registeredOffice = d.REGISTEREDOFFICE,
                              registrationNumber = d.REGISTRATIONNUMBER,
                              paidUpCapital = d.PAIDUPCAPITAL,
                              authorizedCapital = d.AUTHORISEDCAPITAL,
                              shareholderFund = d.SHAREHOLDER_FUND,
                              numberOfEmployees = d.NUMBEROFEMPLOYEES,
                              noOfFemaleEmployees = d.NOOFFEMALEEMPLOYEES,
                              isStartUp = d.ISSTARTUP,
                              isFirstTimeCredit = d.ISFIRSTTIMECREDIT,
                              totalAssets = d.TOTALASSETS,
                              corporateCustomerTypeId = d.CORPORATECUSTOMERTYPEID,
                          }).FirstOrDefault();
            return comany;
        }

        public CustomerCompanyInfomationViewModels GetSingleCustomerCompanyInfo(int customerId, int targetId)
        {
            var comany = (from d in context.TBL_TEMP_CUSTOMER_COMPANYINFO
                          where d.CUSTOMERID == customerId //&& d.TEMPCOMPANYINFOMATIONID == targetId
                          select new CustomerCompanyInfomationViewModels()
                          {
                              annualTurnOver = d.ANNUALTURNOVER,
                              companyEmail = d.COMPANYEMAIL,
                              companyId = d.CUSTOMERID,
                              companyName = d.COMPANYNAME,
                              companyWebsite = d.COMPANYWEBSITE,
                              companyInfomationId = d.TEMPCOMPANYINFOMATIONID,
                              corporateBusinessCategory = d.CORPORATEBUSINESSCATEGORY,
                              registeredOffice = d.REGISTEREDOFFICE,
                              registrationNumber = d.REGISTRATIONNUMBER,
                              paidUpCapital = d.PAIDUPCAPITAL,
                              authorizedCapital = d.AUTHORISEDCAPITAL,
                              shareholderFund = d.SHAREHOLDER_FUND,
                              numberOfEmployees = d.NUMBEROFEMPLOYEES,
                              countryOfParentCompanyId =d.COUNTRYOFPARENTCOMPANYID,
                              companyStructure = d.COMPANYSTRUCTURE,
                              noOfFemaleEmployees = d.NOOFFEMALEEMPLOYEES,
                              isStartUp = d.ISSTARTUP,
                              isFirstTimeCredit = d.ISFIRSTTIMECREDIT,
                              totalAssets = d.TOTALASSETS,
                              corporateCustomerTypeId = d.CORPORATECUSTOMERTYPEID,
                          }).FirstOrDefault();
            return comany;
        }

        public IEnumerable<CustomerAddressViewModels> GetSingleCustomerAddressInfo(int customerId)
        {
            var address = (from x in context.TBL_CUSTOMER_ADDRESS
                           where x.CUSTOMERID == customerId
                           select new CustomerAddressViewModels()
                           {
                               address = x.ADDRESS,
                               addressTypeId = x.ADDRESSTYPEID,
                               cityId = x.CITYID,
                               city = context.TBL_CITY.Where(c => c.CITYID == x.CITYID).Select(s => s.CITYNAME).FirstOrDefault(),
                               customerId = x.CUSTOMERID,
                               homeTown = x.HOMETOWN,
                               nearestLandmark = x.NEARESTLANDMARK,
                               electricMeterNumber = x.ELECTRICMETERNUMBER,
                               pobox = x.POBOX,
                               stateId = x.STATEID,
                               stateName = x.TBL_STATE.STATENAME,
                               addressId = x.ADDRESSID,
                               localGovernmentId = x.LOCALGOVERNMENTID,
                               localGovernmentName = x.TBL_LOCALGOVERNMENT.NAME,
                               active = x.ACTIVE
                           }).ToList();
            return address;
        }

        public IEnumerable<CustomerAddressViewModels> GetSingleCustomerAddressInfo(int customerId, int targetId)
        {

            var address = (from x in context.TBL_TEMP_CUSTOMER_ADDRESS
                           where x.CUSTOMERID == customerId && x.TEMPADDRESSID == targetId
                           select new CustomerAddressViewModels()
                           {
                               address = x.ADDRESS,
                               addressTypeId = x.ADDRESSTYPEID,
                               cityId = x.CITYID,
                               customerId = x.CUSTOMERID,
                               homeTown = x.HOMETOWN,
                               nearestLandmark = x.NEARESTLANDMARK,
                               electricMeterNumber = x.ELECTRICMETERNUMBER,
                               pobox = x.POBOX,
                               stateId = x.STATEID,
                               stateName = x.TBL_STATE.STATENAME,
                               addressId = x.ADDRESSID,
                               active = x.ACTIVE,
                               localGovernmentName = x.TBL_LOCALGOVERNMENT.NAME
                           }).ToList();
            return address;
        }

        public IEnumerable<CustomerPhoneContactViewModels> GetSingleCustomerPhoneContactInfo(int customerId)
        {
            var phoneContact = (from c in context.TBL_CUSTOMER_PHONECONTACT
                                where c.CUSTOMERID == customerId
                                select new CustomerPhoneContactViewModels
                                {
                                    active = c.ACTIVE,
                                    customerId = c.CUSTOMERID,
                                    phone = c.PHONE,
                                    phoneContactId = c.PHONECONTACTID,
                                    phoneNumber = c.PHONENUMBER
                                }).ToList();
            return phoneContact;
        }

        public IEnumerable<CustomerPhoneContactViewModels> GetSingleCustomerPhoneContactInfo(int customerId,
            int targetId)
        {
            var phoneContact = (from c in context.TBL_TEMP_CUSTOMER_PHONCONTACT
                                where c.CUSTOMERID == customerId && c.TEMPPHONECONTACTID == targetId
                                select new CustomerPhoneContactViewModels
                                {
                                    active = c.ACTIVE,
                                    customerId = c.CUSTOMERID,
                                    phone = c.PHONE,
                                    phoneContactId = c.TEMPPHONECONTACTID,
                                    phoneNumber = c.PHONENUMBER
                                }).ToList();
            return phoneContact;
        }

        public IEnumerable<CustomerBvnViewModels> GetSingleCustomerBVNInfo(int customerId)
        {
            var customerBvn = (from b in context.TBL_CUSTOMER_BVN
                               where b.CUSTOMERID == customerId
                               select new CustomerBvnViewModels()
                               {
                                   bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                   customerBvnid = b.CUSTOMERBVNID,
                                   firstname = b.FIRSTNAME,
                                   isValidBvn = b.ISVALIDBVN,
                                   isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                   surname = b.SURNAME
                               }).ToList();
            return customerBvn;
        }

        public IEnumerable<CustomerIdentificationViewModels> GetSingleCustomerIdentificationInfo(int customerId)
        {
            var identification = (from e in context.TBL_CUSTOMER_IDENTIFICATION
                                  where e.CUSTOMERID == customerId
                                  select new CustomerIdentificationViewModels()
                                  {
                                      identificationId = e.IDENTIFICATIONID,
                                      identificationModeId = e.IDENTIFICATIONMODEID.Value,
                                      identificationMode = context.TBL_CUSTOMER_IDENTI_MODE_TYPE
                                          .FirstOrDefault(r => r.IDENTIFICATIONMODEID == e.IDENTIFICATIONMODEID).IDENTIFICATIONMODE,
                                      identificationNo = e.IDENTIFICATIONNO,
                                      issueAuthority = e.ISSUEAUTHORITY,
                                      issuePlace = e.ISSUEPLACE
                                  }).ToList();
            return identification;
        }

        public IEnumerable<CustomerEmploymentHistoryViewModels> GetSingleCustomerEmploymentHistoryInfo(int customerId)
        {
            var employmentHistory = (from s in context.TBL_CUSTOMER_EMPLOYMENTHISTORY
                                     where s.CUSTOMERID == customerId
                                     select new CustomerEmploymentHistoryViewModels()
                                     {
                                         active = s.ACTIVE,
                                         previousEmployer = s.PREVIOUSEMPLOYER,
                                         customerId = s.CUSTOMERID,
                                         employDate = s.EMPLOYDATE,
                                         placeOfWorkId = s.PLACEOFWORKID,
                                         employerAddress = s.EMPLOYERADDRESS,
                                         employerCountryId = s.EMPLOYERCOUNTRYID,
                                         employerName = s.EMPLOYERNAME,
                                         officePhone = s.OFFICEPHONE,
                                         employerStateId = s.EMPLOYERSTATEID,
                                         employerState = s.EMPLOYERSTATE,
                                         yearOfEmployment = s.YEAROFEMPLOYMENT,
                                         totalWorkingExperience = s.TOTALWORKINGEXPERIENCE,
                                         yearsOfCurrentEmployment = s.YEARSOFCURRENTEMPLOYMENT,
                                         terminalBenefits = s.TERMINALBENEFITS,
                                         annualIncome = s.ANNUALINCOME,
                                         monthlyIncome = s.MONTHLYINCOME,
                                         expenditure = s.EXPENDITURE,
                                         isEmployerRelated = s.ISEMPLOYERRELATED,
                                         approvedEmployerId = s.APPROVEDEMPLOYERID,
                                         employerId = s.APPROVEDEMPLOYERID
                                     }).ToList();
            return employmentHistory;
        }

        public CustomerEmploymentHistoryViewModels GetSingleCustomerRelatedEmployer(int customerId)
        {
            var employers = GetSingleCustomerEmploymentHistoryInfo(customerId);
            var activeEmployers = employers.Where(e => e.active && e.employerId > 0).ToList();
            if (activeEmployers.Count > 1)
            {
                throw new SecureException("There cannot be more than one active current employer setup for a customer!");
            }

            var relatedEmployer = employers.Where(e => e.active && e.employerId > 0).FirstOrDefault();
            if (relatedEmployer == null)
            {
                throw new SecureException("There is no active Related Employer setup for this customer!");
            }
            return relatedEmployer;
        }

        public IEnumerable<CustomerEmploymentHistoryViewModels> GetSingleCustomerEmploymentHistoryInfo(int customerId,
            int targetId)
        {
            var employmentHistory = (from s in context.TBL_TEMP_CUSTOMEREMPLOYMENT
                                     where s.CUSTOMERID == customerId && s.TEMPPLACEOFWORKID == targetId
                                     select new CustomerEmploymentHistoryViewModels()
                                     {
                                         active = s.ACTIVE,
                                         previousEmployer = s.PREVIOUSEMPLOYER,
                                         customerId = s.CUSTOMERID,
                                         employDate = s.EMPLOYDATE,
                                         placeOfWorkId = s.TEMPPLACEOFWORKID,
                                         employerAddress = s.EMPLOYERADDRESS,
                                         employerCountryId = s.EMPLOYERCOUNTRYID,
                                         employerName = s.EMPLOYERNAME,
                                         officePhone = s.OFFICEPHONE,
                                         employerStateId = s.EMPLOYERSTATEID,
                                         employerState = s.EMPLOYERSTATE,
                                         yearOfEmployment = s.YEAROFEMPLOYMENT,
                                         totalWorkingExperience = s.TOTALWORKINGEXPERIENCE,
                                         yearsOfCurrentEmployment = s.YEARSOFCURRENTEMPLOYMENT,
                                         terminalBenefits = s.TERMINALBENEFITS,
                                         annualIncome = s.ANNUALINCOME,
                                         monthlyIncome = s.MONTHLYINCOME,
                                         expenditure =s.EXPENDITURE,
                                         isEmployerRelated = s.ISEMPLOYERRELATED,
                                         approvedEmployerId = s.APPROVEDEMPLOYERID
                                     }).ToList();
            return employmentHistory;
        }

        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId,
            short directorTypeId)
        {
            var companyDirectors = (from s in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                    join dt in context.TBL_CUSTOMER_COMPANY_DIREC_TYP on s.COMPANYDIRECTORTYPEID equals dt.COMPANYDIRECTORYTYPEID
                                    where s.CUSTOMERID == customerId && s.COMPANYDIRECTORTYPEID == directorTypeId
                                    select new CustomerCompanyDirectorsViewModels()
                                    {
                                        companyDirectorId = s.COMPANYDIRECTORID,
                                        surname = s.SURNAME,
                                        firstname = s.FIRSTNAME,
                                        middlename = s.MIDDLENAME,
                                        customerNIN = s.CUSTOMERNIN,
                                        numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                        isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                        isThePromoter = s.ISTHEPROMOTER,
                                        bankVerificationNumber = s.CUSTOMERBVN,
                                        companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                        rcNumber = s.REGISTRATION_NUMBER,
                                        taxNumber = s.TAX_NUMBER,
                                        companyDirectorTypeName = dt.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                        dateOfBirth = s.DATEOFBIRTH,
                                        gender = s.GENDER,
                                        maritalStatusId = s.MARITALSTATUSID,
                                        customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA
                                            .Where(a => a.COMPANYDIRECTORID == s.COMPANYDIRECTORID && a.DELETED == false).Select(x =>
                                                new CustomerCompanyBeneficiaryViewModels()
                                                {
                                                    companyBeneficiaryId = x.COMPANY_BENEFICIARYID,
                                                    companyDirectorId = x.COMPANYDIRECTORID,
                                                    surname = x.SURNAME,
                                                    firstname = x.FIRSTNAME,
                                                    numberOfShares = x.NUMBEROFSHARES,
                                                    bankVerificationNumber = x.CUSTOMERBVN,
                                                    isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                                    address = x.ADDRESS,
                                                    phoneNumber = x.PHONENUMBER,
                                                    email = x.EMAILADDRESS,
                                                }).ToList()
                                    }).ToList();
            return companyDirectors;
        }

        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId,
            short directorTypeId, int targetId)
        {
            var companyDirectors = (from s in context.TBL_TEMP_CUSTOMER_DIRECTOR
                                    join dt in context.TBL_CUSTOMER_COMPANY_DIREC_TYP on s.COMPANYDIRECTORTYPEID equals dt.COMPANYDIRECTORYTYPEID
                                    where s.CUSTOMERID == customerId && s.COMPANYDIRECTORTYPEID == directorTypeId
                                                 && s.TEMPCOMPANYDIRECTORID == targetId
                                    select new CustomerCompanyDirectorsViewModels()
                                    {
                                        companyDirectorId = s.TEMPCOMPANYDIRECTORID,
                                        surname = s.SURNAME,
                                        firstname = s.FIRSTNAME,
                                        middlename = s.MIDDLENAME,
                                        customerNIN = s.CUSTOMERNIN,
                                        numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                        isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                        isThePromoter = s.ISTHEPROMOTER,
                                        bankVerificationNumber = s.CUSTOMERBVN,
                                        companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                        rcNumber = s.REGISTRATION_NUMBER,
                                        taxNumber = s.TAX_NUMBER,
                                        companyDirectorTypeName = dt.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                        //dateOfBirth = s.DATEOFBIRTH,
                                        //gender = s.GENDER,
                                        //maritalStatusId = s.MARITALSTATUSID,
                                        customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA
                                            .Where(a => a.COMPANYDIRECTORID == s.COMPANYDIRECTORID && a.DELETED == false).Select(x =>
                                                new CustomerCompanyBeneficiaryViewModels()
                                                {
                                                    companyBeneficiaryId = x.COMPANY_BENEFICIARYID,
                                                    companyDirectorId = x.COMPANYDIRECTORID,
                                                    surname = x.SURNAME,
                                                    firstname = x.FIRSTNAME,
                                                    numberOfShares = x.NUMBEROFSHARES,
                                                    bankVerificationNumber = x.CUSTOMERBVN,
                                                    isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                                    address = x.ADDRESS,
                                                    phoneNumber = x.PHONENUMBER,
                                                    email = x.EMAILADDRESS,
                                                }).ToList()
                                    }).ToList();
            return companyDirectors;
        }

        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId,
            short customerTypeId)
        {
            var companyDirectors = (from s in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                    join dt in context.TBL_CUSTOMER_COMPANY_DIREC_TYP on s.COMPANYDIRECTORTYPEID equals dt.COMPANYDIRECTORYTYPEID
                                    where s.CUSTOMERID == customerId &&
                      s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder &&
                      s.CUSTOMERTYPEID == customerTypeId
                                    select new CustomerCompanyDirectorsViewModels()
                                    {
                                        customerTypeId = s.CUSTOMERTYPEID,
                                        companyDirectorId = s.COMPANYDIRECTORID,
                                        surname = s.SURNAME,
                                        firstname = s.FIRSTNAME,
                                        middlename = s.MIDDLENAME,
                                        customerNIN = s.CUSTOMERNIN,
                                        numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                        isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                        bankVerificationNumber = s.CUSTOMERBVN,
                                        companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                        rcNumber = s.REGISTRATION_NUMBER,
                                        taxNumber = s.TAX_NUMBER,
                                        companyDirectorTypeName = dt.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                        dateOfBirth = s.DATEOFBIRTH,
                                        gender = s.GENDER,
                                        maritalStatusId = s.MARITALSTATUSID
                                    }).ToList();
            return companyDirectors;
        }

        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId,
            short customerTypeId, int targetId)
        {
            var companyDirectors = (from s in context.TBL_TEMP_CUSTOMER_DIRECTOR
                                    join dt in context.TBL_CUSTOMER_COMPANY_DIREC_TYP on s.COMPANYDIRECTORTYPEID equals dt.COMPANYDIRECTORYTYPEID
                                    where s.CUSTOMERID == customerId &&
                                          s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder &&
                                          s.CUSTOMERTYPEID == customerTypeId
                                          && s.TEMPCOMPANYDIRECTORID == targetId
                                    select new CustomerCompanyDirectorsViewModels()
                                    {
                                        customerTypeId = s.CUSTOMERTYPEID,
                                        companyDirectorId = s.TEMPCOMPANYDIRECTORID,
                                        surname = s.SURNAME,
                                        firstname = s.FIRSTNAME,
                                        middlename = s.MIDDLENAME,
                                        customerNIN = s.CUSTOMERNIN,
                                        numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                                        isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                        bankVerificationNumber = s.CUSTOMERBVN,
                                        companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                        rcNumber = s.REGISTRATION_NUMBER,
                                        taxNumber = s.TAX_NUMBER,
                                        companyDirectorTypeName = dt.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                        //dateOfBirth = s.DATEOFBIRTH,
                                        //gender = s.GENDER,
                                        //maritalStatusId = s.MARITALSTATUSID,
                                    }).ToList();
            return companyDirectors;
        }

        public IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId,
            short clientTypeId)
        {
            var clientOrSupplier = (from cs in context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                    join ct in context.TBL_CUSTOMER_TYPE on cs.CUSTOMERTYPEID equals ct.CUSTOMERTYPEID
                                    where cs.CUSTOMERID == customerId && cs.CLIENT_SUPPLIERTYPEID == clientTypeId
                                    select new CustomerClientOrSupplierViewModels()
                                    {
                                        customerTypeId = cs.CUSTOMERTYPEID,
                                        customerTypeName = ct.NAME,
                                        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                                        firstName = cs.FIRSTNAME,
                                        middleName = cs.MIDDLENAME,
                                        lastName = cs.LASTNAME,
                                        taxNumber = cs.TAX_NUMBER,
                                        rcNumber = cs.REGISTRATION_NUMBER,
                                        hasCASAAccount = cs.HAS_CASA_ACCOUNT,
                                        bankName = cs.BANKNAME,
                                        casaAccountNumber = cs.CASA_ACCOUNTNO,
                                        contactPerson = cs.CONTACT_PERSON,
                                        natureOfBusiness = cs.NATURE_OF_BUSINESS,
                                        client_SupplierAddress = cs.ADDRESS,
                                        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                        client_SupplierEmail = cs.EMAILADDRESS,
                                        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                    }).ToList();
            return clientOrSupplier;
        }

        public IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId,
            short clientTypeId, int targetId)
        {
            var clientOrSupplier = (from cs in context.TBL_TEMP_CUST_CLIENT_SUPPLIER
                                    where cs.CUSTOMERID == customerId && cs.CLIENT_SUPPLIERTYPEID == clientTypeId
                                    && cs.TEMPCLIENT_SUPPLIERID == targetId
                                    select new CustomerClientOrSupplierViewModels()
                                    {
                                        customerTypeId = cs.CUSTOMERTYPEID,
                                        // customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(x => x.CUSTOMERTYPEID == cs.CUSTOMERTYPEID).NAME,
                                        client_SupplierId = cs.TEMPCLIENT_SUPPLIERID,
                                        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                                        firstName = cs.FIRSTNAME,
                                        middleName = cs.MIDDLENAME,
                                        lastName = cs.LASTNAME,
                                        taxNumber = cs.TAX_NUMBER,
                                        rcNumber = cs.REGISTRATION_NUMBER,
                                        hasCASAAccount = cs.HAS_CASA_ACCOUNT,
                                        bankName = cs.BANKNAME,
                                        casaAccountNumber = cs.CASA_ACCOUNTNO,
                                        contactPerson = cs.CONTACT_PERSON,
                                        natureOfBusiness = cs.NATURE_OF_BUSINESS,
                                        client_SupplierAddress = cs.ADDRESS,
                                        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                        client_SupplierEmail = cs.EMAILADDRESS,
                                        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                        //  client_SupplierTypeName = context.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.FirstOrDefault(x => x.CLIENT_SUPPLIERTYPEID == cs.CLIENT_SUPPLIERTYPEID).CLIENT_SUPPLIERTYPENAME
                                    }).ToList();
            return clientOrSupplier;
        }

        public IEnumerable<CustomerChildrenViewModel> GetSingleCustomerChildrenInfo(int customerId)
        {
            var children = (from chd in context.TBL_CUSTOMER_CHILDREN
                            where chd.CUSTOMERID == customerId
                            select new CustomerChildrenViewModel()
                            {
                                customerChildrenId = chd.CUSTOMERCHILDRENID,
                                customerId = chd.CUSTOMERID,
                                childName = chd.CHILDNAME,
                                childDateOfBirth = chd.CHILDDATEOFBIRTH
                            }).ToList();
            return children;
        }

        public IEnumerable<CustomerCompanyBeneficiaryViewModels> GetShareholderUltimateBeneficial(int companyDirectorId)
        {
            var customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA
                .Where(a => a.COMPANYDIRECTORID == companyDirectorId && a.DELETED == false).Select(x =>
                    new CustomerCompanyBeneficiaryViewModels()
                    {
                        companyBeneficiaryId = x.COMPANY_BENEFICIARYID,
                        companyDirectorId = x.COMPANYDIRECTORID,
                        surname = x.SURNAME,
                        firstname = x.FIRSTNAME,
                        numberOfShares = x.NUMBEROFSHARES,
                        bankVerificationNumber = x.CUSTOMERBVN,
                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                        address = x.ADDRESS,
                        phoneNumber = x.PHONENUMBER,
                        email = x.EMAILADDRESS,
                    }).ToList();
            return customerCompanyBeneficial;
        }

        public IEnumerable<CasaViewModel> GetCustomerCASAInformation(int customerId)
        {
            var data = new List<CasaViewModel>();

            var casaInformation = context.TBL_CASA.Where(a => a.CUSTOMERID == customerId).Select(x =>
                    new CasaViewModel()
                    {
                        casaAccountId = x.CASAACCOUNTID,
                        productAccountNumber = x.PRODUCTACCOUNTNUMBER,
                        productAccountName = x.PRODUCTACCOUNTNAME,
                        isCurrentAccount = x.ISCURRENTACCOUNT,
                        customerId = x.CUSTOMERID,
                        productId = x.PRODUCTID,
                        productCode = x.TBL_PRODUCT.PRODUCTCODE,
                        productName = x.TBL_PRODUCT.PRODUCTNAME,
                        branchId = x.BRANCHID,
                        branchCode = x.TBL_BRANCH.BRANCHCODE,
                        branchName = x.TBL_BRANCH.BRANCHNAME,
                        currencyId = x.CURRENCYID,
                        currency = x.TBL_CURRENCY.CURRENCYNAME,
                        availableBalance = x.AVAILABLEBALANCE,
                        ledgerBalance = x.LEDGERBALANCE,
                        accountStatusName = x.TBL_CASA_ACCOUNTSTATUS.ACCOUNTSTATUSNAME,
                        relationshipManagerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                        relationshipOfficerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,
                        hasOverdraft = x.HASOVERDRAFT,
                        hasLien = x.HASLIEN
                    }).ToList();

            if(casaInformation.Count() <= 0)
            {
                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var customerinfo = (from a in context.TBL_CUSTOMER
                                        where a.CUSTOMERID == customerId
                                        select new CasaViewModel
                                        {
                                            customerCode = a.CUSTOMERCODE,
                                        }).ToList();

                    if (customerinfo.Count > 0)
                    {
                        data = finacle.GetCustomerAccountsBalanceByCustomerCode(customerinfo[0].customerCode);

                        //Task.Run(async () =>
                        //{
                        //    data = await _customer.GetCustomerAccountsBalanceByCustomerCode(
                        //        customerinfo[0].customerCode);
                        //}).GetAwaiter().GetResult();
                        // return data.ToList();

                    }

                    return data;
                }
                else
                {
                    var casaInformation2 = context.TBL_CASA.Where(a => a.CUSTOMERID == customerId).Select(x =>
                        new CasaViewModel()
                        {
                            casaAccountId = x.CASAACCOUNTID,
                            productAccountNumber = x.PRODUCTACCOUNTNUMBER,
                            productAccountName = x.PRODUCTACCOUNTNAME,
                            isCurrentAccount = x.ISCURRENTACCOUNT,
                            customerId = x.CUSTOMERID,
                            productId = x.PRODUCTID,
                            productCode = x.TBL_PRODUCT.PRODUCTCODE,
                            productName = x.TBL_PRODUCT.PRODUCTNAME,
                            branchId = x.BRANCHID,
                            branchCode = x.TBL_BRANCH.BRANCHCODE,
                            branchName = x.TBL_BRANCH.BRANCHNAME,
                            currencyId = x.CURRENCYID,
                            currency = x.TBL_CURRENCY.CURRENCYNAME,
                            availableBalance = x.AVAILABLEBALANCE,
                            ledgerBalance = x.LEDGERBALANCE,
                            accountStatusName = x.TBL_CASA_ACCOUNTSTATUS.ACCOUNTSTATUSNAME,
                            relationshipManagerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                            relationshipOfficerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,
                            hasOverdraft = x.HASOVERDRAFT,
                            hasLien = x.HASLIEN
                        }).ToList();
                    return casaInformation2;

                }
            }
            return casaInformation;
        }

           

        public IEnumerable<CasaViewModel> GetCustomerCASAInformation(string customerCode)
        {
            var data = new List<CasaViewModel>();

            if (USE_THIRD_PARTY_INTEGRATION)
            {
                data = finacle.GetCustomerAccountsBalanceByCustomerCode(customerCode); ;

                return data;
            }
            else
            {
                var casaInformation = context.TBL_CASA.Where(a => a.CUSTOMERID == customerId).Select(x =>
                    new CasaViewModel()
                    {
                        casaAccountId = x.CASAACCOUNTID,
                        productAccountNumber = x.PRODUCTACCOUNTNUMBER,
                        productAccountName = x.PRODUCTACCOUNTNAME,
                        isCurrentAccount = x.ISCURRENTACCOUNT,
                        customerId = x.CUSTOMERID,
                        productId = x.PRODUCTID,
                        productCode = x.TBL_PRODUCT.PRODUCTCODE,
                        productName = x.TBL_PRODUCT.PRODUCTNAME,
                        branchId = x.BRANCHID,
                        branchCode = x.TBL_BRANCH.BRANCHCODE,
                        branchName = x.TBL_BRANCH.BRANCHNAME,
                        currencyId = x.CURRENCYID,
                        currency = x.TBL_CURRENCY.CURRENCYNAME,
                        availableBalance = x.AVAILABLEBALANCE,
                        ledgerBalance = x.LEDGERBALANCE,
                        accountStatusName = x.TBL_CASA_ACCOUNTSTATUS.ACCOUNTSTATUSNAME,
                        relationshipManagerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                        relationshipOfficerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,
                        hasOverdraft = x.HASOVERDRAFT,
                        hasLien = x.HASLIEN
                    }).ToList();
                return casaInformation;

            }
        }


        public IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId)
        {
            var nextOfKin = context.TBL_CUSTOMER_NEXTOFKIN.Where(a => a.CUSTOMERID == customerId).Select(x =>
                new CustomerNextOfKinViewModels()
                {
                    nextOfKinId = x.NEXTOFKINID,
                    customerId = x.CUSTOMERID,
                    firstName = x.FIRSTNAME,
                    lastName = x.LASTNAME,
                    phoneNumber = x.PHONENUMBER,
                    dateOfBirth = x.DATEOFBIRTH,
                    gender = x.GENDER,
                    relationship = x.RELATIONSHIP,
                    email = x.EMAIL,
                    address = x.ADDRESS,
                    nearestLandmark = x.NEAREST_LANDMARK,
                    stateId = x.TBL_CITY.TBL_LOCALGOVERNMENT.STATEID,
                    cityId = x.CITYID,
                    active = x.ACTIVE,
                }).ToList();
            return nextOfKin;
        }

        public IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId, int targetId)
        {
            var nextOfKin = context.TBL_TEMP_CUSTOMER_NEXTOFKIN
                .Where(a => a.CUSTOMERID == customerId && a.TEMPNEXTOFKINID == targetId).Select(x =>
                    new CustomerNextOfKinViewModels()
                    {
                        nextOfKinId = x.TEMPNEXTOFKINID,
                        customerId = x.CUSTOMERID,
                        firstName = x.FIRSTNAME,
                        lastName = x.LASTNAME,
                        phoneNumber = x.PHONENUMBER,
                        dateOfBirth = x.DATEOFBIRTH,
                        gender = x.GENDER,
                        relationship = x.RELATIONSHIP,
                        email = x.EMAIL,
                        address = x.ADDRESS,
                        nearestLandmark = x.NEAREST_LANDMARK,
                        stateId =
                            context.TBL_CITY.FirstOrDefault(k => k.CITYID == x.CITYID).TBL_LOCALGOVERNMENT.STATEID,
                        cityId = x.CITYID,
                        active = x.ACTIVE,
                    }).ToList();
            return nextOfKin;
        }

        public IEnumerable<GroupCustomerMembersViewModel> GetCustomerAndType(int custormerId)
        {
            List<GroupCustomerMembersViewModel> lstCustomer = new List<GroupCustomerMembersViewModel>();
            var data = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == custormerId)
            //var data = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == custormerId && c.ACCOUNTCREATIONCOMPLETE == true)
                .Select(c => new GroupCustomerMembersViewModel()
                {
                    customerId = c.CUSTOMERID,
                    firstName = c.FIRSTNAME + " " + c.MIDDLENAME,
                    lastName = c.LASTNAME,
                    customerTypeId = c.CUSTOMERTYPEID,
                    customerType = c.TBL_CUSTOMER_TYPE.NAME,
                    isProspect = c.ISPROSPECT,
                    customerRating = c.CUSTOMERRATING,
                    customerCode = c.CUSTOMERCODE
                }).ToList();

            return data;
            //foreach (var item in data)
            //{
            //    if (bureau.VerifyCustomerValidCreditBureau(item.customerId))
            //    {
            //        lstCustomer.Add(item);
            //    }
            //}

            // return lstCustomer;
        }

        #endregion

        #region Customer Information Validation

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

        public bool ValidateCustomerBVN(int customerId, string customerBvn)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                        where a.CUSTOMERID == customerId && a.CUSTOMERBVN == customerBvn
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateCustomerRCnumber(int customerId, string rcNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                        where a.CUSTOMERID == customerId && a.REGISTRATION_NUMBER == rcNumber
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateCustomerTIN(int customerId, string tin)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                        where a.CUSTOMERID == customerId && a.TAX_NUMBER == tin
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateCustomerEmail(int customerId, string email)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                        where a.CUSTOMERID == customerId && a.EMAILADDRESS == email
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        //TBL_CUSTOMER_CLIENT_SUPPLIER
        public bool ValidateClientSupplierEmail(int customerId, string email)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER
                        where a.CUSTOMERID == customerId && a.EMAILADDRESS == email
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateClientSupplierRCnumber(int customerId, string rcNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER
                        where a.CUSTOMERID == customerId && a.REGISTRATION_NUMBER == rcNumber
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateClientSupplierTIN(int customerId, string taxNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER
                        where a.CUSTOMERID == customerId && a.TAX_NUMBER == taxNumber
                        select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool CustomerInformationCompleted(int customerId, UserInfo user)
        {
            var data = (from a in context.TBL_CUSTOMER where a.CUSTOMERID == customerId select a).FirstOrDefault();
            if (data == null) return false;
            data.LASTUPDATEDBY = user.staffId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.ACCOUNTCREATIONCOMPLETE = true;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Updated customer information for : + (" + data.FIRSTNAME + " " + data.LASTNAME + ") ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges() != 0;
            return response;
        }

        public bool ValidateCustomerModification(int customerId)
        {
            bool itemExist = false;
            var unApprovedCustomerUpdate =
                context.TBL_CUSTOMER_MODIFICATION.Where(x =>
                    x.APPROVALCOMPLETED == false && x.CUSTOMERID == customerId);
            if (unApprovedCustomerUpdate.Any())
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateModifiedCustomerRecord(int customerId)
        {
            bool itemExist = false;
            var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER.Where(x =>
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                x.CUSTOMERID == customerId);
            if (unApprovedCustomerUpdate.Any())
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateModifiedCompanyRecord(int customerId)
        {
            bool itemExist = false;
            var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER_COMPANYINFO.Where(x =>
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                x.CUSTOMERID == customerId);
            if (unApprovedCustomerUpdate.Any())
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateModifiedAddressRecord(int customerId)
        {
            bool itemExist = false;
            var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER_ADDRESS.Where(x =>
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                x.CUSTOMERID == customerId);
            if (unApprovedCustomerUpdate.Any())
            {
                itemExist = true;
            }

            return itemExist;
        }

        public bool ValidateModifiedPhoneRecord(int customerId)
        {
            bool itemExist = false;
            var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER_PHONCONTACT.Where(x =>
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                x.CUSTOMERID == customerId);
            if (unApprovedCustomerUpdate.Any())
            {
                itemExist = true;
            }

            return itemExist;
        }

        #endregion

        public IEnumerable<CustomerInformationApprovalViemModel> GetAllCustomerInformationAwaitingApproval(int staffId,
            int companyId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CustomerInformationApproval)
                .ToList();

            return (from a in context.TBL_CUSTOMER_MODIFICATION
                    join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                    join br in context.TBL_BRANCH on b.BRANCHID equals br.BRANCHID
                    join st in context.TBL_STAFF on a.CREATEDBY equals st.STAFFID
                    join c in context.TBL_APPROVAL_TRAIL on a.CUSTOMERMODIFICATIONID equals c.TARGETID
                    join e in context.TBL_APPROVAL_STATUS on c.APPROVALSTATUSID equals e.APPROVALSTATUSID
                    where
                        (c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending ||
                         c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                        && a.APPROVALCOMPLETED == false 
                        //&& a.CREATEDBY == staffId
                        && c.RESPONSESTAFFID == null
                        && c.OPERATIONID == (int)OperationsEnum.CustomerInformationApproval
                        && ids.Contains((int)c.TOAPPROVALLEVELID)
                    orderby a.DATETIMECREATED descending

                    select new CustomerInformationApprovalViemModel
                    {
                        customerId = a.CUSTOMERID,
                        customerCode = b.CUSTOMERCODE,
                        customerModificationId = a.CUSTOMERMODIFICATIONID,
                        targetId = a.TARGETID,
                        customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                        modificationTyepId = a.MODIFICATIONTYPEID,
                        modificationType = a.TBL_CUSTOMER_MODIFICATN_TYPE.MODIFICATIONTYPENAME,
                        approvalStatus = e.APPROVALSTATUSNAME,
                        dateUpdated = a.DATETIMECREATED,
                        createdBy = st.FIRSTNAME + " " + st.LASTNAME,
                        customerBranch = br.BRANCHNAME,
                        operationId = (int)OperationsEnum.CustomerInformationApproval
                    }).ToList();

        }

        public int GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.CustomerInformationApproval;
            entity.externalInitialization = false;
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.StaffId = entity.staffId;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                    workflow.TargetId = entity.targetId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;

                    workflow.LogActivity();

                    if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var mod = context.TBL_CUSTOMER_MODIFICATION.Find(entity.targetId);
                        if (mod != null)
                        {
                            var pp = mod.MODIFICATIONTYPEID;
                            UpdateCustomerInformationDisapproval(mod.MODIFICATIONTYPEID, entity.targetId);
                            mod.APPROVALCOMPLETED = true;
                            context.SaveChanges();
                            trans.Commit();
                            return 2;
                        }
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveCustomerInformation(entity.targetId, (short)workflow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }

                        return 1;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool ApproveCustomerInformation(int modifiedId, short approvalStatusId, UserInfo user)
        {
            bool returnVal = false;
            //Check for the modified record 
            var modifiedData = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modifiedData != null)
            {
                if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.General_Information)
                {
                    returnVal = ApproveGeneralInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Corporate_Information)
                {
                    returnVal = ApproveCompanyInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Address_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Address_Modification)
                {
                    returnVal = ApproveAddressInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Phone_Number_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Phone_Number_Modification)
                {
                    returnVal = ApprovePhoneContactInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Employement_History_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Employment_History_Modification)
                {
                    returnVal = ApproveEmploymentHistoryInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Next_of_Kin_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Next_of_Kin_Addition)
                {
                    returnVal = ApproveNextOfKinInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Client_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Client_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Suplier_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Supplier_Addition)
                {
                    returnVal = ApproveClientSupplierInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Director_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Director_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int)CustomerInformationTrackerEnum.Shareholder_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Shareholder_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Signatory_Adition ||
                         modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Signatory_Modification)
                {
                    returnVal = ApproveDirectorInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }

            }

            return returnVal;
        }

        private bool ApproveGeneralInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_CUSTOMER entity = null;
            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);

            //Check if Customer  exist in the temp table using the customerId
            var temp = context.TBL_TEMP_CUSTOMER.OrderByDescending(x => x.TEMPCUSTOMERID).FirstOrDefault(x => x.CUSTOMERID == targetId);

            if (temp != null) //If temp record is not null select the information from the main table
            {
                entity = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == targetId);
            }

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            if (entity != null) //Update existing customer information with temp record
            {

                detail = $"Approved General Customer Information for customer with code: {entity.CUSTOMERCODE} has been updated by {staff.FIRSTNAME} {staff.LASTNAME} ({staff.STAFFCODE})";

                entity.CRMSCOMPANYSIZEID = temp.CRMSCOMPANYSIZEID;
                entity.CRMSLEGALSTATUSID = temp.CRMSLEGALSTATUSID;
                entity.CRMSRELATIONSHIPTYPEID = temp.CRMSRELATIONSHIPTYPEID;
                entity.ACCOUNTCREATIONCOMPLETE = temp.ACCOUNTCREATIONCOMPLETE;
                entity.CREATIONMAILSENT = temp.CREATIONMAILSENT;
                entity.CUSTOMERCODE = temp.CUSTOMERCODE;
                entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                entity.EMAILADDRESS = temp.EMAILADDRESS;
                entity.FIRSTNAME = temp.FIRSTNAME;
                entity.GENDER = temp.GENDER;
                entity.LASTNAME = temp.LASTNAME;
                entity.MAIDENNAME = temp.MAIDENNAME;
                entity.MARITALSTATUS = temp.MARITALSTATUS;
                entity.TITLE = temp.TITLE;
                entity.MIDDLENAME = temp.MIDDLENAME;
                entity.MISCODE = temp.MISCODE;
                entity.MISSTAFF = temp.MISSTAFF;
                entity.NATIONALITYID = temp.NATIONALITYID;
                entity.OCCUPATION = temp.OCCUPATION;
                entity.PLACEOFBIRTH = temp.PLACEOFBIRTH;
                entity.ISINVESTMENTGRADE = temp.ISINVESTMENTGRADE;
                entity.ISREALATEDPARTY = temp.ISREALATEDPARTY;
                entity.SPOUSE = temp.SPOUSE;
                entity.SUBSECTORID = temp.SUBSECTORID;
                entity.TAXNUMBER = temp.TAXNUMBER;
                entity.CUSTOMERBVN = temp.CUSTOMERBVN;
                entity.ISPOLITICALLYEXPOSED = temp.ISPOLITICALLYEXPOSED;
                entity.RELATIONSHIPOFFICERID = temp.RELATIONSHIPOFFICERID;
                entity.BUSINESSUNTID = temp.BUSINESSUNTID;
            }

            UpdateCustomerCollateralId(entity.CUSTOMERCODE);
            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = detail, //"Approved Customer Information for customer with code: " + entity.CUSTOMERCODE,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveCompanyInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            var customer = context.TBL_CUSTOMER.Find(targetId);
            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);
            TBL_CUSTOMER_COMPANYINFOMATION entity = null;
            //Check if Customer company information exist in the temp table using the customerId
            var temp = context.TBL_TEMP_CUSTOMER_COMPANYINFO.FirstOrDefault(x => x.CUSTOMERID == targetId);
            if (temp != null) //If temp record is not null select the information from the main table
            {
                entity = context.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == targetId);
            }

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            if (entity != null) //Update existing customer company information with temp record
            {
                detail = $"Approved Company Information for customer with code: : {customer.CUSTOMERCODE} has been updated by {staff.FIRSTNAME} {staff.LASTNAME} ({staff.STAFFCODE})";
                entity.ANNUALTURNOVER = temp.ANNUALTURNOVER;
                entity.COMPANYEMAIL = temp.COMPANYEMAIL;
                entity.COMPANYNAME = temp.COMPANYNAME;
                entity.COMPANYWEBSITE = temp.COMPANYWEBSITE;
                entity.CORPORATEBUSINESSCATEGORY = temp.CORPORATEBUSINESSCATEGORY;
                entity.CUSTOMERID = temp.CUSTOMERID;
                entity.REGISTEREDOFFICE = temp.REGISTEREDOFFICE;
                entity.REGISTRATIONNUMBER = temp.REGISTRATIONNUMBER;
                entity.PAIDUPCAPITAL = temp.PAIDUPCAPITAL;
                entity.AUTHORISEDCAPITAL = temp.AUTHORISEDCAPITAL;
                entity.SHAREHOLDER_FUND = temp.SHAREHOLDER_FUND;
                entity.NOOFFEMALEEMPLOYEES = temp.NOOFFEMALEEMPLOYEES;
                entity.ISSTARTUP = temp.ISSTARTUP;
                entity.ISFIRSTTIMECREDIT = temp.ISFIRSTTIMECREDIT;
                entity.NUMBEROFEMPLOYEES = temp.NUMBEROFEMPLOYEES;
                entity.TOTALASSETS = temp.TOTALASSETS;
                entity.CORPORATECUSTOMERTYPEID = temp.CORPORATECUSTOMERTYPEID;
            }
            else
            {
                detail = $"Approved Company Information for customer with code: : {customer.CUSTOMERCODE} has been updated by {staff.FIRSTNAME} {staff.LASTNAME} ({staff.STAFFCODE})";
                var corporateInfo = new TBL_CUSTOMER_COMPANYINFOMATION();
                corporateInfo.ANNUALTURNOVER = temp.ANNUALTURNOVER;
                corporateInfo.COMPANYEMAIL = temp.COMPANYEMAIL;
                corporateInfo.COMPANYNAME = temp.COMPANYNAME;
                corporateInfo.COMPANYWEBSITE = temp.COMPANYWEBSITE;
                corporateInfo.CORPORATEBUSINESSCATEGORY = temp.CORPORATEBUSINESSCATEGORY;
                corporateInfo.CUSTOMERID = temp.CUSTOMERID;
                corporateInfo.REGISTEREDOFFICE = temp.REGISTEREDOFFICE;
                corporateInfo.REGISTRATIONNUMBER = temp.REGISTRATIONNUMBER;
                corporateInfo.PAIDUPCAPITAL = temp.PAIDUPCAPITAL;
                corporateInfo.AUTHORISEDCAPITAL = temp.AUTHORISEDCAPITAL;
                corporateInfo.SHAREHOLDER_FUND = temp.SHAREHOLDER_FUND;
                corporateInfo.NOOFFEMALEEMPLOYEES = entity.NOOFFEMALEEMPLOYEES;
                corporateInfo.ISSTARTUP = entity.ISSTARTUP;
                corporateInfo.ISFIRSTTIMECREDIT = entity.ISFIRSTTIMECREDIT;
                corporateInfo.NUMBEROFEMPLOYEES = entity.NUMBEROFEMPLOYEES;
                corporateInfo.TOTALASSETS = entity.TOTALASSETS;
                corporateInfo.CORPORATECUSTOMERTYPEID = entity.CORPORATECUSTOMERTYPEID;
                context.TBL_CUSTOMER_COMPANYINFOMATION.Add(corporateInfo);
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = detail, //"Approved Customer Company Information:  with Id: " + entity.COMPANYINFOMATIONID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveAddressInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_ADDRESS temp = null;
            TBL_CUSTOMER_ADDRESS entity = null;
            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);
            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer address information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Address_Addition)
            {
                    temp = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x => x.TEMPADDRESSID == targetId);
                    if (temp != null) //If temp record is not null select the information from the main table
                    {
                        var tempModel = new CustomerAddressViewModels();

                        tempModel.active = temp.ACTIVE;
                        tempModel.address = temp.ADDRESS;
                        tempModel.addressTypeId = (short)temp.ADDRESSTYPEID;
                        tempModel.cityId = temp.CITYID;
                        tempModel.customerId = temp.CUSTOMERID;
                        tempModel.stateId = temp.STATEID;
                        tempModel.homeTown = temp.HOMETOWN;
                        tempModel.pobox = temp.POBOX;
                        tempModel.localGovernmentId = temp.LOCALGOVERNMENTID;
                        tempModel.electricMeterNumber = temp.ELECTRICMETERNUMBER;
                        tempModel.nearestLandmark = temp.NEARESTLANDMARK;

                        var transformedAddressModel = TransformAddressModelToAuditAddressModel(tempModel);

                        var recentData = JsonConvert.SerializeObject(transformedAddressModel);

                        //JObject currentDataStr = JObject.Parse(temp.ToString());
                        //var recentData = currentDataStr.ToString();

                        //JObject existingDataStr = JObject.Parse(Convert.ToString(temp));
                        //var existingData = existingDataStr["data"].ToString();

                        detail = $"Customer address for customer with code: : {temp.TBL_CUSTOMER.CUSTOMERCODE} has been added. <br> New address: <br>{recentData}";

                        entity = new TBL_CUSTOMER_ADDRESS();
                        entity.ACTIVE = temp.ACTIVE;
                        entity.ADDRESS = temp.ADDRESS;
                        entity.ADDRESSTYPEID = temp.ADDRESSTYPEID;
                        entity.CITYID = temp.CITYID;
                        entity.CUSTOMERID = temp.CUSTOMERID;
                        entity.STATEID = temp.STATEID;
                        entity.HOMETOWN = temp.HOMETOWN;
                        entity.POBOX = temp.POBOX;
                        entity.STATEID = temp.STATEID;
                        entity.ELECTRICMETERNUMBER = temp.ELECTRICMETERNUMBER;
                        entity.NEARESTLANDMARK = temp.NEARESTLANDMARK;
                        entity.LOCALGOVERNMENTID = temp.LOCALGOVERNMENTID;
                        context.TBL_CUSTOMER_ADDRESS.Add(entity);
                    }
               
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Address_Modification)
            {
                entity = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(x => x.CUSTOMERID == modified.CUSTOMERID);
                

                if (entity != null)
                {
                    var entityModel = new CustomerAddressViewModels();

                    entityModel.active = entity.ACTIVE;
                    entityModel.address = entity.ADDRESS;
                    entityModel.addressTypeId = (short)entity.ADDRESSTYPEID;
                    entityModel.cityId = entity.CITYID;
                    entityModel.customerId = entity.CUSTOMERID;
                    entityModel.stateId = entity.STATEID;
                    entityModel.homeTown = entity.HOMETOWN;
                    entityModel.pobox = entity.POBOX;
                    entityModel.stateId = entity.STATEID;
                    entityModel.localGovernmentId = entity.LOCALGOVERNMENTID;
                    entityModel.electricMeterNumber = entity.ELECTRICMETERNUMBER;
                    entityModel.nearestLandmark = entity.NEARESTLANDMARK;

                    temp = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x => x.TEMPADDRESSID == targetId);
                    if (temp != null) //If temp record is not null select the information from the main table
                    {
                        var tempModel = new CustomerAddressViewModels();

                        tempModel.active = temp.ACTIVE;
                        tempModel.address = temp.ADDRESS;
                        tempModel.addressTypeId = (short)temp.ADDRESSTYPEID;
                        tempModel.cityId = temp.CITYID;
                        tempModel.customerId = temp.CUSTOMERID;
                        tempModel.stateId = temp.STATEID;
                        tempModel.homeTown = temp.HOMETOWN;
                        tempModel.pobox = temp.POBOX;
                        tempModel.localGovernmentId = temp.LOCALGOVERNMENTID;
                        tempModel.electricMeterNumber = temp.ELECTRICMETERNUMBER;
                        tempModel.nearestLandmark = temp.NEARESTLANDMARK;

                        var transformedAddressModel = TransformAddressModelToAuditAddressModel(entityModel);

                        var recentData = JsonConvert.SerializeObject(transformedAddressModel);

                        transformedAddressModel = TransformAddressModelToAuditAddressModel(tempModel);

                        var existingData = JsonConvert.SerializeObject(transformedAddressModel);

                        //JObject currentDataStr = JObject.Parse(con);

                        //var recentData = currentDataStr.ToString();

                        //JObject existingDataStr = JObject.Parse(Convert.ToString(tempModel));

                        //var existingData = existingDataStr["data"].ToString();

                        detail = $"Customer address for customer with code: : {temp.TBL_CUSTOMER.CUSTOMERCODE} has been updated. Existing data :<br> {existingData}. <br> New Data: <br>{recentData}";

                        entity = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(x => x.ADDRESSID == temp.ADDRESSID);
                        entity.ACTIVE = temp.ACTIVE;
                        entity.ADDRESS = temp.ADDRESS;
                        entity.ADDRESSTYPEID = temp.ADDRESSTYPEID;
                        entity.CITYID = temp.CITYID;
                        entity.CUSTOMERID = temp.CUSTOMERID;
                        entity.STATEID = temp.STATEID;
                        entity.HOMETOWN = temp.HOMETOWN;
                        entity.POBOX = temp.POBOX;
                        entity.ELECTRICMETERNUMBER = temp.ELECTRICMETERNUMBER;
                        entity.NEARESTLANDMARK = temp.NEARESTLANDMARK;
                        entity.LOCALGOVERNMENTID = temp.LOCALGOVERNMENTID;
                    }
                }
                
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;


            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = detail, // "Approved Customer Address Information:  with Id: " + entity.ADDRESSID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private CustomerAddressAuditViewModel TransformAddressModelToAuditAddressModel(CustomerAddressViewModels customerAddress)
        {
            var auditAddressModel = new CustomerAddressAuditViewModel();
            if (customerAddress != null)
            {
                auditAddressModel.address = customerAddress.address;
                auditAddressModel.addressType = context.TBL_CUSTOMER_ADDRESS_TYPE.Where(a => a.ADDRESSTYPEID == customerAddress.addressTypeId).Select(s => s.ADDRESS_TYPE_NAME).FirstOrDefault();
                auditAddressModel.city = context.TBL_CITY.Where(a => a.CITYID == customerAddress.cityId).Select(s => s.CITYNAME).FirstOrDefault();
                auditAddressModel.state = context.TBL_STATE.Where(a => a.STATEID == customerAddress.stateId).Select(s => s.STATENAME).FirstOrDefault();
                auditAddressModel.homeTown = customerAddress.homeTown;
                auditAddressModel.pobox = customerAddress.pobox;
                auditAddressModel.localGovernment = context.TBL_LOCALGOVERNMENT.Where(a => a.LOCALGOVERNMENTID == customerAddress.localGovernmentId).Select(s => s.NAME).FirstOrDefault();
                auditAddressModel.electricMeterNumber = customerAddress.electricMeterNumber;
                auditAddressModel.nearestLandmark = customerAddress.nearestLandmark;
            }

            return auditAddressModel;
        }

        private bool ApprovePhoneContactInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_PHONCONTACT temp = null;
            TBL_CUSTOMER_PHONECONTACT entity = null;

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Phone_Number_Addition)
            {
                temp = context.TBL_TEMP_CUSTOMER_PHONCONTACT.FirstOrDefault(x => x.TEMPPHONECONTACTID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = new TBL_CUSTOMER_PHONECONTACT();
                    entity.ACTIVE = temp.ACTIVE;
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.PHONE = temp.PHONE;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    context.TBL_CUSTOMER_PHONECONTACT.Add(entity);
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Phone_Number_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMER_PHONCONTACT.FirstOrDefault(x => x.TEMPPHONECONTACTID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(x =>
                        x.PHONECONTACTID == temp.PHONECONTACTID);
                    entity.ACTIVE = temp.ACTIVE;
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.PHONE = temp.PHONE;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Customer Address Information:  with Id: " + entity.PHONECONTACTID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveEmploymentHistoryInformation(int modifiedId, int targetId, short approvalStatusId,
            UserInfo user)
        {
            TBL_TEMP_CUSTOMEREMPLOYMENT temp = null;
            TBL_CUSTOMER_EMPLOYMENTHISTORY entity = null;

            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Employement_History_Addition)
            {
                temp = context.TBL_TEMP_CUSTOMEREMPLOYMENT.FirstOrDefault(x => x.TEMPPLACEOFWORKID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = new TBL_CUSTOMER_EMPLOYMENTHISTORY();
                    entity.ACTIVE = temp.ACTIVE;
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.EMPLOYDATE = temp.EMPLOYDATE;
                    entity.EMPLOYERADDRESS = temp.EMPLOYERADDRESS;
                    entity.EMPLOYERCOUNTRYID = temp.EMPLOYERCOUNTRYID;
                    entity.EMPLOYERSTATEID = temp.EMPLOYERSTATEID;
                    entity.EMPLOYERSTATE = temp.EMPLOYERSTATE;
                    entity.EMPLOYERNAME = temp.EMPLOYERNAME;
                    entity.OFFICEPHONE = temp.OFFICEPHONE;
                    entity.PREVIOUSEMPLOYER = temp.PREVIOUSEMPLOYER;
                    entity.YEAROFEMPLOYMENT = temp.YEAROFEMPLOYMENT;
                    entity.TOTALWORKINGEXPERIENCE = temp.TOTALWORKINGEXPERIENCE;
                    entity.YEARSOFCURRENTEMPLOYMENT = temp.YEARSOFCURRENTEMPLOYMENT;
                    entity.TERMINALBENEFITS = temp.TERMINALBENEFITS;
                    entity.ANNUALINCOME = temp.ANNUALINCOME;
                    entity.MONTHLYINCOME = temp.MONTHLYINCOME;
                    entity.EXPENDITURE = temp.EXPENDITURE;
                    entity.ISEMPLOYERRELATED = temp.ISEMPLOYERRELATED;
                    entity.APPROVEDEMPLOYERID = temp.APPROVEDEMPLOYERID;

                    context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Add(entity);

                    var saved = context.SaveChanges() > 0;
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Employment_History_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMEREMPLOYMENT.FirstOrDefault(x => x.TEMPPLACEOFWORKID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.FirstOrDefault(x =>
                    x.PLACEOFWORKID == temp.PLACEOFWORKID);
                    entity.ACTIVE = temp.ACTIVE;
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.EMPLOYDATE = temp.EMPLOYDATE;
                    entity.EMPLOYERADDRESS = temp.EMPLOYERADDRESS;
                    entity.EMPLOYERCOUNTRYID = temp.EMPLOYERCOUNTRYID;
                    entity.EMPLOYERSTATEID = temp.EMPLOYERSTATEID;
                    entity.EMPLOYERSTATE = temp.EMPLOYERSTATE;
                    entity.EMPLOYERNAME = temp.EMPLOYERNAME;
                    entity.OFFICEPHONE = temp.OFFICEPHONE;
                    entity.PREVIOUSEMPLOYER = temp.PREVIOUSEMPLOYER;
                    entity.YEAROFEMPLOYMENT = temp.YEAROFEMPLOYMENT;
                    entity.TOTALWORKINGEXPERIENCE = temp.TOTALWORKINGEXPERIENCE;
                    entity.YEARSOFCURRENTEMPLOYMENT = temp.YEARSOFCURRENTEMPLOYMENT;
                    entity.TERMINALBENEFITS = temp.TERMINALBENEFITS;
                    entity.ANNUALINCOME = temp.ANNUALINCOME;
                    entity.MONTHLYINCOME = temp.MONTHLYINCOME;
                    entity.EXPENDITURE = temp.EXPENDITURE;
                    entity.ISEMPLOYERRELATED = temp.ISEMPLOYERRELATED;
                    entity.APPROVEDEMPLOYERID = temp.APPROVEDEMPLOYERID;
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.PLACEOFWORKID = entity.PLACEOFWORKID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Customer Employment History Information:  with Id: " + entity.PLACEOFWORKID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveNextOfKinInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_NEXTOFKIN temp = null;
            TBL_CUSTOMER_NEXTOFKIN entity = null;

            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer Next of Kin information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Next_of_Kin_Addition)
            {
                temp = context.TBL_TEMP_CUSTOMER_NEXTOFKIN.FirstOrDefault(x => x.TEMPNEXTOFKINID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = new TBL_CUSTOMER_NEXTOFKIN();
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.LASTNAME = temp.LASTNAME;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.RELATIONSHIP = temp.RELATIONSHIP;
                    entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                    entity.EMAIL = temp.EMAIL;
                    entity.NEAREST_LANDMARK = temp.NEAREST_LANDMARK;
                    entity.GENDER = temp.GENDER;
                    entity.ADDRESS = temp.ADDRESS;
                    entity.CITYID = temp.CITYID;
                    entity.ACTIVE = temp.ACTIVE;
                    context.TBL_CUSTOMER_NEXTOFKIN.Add(entity);
                    context.SaveChanges();
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Next_of_Kin_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMER_NEXTOFKIN.FirstOrDefault(x => x.TEMPNEXTOFKINID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_NEXTOFKIN.FirstOrDefault(x => x.NEXTOFKINID == temp.NEXTOFKINID);
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.LASTNAME = temp.LASTNAME;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.RELATIONSHIP = temp.RELATIONSHIP;
                    entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                    entity.EMAIL = temp.EMAIL;
                    entity.NEAREST_LANDMARK = temp.NEAREST_LANDMARK;
                    entity.GENDER = temp.GENDER;
                    entity.ADDRESS = temp.ADDRESS;
                    entity.CITYID = temp.CITYID;
                    entity.ACTIVE = temp.ACTIVE;
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.NEXTOFKINID = entity.NEXTOFKINID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Customer Next of Kin Information:  with Id: " + entity.NEXTOFKINID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveClientSupplierInformation(int modifiedId, int targetId, short approvalStatusId,
            UserInfo user)
        {
            TBL_TEMP_CUST_CLIENT_SUPPLIER temp = null;
            TBL_CUSTOMER_CLIENT_SUPPLIER entity = null;

            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Client_Addition ||
                modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Supplier_Addition)
            {
                temp = context.TBL_TEMP_CUST_CLIENT_SUPPLIER.FirstOrDefault(x => x.TEMPCLIENT_SUPPLIERID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = new TBL_CUSTOMER_CLIENT_SUPPLIER();

                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.CUSTOMERTYPEID = temp.CUSTOMERTYPEID;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.MIDDLENAME = temp.MIDDLENAME;
                    entity.LASTNAME = temp.LASTNAME;
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                    entity.TAX_NUMBER = temp.TAX_NUMBER;
                    entity.REGISTRATION_NUMBER = temp.REGISTRATION_NUMBER;
                    entity.BANKNAME = temp.BANKNAME;
                    entity.HAS_CASA_ACCOUNT = temp.HAS_CASA_ACCOUNT;
                    entity.CASA_ACCOUNTNO = temp.CASA_ACCOUNTNO;
                    entity.NATURE_OF_BUSINESS = temp.NATURE_OF_BUSINESS;
                    entity.CONTACT_PERSON = temp.CONTACT_PERSON;
                    entity.CLIENT_SUPPLIERTYPEID = temp.CLIENT_SUPPLIERTYPEID;
                    entity.CREATEDBY = temp.CREATEDBY;
                    entity.DATECREATED = temp.DATECREATED;
                    context.TBL_CUSTOMER_CLIENT_SUPPLIER.Add(entity);
                    var saved = context.SaveChanges() > 0;
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Client_Modification ||
                     modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Suplier_Modification)
            {
                temp = context.TBL_TEMP_CUST_CLIENT_SUPPLIER.FirstOrDefault(x => x.TEMPCLIENT_SUPPLIERID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_CLIENT_SUPPLIER.FirstOrDefault(x =>
                        x.CLIENT_SUPPLIERID == temp.CLIENT_SUPPLIERID);

                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.CUSTOMERTYPEID = temp.CUSTOMERTYPEID;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.MIDDLENAME = temp.MIDDLENAME;
                    entity.LASTNAME = temp.LASTNAME;
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                    entity.TAX_NUMBER = temp.TAX_NUMBER;
                    entity.REGISTRATION_NUMBER = temp.REGISTRATION_NUMBER;
                    entity.BANKNAME = temp.BANKNAME;
                    entity.HAS_CASA_ACCOUNT = temp.HAS_CASA_ACCOUNT;
                    entity.CASA_ACCOUNTNO = temp.CASA_ACCOUNTNO;
                    entity.NATURE_OF_BUSINESS = temp.NATURE_OF_BUSINESS;
                    entity.CONTACT_PERSON = temp.CONTACT_PERSON;
                    entity.CLIENT_SUPPLIERTYPEID = temp.CLIENT_SUPPLIERTYPEID;
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.CLIENT_SUPPLIERID = entity.CLIENT_SUPPLIERID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Customer Top Client Supplier Information:  with Id: " + entity.CLIENT_SUPPLIERID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveDirectorInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_DIRECTOR temp = null;
            TBL_CUSTOMER_COMPANY_DIRECTOR entity = null;

            var detail = string.Empty;
            var staff = context.TBL_STAFF.Find(user.staffId);

            List<TBL_CUSTOMER_COMPANY_BENEFICIA> beneficialList = new List<TBL_CUSTOMER_COMPANY_BENEFICIA>();

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Director_Addition ||
                modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Shareholder_Addition ||
                modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Signatory_Adition)
            {
                temp = context.TBL_TEMP_CUSTOMER_DIRECTOR.Where(x => x.TEMPCOMPANYDIRECTORID == targetId).FirstOrDefault();
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    if (temp.COMPANYDIRECTORTYPEID == (int)CompanyDirectorTypeEnum.Shareholder)
                    {
                        var tempBeneficial = context.TBL_TEMP_CUSTOMER_COMP_BENEFIC.Where(x => x.TEMPCOMPANYDIRECTORID == temp.TEMPCOMPANYDIRECTORID).ToList();
                        if (tempBeneficial != null)
                        {
                            foreach (var item in tempBeneficial)
                            {
                                var beneficial = new TBL_CUSTOMER_COMPANY_BENEFICIA
                                {
                                    SURNAME = item.SURNAME,
                                    FIRSTNAME = item.FIRSTNAME,
                                    MIDDLENAME = item.MIDDLENAME,
                                    CUSTOMERNIN = item.CUSTOMERNIN,
                                    CUSTOMERBVN = item.CUSTOMERBVN,
                                    NUMBEROFSHARES = item.NUMBEROFSHARES,
                                    ISPOLITICALLYEXPOSED = item.ISPOLITICALLYEXPOSED,
                                    ADDRESS = item.ADDRESS,
                                    PHONENUMBER = item.PHONENUMBER,
                                    EMAILADDRESS = item.EMAILADDRESS,
                                    CREATEDBY = user.staffId,
                                    DATECREATED = DateTime.Now,
                                    DELETED = false
                                };
                                beneficialList.Add(beneficial);
                                item.ISCURRENT = false;
                                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                        }
                    }
                    var CustomerRec = context.TBL_CUSTOMER.Find(temp.CUSTOMERID);
                    entity = new TBL_CUSTOMER_COMPANY_DIRECTOR();

                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.SURNAME = temp.SURNAME;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.MIDDLENAME = temp.MIDDLENAME;
                    entity.CUSTOMERNIN = temp.CUSTOMERNIN;
                    entity.CUSTOMERTYPEID = temp.CUSTOMERTYPEID;
                    entity.COMPANYDIRECTORTYPEID = temp.COMPANYDIRECTORTYPEID;
                    entity.CUSTOMERBVN = temp.CUSTOMERBVN;
                    entity.SHAREHOLDINGPERCENTAGE = temp.SHAREHOLDINGPERCENTAGE;
                    entity.ISPOLITICALLYEXPOSED = temp.ISPOLITICALLYEXPOSED;
                    entity.ISTHEPROMOTER = temp.ISTHEPROMOTER;
                    entity.MARITALSTATUSID = temp.MARITALSTATUSID;
                    entity.GENDER = temp.GENDER;
                    entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                    if (temp.ISPOLITICALLYEXPOSED == true)
                    {
                        if (CustomerRec.ISPOLITICALLYEXPOSED == false)
                        {
                            CustomerRec.ISPOLITICALLYEXPOSED = true;
                        }
                    }
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                    entity.CREATEDBY = temp.CREATEDBY;
                    entity.DATECREATED = temp.DATECREATED;
                    entity.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                    context.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(entity);
                    var saved = context.SaveChanges() > 0;
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Director_Modification ||
                     modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Shareholder_Modification ||
                     modified.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Signatory_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x => x.TEMPCOMPANYDIRECTORID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    var CustomerRec = context.TBL_CUSTOMER.Find(temp.CUSTOMERID);

                    entity = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x =>
                        x.COMPANYDIRECTORID == temp.COMPANYDIRECTORID).FirstOrDefault();
                    entity.CUSTOMERID = temp.CUSTOMERID;
                    entity.SURNAME = temp.SURNAME;
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.MIDDLENAME = temp.MIDDLENAME;
                    entity.CUSTOMERNIN = temp.CUSTOMERNIN;
                    entity.CUSTOMERTYPEID = temp.CUSTOMERTYPEID;
                    entity.COMPANYDIRECTORTYPEID = temp.COMPANYDIRECTORTYPEID;
                    entity.CUSTOMERBVN = temp.CUSTOMERBVN;
                    entity.SHAREHOLDINGPERCENTAGE = temp.SHAREHOLDINGPERCENTAGE;
                    entity.ISPOLITICALLYEXPOSED = temp.ISPOLITICALLYEXPOSED;
                    entity.MARITALSTATUSID = temp.MARITALSTATUSID;
                    entity.GENDER = temp.GENDER;
                    entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                    entity.ISTHEPROMOTER = temp.ISTHEPROMOTER;
                    if (temp.ISPOLITICALLYEXPOSED == true)
                    {
                        if (CustomerRec.ISPOLITICALLYEXPOSED == false)
                        {
                            CustomerRec.ISPOLITICALLYEXPOSED = true;
                        }
                    }
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                    entity.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                }
            }
            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.COMPANYDIRECTORID = entity.COMPANYDIRECTORID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Customer Top Client Supplier Information:  with Id: " + entity.COMPANYDIRECTORID,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private void UpdateCustomerInformationDisapproval(int modificationTypeId, int targetId)
        {
            if (modificationTypeId == (int)CustomerInformationTrackerEnum.General_Information)
            {
                var temp = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Corporate_Information)
            {
                var temp = context.TBL_TEMP_CUSTOMER_COMPANYINFO.FirstOrDefault(x => x.CUSTOMERID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Address_Addition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Address_Modification)
            {
                var temp = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x => x.TEMPADDRESSID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Phone_Number_Addition ||
                    modificationTypeId == (int)CustomerInformationTrackerEnum.Phone_Number_Modification)
            {
                var temp = context.TBL_TEMP_CUSTOMER_PHONCONTACT.FirstOrDefault(x => x.TEMPPHONECONTACTID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Employement_History_Addition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Employment_History_Modification)
            {
                var temp = context.TBL_TEMP_CUSTOMEREMPLOYMENT.FirstOrDefault(x => x.TEMPPLACEOFWORKID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Next_of_Kin_Modification ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Next_of_Kin_Addition)
            {
                var temp = context.TBL_TEMP_CUSTOMER_NEXTOFKIN.FirstOrDefault(x => x.TEMPNEXTOFKINID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Client_Addition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Client_Modification ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Suplier_Modification ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Supplier_Addition)
            {
                var temp = context.TBL_TEMP_CUST_CLIENT_SUPPLIER.FirstOrDefault(x => x.TEMPCLIENT_SUPPLIERID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
            else if (modificationTypeId == (int)CustomerInformationTrackerEnum.Director_Modification ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Director_Addition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Shareholder_Modification ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Shareholder_Addition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Signatory_Adition ||
                     modificationTypeId == (int)CustomerInformationTrackerEnum.Signatory_Modification)
            {
                var temp = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x => x.TEMPCOMPANYDIRECTORID == targetId);
                if (temp != null)
                {
                    temp.ISCURRENT = false;
                    temp.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
            }
        }

        public IEnumerable<LookupViewModel> GetAllCRMSLegalStatus()
        {

            var data = context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.LegalStatusType).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
            return data;
        }
        public IEnumerable<LookupViewModel> GetAllCRMSCompanySize()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.CompanySize).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }
        public IEnumerable<LookupViewModel> GetAllCRMSRelationshipType()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.RelationshipType).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllCRMSLegalStatusByType(int type)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.LegalStatusType && x.CUSTOMERTYPEID == type).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }
        public IEnumerable<LookupViewModel> GetAllCRMSCompanySizeByType(int type)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.CompanySize && x.CUSTOMERTYPEID == type).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }
        public IEnumerable<LookupViewModel> GetAllCRMSRelationshipTypeByType(int type)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.RelationshipType && x.CUSTOMERTYPEID == type).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupcustomerId = x.CUSTOMERTYPEID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        #region Customer Related Party
        public IEnumerable<CustomerRelatedPartyViewModel> GetCustomerRelatedParty(int customerId) 
        {
            var related = (from a in context.TBL_CUSTOMER_RELATED_PARTY
                           join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                           join c in context.TBL_COMPANY_DIRECTOR on a.COMPANYDIRECTORID equals c.COMPANYDIRECTORID
                           where a.CUSTOMERID == customerId
                           select new CustomerRelatedPartyViewModel
                           {
                               customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                               directorName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               relationshipType = a.RELATIONSHIPTYPE,
                               relatedPartyId = a.RELATEDPARTYID,
                               customerId = b.CUSTOMERID,
                               companyDirectorId = c.COMPANYDIRECTORID
                           }).ToList();
            return related;
        }


        public bool DeleteRelatedParty(int relatedPartyId, UserInfo user)
        {
            var child = context.TBL_CUSTOMER_RELATED_PARTY.Find(relatedPartyId);
            var customer = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == child.CUSTOMERID);

            if (child != null)
            {
                context.TBL_CUSTOMER_RELATED_PARTY.Remove(child);
                context.SaveChanges();
                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerRelatedPartyDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer Related Party with Related Party ID: " + child.RELATEDPARTYID + " and companydirectorId " + child.COMPANYDIRECTORID + " and relationship " + child.RELATIONSHIPTYPE + " createdBy " + child.CREATEDBY,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                auditTrail.AddAuditTrail(audit);


                var relParty = context.TBL_CUSTOMER_RELATED_PARTY.Any(c => c.CUSTOMERID == customer.CUSTOMERID);

                if (relParty)
                {
                    customer.ISREALATEDPARTY = true;
                }
                else
                {
                    customer.ISREALATEDPARTY = false;
                }
            }
            return context.SaveChanges() > 0;
        }


        public bool Deleteaddress(int addressId, UserInfo user)
        {
            var child = context.TBL_CUSTOMER_ADDRESS.Find(addressId);

            if (child != null)
            {
                context.TBL_CUSTOMER_ADDRESS.Remove(child);

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerContactAddressDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer Related Party with Related Party ID: ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }

            return false;
        }


        public bool Deletcontact(int phoneContactId, UserInfo user)
        {
            var child = context.TBL_CUSTOMER_PHONECONTACT.Find(phoneContactId);

            if (child != null)
            {
                context.TBL_CUSTOMER_PHONECONTACT.Remove(child);

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerPhoneContactDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer Related Party with Related Party ID: ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }

            return false;
        }


        public bool DeleteEmployment(int placeOfWorkId, UserInfo user)
        {
            var child = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Find(placeOfWorkId);

            if (child != null)
            {
                context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Remove(child);

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerEmploymentHistoryDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer Related Party with Related Party ID: ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }

            return false;
        }

        public bool DeleteNextOfKin(int nextOfKinId, UserInfo user)
        {
            var nextOfKin = context.TBL_CUSTOMER_NEXTOFKIN.Find(nextOfKinId);

            if (nextOfKin != null)
            {
                context.TBL_CUSTOMER_NEXTOFKIN.Remove(nextOfKin);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerDetailUpdated,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Customer Next of Kin with Next of Kin ID: ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                auditTrail.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }

            return false;
        }



        public bool AddUpdateCustomerRelatedParty(CustomerRelatedPartyViewModel entity)
        {
            if (entity == null) return false;
            var auditDetail = string.Empty;
            short auditType = 0;
            try
            {
                var customer = context.TBL_CUSTOMER.Where(r => r.CUSTOMERID == entity.customerId).FirstOrDefault();
                TBL_CUSTOMER_RELATED_PARTY relParty;
                if (entity.relatedPartyId > 0)
                {
                    relParty = context.TBL_CUSTOMER_RELATED_PARTY.Where(c=>c.RELATEDPARTYID == entity.relatedPartyId).FirstOrDefault();

                    //get customer record by id
                    var accountCompleted = customer.ACCOUNTCREATIONCOMPLETE;

                    //previous relationship type
                    var previousRelationshipType = relParty?.RELATIONSHIPTYPE;

                    if (relParty != null)
                    {
                        relParty.COMPANYDIRECTORID = entity.companyDirectorId;
                        relParty.CUSTOMERID = entity.customerId;
                        relParty.RELATIONSHIPTYPE = entity.relationshipType;
                        relParty.LASTUPDATEDBY = entity.createdBy;
                        relParty.DATETIMEUPDATED = DateTime.Now;

                        auditDetail = $"Updated new Customer's Insider Related Party Relationship Type for customer: {customer.CUSTOMERCODE}. from {previousRelationshipType} to {entity.relationshipType}";
                        auditType = (short)AuditTypeEnum.CustomerDetailUpdated;
                        customer.ISREALATEDPARTY = true;
                    }
                    else { customer.ISREALATEDPARTY = false; }
                }
                else
                {
                    relParty = new TBL_CUSTOMER_RELATED_PARTY()
                    {
                        RELATEDPARTYID = entity.relatedPartyId,
                        COMPANYDIRECTORID = entity.companyDirectorId,
                        CUSTOMERID = entity.customerId,
                        RELATIONSHIPTYPE = entity.relationshipType,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false
                    };
                    context.TBL_CUSTOMER_RELATED_PARTY.Add(relParty);
                    
                    customer.ISREALATEDPARTY = true;

                    auditDetail = "Added Customer's Insider Related Party for customer ID: + (" + entity.customerId + ") ";
                    auditType = (short)AuditTypeEnum.CustomerDetailAdded;
                }

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = auditType,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = auditDetail,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);

                var response = context.SaveChanges() != 0;
                return response;

            }
            catch (Exception ex)
            {
                return false;
                throw new SecureException(ex.Message);
            }
        }
        public bool ValidateRelatedPartyEntry(int customerId, int companyDirectorId)
        {
            var exist = (from a in context.TBL_CUSTOMER_RELATED_PARTY
                         where a.CUSTOMERID == customerId && a.COMPANYDIRECTORID == companyDirectorId
                         select a).ToList();
            if (exist.Any())
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Prospective Customer
        public IEnumerable<CustomerViewModels> GetAllProspectiveCustomer()
        {
            //var customers = (from x in GetCustomersLite()
            //                 where x.isProspect == true
            //                 select x)?.ToList();

            var customerInfo = GetCustomersLite().Where(x => x.isProspect == true).ToList();
                            
            if (customerInfo.Count > 0)
            {
                return customerInfo;
            }
            return null;
        }

        public bool UpdatePropectToCustomer(int customerId, CustomerViewModels entity)
        {
            try
            {
                var customerMain = context.TBL_CUSTOMER.Find(customerId);
                if (customerMain != null)
                {
                    TBL_TEMP_CUSTOMER customer = new TBL_TEMP_CUSTOMER();
                    customer.CUSTOMERCODE = entity.customerCode;
                    customer.CUSTOMERTYPEID = entity.customerTypeId;
                    customer.FIRSTNAME = entity.firstName;
                    customer.MIDDLENAME = entity.middleName;
                    customer.LASTNAME = entity.lastName;

                    customer.CUSTOMERID = customerMain.CUSTOMERID;
                    customer.BRANCHID = customerMain.BRANCHID;
                    customer.COMPANYID = customerMain.COMPANYID;
                    customer.CUSTOMERSENSITIVITYLEVELID = customerMain.CUSTOMERSENSITIVITYLEVELID;
                    customer.DATEOFBIRTH = customerMain.DATEOFBIRTH;
                    customer.EMAILADDRESS = customerMain.EMAILADDRESS;
                    customer.GENDER = customerMain.GENDER;
                    customer.MAIDENNAME = customerMain.MAIDENNAME;
                    customer.MARITALSTATUS = customerMain.MARITALSTATUS;
                    customer.TITLE = customerMain.TITLE;
                    customer.MISCODE = customerMain.MISCODE;
                    customer.MISSTAFF = customerMain.MISSTAFF;
                    customer.NATIONALITYID = customerMain.NATIONALITYID;
                    customer.OCCUPATION = customerMain.OCCUPATION;
                    customer.PLACEOFBIRTH = customerMain.PLACEOFBIRTH;
                    customer.ISPOLITICALLYEXPOSED = customerMain.ISPOLITICALLYEXPOSED;
                    customer.ISINVESTMENTGRADE = customerMain.ISINVESTMENTGRADE;
                    customer.ISREALATEDPARTY = customerMain.ISREALATEDPARTY;
                    customer.RELATIONSHIPOFFICERID = customerMain.RELATIONSHIPOFFICERID;
                    customer.SPOUSE = customerMain.SPOUSE;
                    customer.SUBSECTORID = customerMain.SUBSECTORID;
                    customer.TAXNUMBER = customerMain.TAXNUMBER;
                    customer.RISKRATINGID = customerMain.RISKRATINGID;
                    customer.CUSTOMERBVN = customerMain.CUSTOMERBVN;
                    customer.CREATEDBY = customerMain.CREATEDBY;
                    customer.DATETIMECREATED = customerMain.DATETIMECREATED;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                    customer.COUNTRYOFRESIDENTID = entity.countryOfResidentId;
                    customer.NUMBEROFDEPENDENTS = entity.numberOfDependents;
                    customer.NUMBEROFLOANSTAKEN = entity.numberOfLoansTaken;
                    customer.MONTHLYLOANREPAYMENT = entity.loanMonthlyRepaymentFromOtherBanks;
                    customer.DATEOFRELATIONSHIPWITHBANK = entity.dateOfRelationshipWithBank;
                    customer.RELATIONSHIPTYPEID = entity.relationshipTypeId;
                    customer.TEAMLDR = entity.teamLDP;
                    customer.TEAMNPL = entity.teamNPL;
                    customer.CORR = entity.corr;
                    customer.BUSINESSUNTID = entity.businessUnitId;
                    customer.PASTDUEOBLIGATIONS = entity.pastDueObligations;
                    context.TBL_TEMP_CUSTOMER.Add(customer);
                    //try
                    //{
                    //    var resul = context.SaveChanges() > 0;
                    //}
                    //catch(Exception ex)
                    //{
                    //    var a = ex;
                    //}

                }

                var modified = new TBL_CUSTOMER_MODIFICATION
                {
                    CUSTOMERID = entity.customerId,
                    TARGETID = entity.customerId,
                    MODIFICATIONTYPEID = (int)CustomerInformationTrackerEnum.General_Information,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Updated TBL_CUSTOMER: " + entity.customerName + " with code: " + entity.customerCode +
                             " on" + " (" + entity.customerId + ") ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                using (var trans = context.Database.BeginTransaction())
                {
                    if (USE_THIRD_PARTY_INTEGRATION)
                    {
                        if ((customerMain != null && customerMain.ISPROSPECT == true) || entity.isProspect == true)
                        {
                            finacle.AddCustomerAccounts(customerId, entity.customerCode);
                        }
                    }
                    customerMain.ISPROSPECT = false;
                    //context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    var output = context.SaveChanges() > 0;

                    var targetId = modified.CUSTOMERMODIFICATIONID;


                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                    workflow.TargetId = targetId;
                    workflow.OperationId = (int)OperationsEnum.CustomerInformationApproval;
                    workflow.ExternalInitialization = true;

                    var response = workflow.LogActivity();

                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }
                    trans.Rollback();
                    return false;
                }
            }catch(Exception e)
            {
                throw e;
            }
        }
        #endregion
        public IEnumerable<CustomerRelatedDirectorViewModel> DirectorRelatedCustomer(string bvn)
        {
            List<CustomerRelatedDirectorViewModel> relatedCust = new List<CustomerRelatedDirectorViewModel>();


            relatedCust = (from a in context.TBL_CUSTOMER
                           join b in context.TBL_CUSTOMER_COMPANY_DIRECTOR on a.CUSTOMERID equals b.CUSTOMERID
                           //     join c in context.TBL_CUSTOMER_COMPANY_BENEFICIA on b.COMPANYDIRECTORTYPEID equals c.COMPANYDIRECTORID
                           where b.CUSTOMERBVN == bvn
                           select new CustomerRelatedDirectorViewModel
                           {
                               customerCode =a.CUSTOMERCODE,
                               customerId = a.CUSTOMERID,
                               customerName = a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME,
                               customerTypeId = a.CUSTOMERTYPEID,
                               customerTypeName = context.TBL_CUSTOMER_TYPE.Where(x => x.CUSTOMERTYPEID == a.CUSTOMERTYPEID).FirstOrDefault().NAME,
                               directorTypeName = context.TBL_CUSTOMER_COMPANY_DIREC_TYP.Where(x => x.COMPANYDIRECTORYTYPEID == b.COMPANYDIRECTORTYPEID).FirstOrDefault().COMPANYDIRECTORYTYPENAME,
                           }).ToList();

            var ultimateBeneficial = (from a in context.TBL_CUSTOMER
                                      join b in context.TBL_CUSTOMER_COMPANY_DIRECTOR on a.CUSTOMERID equals b.CUSTOMERID
                                      join c in context.TBL_CUSTOMER_COMPANY_BENEFICIA on b.COMPANYDIRECTORTYPEID equals c.COMPANYDIRECTORID
                                      where c.CUSTOMERBVN == bvn
                                      select new CustomerRelatedDirectorViewModel
                                      {
                                          customerCode = a.CUSTOMERCODE,
                                          customerId = a.CUSTOMERID,
                                          customerName = a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME,
                                          customerTypeId = a.CUSTOMERTYPEID,
                                          customerTypeName = context.TBL_CUSTOMER_TYPE.Where(x => x.CUSTOMERTYPEID == a.CUSTOMERTYPEID).FirstOrDefault().NAME,
                                          directorTypeName = context.TBL_CUSTOMER_COMPANY_DIREC_TYP.Where(x => x.COMPANYDIRECTORYTYPEID == b.COMPANYDIRECTORTYPEID).FirstOrDefault().COMPANYDIRECTORYTYPENAME,
                                      }).ToList();

            return relatedCust.Concat(ultimateBeneficial);
        }
        public IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLMSLoanId(int loanApplicationId)
        {
            var loanCust = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                            where a.LOANAPPLICATIONID == loanApplicationId
                            select a.CUSTOMERID).ToList();
            var customers = GetCustomers();
            if (loanCust.Any())
            {
               return customers = customers.Where(x => loanCust.Contains(x.customerId));
            }

            return new List< CustomerViewModels>();
        }

        public Tuple<List<MultipleFsCaptionOutputViewModel>, bool> preBulkFsCaption(byte[] file, UserInfo user, bool isFinal, int customerId)
        {
            List<MultipleFsCaptionOutputViewModel> fsCaptionInputs = GetBulkFsCaptionInputs(file, customerId);

            return new Tuple<List<MultipleFsCaptionOutputViewModel>, bool>(fsCaptionInputs, true);
        }

        private List<MultipleFsCaptionOutputViewModel> GetBulkFsCaptionInputs(byte[] file, int customerId)
        {

            List<MultipleFsCaptionOutputViewModel> bulkEntries = new List<MultipleFsCaptionOutputViewModel>();
            
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5");
            MemoryStream ms = new MemoryStream(file);
            ExcelFile ef = ExcelFile.Load(ms, LoadOptions.XlsxDefault);
            //ExcelWorksheet ws = ef.Worksheets.ActiveWorksheet;
            ExcelWorksheet ws = ef.Worksheets[0]; //.ActiveWorksheet;
            CellRange range = ef.Worksheets.ActiveWorksheet.GetUsedCellRange(true);

            for (int j = range.FirstRowIndex; j <= range.LastRowIndex; j++)
            {
                MultipleFsCaptionOutputViewModel currentLine = new MultipleFsCaptionOutputViewModel();
                int ctr = 0;
                currentLine.errorMessages = new List<string>();
                for (int i = range.FirstColumnIndex; i <= range.LastColumnIndex; i++)
                {
                    ExcelCell cell = range[j - range.FirstRowIndex, i - range.FirstColumnIndex];

                    string cellName = CellRange.RowColumnToPosition(j, i);
                    string cellRow = ExcelRowCollection.RowIndexToName(j);
                    string cellColumn = ExcelColumnCollection.ColumnIndexToName(i);
                    if (Convert.ToInt32(cellRow) == 1) continue;
                    ctr = Convert.ToInt32(cellRow);
                    switch (cellColumn)
                    {
                        case "A":
                            currentLine.passed = true;
                            try { currentLine.fsGroup = cell.Value.ToString(); } catch (Exception e) { currentLine.passed = false; currentLine.errorMessages.Add(e.Message); }
                            break;
                        case "B":
                            currentLine.passed = true;
                            try { currentLine.fsItem = cell.Value.ToString(); }
                            catch (Exception e)
                            {
                                currentLine.passed = false; currentLine.errorMessages.Add(e.Message);
                            }
                            break;
                        case "C":
                            currentLine.passed = true;
                            try
                            {
                              currentLine.fsValue = Convert.ToDecimal(cell.Value);
                            }
                            catch (Exception e)
                            {
                                currentLine.passed = false; currentLine.errorMessages.Add(e.Message);
                            }
                            
                            break;
                    }

                }

                if (ctr > 1) bulkEntries.Add(currentLine);

            };

            foreach (var bulkEntry in bulkEntries)
            {
                try
                {
                    bulkEntry.referenceId = CommonHelpers.GenerateRandomDigitCode(10);
                    bulkEntry.customerId = customerId;

                    if (bulkEntry.fsGroup == null)
                    {
                        bulkEntry.passed = false;
                        bulkEntry.errorMessages.Add("FS Caption group id can not be null ");
                    }

                    if (bulkEntry.fsItem == null)
                    {
                        bulkEntry.passed = false;
                        bulkEntry.errorMessages.Add("FS Caption name can not be null ");
                    }

                    if (bulkEntry.fsValue <= 0)
                    {
                        bulkEntry.passed = false;
                        bulkEntry.errorMessages.Add("FS Caption value can not be null or equal to zero ");
                    }

                    if(bulkEntry.passed == true)
                    {
                        bulkEntry.validityStatus = "Passed";
                    }
                    else
                    {
                        bulkEntry.validityStatus = "Failed";
                    }
                        

                            

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return bulkEntries;

        }

        public bool saveBulkFsCaptionEntries(List<MultipleFsCaptionOutputViewModel> models, UserInfo user)
        {
            
            using (TransactionScope transactionScope = new TransactionScope())
            {
                foreach (var fsRequest in models)
                {
                    fsRequest.createdBy = user.createdBy;
                    var fsg = context.TBL_CUSTOMER_FS_CAPTION_GROUP.Where(x => fsRequest.fsGroup.ToLower().Contains(x.FSCAPTIONGROUPNAME.ToLower())).FirstOrDefault();
                    if (fsg == null)
                    {
                        var fsGroupData = addBulkFsGroup(fsRequest);
                        var saveFsg = context.TBL_CUSTOMER_FS_CAPTION_GROUP.Add(fsGroupData);
                        context.SaveChanges();

                        var fsi = context.TBL_CUSTOMER_FS_CAPTION.Where(x => fsRequest.fsItem.ToLower().Contains(x.FSCAPTIONNAME.ToLower()) ).FirstOrDefault();
                        if (fsi == null)
                        {
                            fsRequest.fsGroupId = saveFsg.FSCAPTIONGROUPID;
                            var fsItemData = addBulkFsItem(fsRequest);
                            var saveFsi = context.TBL_CUSTOMER_FS_CAPTION.Add(fsItemData);
                            context.SaveChanges();

                            fsRequest.captionId = saveFsi.FSCAPTIONID;
                            var fsDetailData = addBulkFsDetail(fsRequest);
                            var saveFsd = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Add(fsDetailData);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                            var fsi = context.TBL_CUSTOMER_FS_CAPTION.Where(x => fsRequest.fsItem.ToLower().Contains(x.FSCAPTIONNAME.ToLower()) && x.FSCAPTIONGROUPID == fsg.FSCAPTIONGROUPID).FirstOrDefault();
                            if (fsi == null)
                            {
                                fsRequest.fsGroupId = fsg.FSCAPTIONGROUPID;
                                var fsItemData = addBulkFsItem(fsRequest);
                                var saveFsi = context.TBL_CUSTOMER_FS_CAPTION.Add(fsItemData);
                                context.SaveChanges();

                                fsRequest.captionId = saveFsi.FSCAPTIONID;
                                var fsDetailData = addBulkFsDetail(fsRequest);
                                var saveFsd = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Add(fsDetailData);
                                context.SaveChanges();
                            }
                    }
                }
                
                transactionScope.Complete();
                transactionScope.Dispose();
            }

            auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.FsCaptionBulkUpload,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Added TBL_CUSTOMER_FS_CAPTION_GROUP",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });

            context.SaveChanges();
            return true;
        }

        private TBL_CUSTOMER_FS_CAPTION_GROUP addBulkFsGroup(MultipleFsCaptionOutputViewModel fsGroup)
        {

            var fsg = context.TBL_CUSTOMER_FS_CAPTION_GROUP.Add(new TBL_CUSTOMER_FS_CAPTION_GROUP
            {
                FSCAPTIONGROUPNAME = fsGroup.fsGroup,
                POSITION = 1,
                CREATEDBY = fsGroup.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
            });

            return fsg;
        }

        private TBL_CUSTOMER_FS_CAPTION addBulkFsItem(MultipleFsCaptionOutputViewModel fsItem)
        {

            var fsg = context.TBL_CUSTOMER_FS_CAPTION.Add(new TBL_CUSTOMER_FS_CAPTION
            {
                FSCAPTIONNAME = fsItem.fsItem,
                FSCAPTIONGROUPID = fsItem.fsGroupId,
                ISRATIO = false,
                POSITION = 1,
                CREATEDBY = fsItem.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
            });

            return fsg;
        }

        private TBL_CUSTOMER_FS_CAPTION_DETAIL addBulkFsDetail(MultipleFsCaptionOutputViewModel fsDetail)
        {

            var fsd = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Add(new TBL_CUSTOMER_FS_CAPTION_DETAIL
            {
                CUSTOMERID = fsDetail.customerId,
                FSCAPTIONID = fsDetail.captionId,
                FSDATE = DateTime.Now,
                AMOUNT = fsDetail.fsValue,
                CREATEDBY = fsDetail.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
            });

            return fsd;
        }


    }
}
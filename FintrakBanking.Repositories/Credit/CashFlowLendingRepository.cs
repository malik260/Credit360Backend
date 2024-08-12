using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
        private IWorkflow workflow;
        private ICustomerCreditBureauRepository customerBureauReport;
        public CashFlowLendingRepository(FinTrakBankingContext _context, 
                                        ICustomerRepository _customer, 
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                         CustomerDetails _customerRequest,
                                         ICreditLimitValidationsRepository _limitValidation,
                                         ICasaRepository _casa,
                                         IWorkflow _workflow,
                                         ICustomerCreditBureauRepository _customerBureauReport
                                         )
        {
            this.context = _context;
            this.customer = _customer;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.customerRequest = _customerRequest;
            limitValidation = _limitValidation;
            casa = _casa;
            workflow = _workflow;
            customerBureauReport = _customerBureauReport;


        }

        #region  CUSTOMER PROFILE METHODS
        public APIResponse AddCustomer(IncomingCustomerViewModels model)
        {
           APIResponse response = new APIResponse();

           if (model.accountOfficerStaffCode == string.Empty || model.accountOfficerStaffCode == null) { return fireResponse("Missing Account Officer Code", "99", ""); }

           if (model.customerType == "1")
           {
                if (model.individualCustomerInformation.customerCode == string.Empty) { return fireResponse("Missing Customer Number", "99", ""); }

                var businessunit = context.TBL_PROFILE_BUSINESS_UNIT.Where(O => O.BUSINESSUNITSHORTCODE == model.individualCustomerInformation.businessUnit.ToUpper()).FirstOrDefault();
                if (businessunit == null) { return fireResponse("Customer's business unit does not exist in Fintrak!", "99", ""); }

                var existingCustomer = context.TBL_CUSTOMER.Where(O => O.CUSTOMERCODE == model.individualCustomerInformation.customerCode).FirstOrDefault();
                if (existingCustomer != null) { return fireResponse("Customer already exist in Fintrak!", "77", ""); }

                model.individualCustomerInformation.businessUnitId = businessunit.BUSINESSUNITID;
                return AddIndividualCustomer(model);
           }
           else if (model.customerType == "2")
           {
                if (model.corporateCustomerInformation.customerCode == string.Empty) { return fireResponse("Missing Customer Number", "99", model.request_Id); }

                var businessunit = context.TBL_PROFILE_BUSINESS_UNIT.Where(O => O.BUSINESSUNITSHORTCODE == model.corporateCustomerInformation.businessUnit.ToUpper()).FirstOrDefault();
                if (businessunit == null) { return fireResponse("Customer's business unit does not exist in Fintrak!", "99", ""); }

                var existingCustomer = context.TBL_CUSTOMER.Where(O => O.CUSTOMERCODE == model.corporateCustomerInformation.customerCode).FirstOrDefault();
                if (existingCustomer != null) { return fireResponse("Customer already exist in Fintrak!", "77", ""); }

                model.corporateCustomerInformation.businessUnitId = businessunit.BUSINESSUNITID;
                return AddCorporateCustomer(model);
           }
           else { return fireResponse("Uknown Customer Type", "99", ""); }
        }

        private APIResponse AddIndividualCustomer(IncomingCustomerViewModels model)
        {
            APIResponse response = new APIResponse();
            using (var trans = context.Database.BeginTransaction())
            {
                //if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99",""); }

                //if (model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99",""); }

                //if (model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99",""); }

                //List<string> creditBureautype = model.creditBureauReport.Select(x => x.creditBureauType).ToList();
                //if (creditBureautype.Contains(CreditBureauEnum.CRCCreditBureau.ToString()) == false) { return fireResponse("Missing CRC credit bureau", "99",""); }

                if (!DateTime.TryParseExact(model.individualCustomerInformation.dateOfBirth, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateOfBirth)) { return fireResponse("Date of birth not in the right format", "99", ""); }

                List<string> mStatus = new List<string> { "single", "married", "divorced", "widowed" };
                if (model.individualCustomerInformation.maritalStatus.ToLower() == "single") { model.individualCustomerInformation.maritalStatus = "1"; }
                if (model.individualCustomerInformation.maritalStatus.ToLower() == "married") { model.individualCustomerInformation.maritalStatus = "2"; }
                if (model.individualCustomerInformation.maritalStatus.ToLower() == "divorced") { model.individualCustomerInformation.maritalStatus = "3"; }
                if (model.individualCustomerInformation.maritalStatus.ToLower() == "widowed") { model.individualCustomerInformation.maritalStatus = "4"; }
                if (model.individualCustomerInformation.maritalStatus == "0") { fireResponse("Zero value is not a recognized marital status.", "99", ""); }
                if (!mStatus.Contains(model.individualCustomerInformation.maritalStatus)) { fireResponse($"Value, '{model.individualCustomerInformation.maritalStatus}' is not a valid marital status.", "99", ""); }

                var accountOfficer = context.TBL_STAFF.Where(x => x.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();
                if (accountOfficer == null) return fireResponse("Account officer does not exist in Fintrak Credit360", "99", "");
                model.createdBy = accountOfficer.STAFFID;
                model.staffId = accountOfficer.STAFFID;
                model.companyId = accountOfficer.COMPANYID;
                model.branchId = accountOfficer.BRANCHID;

                if (model.individualCustomerInformation.subSector == null) return fireResponse("Missing Sub-Sector Code", "99", "");

                var subsector = context.TBL_SUB_SECTOR.Where(x => x.CODE == model.individualCustomerInformation.subSector).FirstOrDefault();
                if (subsector == null) return fireResponse("Sub-Sector does not exist in Fintrak Credit360", "99", "");

                var sector = context.TBL_SECTOR.Where(x => x.SECTORID == subsector.SECTORID).FirstOrDefault();
                model.sectorId = sector.SECTORID;
                model.subsectorId = subsector.SUBSECTORID;

                if (saveIndividualCustomerInformation(model))
                {
                    customer.UpdateCustomerCollateralId(model.individualCustomerInformation.customerCode);
                    //SaveLoanDocument();
                    trans.Commit();
                    return fireResponse("Success", "00", model.request_Id);
                }
                else { return fireResponse("Unresolved error: could not save customer information", "99", ""); }
            }
        }

        private APIResponse AddCorporateCustomer(IncomingCustomerViewModels model)
        {
            APIResponse response = new APIResponse();
            using (var trans = context.Database.BeginTransaction())
            {
                //if (model.creditBureauReport == null || model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99",""); }

                //if (model.creditBureauReport == null || model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99",""); }

                var accountOfficer = context.TBL_STAFF.Where(x => x.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();
                if (accountOfficer == null) return fireResponse("Account officer does not exist in Fintrak Credit360", "99", "");
                model.createdBy = accountOfficer.STAFFID;
                model.staffId = accountOfficer.STAFFID;
                model.companyId = accountOfficer.COMPANYID;
                model.branchId = accountOfficer.BRANCHID;

                if (model.corporateCustomerInformation.subSector == null) return fireResponse("Missing Sub-Sector Code", "99", "");

                var subsector = context.TBL_SUB_SECTOR.Where(x => x.CODE == model.corporateCustomerInformation.subSector).FirstOrDefault();
                if (subsector == null) return fireResponse("Sub-Sector does not exist in Fintrak Credit360", "99", "");

                var sector = context.TBL_SECTOR.Where(x => x.SECTORID == subsector.SECTORID).FirstOrDefault();
                model.sectorId = sector.SECTORID;
                model.subsectorId = subsector.SUBSECTORID;

                if (!DateTime.TryParseExact(model.corporateCustomerInformation.dateOfIncorporation, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateOfIncorporation)) { return fireResponse("Date of Incorporation not in the right format", "99", ""); }

                // List<string> creditBureautype = model.creditBureauReport.Select(x => x.creditBureauType).ToList();

                //List<int> crcCreditBureauType = new List<int> { 3 };
                //if (creditBureautype.Contains(crcCreditBureauType) == false)
                //{ return fireResponse("Missing CRC credit bureau", "99",""); }

                if (saveCorporateCustomerInformation(model))
                {
                    customer.UpdateCustomerCollateralId(model.corporateCustomerInformation.customerCode);
                    trans.Commit();
                    return fireResponse("Success", "00", model.request_Id);
                }
                else { return fireResponse("Unresolved error: could not save customer information", "99", ""); }
            }
        }

        private bool saveIndividualCustomerInformation(IncomingCustomerViewModels entity)
        {
            ApiIndividualCustomerDetails model = entity.individualCustomerInformation;
            DateTime.TryParseExact(model.dateOfBirth, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateOfBirth);


            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
                BRANCHID = (short) entity.branchId, //entity.userBranchId,
                COMPANYID = (int) entity.companyId, //entity.companyId,
                CREATEDBY = (int) entity.createdBy, //(int)entity.createdBy,
                CREATIONMAILSENT = true, //entity.creationMailSent,
                CUSTOMERCODE = model.customerCode,
                CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
                CUSTOMERTYPEID = (short)CustomerTypeEnum.Individual,
                DATEOFBIRTH = dateOfBirth,
                DATETIMECREATED = DateTime.Now,
                EMAILADDRESS = model.emailAddress,
                FIRSTNAME = model.firstName,
                GENDER = model.gender,
                LASTNAME = model.lastName,
                //MAIDENNAME = model.maidenName,
                MARITALSTATUS = model.maritalStatus != "" ? int.Parse(model.maritalStatus) : 0,
                TITLE = model.title,
                MIDDLENAME = model.middleName,
                //MISCODE = model.misCode,
                //MISSTAFF = model.misStaff,
                NATIONALITYID = context.TBL_COUNTRY.Where(X => X.NAME.ToUpper() == model.countryOfOrigin.ToUpper()).FirstOrDefault()?.COUNTRYID,
                OCCUPATION = model.occupation,
                PLACEOFBIRTH = model.PlaceOfBirth,
                ISPOLITICALLYEXPOSED = model.politicallyExposed == "1" ? true : false,
                //ISINVESTMENTGRADE = model,
                ISREALATEDPARTY = entity.insiderRelatedParties != null ? entity.insiderRelatedParties.Count > 0 ?  true : false : false,
                RELATIONSHIPOFFICERID = entity.createdBy,
                SPOUSE = model.spouse,
                SUBSECTORID = entity.subsectorId,
                //se = entity.sectorId,
                //RISKRATINGID = model.riskRatingId,
                CUSTOMERBVN = model.customerBvn,
                //PROSPECTCUSTOMERCODE = model.prospectCustomerCode,
                ISPROSPECT = false,
                CRMSCOMPANYSIZEID = int.TryParse(model.crmsCompanySize, out int result) ? result : 0,
                //CRMSLEGALSTATUSID = model.crmsLegalStatus,
                CRMSRELATIONSHIPTYPEID = context.TBL_CRMS_REGULATORY.Where(x => x.CODE.ToUpper() == (model.crmsRelationship.ToUpper())).FirstOrDefault()?.CRMSREGULATORYID,
                COUNTRYOFRESIDENTID = context.TBL_COUNTRY.Where(X => X.NAME.ToUpper() == model.countryOfResidence.ToUpper()).FirstOrDefault()?.COUNTRYID ,
                NUMBEROFDEPENDENTS = model.children.Count(),
                //NUMBEROFLOANSTAKEN = model.numberOfLoansTaken,
                //MONTHLYLOANREPAYMENT = model.loanMonthlyRepaymentFromOtherBanks,
                //DATEOFRELATIONSHIPWITHBANK = model.dateOfRelationshipWithBank,
                //RELATIONSHIPTYPEID = model.relationshipTypeCode,
                TEAMLDR = model.teamLdr,
                TEAMNPL = model.teamNpl,
                //CORR = model.corr,
                PASTDUEOBLIGATIONS = model.pastDueObligation != "" ? int.Parse(model.pastDueObligation) : 0,
                APIREQUESTID = entity.request_Id,
                BUSINESSUNTID = model.businessUnitId
            };

            var test = context.TBL_CUSTOMER.Add(customer);
            context.SaveChanges();

            if (entity.customerAddresses != null)
            {
                foreach (var address in entity.customerAddresses)
                {
                    var state = context.TBL_STATE.Where(O => O.STATENAME.ToLower() == address.state.ToLower()).FirstOrDefault();

                    customer.TBL_CUSTOMER_ADDRESS.Add(new TBL_CUSTOMER_ADDRESS()
                    {
                        ADDRESS = address.address,
                        CUSTOMERID = customer.CUSTOMERID,
                        STATEID = state != null ? state.STATEID : 1,
                        CITYID = 1,
                        ADDRESSTYPEID = 1,
                        ACTIVE = true,
                        NEARESTLANDMARK = address.nearestLandmark,
                        ELECTRICMETERNUMBER = address.utilityBillNumber,
                    });
                }
            }
            

            if (entity.customerContacts != null)
            {
                foreach (var contact in entity.customerContacts)
                {
                    customer.TBL_CUSTOMER_PHONECONTACT.Add(new TBL_CUSTOMER_PHONECONTACT()
                    {
                        ACTIVE = true,
                        CUSTOMERID = customer.CUSTOMERID,
                        PHONE = contact.officeMobileNumber,
                        PHONENUMBER = contact.officeLandNumber,
                    });
                }
            }

            if (entity.companyDirectors != null)
            {
                //foreach (var director in entity.companyDirectors)
                //{
                //    customer.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(new TBL_CUSTOMER_COMPANY_DIRECTOR()
                //    {
                //        FIRSTNAME = director.firstName,
                //        CUSTOMERID = customer.CUSTOMERID,
                //        CUSTOMERTYPEID = short.Parse(model.customerType),
                //        COMPANYDIRECTORTYPEID = 1,
                //        SHAREHOLDINGPERCENTAGE = 0,
                //        ISPOLITICALLYEXPOSED = director.politicallyExposed == "1" ? true : false,
                //        CREATEDBY = entity.staffId.Value,
                //        DATECREATED = DateTime.Now,
                //        SURNAME = director.lastName,
                //        MIDDLENAME = director.otherNames,
                //        GENDER = director.gender,
                //        MARITALSTATUSID = director.maritalStatus.ToLower() == "single" ? 1 : director.maritalStatus.ToLower() == "married" ? 2 : 0,
                //        CUSTOMERBVN = director.bvn,
                //        CUSTOMERNIN = director.nin,
                //        ADDRESS = director.address,
                //        EMAILADDRESS = director.email,
                //        PHONENUMBER = director.phoneNumber,
                //        //REGISTRATION_NUMBER = null,
                //        //TAX_NUMBER = null,
                //        DATEOFBIRTH = DateTime.Parse(director.dateOfBirth)
                //    });
                //}

            }

            //THIS FOREACH LOOP IS NOT YET TESTED. PLEASE COMMENT REMOVE AFTER SIMULATION
            //AddCustomerClientSupplier(customer, entity);
           

            return context.SaveChanges() > 0;
        }

        private bool saveCorporateCustomerInformation(IncomingCustomerViewModels model)
        {
            try
            {
                ApiCustomerBusinessDetailsViewModel corporateDetails = model.corporateCustomerInformation;
                var crmsType = context.TBL_CRMS_REGULATORY.Where(x => x.CODE == corporateDetails.crmsCompanySize).FirstOrDefault();
                DateTime.TryParseExact(corporateDetails.dateOfIncorporation, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateOfIncorporation);

                var customer = new TBL_CUSTOMER
                {
                    ACCOUNTCREATIONCOMPLETE = false, //entity.accountCreationComplete,
                    BRANCHID = (short)model.branchId, //entity.userBranchId,
                    COMPANYID = (int)model.companyId, //entity.companyId,
                    CREATEDBY = (int)model.createdBy, //(int)entity.createdBy,
                    CREATIONMAILSENT = true, //entity.creationMailSent,
                    CUSTOMERCODE = corporateDetails.customerCode,
                    CUSTOMERSENSITIVITYLEVELID = 1, //entity.customerSensitivityLevelId,
                    CUSTOMERTYPEID = (short)CustomerTypeEnum.Corporate,
                    //DATEOFBIRTH = Convert.ToDateTime(corporateDetails.dateOfIncorporation),
                    DATEOFBIRTH = dateOfIncorporation,
                    DATETIMECREATED = DateTime.Now,
                    EMAILADDRESS = corporateDetails.emailAddress,
                    FIRSTNAME = corporateDetails.corporateName,
                    //MISCODE = model.misCode,
                    //MISSTAFF = model.misStaff,
                    //NATIONALITYID = context.TBL_COUNTRY.Where(X => X.NAME == corporateDetails.countryOfOrigin).FirstOrDefault()?.COUNTRYID,
                    RELATIONSHIPOFFICERID = model.createdBy,
                    ISPOLITICALLYEXPOSED = corporateDetails.politicallyExposed == "1" ? true : false,
                    //ISINVESTMENTGRADE = model,

                    //SUBSECTORID = model.,
                    //RISKRATINGID = model.riskRatingId,
                    //CUSTOMERBVN = corporateDetails.customerBvn,
                    //PROSPECTCUSTOMERCODE = model.prospectCustomerCode,
                    ISPROSPECT = false,
                    CRMSCOMPANYSIZEID = crmsType?.CRMSREGULATORYID,
                    //CRMSLEGALSTATUSID = model.crmsLegalStatus,
                    CRMSRELATIONSHIPTYPEID = context.TBL_CRMS_REGULATORY.Where(x => x.CODE == (corporateDetails.crmsRelationship))?.FirstOrDefault()?.CRMSREGULATORYID,
                    // COUNTRYOFRESIDENTID = context.TBL_COUNTRY.Where(X => X.NAME == corporateDetails.countryOfResidence).FirstOrDefault()?.COUNTRYID,
                    SUBSECTORID = model.subsectorId,
                    //NUMBEROFLOANSTAKEN = model.numberOfLoansTaken,
                    //MONTHLYLOANREPAYMENT = model.loanMonthlyRepaymentFromOtherBanks,
                    //DATEOFRELATIONSHIPWITHBANK = model.dateOfRelationshipWithBank,
                    //RELATIONSHIPTYPEID = model.relationshipTypeCode,
                    TEAMLDR = corporateDetails.teamLdr,
                    TEAMNPL = corporateDetails.teamNpl,
                    APIREQUESTID = model.request_Id,
                    //CORR = model.corr,
                    //PASTDUEOBLIGATIONS = Convert.ToDecimal(corporateDetails.pastDueObligation),
                    BUSINESSUNTID = corporateDetails.businessUnitId,
                    TAXNUMBER = corporateDetails.tin
                };

                var test = context.TBL_CUSTOMER.Add(customer);
                context.SaveChanges();

                //if (model.companyDirectors != null)
                //{
                //    foreach (var director in model.companyDirectors)
                //    {
                //        var customerDir = new TBL_CUSTOMER_COMPANY_DIRECTOR()
                //        {
                //            FIRSTNAME = director.firstName,
                //            CUSTOMERID = customer.CUSTOMERID,
                //            CUSTOMERTYPEID = short.Parse(model.customerType),
                //            COMPANYDIRECTORTYPEID = 1,
                //            SHAREHOLDINGPERCENTAGE = 0,
                //            ISPOLITICALLYEXPOSED = director.politicallyExposed == "1" ? true : false,
                //            CREATEDBY = model.staffId.Value,
                //            DATECREATED = DateTime.Now,
                //            SURNAME = director.lastName,
                //            MIDDLENAME = director.otherNames,
                //            GENDER = director.gender,
                //            MARITALSTATUSID = director.maritalStatus.ToLower() == "single" ? 1 : director.maritalStatus.ToLower() == "married" ? 2 : 0,
                //            CUSTOMERBVN = director.bvn,
                //            CUSTOMERNIN = director.nin,
                //            ADDRESS = director.address,
                //            EMAILADDRESS = director.email,
                //            PHONENUMBER = director.phoneNumber,
                //            //REGISTRATION_NUMBER = null,
                //            //TAX_NUMBER = null,
                //            DATEOFBIRTH = DateTime.Parse(director.dateOfBirth)
                //        };
                //        var test2 = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(customerDir);
                //        context.SaveChanges();
                //    }

                //}

                ////THIS FOREACH LOOP IS NOT YET TESTED. PLEASE REMOVE COMMENT AFTER SIMULATION

                if (model.customerAddresses != null)
                {
                    foreach (var address in model.customerAddresses)
                    {
                        var state = context.TBL_STATE.Where(O => O.STATENAME.ToLower() == address.state.ToLower()).FirstOrDefault();

                        customer.TBL_CUSTOMER_ADDRESS.Add(new TBL_CUSTOMER_ADDRESS()
                        {
                            ADDRESS = address.address,
                            CUSTOMERID = customer.CUSTOMERID,
                            STATEID = state != null ? state.STATEID : 1,
                            CITYID = 1,
                            ADDRESSTYPEID = 1,
                            ACTIVE = true,
                            NEARESTLANDMARK = address.nearestLandmark,
                            ELECTRICMETERNUMBER = address.utilityBillNumber,
                        });
                    }
                }

                if (model.customerContacts != null)
                {
                    foreach (var contact in model.customerContacts)
                    {
                        customer.TBL_CUSTOMER_PHONECONTACT.Add(new TBL_CUSTOMER_PHONECONTACT()
                        {
                            ACTIVE = true,
                            CUSTOMERID = customer.CUSTOMERID,
                            PHONE = contact.officeMobileNumber,
                            PHONENUMBER = contact.officeLandNumber,
                        });
                    }
                }

             
                //AddCustomerClientSupplier(customer, model);

                return context.SaveChanges() > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
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

        public void AddCustomerClientSupplier(TBL_CUSTOMER customer, IncomingCustomerViewModels model)
        {
            if (model.corporateSuppliers != null)
            {
                foreach (var supplier in model.corporateSuppliers)
                {
                    TBL_CUSTOMER_CLIENT_SUPPLIER clientSupplier = new TBL_CUSTOMER_CLIENT_SUPPLIER();

                    //clientSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Find(customer.CUSTOMERID);
                    //var accountCompleted = context.TBL_CUSTOMER.Find(customer.CUSTOMERID)?.ACCOUNTCREATIONCOMPLETE;
                    clientSupplier.CUSTOMERID = customer.CUSTOMERID;
                    clientSupplier.CUSTOMERTYPEID = (short)CustomerTypeEnum.Corporate;
                    clientSupplier.FIRSTNAME = supplier.corporateName;
                    clientSupplier.MIDDLENAME = null;
                    clientSupplier.LASTNAME = null;
                    clientSupplier.TAX_NUMBER = null;
                    clientSupplier.REGISTRATION_NUMBER = supplier.rcNumber;
                    clientSupplier.HAS_CASA_ACCOUNT = supplier.accountNumber != null;
                    clientSupplier.CASA_ACCOUNTNO = supplier.accountNumber;
                    clientSupplier.BANKNAME = supplier.BankName;
                    clientSupplier.NATURE_OF_BUSINESS = supplier.natureOfBusiness;
                    clientSupplier.CONTACT_PERSON = supplier.contactPerson;
                    clientSupplier.ADDRESS = supplier.address;
                    clientSupplier.PHONENUMBER = supplier.phoneNumber;
                    clientSupplier.EMAILADDRESS = supplier.email;
                    clientSupplier.CLIENT_SUPPLIERTYPEID = (short)CompanyClientOrSupplierTypeEnum.Supplier;
                    clientSupplier.CREATEDBY = (int)model.createdBy;
                    clientSupplier.DATECREATED = genSetup.GetApplicationDate();

                    context.TBL_CUSTOMER_CLIENT_SUPPLIER.Add(clientSupplier);
                }
            }

            if (model.individualSuppliers != null)
            {
                foreach (var supplier in model.individualSuppliers)
                {
                    TBL_CUSTOMER_CLIENT_SUPPLIER clientSupplier = new TBL_CUSTOMER_CLIENT_SUPPLIER();

                    //clientSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Find(customer.CUSTOMERID);
                    //var accountCompleted = context.TBL_CUSTOMER.Find(customer.CUSTOMERID)?.ACCOUNTCREATIONCOMPLETE;
                    clientSupplier.CUSTOMERID = customer.CUSTOMERID;
                    clientSupplier.CUSTOMERTYPEID = (short)CustomerTypeEnum.Individual;
                    clientSupplier.FIRSTNAME = supplier.firstName;
                    clientSupplier.MIDDLENAME = supplier.otherNames;
                    clientSupplier.LASTNAME = supplier.lastName;
                    clientSupplier.TAX_NUMBER = null;
                    clientSupplier.REGISTRATION_NUMBER = null;
                    clientSupplier.HAS_CASA_ACCOUNT = supplier.accountNumber != null;
                    clientSupplier.CASA_ACCOUNTNO = supplier.accountNumber;
                    clientSupplier.BANKNAME = supplier.BankName;
                    clientSupplier.NATURE_OF_BUSINESS = supplier.natureOfBusiness;
                    clientSupplier.CONTACT_PERSON = null;
                    clientSupplier.ADDRESS = supplier.address;
                    clientSupplier.PHONENUMBER = supplier.phoneNumber;
                    clientSupplier.EMAILADDRESS = supplier.email;
                    clientSupplier.CLIENT_SUPPLIERTYPEID = (short)CompanyClientOrSupplierTypeEnum.Supplier;
                    clientSupplier.CREATEDBY = (int)model.createdBy;
                    clientSupplier.DATECREATED = genSetup.GetApplicationDate();


                    context.TBL_CUSTOMER_CLIENT_SUPPLIER.Add(clientSupplier);
                }
            }
        }

        #endregion END OF CUSTOMER PROFILE METHODS


            #region  LOAN REQUEST METHODS
            public APIResponse submitRequest(CflLoanApplication model)
        {
            APIResponse response = new APIResponse();

            if(model.callStatusCode == "02")
            {
                if (model.accountOfficerStaffCode == string.Empty) return fireResponse("Missing account officer code", "99", "");

                var accountOfficer1 = context.TBL_STAFF.Where(x => x.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();
                if (accountOfficer1 == null) return fireResponse("Account officer does not exist in Fintrak Credit360", "99", "");

                var application = context.TBL_LOAN_APPLICATION.Where(O => O.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault();
                if (application == null) { return fireResponse("Application does not exist in Fintrak Credit360", "99", ""); }

                if (model.creditBureauReport == null || model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99", ""); }

                if (model.creditBureauReport == null || model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99", ""); }

                model.createdBy = accountOfficer1.STAFFID;
                model.customerCode = context.TBL_CUSTOMER.Find(application.CUSTOMERID)?.CUSTOMERCODE;
                SaveLoanDocument(model);
                return fireResponse("Offer Letter successfully saved", "00", application.APIREQUESTID);
            }

            var product = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == model.productCode).FirstOrDefault();

            if (product == null) { return fireResponse("Product Code does not exist", "99",""); }
            if (model.requestId == null) { return fireResponse("Missing application unique indentifier", "99",""); }

            var subSector = context.TBL_SUB_SECTOR.Where(x => x.CODE == model.subSectorCode).FirstOrDefault();
            if (subSector == null) { return fireResponse("Sub Sector Code does not exist in Fintrak Credit360", "99",""); }

            //var sector = context.TBL_SECTOR.Where(x => x.CODE == subSector.CODE).FirstOrDefault();
            var sector = context.TBL_SECTOR.Where(x => x.CODE == model.sectorCode).FirstOrDefault();
            if (sector == null) { return fireResponse("Sector Code does not exist in Fintrak Credit360", "99", ""); }

            var currency = context.TBL_CURRENCY.Where(x => x.CURRENCYCODE == model.currencyCode || x.CURRENCYCODE =="NGN").FirstOrDefault();
            if (currency == null) return fireResponse("Missing currency code", "99","");

            if (model.accountOfficerStaffCode == string.Empty) return fireResponse("Missing account officer code", "99","");

            var accountOfficer = context.TBL_STAFF.Where(x => x.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();
            if (accountOfficer == null) return fireResponse("Account officer does not exist in Fintrak Credit360", "99", "");

            var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFCODE == model.relationshipManagerStaffCode).FirstOrDefault();

            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == model.customerCode).FirstOrDefault();
            if(customer == null) return fireResponse("This customer is not profiled on Fintrak Credit360 application", "99", "");

            //var casa = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == model.settlementAccount).FirstOrDefault();
            var casa = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == model.settlementAccount && x.CUSTOMERID == customer.CUSTOMERID).FirstOrDefault();

            LoanApplicationViewModel loanApp = new LoanApplicationViewModel();
            loanApp.customerId = customer.CUSTOMERID;
            loanApp.proposedTenor = Convert.ToInt16(model.tenor);

            var currentDate = genSetup.GetApplicationDate();
            var endDate = currentDate.AddMonths(loanApp.proposedTenor);
            loanApp.proposedTenor = (int)(endDate - currentDate).TotalDays;

            loanApp.tenorModeId = (short)TenorModeEnum.Months;
            loanApp.proposedAmount = Convert.ToDecimal(model.loanAmount);
            loanApp.productId = product.PRODUCTID;
            loanApp.productClassId = product.PRODUCTCLASSID;
            loanApp.productClassProcessId = product.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID;
            loanApp.loanPurpose = model.purpose;
            loanApp.sectorId = (int)subSector?.SECTORID;
            loanApp.subSectorId = (short)subSector?.SUBSECTORID;
            loanApp.exchangeRate = model.exchangeRate != string.Empty ? Convert.ToDouble(model.exchangeRate ) : (double)0;
            loanApp.currencyCode = currency.CURRENCYCODE;
            loanApp.interestRate = Convert.ToDouble(model.interestRate);
            loanApp.editMode = model.callStatusCode == "01" ?  true : false;
            loanApp.relationshipOfficerId = accountOfficer.STAFFID;
            loanApp.companyId = model.companyId;
            loanApp.loanInformation = "<p></p>";
            loanApp.casaAccountId = casa?.CASAACCOUNTID;
            loanApp.branchId = accountOfficer.BRANCHID;
            loanApp.createdBy = accountOfficer.STAFFID;


            if(context.TBL_LOAN_APPLICATION.Any(x=>x.APIREQUESTID == model.requestId && x.DELETED != true))
            {
                if(model.callStatusCode == "01")
                {
                    loanApp.loanApplicationId = context.TBL_LOAN_APPLICATION.Where(O => O.APIREQUESTID == model.requestId).FirstOrDefault().LOANAPPLICATIONID;
                    loanApp.loanApplicationDetailId = context.TBL_LOAN_APPLICATION_DETAIL.Where(O => O.LOANAPPLICATIONID == loanApp.loanApplicationId).FirstOrDefault().LOANAPPLICATIONDETAILID;

                    response.requestId = model.requestId;
                    if (UpdateLoanApplicationDetail(loanApp))
                    {
                        SaveLoanDocument(model);
                        response.StatusCode = "00";
                        response.Message = "Success";
                    }
                    else
                    {
                        response.StatusCode = "99";
                        response.Message = "CFL Failed!";
                    }

                    SaveCashflowRequestToApiLog(model, response);
                    return response;
                }
                else { return fireResponse("New application request Id already exist", "99", ""); }
            }

            var loanExist = context.TBL_LOAN_APPLICATION_DETAIL.Any(o => o.APPROVEDAMOUNT == loanApp.proposedAmount
                 && o.APPROVEDINTERESTRATE == loanApp.interestRate && o.APPROVEDTENOR == loanApp.proposedTenor && o.CURRENCYID == currency.CURRENCYID && o.CUSTOMERID == customer.CUSTOMERID
                 && o.SUBSECTORID == loanApp.sectorId && o.CREATEDBY == 1 && o.DELETED != true);

            if (loanExist == true) return fireResponse("This loan application has already been saved", "99", "");

            if (model.creditBureauReport == null || model.creditBureauReport.Count <= 0) { return fireResponse("Missing Credit Bureau Report", "99", ""); }

            if (model.creditBureauReport == null || model.creditBureauReport.Count < 3) { return fireResponse("At least 3 Credit Reports are required", "99", ""); }

            response.applicationReferenceNumber = AddLoanApplication(loanApp, model.requestId);
            model.createdBy = accountOfficer.STAFFID;
            model.customerCode = customer.CUSTOMERCODE;
            model.applicationReferenceNumber = response.applicationReferenceNumber;
            SaveLoanDocument(model);

            response.StatusCode = "00";
            response.Message = "Success";
            if (response.applicationReferenceNumber == null)
            {
                response.StatusCode = "99";
                response.Message = "CFL Failed!";
            }

            SaveCashflowRequestToApiLog(model, response);
            return response;
        }

        private void SaveCashflowRequestToApiLog(CflLoanApplication request, APIResponse response)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            request.creditBureauReport = null;
            request.loanApplicationFiles = null;

            var jsonRequest = js.Serialize(request);
            var jsonResponse = js.Serialize(response);

            context.TBL_CUSTOM_API_LOGS.Add(new TBL_CUSTOM_API_LOGS()
            {
                APIURL = request.applicationUrl,
                LOGTYPEID = 1,
                REFERENCENUMBER = request.requestId,
                REQUESTDATETIME = DateTime.Now,
                RESPONSEDATETIME = DateTime.Now,
                REQUESTMESSAGE = jsonRequest,
                RESPONSEMESSAGE = jsonResponse,
            });

            context.SaveChanges();
        }


        //private void SaveLoanDocument(CflLoanApplication model)
        private void SaveLoanDocument(CflLoanApplication model)
        {
            
            FinTrakBankingDocumentsContext docContext = new FinTrakBankingDocumentsContext();
            var staffId = context.TBL_STAFF.Where(s => s.STAFFCODE == model.accountOfficerStaffCode).FirstOrDefault();
            var loanApplicationId = context.TBL_LOAN_APPLICATION.Where(O => O.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault().LOANAPPLICATIONID;
            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == model.customerCode).FirstOrDefault();

            foreach (var loanFile in model.loanApplicationFiles)
            {

                var existing = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
                    && x.OPERATIONID == (int)OperationsEnum.CreditAppraisal
                    && x.TARGETID == loanApplicationId
                    && x.CUSTOMERCODE == model.customerCode)
                    .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false && x.FILENAME == loanFile.caption + "." + loanFile.fileExtension)
                        , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => new { us, up }
                    ).Select(x => new DocumentUploadViewModel
                    {
                        documentUploadId = x.up.DOCUMENTUPLOADID,
                        documentUsageId = x.us.DOCUMENTUSAGEID,
                        fileName = x.up.FILENAME,
                        fileExtension = x.up.FILEEXTENSION,
                        fileSize = x.up.FILESIZE,
                        fileSizeUnit = x.up.FILESIZEUNIT,
                        companyId = x.up.COMPANYID,
                        issueDate = x.up.ISSUEDATE,
                        expiryDate = x.up.EXPIRYDATE,
                        createdBy = (int)x.up.CREATEDBY
                    }).FirstOrDefault();

                if (existing != null) {
                    var oldUpload = docContext.TBL_DOCUMENT_UPLOAD.Find(existing.documentUploadId);
                    var oldUsage = docContext.TBL_DOCUMENT_USAGE.Find(existing.documentUsageId);

                    oldUpload.DELETED = true;
                    oldUpload.DELETEDBY = model.createdBy;
                    oldUpload.DATETIMEDELETED = DateTime.Now;

                    oldUsage.DELETED = true;
                    oldUsage.DELETEDBY = model.createdBy;
                    oldUsage.DATETIMEDELETED = DateTime.Now;
                }

                var document = new TBL_DOCUMENT_UPLOAD()
                {
                    DOCUMENTTYPEID = Convert.ToInt32( loanFile.documentTypeId), // ?? 236, //Offer Letter
                    FILENAME = loanFile.caption+"."+loanFile.fileExtension,
                    FILEEXTENSION = loanFile.fileExtension,
                    FILESIZE = loanFile.fileData.Length,
                    FILEDATA = Convert.FromBase64String(loanFile.fileData),
                    COMPANYID = model.companyId,
                    DELETED = false,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = staffId.STAFFID,
                };

                docContext.TBL_DOCUMENT_UPLOAD.Add(document);
                docContext.SaveChanges();

                docContext.TBL_DOCUMENT_USAGE.Add(new TBL_DOCUMENT_USAGE()
                {
                    DOCUMENTUPLOADID = document.DOCUMENTUPLOADID,
                    TARGETID = loanApplicationId, //context.TBL_LOAN_APPLICATION.Where(O => O.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault().LOANAPPLICATIONID,
                    TARGETREFERENCENUMBER = model.applicationReferenceNumber,
                    CUSTOMERCODE = model.customerCode,
                    OPERATIONID = (int) OperationsEnum.CreditAppraisal,
                    ISPRIMARYDOCUMENT = false,
                    DELETED = false,
                    CREATEDBY = staffId.STAFFID,
                    DATETIMECREATED = DateTime.Now
                });
            }

            foreach (var loanFile in model.creditBureauReport)
            {
                var caption = string.Empty;
                if (Convert.ToInt16(loanFile.creditBureauType) == (short)CreditBureauEnum.CRCCreditBureau) caption = "CRCCreditBureau";
                if (Convert.ToInt16(loanFile.creditBureauType) == (short)CreditBureauEnum.XDSCreditBureau) caption = "FirstCentralCreditBureau";
                if (Convert.ToInt16(loanFile.creditBureauType) == (short)CreditBureauEnum.CRMS) caption = "CRMSCreditBureau";

                //================================================

                /*
                 * The following block of code does not validate for specific loan request. Hence some loan request did not display credit bureau documents
                var previousSearch = customerBureauReport.GetCustomerCreditBureauReportLog(customer.CUSTOMERID, null);
                bool hascrms = false;

                foreach (var i in previousSearch)
                {
                    if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
                };

                if (previousSearch.Count() >= 2 && !hascrms && Convert.ToInt16(loanFile.creditBureauType) != (short)CreditBureauEnum.CRMS)
                    continue;

                if (previousSearch.Count() >= 3)
                    continue;

                //var existing = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(O => O.CUSTOMERID == customer.CUSTOMERID && O.CREDITBUREAUID == entity.creditBureauId
                //                                                                && O.COMPANYDIRECTORID == entity.companyDirectorId && O.DELETED == false
                //                                                                && (DbFunctions.DiffDays(O.DATETIMECREATED, DateTime.Now).Value <= 90));

                var existingCreditBureau = previousSearch.FirstOrDefault(O => O.creditBureauId == int.Parse(loanFile.creditBureauType));

                if (existingCreditBureau != null)
                    continue;
                */

                // if (entity.companyDirectorId == 0) entity.companyDirectorId = null;
                var data = new Entities.Models.TBL_CUSTOMER_CREDIT_BUREAU()
                {
                    COMPANYDIRECTORID = null, //entity.companyDirectorId,
                    CHARGEAMOUNT = 0, // entity.chargeAmount,
                    CREDITBUREAUID = Convert.ToInt16(loanFile.creditBureauType),// entity.creditBureauId,
                    CUSTOMERID = customer.CUSTOMERID,
                    ISREPORTOKAY = true, //entity.isReportOkay,
                    USEDINTEGRATION = false, //entity.usedIntegration,
                    DATECOMPLETED = DateTime.Now, // entity.dateCompleted,
                    DATETIMECREATED = DateTime.Now,
                    BRANCHID = model.userBranchId,
                    CREATEDBY = model.createdBy
                };

                context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);
                context.SaveChanges();
                // ==========================================

                if (loanFile.reportFileDateinPDF == null || loanFile.reportFileDateinPDF == string.Empty) continue;

                var existing = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
                    && x.OPERATIONID == (int)OperationsEnum.CreditAppraisal
                    && x.TARGETID == loanApplicationId
                    && x.CUSTOMERCODE == model.customerCode)
                    .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false && x.FILENAME == caption + "." + "pdf")
                        , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => new { us, up }
                    ).Select(x => new DocumentUploadViewModel
                    {
                        documentUploadId = x.up.DOCUMENTUPLOADID,
                        documentUsageId = x.us.DOCUMENTUSAGEID,
                        fileName = x.up.FILENAME,
                        fileExtension = x.up.FILEEXTENSION,
                        fileSize = x.up.FILESIZE,
                        fileSizeUnit = x.up.FILESIZEUNIT,
                        companyId = x.up.COMPANYID,
                        issueDate = x.up.ISSUEDATE,
                        expiryDate = x.up.EXPIRYDATE,
                        createdBy = (int)x.up.CREATEDBY
                    }).FirstOrDefault();

                if (existing != null)
                {
                    var oldUpload = docContext.TBL_DOCUMENT_UPLOAD.Find(existing.documentUploadId);
                    var oldUsage = docContext.TBL_DOCUMENT_USAGE.Find(existing.documentUsageId);

                    oldUpload.DELETED = true;
                    oldUpload.DELETEDBY = model.createdBy;
                    oldUpload.DATETIMEDELETED = DateTime.Now;

                    oldUsage.DELETED = true;
                    oldUsage.DELETEDBY = model.createdBy;
                    oldUsage.DATETIMEDELETED = DateTime.Now;
                }

                var document = new TBL_DOCUMENT_UPLOAD()
                {
                    DOCUMENTTYPEID = Convert.ToInt32(loanFile.documentTypeId), 
                    FILENAME = caption + "." + "pdf",
                    FILEEXTENSION = "pdf",
                    FILESIZE = (int)loanFile.reportFileDateinPDF.Length,
                    FILEDATA = Convert.FromBase64String(loanFile.reportFileDateinPDF),
                    COMPANYID = model.companyId,
                    DELETED = false,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = staffId.STAFFID,
                };

                docContext.TBL_DOCUMENT_UPLOAD.Add(document);
                docContext.SaveChanges();

                docContext.TBL_DOCUMENT_USAGE.Add(new TBL_DOCUMENT_USAGE()
                {
                    DOCUMENTUPLOADID = document.DOCUMENTUPLOADID,
                    TARGETID = loanApplicationId, //context.TBL_LOAN_APPLICATION.Where(O => O.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault().LOANAPPLICATIONID,
                    TARGETREFERENCENUMBER = model.applicationReferenceNumber,
                    CUSTOMERCODE = model.customerCode,
                    OPERATIONID = (int)OperationsEnum.CreditAppraisal,
                    ISPRIMARYDOCUMENT = false,
                    DELETED = false,
                    CREATEDBY = staffId.STAFFID,
                    DATETIMECREATED = DateTime.Now
                });
            }
            var n = docContext;
            docContext.SaveChanges();
        }

        public string AddLoanApplication(LoanApplicationViewModel loan, string apiRequestId)
        {
           // ValidateLoanApplicationLimits(loan);
            var additionalAmount = loan.LoanApplicationDetail.Sum(x => x.exchangeAmount);
            var savedDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId && c.DELETED == false).ToList();

            decimal cumulativeSum = 0;
            foreach (var s in savedDetails) { cumulativeSum = cumulativeSum + (s.PROPOSEDAMOUNT * (decimal)s.EXCHANGERATE); }

            if (loan.relationshipOfficerId != 0)
            {
                var validation = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId);
                if (validation.maximumAllowedLimit > 0) if ((cumulativeSum + additionalAmount) > (decimal)validation.limit) fireResponse($"RM Limit Exceeded. The limit of this RM is {validation.limit}","99","");
            }

            loan.applicationAmount = cumulativeSum + additionalAmount;

            if (loan.editMode == true )
            {
                if (UpdateLoanApplicationDetail(loan)) return loan.applicationReferenceNumber;
                else return null;
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
                    var app = AddloanApplicationSub(loan, apiRequestId);

                    if (AddLoanApplicationDetail(loan, app))
                    {
                        SubmitLoanApplicationForCam(app.LOANAPPLICATIONID, loan.createdBy, 0);
                        context.SaveChanges();                       
                        return app.APPLICATIONREFERENCENUMBER;
                    }
                }
            }
            else
            {
                var limit = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId).limit;
                if ((limit != 0 && loan.applicationAmount != 0 && loan.applicationAmount > (decimal)limit)) throw new SecureException($"RM Limit Exceeded. The limit of this RM is {limit}");
                UpdateLoanApplication(loan);
            }

            return null;
        }

        private bool AddLoanApplicationDetail(LoanApplicationViewModel loan, TBL_LOAN_APPLICATION app)//List<LoanApplicationDetailViewModel> entity, int createdBy)
        {
            var createdBy = loan.createdBy;
            //foreach (var a in entity)
            //{
            var a = loan.LoanApplicationDetail.FirstOrDefault();
            if (app.PRODUCTID == null) app.PRODUCTID = loan.productId;

            //if (a.repaymentScheduleId <= 0)
            //{
            //    fireResponse("Please select a repayment pattern","99","");
            //}

            //if (a.proposedTenor == 0)
            //{
            //    fireResponse("Tenor can not be ZERO (0)","99","");
            //}
      

            var data = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDAMOUNT = loan.proposedAmount,
                APPROVEDINTERESTRATE = (double)app.INTERESTRATE,
                APPROVEDPRODUCTID = (short)app.PRODUCTID,
                APPROVEDTENOR = loan.proposedTenor,
                
                EXCHANGERATE = loan.exchangeRate,
                CURRENCYID = 1,
                CUSTOMERID = (int)app.CUSTOMERID,
                LOANAPPLICATIONID = app.LOANAPPLICATIONID,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,
                
                
                //EQUITYCASAACCOUNTID = a?.equityCasaAccountId,
                //EQUITYAMOUNT = a?.equityAmount ,
                
                //PROPOSEDAMOUNT = app.APPROVEDAMOUNT,
                PROPOSEDAMOUNT = app.APPLICATIONAMOUNT,
                PROPOSEDINTERESTRATE = (int)app.INTERESTRATE,
                PROPOSEDPRODUCTID = (short)app.PRODUCTID,
                PROPOSEDTENOR = loan.proposedTenor, //Convert.ToInt32(Math.Round(((decimal)(app.pr / 12) * (decimal)365))),
                DELETED = false,
                SUBSECTORID = loan.subSectorId,
                CREATEDBY = loan.createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = loan.loanPurpose,
                CASAACCOUNTID = loan.casaAccountId,
                OPERATINGCASAACCOUNTID = loan.casaAccountId,
                REPAYMENTSCHEDULEID = (short)FrequencyTypeEnum.Monthly,
                REPAYMENTTERMS = "Monthly",
                LOANDETAILREVIEWTYPEID = (short)LoanDetailReviewTypeEnum.Initial,
                //CRMSFUNDINGSOURCEID = loan.fundingSource,
                //CRMSREPAYMENTSOURCEID = a?.crmsPaymentSourceId,
                //CRMSFUNDINGSOURCECATEGORY = a?.crmsFundingSourceCategory,
                //CRMS_ECCI_NUMBER = a?.crms_ECCI_Number,
                //FIELD1 = a.fieldOne,
                //FIELD2 = a.fieldTwo,
                //FIELD3 = a.fieldThree,
                //PRODUCTPRICEINDEXID = a?.productPriceIndexId,
                //PRODUCTPRICEINDEXRATE = a?.productPriceIndexRate,
               
                TENORFREQUENCYTYPEID = (short) TenorModeEnum.Months,
                CRMSVALIDATED = false,
                ISTAKEOVERAPPLICATION = false,
            };

            //var loanExist = context.TBL_LOAN_APPLICATION_DETAIL.Any(o => o.APPROVEDAMOUNT == data.APPROVEDAMOUNT
            //        && o.APPROVEDINTERESTRATE == data.APPROVEDINTERESTRATE && o.APPROVEDTENOR == data.APPROVEDTENOR && o.CURRENCYID == data.CURRENCYID && o.CUSTOMERID == data.CUSTOMERID
            //        && o.SUBSECTORID == data.SUBSECTORID && o.CREATEDBY == data.CREATEDBY && o.DELETED != true);

            //if (loanExist == true) throw new SecureException("This loan application has already been saved!");

            var appl = context.TBL_LOAN_APPLICATION_DETAIL.Add(data);

            context.SaveChanges();

            var productFeesModel = new List<ProductFeesViewModel>();
            var productFees = context.TBL_PRODUCT_CHARGE_FEE.Where(x => x.PRODUCTID == app.PRODUCTID);

            foreach(var productFee in productFees)
            {
                var feeModel = new ProductFeesViewModel()
                {
                    loanChargeFeeId = productFee.CHARGEFEEID,
                    defaultfeeRateValue = productFee.RATEVALUE,
                    recommededFeeRateValue = productFee.RATEVALUE,
                    rate = productFee.RATEVALUE,
                    feeId = productFee.CHARGEFEEID,
                    loanApplicationDetailId = appl.LOANAPPLICATIONDETAILID,
                };

                productFeesModel.Add(feeModel);
            }


            ProductFees(productFeesModel, appl.LOANAPPLICATIONDETAILID, loan.createdBy);
            

            return context.SaveChanges() > 0;

        }

        //private void saveFacilityFees(List<productFees> fees )
        //{
        //    foreach(var fee in fees)
        //    {
        //        var chargeFeeSetup = context.TBL_CHARGE_FEE.Where(x=>x.SHORTNAME == fee.feeCode ).Join(context.TBL_CHARGE_FEE_DETAIL.Where(x=>x. .FirstOrDefault();

        //        //var chargeFeeSetup = (from x in context.TBL_CHARGE_FEE
        //        //                      join d in context.TBL_CHARGE_FEE_DETAIL on x.CHARGEFEEID equals d.CHARGEFEEID
        //        //                      where x.SHORTNAME == fee.feeCode && d.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Primary  ).FirstOrDefault();

        //        var facilityFee = new TBL_LOAN_APPLICATION_DETL_FEE
        //        {
        //            CHARGEFEEID = chargeFeeSetup.CHARGEFEEID,
        //            HASCONSESSION = false,
        //            APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
        //            LOANAPPLICATIONDETAILID = 1,
        //            DEFAULT_FEERATEVALUE = chargeFeeSetup.,
        //            RECOMMENDED_FEERATEVALUE = Convert.ToDecimal(fee.feeRate),
        //            CREATEDBY = 1,
        //            DATETIMECREATED = DateTime.Now,
        //            DELETED = false,

        //        };
        //    }

        //}

        private TBL_LOAN_APPLICATION AddloanApplicationSub(LoanApplicationViewModel loan, string apiRequestId)
        {
            short productClassProcessId = 0;
            short? productClassId = null;
            var isGroupLoan = false;
            var response = 0;
            int loanId = 0;
            //var proposedProductId = loan.LoanApplicationDetail.FirstOrDefault()?.proposedProductId ?? 0;

            // ValidateLoanApplicationLimits(loan); // init only
            var workflowProductId = GetWorkflowProductId(loan.productId);

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
                BRANCHID = loan.branchId ?? 0,
                RELATIONSHIPOFFICERID = loan.createdBy, 
                RELATIONSHIPMANAGERID = loan.createdBy, 
                MISCODE = loan.misCode ?? "002",
                TEAMMISCODE = loan.teamMisCode ?? "002",
                INTERESTRATE = loan.interestRate,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                LOANINFORMATION = loan.loanInformation,
                ISRELATEDPARTY = loan.isRelatedParty,
                ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed,
                OWNEDBY = (int)loan.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                CUSTOMERGROUPID = loan.customerGroupId,
                CASAACCOUNTID = loan.casaAccountId,
                APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONAMOUNT = loan.proposedAmount,
                APPLICATIONTENOR = loan.proposedTenor,
                ISINVESTMENTGRADE = loan.isInvestmentGrade,
                CAPREGIONID = loan.regionId,
                REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                LOANTERMSHEETID = loan.loantermSheetId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                // FLOWCHANGEID = loan.flowchangeId,
                OPERATIONID = (short)OperationsEnum.CreditAppraisal,
                LOANAPPLICATIONTYPEID = (short)LoanTypeEnum.Single,
                COLLATERALDETAIL = loan.collateralDetail,
                ISADHOCAPPLICATION = false,
                LOANSWITHOTHERS = loan.loansWithOthers,
                OWNERSHIPSTRUCTURE = loan.ownershipStructure,
                LOANAPPROVEDLIMITID = loan.loanApprovedLimitId,
                PRODUCTID = workflowProductId,
                APIREQUESTID = apiRequestId,
                
         
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
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
            //    STAFFID = loan.createdBy,
            //    BRANCHID = (short)loan.userBranchId,
            //    DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = loan.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = loan.loanApplicationId,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName(),
            //};

            //this.auditTrail.AddAuditTrail(audit);

            

            return loanData;
        }

        private bool UpdateLoanApplicationDetail(LoanApplicationViewModel update)
        {
            UpdateLoanApplication(update); 

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == update.loanApplicationDetailId);
            //var update = update.LoanApplicationDetail.SingleOrDefault();

            //if (update == null) fireResponse("Sequence contain not single! " + loan.LoanApplicationDetail.Count(), "99","");
            if (detail == null) fireResponse("Sequence contain not single! " + 0, "99", "");
            var product = context.TBL_PRODUCT.Find(update.productId);

            //if (update.repaymentScheduleId <= 0 && (detail.TBL_PRODUCT1.PRODUCTCLASSID != (int)ProductClassEnum.BondAndGuarantees))
            //{
            //    fireResponse("Please select a repayment pattern for the product " + update.productName, "99","");
            //}
            detail.APPROVEDAMOUNT = update.proposedAmount;
            detail.APPROVEDINTERESTRATE = update.interestRate;
            detail.APPROVEDPRODUCTID = update.productId;
            detail.APPROVEDTENOR = update.proposedTenor;

            // LEFT TO RIGHT MAPPING
            detail.SUBSECTORID = update.subSectorId;
            detail.PROPOSEDAMOUNT = update.proposedAmount;
            detail.PROPOSEDINTERESTRATE = update.interestRate;
            //detail.PROPOSEDPRODUCTID = (short) update.proposedProductId;
            detail.PROPOSEDTENOR = update.proposedTenor;
            detail.REPAYMENTSCHEDULEID = update.repaymentScheduleId;
            detail.REPAYMENTTERMS = update.repaymentTerm;
            detail.LOANPURPOSE = update.loanPurpose;
            //detail.PRODUCTPRICEINDEXID = product.PRODUCTPRICEINDEXID;
            //detail.PRODUCTPRICEINDEXRATE = product.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXRATE;
            detail.CASAACCOUNTID = update.casaAccountId;
            //detail.OPERATINGCASAACCOUNTID = update.operatingCasaAccountId;
            //detail.EQUITYCASAACCOUNTID = update.equityCasaAccountId;
            detail.CURRENCYID = update.currencyId;
            detail.TENORFREQUENCYTYPEID = update.tenorModeId;
            detail.ISTAKEOVERAPPLICATION = update.isTakeOverApplication;
            detail.PROPOSEDPRODUCTID = update.productId;
            detail.CURRENCYID = context.TBL_CURRENCY.Where(O => O.CURRENCYCODE == update.currencyCode).FirstOrDefault().CURRENCYID;

            var productClassId = detail.TBL_PRODUCT1.PRODUCTCLASSID;

            return context.SaveChanges() > 0;

        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                        where a.DELETED == false
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                            customerGroupId = a.CUSTOMERGROUPID,
                                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                                            //createdBy = a.CreatedBy,
                                            customerId = a.CUSTOMERID,
                                            //dateTimeCreated = a.DateTimeCreated
                                        }).ToList();

            return customerGroupMapping;
        }

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (loanTypeId == (int)LoanTypeEnum.CustomerGroup && customer.Count() == 1)
            {
                var customerGroupMappings = new List<CustomerGroupMappingViewModel>();
                var customerId = customer.FirstOrDefault().customerId;
                var customerGroups = GetCustomerGroupMapping().Where(m => m.customerGroupId == customerId).ToList();
                foreach (var customerGroup in customerGroups)
                {
                    var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                                where a.CUSTOMERGROUPID == customerGroup.customerGroupId && a.DELETED == false
                                                select new CustomerGroupMappingViewModel
                                                {
                                                    customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                                    customerGroupId = a.CUSTOMERGROUPID,
                                                    relationshipTypeId = a.RELATIONSHIPTYPEID,
                                                    relationshipTypeName = a.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                                    customerId = a.CUSTOMERID,
                                                    customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                                    customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME,
                                                    customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                                }).ToList();
                    if (customerGroupMapping.Count() > 0) customerGroupMappings.AddRange(customerGroupMapping);
                }

                if (customerGroupMappings.Count() > 0)
                {
                    customer = customerGroupMappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                }
            }

            foreach (var item in customer)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == item.customerId).CUSTOMERCODE.Trim();
                //var customCode = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == item.customerId).Select(x => x.CUSTOMERCODE).FirstOrDefault();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                customerName = a.CUSTOMERNAME,
                                customerCode = a.CUSTOMERID.Trim(),
                                facilityType = a.ADJFACILITYTYPE,
                                approvedAmount = a.LOANAMOUNYTCY ?? 0,
                                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
                                currency = a.CURRENCYNAME,
                                exposureTypeCodeString = a.EXPOSURETYPECODE,
                                adjFacilityTypeString = a.ADJFACILITYTYPE,
                                adjFacilityTypeCode = a.ADJFACILITYTYPEid,
                                productIdString = a.PRODUCTID,
                                productCode = a.PRODUCTCODE,
                                tenorString = a.TENOR,
                                productName = a.PRODUCTNAME,
                                //existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                //proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
                                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                bookingDate = a.BOOKINGDATE,
                                maturityDate = a.MATURITYDATE,
                                //maturityDateString = a.MATURITYDATE,
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).ToList();

                if (exposure.Count() > 0)
                {
                    foreach (var e in exposure)
                    {
                        e.exposureTypeId = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
                        //e.productId = int.Parse(e.productIdString);
                        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);

                        //e.bookingDate = e.bookingDateString;
                        //e.maturityDate = DateTime.Parse(e.maturityDateString);
                        //e.productId = int.Parse(e.productIdString);
                    }
                    exposures.AddRange(exposure);
                }

            }

            if (exposures.Count() == 0)
            {
                return exposures;
            }

            return exposures;
        }

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId, short loantypeId) // not used!
        {
            return GetCurrentCustomerExposure(customerIds, loantypeId, companyId); // old ify impl
        }

        public List<CurrentCustomerExposure> GetExposures(TBL_LOAN_APPLICATION loanApplication)
        {
            var exposures = new List<CurrentCustomerExposure>();
            var customerIds = new List<CustomerExposure>();
            if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID, loanApplication.LOANAPPLICATIONTYPEID);
            }
            else
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERGROUPID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID, loanApplication.LOANAPPLICATIONTYPEID);
            }
            return exposures;
        }

        private void UpdateLoanApplication(LoanApplicationViewModel loan)
        {
            var application = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loan.loanApplicationId).ToList();

            // decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + application.Sum(a => a.PROPOSEDAMOUNT);
            // decimal totalApplicationAmount = 0; 
            //foreach (var item in application)
            //{
            //    var exchangeValue = ((decimal)item.PROPOSEDAMOUNT * (decimal)item.EXCHANGERATE);
            //    totalApplicationAmount = totalApplicationAmount + exchangeValue;
            //}

            var loanData = context.TBL_LOAN_APPLICATION.Find(loan.loanApplicationId);
            decimal totalAmount = application.Sum(a => a.PROPOSEDAMOUNT * (decimal)a.EXCHANGERATE) + (GetExposures(loanData).Sum(e => e.outstandingsLcy));

            loanData.REQUIRECOLLATERAL = loan.requireCollateral;
            loanData.TOTALEXPOSUREAMOUNT = totalAmount;
            loanData.INTERESTRATE = loan.interestRate;
            loanData.APPLICATIONDATE = genSetup.GetApplicationDate();
            loanData.LOANINFORMATION = loan.loanInformation;
            loanData.ISRELATEDPARTY = loan.isRelatedParty;
            loanData.ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed;
            loanData.OWNEDBY = (int)loan.createdBy;
            loanData.DATETIMECREATED = genSetup.GetApplicationDate();
            loanData.SYSTEMDATETIME = DateTime.Now;
            loanData.CASAACCOUNTID = loan.casaAccountId;
            loanData.APPLICATIONAMOUNT = loan.proposedAmount;
            loanData.APPLICATIONTENOR = application.Max(c => c.PROPOSEDTENOR);
            loanData.COLLATERALDETAIL = loan.collateralDetail;
            loanData.CAPREGIONID = loan.regionId;
            loanData.REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId;
            loanData.LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId;
            loanData.LOANTERMSHEETID = loan.loantermSheetId;
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
            int branchId = application?.branchId ?? 0;
            int customerId = application?.customerId ?? 0;
            int productId = application?.productId ?? 0;
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
                if (branch?.NPL_LIMIT > 0 && branch?.NPL_LIMIT < (branchNplAmount + applicationAmount)) throw new SecureException("Branch NPL Limit exceeded!");
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
                    //if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var sectorId = context.TBL_SUB_SECTOR.Find(facility.subSectorId);
                        var setcorName = context.TBL_SECTOR.Find(sectorId.SECTORID).NAME ?? "N/A";
                        var sectorValidation = limitValidation.ValidateNPLBySector(facility.subSectorId);
                        decimal sectorAmount = (decimal)sectorValidation.outstandingBalance + (facility.proposedAmount * (decimal)facility.exchangeRate);
                        decimal sectorsAmount = (decimal)sectorValidation.outstandingSectorsBalance + (facility.proposedAmount * (decimal)facility.exchangeRate);
                        decimal percentageTotalExposure = decimal.Round((sectorAmount / sectorsAmount), 5, MidpointRounding.AwayFromZero);
                        if (percentageTotalExposure > 0 && percentageTotalExposure >= sectorValidation.maximumAllowedLimit) throw new SecureException("Sector Limit for sector, " + facility.sectorName + " exceeded!");
                        ///if (sectorValidation.maximumAllowedLimit > 0 && sectorValidation.maximumAllowedLimit <= sectorAmount) throw new SecureException("Sector Limit for sector, " + facility.sectorName + " exceeded!");
                    }
                }

            }
            try
            {
                if (limitValidation.ProductLimitExceeded(productId, application.proposedAmount))
                {
                    fireResponse("Product Limit exceeded!","99","");
                }
            }
            catch (Exception ex) { throw ex; }

            var exposure = GetCurrentCompanyExposure();
            var proposedExposure = exposure.proposedLimit + applicationAmount;
            var company = context.TBL_COMPANY.Find(application.companyId);
            if (proposedExposure >= company.SHAREHOLDERSFUND)
            {
                fireResponse("Company Limit Exceeded","99","");
            }

            var insiderLimit = limitValidation.ValidateNPLByInsiderCustomer();
            var insiderExposure = insiderLimit.outstandingBalance + (double)applicationAmount;
            if (insiderExposure >= (double)insiderLimit.maximumAllowedLimit)
            {
                fireResponse("Insider Limit Exceeded", "99","");
            }

            if (limitValidation.IsDirectorRelatedGroup(application.customerGroupId) || limitValidation.CustomerIsDirector(application.customerId))
            {
                var directorLimit = limitValidation.ValidateNPLByDirectors(application);
                var directorExposure = (double)applicationAmount + directorLimit.outstandingBalance;
                if (directorExposure >= (double)directorLimit.maximumAllowedLimit)
                {
                    fireResponse("Director Limit Exceeded","99","");
                }

            }

            var singleObligor = limitValidation.ValidateSingleObligorLimit(application);
            var proposedObligorLimit = singleObligor.outstandingBalance + (double)applicationAmount;
            if (proposedObligorLimit >= (double)singleObligor.maximumAllowedLimit)
            {
                fireResponse("Single Obligor Limit Exceeded","99","");
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
                        pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
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
                    pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
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
                pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
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
            foreach (var fee in fees)
            {
                var feeRecord = new TBL_LOAN_APPLICATION_DETL_FEE()
                {
                    CHARGEFEEID = fee.feeId,
                    RECOMMENDED_FEERATEVALUE = fee.rate,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = createdBy,
                    HASCONSESSION = false,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                    LOANAPPLICATIONDETAILID = fee.loanApplicationDetailId,
                    DEFAULT_FEERATEVALUE = fee.rate
                };
                context.TBL_LOAN_APPLICATION_DETL_FEE.Add(feeRecord);
            }
         
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

        private short SubmitLoanApplicationForCam(int applicationId, int staffId, int checkListIndex)
        {

            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId).FirstOrDefault();

            if (appl.LOANAPPROVEDLIMITID > 0)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.BookingRequestInitiated;
                workflow.NextProcess(appl.COMPANYID, appl.OWNEDBY, (int)OperationsEnum.IndividualDrawdownRequest, appl.FLOWCHANGEID, appl.LOANAPPLICATIONID, null, "New approved application", true, false, false,false, null);
                context.SaveChanges();
                return 1;
            }

            if (appl.PRODUCT_CLASS_PROCESSID == (int)ProductClassProcessEnum.ProductBased && checkListIndex == (int)ChecklistErrorEnum.NegetiveChecklist)
            {
                appl.PRODUCT_CLASS_PROCESSID = (int)ProductClassProcessEnum.CAMBased;
            }

            appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ChecklistCompleted;

            int? receiverLevelId = null;
            int operationId;
            //var product = context.TBL_PRODUCT.Find(detail.PROPOSEDPRODUCTID);
            //var productBahaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == detail.PROPOSEDPRODUCTID).FirstOrDefault();


            operationId = appl.OPERATIONID; 
            workflow.OperationId = operationId;
            appl.OPERATIONID = operationId;
            receiverLevelId = GetFirstReceiverLevel(staffId, (short)OperationsEnum.CreditAppraisal, appl.PRODUCTCLASSID, appl.PRODUCTID);
            workflow.NextLevelId = receiverLevelId;
            workflow.ToStaffId = staffId;

            workflow.StaffId = staffId;
            workflow.ToStaffId = staffId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.ProductId = appl.PRODUCTID;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "New loan application";
            workflow.ExclusiveFlowChangeId = appl.FLOWCHANGEID;

            if (workflow.LogActivity()) return 1;
            else return 0;
        }

        private int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, int? productId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId && x.PRODUCTID == productId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID).ToList();
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId).ToList();
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public int AddLoanDocument(LoanDocumentViewModel model, byte[] file)
        {
            FinTrakBankingDocumentsContext docContext = new  FinTrakBankingDocumentsContext();

            var existing = docContext.TBL_MEDIA_LOAN_DOCUMENTS
                .Where(x => x.FILENAME == model.fileName
                    && x.FILEEXTENSION == model.fileExtension
                    && x.LOANREFERENCENUMBER == model.loanReferenceNumber
                    );

            if (existing.Count() > 0 && model.overwrite == false) return 3;

            if (existing.Count() > 0 && model.overwrite == true)
            {
                docContext.TBL_MEDIA_LOAN_DOCUMENTS.RemoveRange(existing);
            }

            var data = new TBL_MEDIA_LOAN_DOCUMENTS
            {
                FILEDATA = file,
                LOANAPPLICATIONNUMBER = model.loanApplicationNumber,
                LOANREFERENCENUMBER = model.loanReferenceNumber,
                DOCUMENTTITLE = model.documentTitle,
                DOCUMENTTYPEID = model.documentTypeId,
                LOAN_BOOKING_REQUESTID = model.SourceId,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                SYSTEMDATETIME = DateTime.Now,
                PHYSICALFILENUMBER = model.physicalFileNumber,
                PHYSICALLOCATION = model.physicalLocation,
                ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                CREATEDBY = (int)model.createdBy,
            };

            docContext.TBL_MEDIA_LOAN_DOCUMENTS.Add(data);

            // Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"Added Loan Document with title : '{ model.documentTitle }' ",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName()
            //};
            //this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return docContext.SaveChanges() == 0 ? 1 : 2;
        }

        private APIResponse fireResponse(string message, string statusCode, string requestId)
        {
            APIResponse response = new APIResponse();
            response.StatusCode = statusCode;
            response.Message = message;
            response.requestId = requestId;

            return response;
        }

        #endregion END OF LOAN REQUEST METHODS


        
    }
}

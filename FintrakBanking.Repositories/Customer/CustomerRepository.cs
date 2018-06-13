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
        
        private int customerId;
        int status = 0;
        bool USE_THIRD_PARTY_INTEGRATION;

        public CustomerRepository(IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup,
            IWorkflow _workFlow,
            IApprovalLevelStaffRepository _level,
            FinTrakBankingContext _context,
            ICustomerCreditBureauRepository bureau, IIntegrationWithFinacle finacle)
        {
            context = _context;
            workflow = _workFlow;
            auditTrail = _auditTrail;
            _genSetup = genSetup;
            level = _level;
            this.finacle = finacle; 
            this.bureau = bureau;
            var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (global != null) this.USE_THIRD_PARTY_INTEGRATION = global.USE_THIRD_PARTY_INTEGRATION;
        }



        public dynamic GetCustomerRating(int custormerId)
        {

            var data = (from c in context.TBL_CUSTOMER
                where c.CUSTOMERID == custormerId
                select new
                {
                    isInvestment = c.TBL_CUSTOMER_RISK_RATING.ISINVESTMENTGRADE,
                    rating = c.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                }).FirstOrDefault();
            return data;
        }

        public bool AddCustomer(CustomerViewModels entity)
        {
            if (USE_THIRD_PARTY_INTEGRATION)
                entity.isPoliticallyExposed = finacle.GetExposePersonStatus(entity.customerCode);

            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete,
                BRANCHID = entity.userBranchId,
                COMPANYID = entity.companyId,
                CREATEDBY = (int) entity.createdBy,
                CREATIONMAILSENT = entity.creationMailSent,
                CUSTOMERCODE = entity.customerCode,
                CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId,
                CUSTOMERTYPEID = entity.customerTypeId,
                DATEOFBIRTH = entity.dateOfBirth,
                DATETIMECREATED = DateTime.Now,
                EMAILADDRESS = entity.emailAddress,
                FIRSTNAME = entity.firstName,
                GENDER = entity.gender,
                LASTNAME = entity.lastName,
                MAIDENNAME = entity.maidenName,
                MARITALSTATUS = entity.maritalStatus,
                TITLE = entity.title,
                MIDDLENAME = entity.middleName,
                MISCODE = entity.misCode,
                MISSTAFF = entity.misStaff,
                NATIONALITY = entity.nationality,
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
                CUSTOMERBVN = entity.customerBVN
            };
            context.TBL_CUSTOMER.Add(customer);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer  {entity.customerName} with Code: {entity.customerCode} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //customerId = customer.CUSTOMERID;

            //status = 1;
            //if (entity.CustomerAddresses.Count > 0)
            //{
            //    AddCustomerAddresses(entity.CustomerAddresses, status);
            //}

            //if (entity.CustomerBvn.Count > 0)
            //{
            //    AddCustomerBvn(entity.CustomerBvn, status);
            //}

            //if (entity.CustomerPhoneContact.Count > 0)
            //{
            //    AddCustomerPhoneContact(entity.CustomerPhoneContact);
            //}

            //if (entity.CustomerCompanyInfomation.Count > 0)
            //{
            //    AddCustomerCompanyInfomation(entity.CustomerCompanyInfomation, status);
            //}

            //if (entity.CustomerIdentification.Count > 0)
            //{
            //    AddCustomerIdentification(entity.CustomerIdentification);
            //}

            //if (entity.CustomerEmploymentHistory.Count > 0)
            //{
            //    AddCustomerEmploymentHistory(entity.CustomerEmploymentHistory);
            //}
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

            // Audit Section ---------------------------
           

            //end of Audit section -------------------------------


            // return response;
        }


        private void AddCustomerAddresses(List<CustomerAddressViewModels> entity,
            int status)
        {
            var address = new TBL_CUSTOMER_ADDRESS();
            foreach (var ent in entity)
            {
                address.ACTIVE = ent.active;
                address.ADDRESS = ent.address;
                address.ADDRESSTYPEID = (short) ent.addressTypeId;
                address.CITYID = ent.cityId;
                address.CUSTOMERID = ent.customerId;
                address.STATEID = ent.stateId;
                address.HOMETOWN = ent.homeTown;
                address.POBOX = ent.pobox;
                address.STATEID = ent.stateId;

                context.TBL_CUSTOMER_ADDRESS.Add(address);

            }
        }

        public bool AddCustomerAddresses(CustomerAddressViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_ADDRESS address;
                    // if (entity.addressId != 0 || entity.addressId < 0)  //Check if record is new or modified record
                    // {
                    address = context.TBL_CUSTOMER_ADDRESS.Find(entity.addressId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false and entity.canModified equal true, record insert directly to the main table 
                    if (address != null && accountCompleted == false)
                    {
                        address.ACTIVE = entity.active;
                        address.ADDRESS = entity.address;
                        address.ADDRESSTYPEID = (short) entity.addressTypeId;
                        address.CITYID = entity.cityId;
                        address.STATEID = entity.stateId;
                        address.HOMETOWN = entity.homeTown;
                        address.POBOX = entity.pobox;
                        address.STATEID = entity.stateId;
                        address.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                        address.NEARESTLANDMARK = entity.nearestLandmark;
                    }
                    else if (address == null && accountCompleted == false)
                    {
                        address = new TBL_CUSTOMER_ADDRESS();
                        address.ACTIVE = entity.active;
                        address.ADDRESS = entity.address;
                        address.ADDRESSTYPEID = (short) entity.addressTypeId;
                        address.CITYID = entity.cityId;
                        address.CUSTOMERID = entity.customerId;
                        address.STATEID = entity.stateId;
                        address.HOMETOWN = entity.homeTown;
                        address.POBOX = entity.pobox;
                        address.STATEID = entity.stateId;
                        address.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                        address.NEARESTLANDMARK = entity.nearestLandmark;
                        context.TBL_CUSTOMER_ADDRESS.Add(address);
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer address information has existing record being modified and approved
                        var existingTempAddress = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x =>
                            x.ADDRESSID == entity.addressId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.addressId != 0 || entity.addressId > 0)
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Address_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Address_Addition;
                        }



                        TBL_TEMP_CUSTOMER_ADDRESS temp = null;
                        if (existingTempAddress != null && entity.addressId > 0
                        ) //if customer address information has existing record being modified and approved, update it with the new change
                        {
                            temp = existingTempAddress;

                            temp.ACTIVE = entity.active;
                            temp.ADDRESS = entity.address;
                            temp.ADDRESSTYPEID = (short) entity.addressTypeId;
                            temp.CITYID = entity.cityId;
                            temp.STATEID = entity.stateId;
                            temp.HOMETOWN = entity.homeTown;
                            temp.POBOX = entity.pobox;
                            temp.STATEID = entity.stateId;
                            temp.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                            temp.NEARESTLANDMARK = entity.nearestLandmark;
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPADDRESSID;
                        }
                        else //if customer address information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMER_ADDRESS();

                            temp.ADDRESSID = entity.addressId;
                            temp.CUSTOMERID = entity.customerId;
                            temp.ACTIVE = entity.active;
                            temp.ADDRESS = entity.address;
                            temp.ADDRESSTYPEID = (short) entity.addressTypeId;
                            temp.CITYID = entity.cityId;
                            temp.STATEID = entity.stateId;
                            temp.HOMETOWN = entity.homeTown;
                            temp.POBOX = entity.pobox;
                            temp.STATEID = entity.stateId;
                            temp.ELECTRICMETERNUMBER = entity.electricMeterNumber;
                            temp.NEARESTLANDMARK = entity.nearestLandmark;
                            temp.CREATEDBY = entity.createdBy;
                            temp.DATETIMECREATED = DateTime.Now;
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUSTOMER_ADDRESS.Add(temp);
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
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_ADDRESS for customer ID: + (" + entity.customerId + ") ",
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
                    throw new Exception(ex.Message);
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
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_BVN for customer ID: + (" + entity.customerId + ") ",
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
                    throw new Exception(ex.Message);
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
                            AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                            STAFFID = staffId,
                            BRANCHID = BranchId,
                            DETAIL = "Added new TBL_CUSTOMER_CHILDREN for customer ID: + (" + entity.customerId + ") ",
                            IPADDRESS = entity.userIPAddress,
                            URL = entity.applicationUrl,
                            APPLICATIONDATE = _genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now
                        };

                        this.auditTrail.AddAuditTrail(audit);
                    }

                    context.TBL_CUSTOMER_CHILDREN.AddRange(custChildren);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
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
                try
                {

                    TBL_CUSTOMER_PHONECONTACT phone;

                    phone = context.TBL_CUSTOMER_PHONECONTACT.Find(entity.phoneContactId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
                    //If Customer main table ACCOUNTCREATIONCOMPLETE column equal false, record insert directly to the main table 
                    if (phone != null && accountCompleted == false)
                    {
                        phone.ACTIVE = entity.active;
                        phone.PHONE = entity.phone;
                        phone.PHONENUMBER = entity.phoneNumber;
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
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer phone contact information has existing record being modified and approved
                        var existingTempPhone = context.TBL_TEMP_CUSTOMER_PHONCONTACT.FirstOrDefault(x =>
                            x.TEMPPHONECONTACTID == entity.phoneContactId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.phoneContactId != 0 || entity.phoneContactId > 0)
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Phone_Number_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Phone_Number_Addition;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPPHONECONTACTID;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUSTOMER_PHONCONTACT.Add(temp);

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
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new phone contact for customer ID: + (" + entity.customerId + ") ",
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
                    throw new Exception(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerNextOfKin(CustomerNextOfKinViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_NEXTOFKIN next;

                    next = context.TBL_CUSTOMER_NEXTOFKIN.Find(entity.nextOfKinId);
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
                    }

                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer next of kin information has existing record being modified and approved
                        var existingTempNext = context.TBL_TEMP_CUSTOMER_NEXTOFKIN.FirstOrDefault(x =>
                            x.NEXTOFKINID == entity.nextOfKinId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.nextOfKinId != 0 || entity.nextOfKinId > 0)
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Next_of_Kin_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Next_of_Kin_Addition;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUSTOMER_NEXTOFKIN.Add(temp);
                            //  var res = context.SaveChanges() > 0;

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
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_NEXTOFKIN for customer ID: + (" + entity.customerId + ") ",
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
                    throw new Exception(ex.Message);
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
                    if (entity.companyInfomationId != 0 || entity.companyInfomationId < 0
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
                        }
                        else //If customer main table AccountCreationCompleted equals true then save record in temp table
                        {
                            //Check if customer company information has existing record being modified and approved
                            var existingTempCompany = context.TBL_TEMP_CUSTOMER_COMPANYINFO.FirstOrDefault(x =>
                                x.CUSTOMERID == company.CUSTOMERID && x.ISCURRENT == false &&
                                x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
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
                                temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                                temp.ISCURRENT = true;
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
                                temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                                temp.ISCURRENT = true;

                                context.TBL_TEMP_CUSTOMER_COMPANYINFO.Add(temp);
                            }

                            //Insert new row to TBL_CUSTOMER_MODIFICATION 
                            var modified = new TBL_CUSTOMER_MODIFICATION
                            {
                                CUSTOMERID = entity.customerId,
                                TARGETID = entity.customerId,
                                MODIFICATIONTYPEID = (int) CustomerInformationTrackerEnum.Corporate_Information,
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
                                    workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                    workflow.TargetId = targetId;
                                    workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                    throw new Exception(ex.Message);
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
                        context.TBL_CUSTOMER_COMPANYINFOMATION.Add(company);
                        company.SHAREHOLDER_FUND = entity.shareholderFund;
                    }

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added Customer's Company Information with CustomerId : " + entity.customerId,
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
                    throw new Exception(ex.Message);
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
                context.TBL_CUSTOMER_COMPANYINFOMATION.Add(info);

                // Audit Section ---------------------------
                var customer = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == info.CUSTOMERID);
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short) AuditTypeEnum.CustomerGroupAdded,
                    STAFFID = ent.createdBy,
                    BRANCHID = (short) ent.userBranchId,
                    DETAIL = "Added Customer's Customer Info for: " + customer.FIRSTNAME + " " + customer.LASTNAME +
                             " with Id: " + info.CUSTOMERID + " to company" + " (" + info.COMPANYNAME + ") " + " on " +
                             info.COMPANYINFOMATIONID,
                    IPADDRESS = ent.userIPAddress,
                    URL = ent.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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
                try
                {
                    List<TBL_CUSTOMER_COMPANY_BENEFICIA> beneficialList = new List<TBL_CUSTOMER_COMPANY_BENEFICIA>();
                    if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.Shareholder)
                    {

                        if (entity.customerCompanyBeneficial != null)
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
                                    DATECREATED = DateTime.Now
                                };
                                beneficialList.Add(beneficial);
                            }
                        }

                    }
                    else
                    {
                        entity.customerTypeId = (int) CustomerTypeEnum.Individual;
                    }

                    TBL_CUSTOMER_COMPANY_DIRECTOR directors;
                    directors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Find(entity.companyDirectorId);
                    var accountCompleted = context.TBL_CUSTOMER.Find(entity.customerId).ACCOUNTCREATIONCOMPLETE;
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
                        directors.ADDRESS = entity.address;
                        directors.PHONENUMBER = entity.phoneNumber;
                        directors.EMAILADDRESS = entity.email;
                        directors.CREATEDBY = entity.createdBy;
                        directors.DATECREATED = DateTime.Now;
                        directors.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                        context.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(directors);
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer company director information has existing record being modified and approved
                        var existingTempDirector = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x =>
                            x.COMPANYDIRECTORID == entity.companyDirectorId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.companyDirectorId != 0 || entity.companyDirectorId > 0)
                        {
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.Shareholder)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Shareholder_Modification;
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.Account_Signatory)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Signatory_Modification;
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.BoardMember)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Director_Modification;
                        }
                        else
                        {
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.Shareholder)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Shareholder_Addition;
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.Account_Signatory)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Signatory_Adition;
                            if (entity.companyDirectorTypeId == (int) CompanyDirectorTypeEnum.BoardMember)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Director_Addition;
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
                            // temp.TBL_TEMP_COMPANY_BENEFICIA = beneficialList;

                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
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

                                context.TBL_CUSTOMER_MODIFICATION.Add(modified);
                                var output = context.SaveChanges() > 0;
                                var targetId =
                                    modified
                                        .CUSTOMERMODIFICATIONID; //User the new inserted row in TBL_CUSTOMER_MODIFICATION as the approval trail targetId

                                workflow.StaffId = entity.staffId;
                                workflow.CompanyId = entity.companyId;
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL =
                            "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
                        IPADDRESS = entity.userIPAddress,
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
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
                    throw new Exception(ex.Message);
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
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL =
                            "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
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
                    throw new Exception(ex.Message);
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
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.client_SupplierId != 0 || entity.client_SupplierId > 0)
                        {
                            if (entity.client_SupplierTypeId == (int) CompanyClientOrSupplierTypeEnum.Client)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Client_Modification;
                            else
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Suplier_Modification;
                        }
                        else
                        {
                            if (entity.client_SupplierTypeId == (int) CompanyClientOrSupplierTypeEnum.Client)
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Client_Addition;
                            else
                                modificationTypeId = (int) CustomerInformationTrackerEnum.Supplier_Addition;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
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
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
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
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_CLIENT_SUPPLIER for customer ID: + (" + entity.customerId +
                                 ") ",
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
                    throw new Exception(ex.Message);
                }
            }

            return false;
        }

        public bool AddCustomerEmploymentHistory(CustomerEmploymentHistoryViewModels entity)
        {
            if (entity != null)
            {
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
                        history.EMPLOYERSTATEID = entity.employerStateId;
                        history.EMPLOYERNAME = entity.employerName;
                        history.OFFICEPHONE = entity.officePhone;
                        history.PREVIOUSEMPLOYER = entity.previousEmployer;
                    }
                    else if (history == null && accountCompleted == false)
                    {
                        history = new TBL_CUSTOMER_EMPLOYMENTHISTORY();

                        history.ACTIVE = entity.active;
                        history.CUSTOMERID = entity.customerId;
                        history.EMPLOYDATE = entity.employDate;
                        history.EMPLOYERADDRESS = entity.employerAddress;
                        history.EMPLOYERCOUNTRYID = entity.employerCountryId;
                        history.EMPLOYERSTATEID = entity.employerStateId;
                        history.EMPLOYERNAME = entity.employerName;
                        history.OFFICEPHONE = entity.officePhone;
                        history.PREVIOUSEMPLOYER = entity.previousEmployer;
                        context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Add(history);
                    }
                    else //If customer main table AccountCreationCompleted equals true then save record in temp table
                    {
                        //Check if customer employment information has existing record being modified and approved
                        var existingTempAddress = context.TBL_TEMP_CUSTOMEREMPLOYMENT.FirstOrDefault(x =>
                            x.PLACEOFWORKID == entity.placeOfWorkId && x.ISCURRENT == false &&
                            x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                        short modificationTypeId = 0;
                        int modifiedTargetId = 0;

                        if (entity.placeOfWorkId != 0 || entity.placeOfWorkId > 0)
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Employment_History_Modification;
                        }
                        else
                        {
                            modificationTypeId = (int) CustomerInformationTrackerEnum.Employement_History_Addition;
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
                            temp.EMPLOYERSTATEID = entity.employerStateId;
                            temp.EMPLOYERNAME = entity.employerName;
                            temp.OFFICEPHONE = entity.officePhone;
                            temp.PREVIOUSEMPLOYER = entity.previousEmployer;
                            existingTempAddress.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            existingTempAddress.ISCURRENT = true;
                            modifiedTargetId = temp.TEMPPLACEOFWORKID;
                        }
                        else //if customer employment information has no existing record being modified and approved, insert new row
                        {
                            temp = new TBL_TEMP_CUSTOMEREMPLOYMENT();
                            temp.ACTIVE = entity.active;
                            temp.CUSTOMERID = entity.customerId;
                            temp.EMPLOYDATE = entity.employDate;
                            temp.EMPLOYERADDRESS = entity.employerAddress;
                            temp.EMPLOYERCOUNTRYID = entity.employerCountryId;
                            temp.EMPLOYERSTATEID = entity.employerStateId;
                            temp.EMPLOYERNAME = entity.employerName;
                            temp.OFFICEPHONE = entity.officePhone;
                            temp.PREVIOUSEMPLOYER = entity.previousEmployer;
                            temp.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                            temp.ISCURRENT = true;
                            context.TBL_TEMP_CUSTOMEREMPLOYMENT.Add(temp);
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
                                workflow.StatusId = (int) ApprovalStatusEnum.Pending;
                                workflow.TargetId = targetId;
                                workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;
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
                                throw new Exception(ex.Message);
                            }
                        }

                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short) entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_EMPLOYMENTHISTORY for customer ID: + (" + entity.customerId +
                                 ") ",
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
                    throw new Exception(ex.Message);
                }

            }

            return false;
        }

        public bool DeleteChild(int childId)
        {
            var child = context.TBL_CUSTOMER_CHILDREN.Find(childId);

            if (child != null)
            {
                context.TBL_CUSTOMER_CHILDREN.Remove(child);
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
                    AUDITTYPEID = (short) AuditTypeEnum.CustomerDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short) user.BranchId,
                    DETAIL = "Deleted Customer: " + customer.LASTNAME + " with code: " + customer.CUSTOMERCODE,
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
            }
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        public CustomerViewModels GetCustomer(int custormerId)
        {
            var data = from a in context.TBL_CUSTOMER
                join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                where a.DELETED == false
                select new CustomerViewModels
                {
                    accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                    branchId = a.BRANCHID,
                    branchName = a.TBL_BRANCH.BRANCHNAME,
                    companyMainId = a.COMPANYID,
                    createdBy = a.CREATEDBY,
                    creationMailSent = a.CREATIONMAILSENT,
                    customerCode = a.CUSTOMERCODE,
                    customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                    customerTypeId = (short) a.CUSTOMERTYPEID,
                    dateOfBirth = (DateTime) a.DATEOFBIRTH,
                    customerId = a.CUSTOMERID,
                    emailAddress = a.EMAILADDRESS,
                    firstName = a.FIRSTNAME,
                    gender = a.GENDER,
                    lastName = a.LASTNAME,
                    maidenName = a.MAIDENNAME,
                    maritalStatus = a.MARITALSTATUS.Value,
                    title = a.TITLE,
                    middleName = a.MIDDLENAME,
                    misCode = a.MISCODE,
                    misStaff = a.MISSTAFF,
                    nationality = a.NATIONALITY,
                    occupation = a.OCCUPATION,
                    placeOfBirth = a.PLACEOFBIRTH,
                    isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                    relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                    relationshipOfficerName = st.FIRSTNAME + " " + st.LASTNAME,
                    spouse = a.SPOUSE,
                    sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                    sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                    subSectorId = (short) a.SUBSECTORID,
                    subSectorName = a.TBL_SUB_SECTOR.NAME,
                    taxNumber = a.TAXNUMBER,
                    riskRatingId = a.RISKRATINGID,
                    //   riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                    customerBVN = a.CUSTOMERBVN
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
                    GetSingleCustomerDirectorInfo(cust.customerId, (int) CompanyDirectorTypeEnum.BoardMember).ToList();
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
                        accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                        branchId = a.BRANCHID,
                        branchName = a.TBL_BRANCH.BRANCHNAME,
                        companyMainId = a.COMPANYID,
                        createdBy = a.CREATEDBY,
                        creationMailSent = a.CREATIONMAILSENT,
                        customerCode = a.CUSTOMERCODE,
                        customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                        customerTypeId = (short) a.CUSTOMERTYPEID,
                        dateOfBirth = (DateTime) a.DATEOFBIRTH,
                        customerId = a.CUSTOMERID,
                        emailAddress = a.EMAILADDRESS,
                        firstName = a.FIRSTNAME,
                        gender = a.GENDER,
                        lastName = a.LASTNAME,
                        maidenName = a.MAIDENNAME,
                        maritalStatus = a.MARITALSTATUS.Value,
                        title = a.TITLE,
                        middleName = a.MIDDLENAME,
                        //customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                        // customerTypeName =  context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                        misCode = a.MISCODE,
                        misStaff = a.MISSTAFF,
                        nationality = a.NATIONALITY,
                        occupation = a.OCCUPATION,
                        placeOfBirth = a.PLACEOFBIRTH,
                        isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                        relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                        relationshipOfficerName = st.FIRSTNAME + " " + st.LASTNAME,
                        spouse = a.SPOUSE,
                        sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                        sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                        subSectorId = (short) a.SUBSECTORID,
                        subSectorName = a.TBL_SUB_SECTOR.NAME,
                        taxNumber = a.TAXNUMBER,
                        riskRatingId = a.RISKRATINGID,
                        //   riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                        customerBVN = a.CUSTOMERBVN,
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
                        accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                        branchId = a.BRANCHID,
                        branchName = a.TBL_BRANCH.BRANCHNAME,
                        companyMainId = a.COMPANYID,
                        createdBy = a.CREATEDBY,
                        creationMailSent = a.CREATIONMAILSENT,
                        customerCode = a.CUSTOMERCODE,
                        customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                        customerTypeId = (short) a.CUSTOMERTYPEID,
                        dateOfBirth = (DateTime) a.DATEOFBIRTH,
                        customerId = a.CUSTOMERID,
                        emailAddress = a.EMAILADDRESS,
                        firstName = a.FIRSTNAME,
                        gender = a.GENDER,
                        lastName = a.LASTNAME,
                        maidenName = a.MAIDENNAME,
                        maritalStatus = a.MARITALSTATUS.Value,
                        title = a.TITLE,
                        middleName = a.MIDDLENAME,
                        //customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                        customerTypeName = context.TBL_CUSTOMER_TYPE
                            .FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                        misCode = a.MISCODE,
                        misStaff = a.MISSTAFF,
                        nationality = a.NATIONALITY,
                        occupation = a.OCCUPATION,
                        placeOfBirth = a.PLACEOFBIRTH,
                        isPoliticallyExposed = a.ISPOLITICALLYEXPOSED == false
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
                        subSectorId = (short) a.SUBSECTORID,
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
                                (short) CompanyDirectorTypeEnum.BoardMember)
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
                                email = s.EMAILADDRESS
                            }).ToList(),
                        CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s =>
                                s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID ==
                                (short) CompanyDirectorTypeEnum.Shareholder)
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
                customerTypeId = (short) a.CUSTOMERTYPEID,
                dateOfBirth = (DateTime) a.DATEOFBIRTH,
                customerId = a.CUSTOMERID,
                emailAddress = a.EMAILADDRESS,
                firstName = a.FIRSTNAME,
                gender = a.GENDER,
                lastName = a.LASTNAME,
                maidenName = a.MAIDENNAME,
                maritalStatus = a.MARITALSTATUS.Value,
                title = a.TITLE,
                middleName = a.MIDDLENAME,
                //customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                customerTypeName =
                    a.TBL_CUSTOMER_TYPE
                        .NAME, // context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                misCode = a.MISCODE,
                misStaff = a.MISSTAFF,
                nationality = a.NATIONALITY,
                occupation = a.OCCUPATION,
                placeOfBirth = a.PLACEOFBIRTH,
                isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                //relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                //         + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                spouse = a.SPOUSE,
                sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                subSectorId = (short) a.SUBSECTORID,
                subSectorName = a.TBL_SUB_SECTOR.NAME,
                taxNumber = a.TAXNUMBER,
                riskRatingId = a.RISKRATINGID,
                // riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                customerBVN = a.CUSTOMERBVN,
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
                    name = a.NAME,
                    customerTypeId = a.CUSTOMERTYPEID
                };
            return type;
        }

        public IEnumerable<CustomerAddressTypeViewModels> GetCustomerAddressType()
        {
            var type = from a in context.TBL_CUSTOMER_ADDRESS_TYPE
                where a.ADDRESSTYPEID != (int) CustomerAddressTypeEnum.Corporate
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
            return type;
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
            var customerMain = context.TBL_CUSTOMER.Find(customerId);
            if (customerMain != null && customerMain.ACCOUNTCREATIONCOMPLETE == false && entity.canModified == true)
            {

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
                customerMain.MARITALSTATUS = entity.maritalStatus;
                customerMain.TITLE = entity.title;
                customerMain.MIDDLENAME = entity.middleName;
                customerMain.MISCODE = entity.misCode;
                customerMain.MISSTAFF = entity.misStaff;
                customerMain.NATIONALITY = entity.nationality;
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
                customerMain.DATETIMEUPDATED = DateTime.Now;
                customerMain.LASTUPDATEDBY = entity.deletedBy;
                return context.SaveChanges() != 0;
            }
            else
            {
                TBL_TEMP_CUSTOMER customer;
                var existingTempCustomer = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x =>
                    x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower() && x.COMPANYID == entity.companyId &&
                    x.ISCURRENT == false && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved);
                var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER.Where(x =>
                    x.ISCURRENT == true && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending &&
                    x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower());
                //////if (unApprovedCustomerUpdate.Any())
                //////{
                //////    throw new Exception("Customer is already undergoing approval");
                //////}

                if (existingTempCustomer != null)
                {
                    customer = existingTempCustomer;
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
                    customer.MARITALSTATUS = entity.maritalStatus;
                    customer.TITLE = entity.title;
                    customer.MIDDLENAME = entity.middleName;
                    customer.MISCODE = entity.misCode;
                    customer.MISSTAFF = entity.misStaff;
                    customer.NATIONALITY = entity.nationality;
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
                    customer.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                }
                else
                {
                    customer = new TBL_TEMP_CUSTOMER();
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
                    customer.MARITALSTATUS = entity.maritalStatus;
                    customer.TITLE = entity.title;
                    customer.MIDDLENAME = entity.middleName;
                    customer.MISCODE = entity.misCode;
                    customer.MISSTAFF = entity.misStaff;
                    customer.NATIONALITY = entity.nationality;
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
                    customer.APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                    context.TBL_TEMP_CUSTOMER.Add(customer);
                }

                var modified = new TBL_CUSTOMER_MODIFICATION
                {
                    CUSTOMERID = entity.customerId,
                    TARGETID = entity.customerId,
                    MODIFICATIONTYPEID = (int) CustomerInformationTrackerEnum.General_Information,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short) entity.userBranchId,
                    DETAIL = "Updated TBL_CUSTOMER: " + entity.customerName + " with code: " + entity.customerCode +
                             " on" + " (" + entity.customerId + ") ",
                    IPADDRESS = entity.userIPAddress,
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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
                        throw new Exception(ex.Message);
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
                        relationshipOfficerId = c.relationshipOfficerId

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
                where x.firstName.ToLower().Contains(searchQuery.ToLower())
                      || x.lastName.ToLower().Contains(searchQuery.ToLower())
                      || x.middleName.ToLower().Contains(searchQuery.ToLower())
                      || x.customerCode.Contains(searchQuery)
                      || x.branchName.Contains(searchQuery)
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
            var customers = GetCustomers();
            if (loanCust.Any())
            {
                customers = customers.Where(x => loanCust.Contains(x.customerId));
            }

            return customers;
        }

        public CustomerViewModels GetSingleCustomerGeneralInfo(string customerCode)
        {
            var data = (from a in context.TBL_CUSTOMER
                where a.DELETED == false && a.CUSTOMERCODE == customerCode
                select new CustomerViewModels
                {
                    accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                    branchId = a.BRANCHID,
                    branchName = a.TBL_BRANCH.BRANCHNAME,
                    companyMainId = a.COMPANYID,
                    createdBy = a.CREATEDBY,
                    creationMailSent = a.CREATIONMAILSENT,
                    customerCode = a.CUSTOMERCODE,
                    customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                    customerTypeId = (short) a.CUSTOMERTYPEID,
                    dateOfBirth = (DateTime) a.DATEOFBIRTH,
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
                    misCode = a.MISCODE,
                    misStaff = a.MISSTAFF,
                    nationality = a.NATIONALITY,
                    occupation = a.OCCUPATION,
                    placeOfBirth = a.PLACEOFBIRTH,
                    isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                    relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                    spouse = a.SPOUSE,
                    sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                    sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                    subSectorId = (short) a.SUBSECTORID,
                    subSectorName = a.TBL_SUB_SECTOR.NAME,
                    taxNumber = a.TAXNUMBER,
                    relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID)
                        .Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                    riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                    customerBVN = a.CUSTOMERBVN,
                }).FirstOrDefault();
         // if (USE_THIRD_PARTY_INTEGRATION)
          //    data.isPoliticallyExposed = finacle.GetExposePersonStatus(data.customerCode);

            return data;
        }

        public CustomerViewModels GetSingleCustomerGeneralInfoByCustomerId(int customerId)
        {
            var data = (from a in context.TBL_CUSTOMER
                where a.DELETED == false && a.CUSTOMERID == customerId
                select new CustomerViewModels
                {
                    accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                    branchId = a.BRANCHID,
                    branchName = a.TBL_BRANCH.BRANCHNAME,
                    companyMainId = a.COMPANYID,
                    createdBy = a.CREATEDBY,
                    creationMailSent = a.CREATIONMAILSENT,
                    customerCode = a.CUSTOMERCODE,
                    customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                    customerTypeId = (short) a.CUSTOMERTYPEID,
                    dateOfBirth = (DateTime) a.DATEOFBIRTH,
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
                    misCode = a.MISCODE,
                    misStaff = a.MISSTAFF,
                    nationality = a.NATIONALITY,
                    occupation = a.OCCUPATION,
                    placeOfBirth = a.PLACEOFBIRTH,
                    relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                    spouse = a.SPOUSE,
                    isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                    sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                    sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                    subSectorId = (short) a.SUBSECTORID,
                    subSectorName = a.TBL_SUB_SECTOR.NAME,
                    taxNumber = a.TAXNUMBER,
                    relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID)
                        .Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                    riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                    customerBVN = a.CUSTOMERBVN,
                }).FirstOrDefault();
           // if (USE_THIRD_PARTY_INTEGRATION)
               // data.isPoliticallyExposed = finacle.GetExposePersonStatus(data.customerCode);
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
                    customerTypeId = (short) a.CUSTOMERTYPEID,
                    dateOfBirth = (DateTime) a.DATEOFBIRTH,
                    customerId = a.CUSTOMERID,
                    emailAddress = a.EMAILADDRESS,
                    firstName = a.FIRSTNAME,
                    gender = a.GENDER,
                    lastName = a.LASTNAME,
                    maidenName = a.MAIDENNAME,
                    maritalStatus = a.MARITALSTATUS.Value,
                    title = a.TITLE,
                    middleName = a.MIDDLENAME,
                    customerTypeName = context.TBL_CUSTOMER_TYPE
                        .FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                    misCode = a.MISCODE,
                    misStaff = a.MISSTAFF,
                    nationality = a.NATIONALITY,
                    occupation = a.OCCUPATION,
                    placeOfBirth = a.PLACEOFBIRTH,
                    isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                    relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                    spouse = a.SPOUSE,
                    //  sectorId = context.TBL_SUB_SECTOR.FirstOrDefault(c=>c.SUBSECTORID == (short)a.SUBSECTORID).SECTORID,
                    // sectorName = context.TBL_SECTOR.FirstOrDefault(d=>d.TBL_SUB_SECTOR.FirstOrDefault(l=>l.SUBSECTORID==a.SUBSECTORID).NAME,
                    subSectorId = (short) a.SUBSECTORID,
                    subSectorName = context.TBL_SUB_SECTOR.FirstOrDefault(r => r.SUBSECTORID == a.SUBSECTORID).NAME,
                    taxNumber = a.TAXNUMBER,
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
                    shareholderFund = d.SHAREHOLDER_FUND
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
                    shareholderFund = d.SHAREHOLDER_FUND
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
                    customerId = x.CUSTOMERID,
                    homeTown = x.HOMETOWN,
                    nearestLandmark = x.NEARESTLANDMARK,
                    electricMeterNumber = x.ELECTRICMETERNUMBER,
                    pobox = x.POBOX,
                    stateId = x.STATEID,
                    addressId = x.ADDRESSID,
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
                    addressId = x.ADDRESSID,
                    active = x.ACTIVE
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
                    employerStateId = s.EMPLOYERSTATEID
                }).ToList();
            return employmentHistory;
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
                    employerStateId = s.EMPLOYERSTATEID
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
                    customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA
                        .Where(a => a.COMPANYDIRECTORID == s.COMPANYDIRECTORID).Select(x =>
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
                    bankVerificationNumber = s.CUSTOMERBVN,
                    companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                    rcNumber = s.REGISTRATION_NUMBER,
                    taxNumber = s.TAX_NUMBER,
                    companyDirectorTypeName =dt.COMPANYDIRECTORYTYPENAME,
                    customerId = s.CUSTOMERID,
                    customerName = s.FIRSTNAME + " " + s.SURNAME,
                    address = s.ADDRESS,
                    phoneNumber = s.PHONENUMBER,
                    email = s.EMAILADDRESS,
                    customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA
                        .Where(a => a.COMPANYDIRECTORID == s.COMPANYDIRECTORID).Select(x =>
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
                      s.COMPANYDIRECTORTYPEID == (short) CompanyDirectorTypeEnum.Shareholder &&
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
                }).ToList();
            return companyDirectors;
        }

        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId,
            short customerTypeId, int targetId)
        {
            var companyDirectors = (from s in context.TBL_TEMP_CUSTOMER_DIRECTOR
                                    join dt in context.TBL_CUSTOMER_COMPANY_DIREC_TYP on s.COMPANYDIRECTORTYPEID equals dt.COMPANYDIRECTORYTYPEID
                where s.CUSTOMERID == customerId &&
                      s.COMPANYDIRECTORTYPEID == (short) CompanyDirectorTypeEnum.Shareholder &&
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
                .Where(a => a.COMPANYDIRECTORID == companyDirectorId).Select(x =>
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
            var data = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == custormerId && c.ACCOUNTCREATIONCOMPLETE == true)
                .Select(c => new GroupCustomerMembersViewModel()
                {
                    customerId = c.CUSTOMERID,
                    firstName = c.FIRSTNAME + " " + c.MIDDLENAME,
                    lastName = c.LASTNAME,
                    customerTypeId = c.CUSTOMERTYPEID,
                    customerType = c.TBL_CUSTOMER_TYPE.NAME
                });

            foreach (var item in data)
            {
                if (bureau.VerifyCustomerValidCreditBureau(item.customerId))
                {
                    lstCustomer.Add(item);
                }
            }

            return lstCustomer;
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
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Updated customer information for : + (" + data.FIRSTNAME + " " + data.LASTNAME + ") ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending &&
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
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending &&
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
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending &&
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
                x.ISCURRENT == true && x.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending &&
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
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int) OperationsEnum.CustomerInformationApproval)
                .ToList();

            return (from a in context.TBL_CUSTOMER_MODIFICATION
                join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                join br in context.TBL_BRANCH on b.BRANCHID equals br.BRANCHID
                join st in context.TBL_STAFF on a.CREATEDBY equals st.STAFFID
                join c in context.TBL_APPROVAL_TRAIL on a.CUSTOMERMODIFICATIONID equals c.TARGETID
                join e in context.TBL_APPROVAL_STATUS on c.APPROVALSTATUSID equals e.APPROVALSTATUSID
                where
                    (c.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending ||
                     c.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing)
                    && a.APPROVALCOMPLETED == false
                    && c.RESPONSESTAFFID == null
                    && c.OPERATIONID == (int) OperationsEnum.CustomerInformationApproval
                    && ids.Contains((int) c.TOAPPROVALLEVELID)
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
                    operationId = (int) OperationsEnum.CustomerInformationApproval
                }).ToList();

        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int) OperationsEnum.CustomerInformationApproval;
            entity.externalInitialization = false;
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.StaffId = entity.staffId;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = ((short) entity.approvalStatusId == (short) ApprovalStatusEnum.Approved)
                        ? (short) ApprovalStatusEnum.Processing
                        : (short) entity.approvalStatusId;
                    workflow.TargetId = entity.targetId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = (int) OperationsEnum.CustomerInformationApproval;

                    workflow.LogActivity();

                    if (workflow.NewState == (int) ApprovalState.Ended)
                    {
                        var response = ApproveCustomerInformation(entity.targetId, (short) workflow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }

                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
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
                if (modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.General_Information)
                {
                    returnVal = ApproveGeneralInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Corporate_Information)
                {
                    returnVal = ApproveCompanyInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Address_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Address_Modification)
                {
                    returnVal = ApproveAddressInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Phone_Number_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Phone_Number_Modification)
                {
                    returnVal = ApprovePhoneContactInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Employement_History_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Employment_History_Modification)
                {
                    returnVal = ApproveEmploymentHistoryInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Next_of_Kin_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Next_of_Kin_Addition)
                {
                    returnVal = ApproveNextOfKinInformation(modifiedData.CUSTOMERMODIFICATIONID, modifiedData.TARGETID,
                        approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Client_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Client_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Suplier_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Supplier_Addition)
                {
                    returnVal = ApproveClientSupplierInformation(modifiedData.CUSTOMERMODIFICATIONID,
                        modifiedData.TARGETID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Director_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Director_Addition ||
                         modifiedData.MODIFICATIONTYPEID ==
                         (int) CustomerInformationTrackerEnum.Shareholder_Modification ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Shareholder_Addition ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Signatory_Adition ||
                         modifiedData.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Signatory_Modification)
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
            //Check if Customer  exist in the temp table using the customerId
            var temp = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == targetId);
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
                entity.NATIONALITY = temp.NATIONALITY;
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
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Information for customer with code: " + entity.CUSTOMERCODE,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveCompanyInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
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
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Company Information:  with Id: " + entity.COMPANYINFOMATIONID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveAddressInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_ADDRESS temp = null;
            TBL_CUSTOMER_ADDRESS entity = null;

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer address information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Address_Addition)
            {
                temp = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x => x.TEMPADDRESSID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
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
                    context.TBL_CUSTOMER_ADDRESS.Add(entity);
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Address_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMER_ADDRESS.FirstOrDefault(x => x.TEMPADDRESSID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(x => x.ADDRESSID == temp.ADDRESSID);
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
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;


            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Address Information:  with Id: " + entity.ADDRESSID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
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
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Phone_Number_Addition)
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
            else if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Phone_Number_Modification)
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
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Address Information:  with Id: " + entity.PHONECONTACTID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Employement_History_Addition)
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
                    entity.EMPLOYERNAME = temp.EMPLOYERNAME;
                    entity.OFFICEPHONE = temp.OFFICEPHONE;
                    entity.PREVIOUSEMPLOYER = temp.PREVIOUSEMPLOYER;
                    context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Add(entity);
                    var saved = context.SaveChanges() > 0;
                }
            }
            else if (modified.MODIFICATIONTYPEID ==
                     (int) CustomerInformationTrackerEnum.Employment_History_Modification)
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
                    entity.EMPLOYERNAME = temp.EMPLOYERNAME;
                    entity.OFFICEPHONE = temp.OFFICEPHONE;
                    entity.PREVIOUSEMPLOYER = temp.PREVIOUSEMPLOYER;
                }
            }

            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.PLACEOFWORKID = entity.PLACEOFWORKID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Employment History Information:  with Id: " + entity.PLACEOFWORKID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveNextOfKinInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_NEXTOFKIN temp = null;
            TBL_CUSTOMER_NEXTOFKIN entity = null;

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer Next of Kin information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Next_of_Kin_Addition)
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
            else if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Next_of_Kin_Modification)
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
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Next of Kin Information:  with Id: " + entity.NEXTOFKINID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Client_Addition ||
                modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Supplier_Addition)
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
            else if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Client_Modification ||
                     modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Suplier_Modification)
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
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Top Client Supplier Information:  with Id: " + entity.CLIENT_SUPPLIERID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        private bool ApproveDirectorInformation(int modifiedId, int targetId, short approvalStatusId, UserInfo user)
        {
            TBL_TEMP_CUSTOMER_DIRECTOR temp = null;
            TBL_CUSTOMER_COMPANY_DIRECTOR entity = null;

            var modified = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modified != null)
            {
                modified.APPROVALCOMPLETED = true;
            }

            //Check if Customer phone contact information exist in the temp table using the targetId
            if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Director_Addition ||
                modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Shareholder_Addition ||
                modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Signatory_Adition)
            {
                temp = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x => x.TEMPCOMPANYDIRECTORID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
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
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                    entity.CREATEDBY = temp.CREATEDBY;
                    entity.DATECREATED = temp.DATECREATED;
                    // entity.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                    context.TBL_CUSTOMER_COMPANY_DIRECTOR.Add(entity);
                    var saved = context.SaveChanges() > 0;
                }
            }
            else if (modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Director_Modification ||
                     modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Shareholder_Modification ||
                     modified.MODIFICATIONTYPEID == (int) CustomerInformationTrackerEnum.Signatory_Modification)
            {
                temp = context.TBL_TEMP_CUSTOMER_DIRECTOR.FirstOrDefault(x => x.TEMPCOMPANYDIRECTORID == targetId);
                if (temp != null) //If temp record is not null select the information from the main table
                {
                    entity = context.TBL_CUSTOMER_COMPANY_DIRECTOR.FirstOrDefault(x =>
                        x.COMPANYDIRECTORID == temp.COMPANYDIRECTORID);
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
                    entity.ADDRESS = temp.ADDRESS;
                    entity.PHONENUMBER = temp.PHONENUMBER;
                    entity.EMAILADDRESS = temp.EMAILADDRESS;
                }
            }
            //update the temp table, set ISCURRENT to false and APPROVALSTATUSID to approvalStatusId

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.COMPANYDIRECTORID = entity.COMPANYDIRECTORID;

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short) user.BranchId,
                DETAIL = "Approved Customer Top Client Supplier Information:  with Id: " + entity.COMPANYDIRECTORID,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
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

        public bool AddUpdateCustomerRelatedParty(CustomerRelatedPartyViewModel entity)
        {
            if (entity == null) return false;
            try
            {
                TBL_CUSTOMER_RELATED_PARTY relParty;
                if (entity.relatedPartyId > 0)
                {
                    relParty = context.TBL_CUSTOMER_RELATED_PARTY.Find();
                    if (relParty != null)
                    {
                        relParty.COMPANYDIRECTORID = entity.companyDirectorId;
                        relParty.CUSTOMERID = entity.customerId;
                        relParty.RELATIONSHIPTYPE = entity.relationshipType;
                        relParty.LASTUPDATEDBY = entity.createdBy;
                        relParty.DATETIMEUPDATED = DateTime.Now;
                    }
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
                }

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerRelatedPartyAddedUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Added/updated Customer Related Party Information",
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
                return false;
                throw new Exception(ex.Message);
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
    }
}


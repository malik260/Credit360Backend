using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Customer
{
    public class CustomerRepository : ICustomerRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private IWorkflow workflow;
        private IApprovalLevelStaffRepository level;
        private int customerId;
        int status = 0;

        public CustomerRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository genSetup,
                                     IWorkflow _workFlow,
                                      IApprovalLevelStaffRepository _level,
                                    FinTrakBankingContext _context)
        {
            context = _context;
            workflow = _workFlow;
            auditTrail = _auditTrail;
            _genSetup = genSetup;
            level = _level;
        }

        double StackHoldersFund = 1000000000000;



        public dynamic GetCustomerRating(int custormerId)
        {

            var data = (from c in context.TBL_CUSTOMER
                        where c.CUSTOMERID == custormerId
                        select new
                        {

                            shFund = c.TBL_CUSTOMER_RISK_RATING.MAX_SHAREHOLDER_FUND_PERCENTAG,
                            isInvestment = c.TBL_CUSTOMER_RISK_RATING.ISINVESTMENTGRADE,
                            rating = c.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                            limit = ((double)c.TBL_CUSTOMER_RISK_RATING.MAX_SHAREHOLDER_FUND_PERCENTAG / 100.00) * StackHoldersFund
                        }).FirstOrDefault();
            return data;
        }

        public bool AddCustomer(CustomerViewModels entity)
        {
            var customer = new TBL_CUSTOMER
            {
                ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete,
                BRANCHID = entity.userBranchId,
                COMPANYID = entity.companyId,
                CREATEDBY = (int)entity.createdBy,
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
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer  { entity.customerName } with Code: { entity.customerCode } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

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
                address.ADDRESSTYPEID = (short)ent.addressTypeId;
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
                    if (entity.addressId != 0 || entity.addressId < 0)
                    {
                        address = context.TBL_CUSTOMER_ADDRESS.Find(entity.addressId);
                        if (address != null)
                        {
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
                        }
                    }
                    else
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
                        context.TBL_CUSTOMER_ADDRESS.Add(address);
                    }

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
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
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
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
                            AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
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
                    if (entity.phoneContactId != 0 || entity.phoneContactId < 0)
                    {
                        phone = context.TBL_CUSTOMER_PHONECONTACT.Find(entity.phoneContactId);
                        if (phone != null)
                        {
                            phone.ACTIVE = entity.active;
                            phone.PHONE = entity.phone;
                            phone.PHONENUMBER = entity.phoneNumber;
                        }
                    }
                    else
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

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
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
                    if (entity.nextOfKinId != 0 || entity.nextOfKinId < 0)
                    {
                        next = context.TBL_CUSTOMER_NEXTOFKIN.Find(entity.nextOfKinId);
                        if (next != null)
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
                    }
                    else
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

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
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
                    if (entity.companyInfomationId != 0 || entity.companyInfomationId < 0)
                    {
                        company = context.TBL_CUSTOMER_COMPANYINFOMATION.Find(entity.companyInfomationId);
                        if (company != null)
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
                    }
                    else
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
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
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
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                    STAFFID = ent.createdBy,
                    BRANCHID = (short)ent.userBranchId,
                    DETAIL = "Added Customer's Customer Info for: " + customer.FIRSTNAME + " " + customer.LASTNAME + " with Id: " + info.CUSTOMERID + " to company" + " (" + info.COMPANYNAME + ") " + " on " + info.COMPANYINFOMATIONID,
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
                    if (entity.companyDirectorTypeId == (int)CompanyDirectorTypeEnum.Shareholder)
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
                        entity.customerTypeId = (int)CustomerTypeEnum.Individual;
                    }
                    TBL_CUSTOMER_COMPANY_DIRECTOR directors;
                    if (entity.companyDirectorId != 0 || entity.companyDirectorId < 0)
                    {
                        directors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Find(entity.companyDirectorId);
                        if (directors != null)
                        {
                            directors.CUSTOMERID = entity.customerId;
                            directors.SURNAME = entity.surname;
                            directors.FIRSTNAME = entity.firstname;
                            directors.MIDDLENAME = entity.middlename;
                            directors.CUSTOMERNIN = entity.customerNIN;
                            directors.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                            directors.CUSTOMERBVN = entity.bankVerificationNumber;
                            directors.SHAREHOLDINGPERCENTAGE = entity.numberOfShares;
                            //directors.REGISTRATION_NUMBER = entity.rcNumber;
                            //directors.TAX_NUMBER = entity.taxNumber;
                            directors.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                            directors.ADDRESS = entity.address;
                            directors.PHONENUMBER = entity.phoneNumber;
                            directors.EMAILADDRESS = entity.email;
                            directors.TBL_CUSTOMER_COMPANY_BENEFICIA = beneficialList;
                        }
                    }
                    else
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
                        //directors.REGISTRATION_NUMBER = entity.rcNumber;
                        //directors.TAX_NUMBER = entity.taxNumber;
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

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
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
                        string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
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
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_IDENTIFICATION for customer ID: + (" + entity.customerId + ") ",
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
                    if (entity.client_SupplierId != 0 || entity.client_SupplierId < 0)
                    {
                        clientSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Find(entity.client_SupplierId);
                        if (clientSupplier != null)
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
                    }
                    else
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

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_CLIENT_SUPPLIER for customer ID: + (" + entity.customerId + ") ",
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
                    if (entity.placeOfWorkId != 0 || entity.placeOfWorkId < 0)
                    {
                        history = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Find(entity.placeOfWorkId);

                        if (history != null)
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
                    }
                    else
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

                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added new TBL_CUSTOMER_EMPLOYMENTHISTORY for customer ID: + (" + entity.customerId + ") ",
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
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
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


        IQueryable<CustomerViewModels> GetCustomers()
        {
            return from a in context.TBL_CUSTOMER
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
                       customerTypeId = (short)a.CUSTOMERTYPEID,
                       dateOfBirth = (DateTime)a.DATEOFBIRTH,
                       customerId = a.CUSTOMERID,
                       emailAddress = a.EMAILADDRESS,
                       firstName = a.FIRSTNAME,
                       gender = a.GENDER,
                       lastName = a.LASTNAME,
                       maidenName = a.MAIDENNAME,
                       maritalStatus = a.MARITALSTATUS.Value,
                       title = a.TITLE,
                       middleName = a.MIDDLENAME,
                       customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                       customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                       misCode = a.MISCODE,
                       misStaff = a.MISSTAFF,
                       nationality = a.NATIONALITY,
                       occupation = a.OCCUPATION,
                       placeOfBirth = a.PLACEOFBIRTH,
                       isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                       relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                       relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                      + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                       spouse = a.SPOUSE,
                       sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                       subSectorId = (short)a.SUBSECTORID,
                       subSectorName = a.TBL_SUB_SECTOR.NAME,
                       taxNumber = a.TAXNUMBER,
                       riskRatingId = a.RISKRATINGID,
                       riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                       customerBVN = a.CUSTOMERBVN,
                       CustomerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == a.CUSTOMERID).Select(x => new CustomerAddressViewModels()
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
                           addressId = x.ADDRESSID
                       }).ToList(),
                       CustomerPhoneContact = context.TBL_CUSTOMER_PHONECONTACT.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => new CustomerPhoneContactViewModels
                       {
                           active = c.ACTIVE,
                           customerId = c.CUSTOMERID,
                           phone = c.PHONE,
                           phoneContactId = c.PHONECONTACTID,
                           phoneNumber = c.PHONENUMBER
                       }).ToList(),
                       CustomerCompanyInfomation = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(d => d.CUSTOMERID == a.CUSTOMERID).Select(d => new CustomerCompanyInfomationViewModels()
                       {
                           annualTurnOver = d.ANNUALTURNOVER,
                           companyEmail = d.COMPANYEMAIL,
                           companyId = d.CUSTOMERID,
                           companyName = d.COMPANYNAME,
                           companyWebsite = d.COMPANYWEBSITE,
                           companyInfomationId = d.COMPANYINFOMATIONID,
                           corporateBusinessCategory = d.CORPORATEBUSINESSCATEGORY,
                           createdBy = a.CREATEDBY,
                           registeredOffice = d.REGISTEREDOFFICE,
                           registrationNumber = d.REGISTRATIONNUMBER,
                           paidUpCapital = d.PAIDUPCAPITAL,
                           authorizedCapital = d.AUTHORISEDCAPITAL,
                           shareholderFund = d.SHAREHOLDER_FUND
                       }).ToList(),
                       CustomerEmploymentHistory = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(s => s.CUSTOMERID == a.CUSTOMERID).Select(s => new CustomerEmploymentHistoryViewModels()
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
                       }).ToList(),
                       CustomerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                       .Select(s => new CustomerCompanyDirectorsViewModels()
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
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           customerId = s.CUSTOMERID,
                           customerName = s.FIRSTNAME + " " + s.SURNAME,
                           address = s.ADDRESS,
                           phoneNumber = s.PHONENUMBER,
                           email = s.EMAILADDRESS
                       }).ToList(),
                       CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                       .Select(s => new CustomerCompanyShareholderViewModels()
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
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           customerId = s.CUSTOMERID,
                           customerName = s.FIRSTNAME + " " + s.SURNAME,
                           address = s.ADDRESS,
                           phoneNumber = s.PHONENUMBER,
                           email = s.EMAILADDRESS
                       }).ToList(),
                       CustomerCompanyAccountSignatory = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory)
                       .Select(s => new CustomerCompanyAccountSignatoryViewModels()
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
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           customerId = s.CUSTOMERID,
                           customerName = s.FIRSTNAME + " " + s.SURNAME,
                           address = s.ADDRESS,
                           phoneNumber = s.PHONENUMBER,
                           email = s.EMAILADDRESS
                       }).ToList(),
                       CustomerClientOrSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == a.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                       .Select(cs => new CustomerClientOrSupplierViewModels()
                       {
                           client_SupplierId = cs.CLIENT_SUPPLIERID,
                           clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                           firstName = cs.FIRSTNAME,
                           middleName = cs.MIDDLENAME,
                           lastName = cs.LASTNAME,
                           taxNumber = cs.TAX_NUMBER,
                           rcNumber = cs.REGISTRATION_NUMBER,
                           hasCASAAccount = (bool)cs.HAS_CASA_ACCOUNT,
                           bankName = cs.BANKNAME,
                           casaAccountNumber = cs.CASA_ACCOUNTNO,
                           natureOfBusiness = cs.NATURE_OF_BUSINESS,
                           contactPerson = cs.CONTACT_PERSON,
                           client_SupplierAddress = cs.ADDRESS,
                           client_SupplierPhoneNumber = cs.PHONENUMBER,
                           client_SupplierEmail = cs.EMAILADDRESS,
                           client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                       }).ToList(),
                       CustomerSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == a.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                       .Select(cs => new CustomerSupplierViewModels()
                       {
                           client_SupplierId = cs.CLIENT_SUPPLIERID,
                           clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME + " " + cs.MIDDLENAME,
                           firstName = cs.FIRSTNAME,
                           middleName = cs.MIDDLENAME,
                           lastName = cs.LASTNAME,
                           taxNumber = cs.TAX_NUMBER,
                           rcNumber = cs.REGISTRATION_NUMBER,
                           hasCASAAccount = (bool)cs.HAS_CASA_ACCOUNT,
                           bankName = cs.BANKNAME,
                           casaAccountNumber = cs.CASA_ACCOUNTNO,
                           contactPerson = cs.CONTACT_PERSON,
                           natureOfBusiness = cs.NATURE_OF_BUSINESS,
                           client_SupplierAddress = cs.ADDRESS,
                           client_SupplierPhoneNumber = cs.PHONENUMBER,
                           client_SupplierEmail = cs.EMAILADDRESS,
                           client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                       }).ToList(),
                       CustomerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID)
                       .Select(x => new CollateralViewModel()
                       {
                           collateralId = x.COLLATERALCUSTOMERID,
                           collateralTypeId = x.COLLATERALTYPEID,
                           collateralSubTypeId = x.COLLATERALSUBTYPEID,
                           customerId = x.CUSTOMERID,
                           currencyId = x.CURRENCYID,
                           currency = x.TBL_CURRENCY.CURRENCYNAME,
                           collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                           collateralCode = x.COLLATERALCODE,
                           camRefNumber = x.CAMREFNUMBER,
                           allowSharing = x.ALLOWSHARING,
                           isLocationBased = x.ISLOCATIONBASED,
                           valuationCycle = x.VALUATIONCYCLE,
                           collateralValue = x.COLLATERALVALUE,
                           haircut = x.HAIRCUT,
                           approvalStatus = x.APPROVALSTATUS,
                       }).ToList(),
                       CustomerChildren = context.TBL_CUSTOMER_CHILDREN.Where(chd => chd.CUSTOMERID == a.CUSTOMERID)
                       .Select(kk => new CustomerChildrenViewModel()
                       {
                           customerChildrenId = kk.CUSTOMERCHILDRENID,
                           customerId = kk.CUSTOMERID,
                           childName = kk.CHILDNAME,
                           childDateOfBirth = kk.CHILDDATEOFBIRTH
                       }).ToList(),
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
                       customerTypeId = (short)a.CUSTOMERTYPEID,
                       dateOfBirth = (DateTime)a.DATEOFBIRTH,
                       customerId = a.CUSTOMERID,
                       emailAddress = a.EMAILADDRESS,
                       firstName = a.FIRSTNAME,
                       gender = a.GENDER,
                       lastName = a.LASTNAME,
                       maidenName = a.MAIDENNAME,
                       maritalStatus = a.MARITALSTATUS.Value,
                       title = a.TITLE,
                       middleName = a.MIDDLENAME,
                       customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                       customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                       misCode = a.MISCODE,
                       misStaff = a.MISSTAFF,
                       nationality = a.NATIONALITY,
                       occupation = a.OCCUPATION,
                       placeOfBirth = a.PLACEOFBIRTH,
                       isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                       relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                       relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                      + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                       spouse = a.SPOUSE,
                       sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                       subSectorId = (short)a.SUBSECTORID,
                       subSectorName = a.TBL_SUB_SECTOR.NAME,
                       taxNumber = a.TAXNUMBER,
                       riskRatingId = a.RISKRATINGID,
                       riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                       customerBVN = a.CUSTOMERBVN,
                       CustomerPhoneContact = context.TBL_CUSTOMER_PHONECONTACT.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => new CustomerPhoneContactViewModels
                       {
                           active = c.ACTIVE,
                           customerId = c.CUSTOMERID,
                           phone = c.PHONE,
                           phoneContactId = c.PHONECONTACTID,
                           phoneNumber = c.PHONENUMBER
                       }).ToList(),
                       CustomerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
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
                       CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
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
                customerTypeId = (short)a.CUSTOMERTYPEID,
                dateOfBirth = (DateTime)a.DATEOFBIRTH,
                customerId = a.CUSTOMERID,
                emailAddress = a.EMAILADDRESS,
                firstName = a.FIRSTNAME,
                gender = a.GENDER,
                lastName = a.LASTNAME,
                maidenName = a.MAIDENNAME,
                maritalStatus = a.MARITALSTATUS.Value,
                title = a.TITLE,
                middleName = a.MIDDLENAME,
                customerAccountNo = context.TBL_CASA.FirstOrDefault(ca => ca.CUSTOMERID == a.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                misCode = a.MISCODE,
                misStaff = a.MISSTAFF,
                nationality = a.NATIONALITY,
                occupation = a.OCCUPATION,
                placeOfBirth = a.PLACEOFBIRTH,
                isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                         + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                spouse = a.SPOUSE,
                sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                subSectorId = (short)a.SUBSECTORID,
                subSectorName = a.TBL_SUB_SECTOR.NAME,
                taxNumber = a.TAXNUMBER,
                riskRatingId = a.RISKRATINGID,
                riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
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
                            fullName = cs.TBL_CUSTOMER.LASTNAME + " " + cs.TBL_CUSTOMER.FIRSTNAME + "(" + cs.TBL_CUSTOMER.CUSTOMERCODE + ")",
                            firstName = cs.TBL_CUSTOMER.FIRSTNAME,
                            lastName = cs.TBL_CUSTOMER.LASTNAME,
                            customerCode = cs.TBL_CUSTOMER.CUSTOMERCODE,
                        });

            return data;
        }

        public CustomerViewModels GetCustomer(int custormerId)
        {

            var data = GetCustomers().FirstOrDefault(a => a.customerId == custormerId);
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
                customerMain.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                customerMain.CREATIONMAILSENT = entity.creationMailSent;
                customerMain.CUSTOMERCODE = entity.customerCode;
                customerMain.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
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
                customerMain.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                customerMain.ISREALATEDPARTY = entity.isRealatedParty;
                customerMain.SPOUSE = entity.spouse;
                customerMain.SUBSECTORID = entity.subSectorId;
                customerMain.TAXNUMBER = entity.taxNumber;
                customerMain.DATETIMEUPDATED = DateTime.Now;
                customerMain.LASTUPDATEDBY = entity.deletedBy;
                customerMain.RISKRATINGID = entity.riskRatingId;
                customerMain.CUSTOMERBVN = entity.customerBVN;
                return context.SaveChanges() != 0;
            }
            else
            {
                TBL_TEMP_CUSTOMER customer;
                var existingTempCustomer = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x => x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower() && x.COMPANYID == entity.companyId && x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                var unApprovedCustomerUpdate = context.TBL_TEMP_CUSTOMER.Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                 x.CUSTOMERCODE.ToLower() == entity.customerCode.ToLower());
                if (unApprovedCustomerUpdate.Any())
                {
                    throw new Exception("Customer is already undergoing approval");
                }

                //  var customer = context.TBL_CUSTOMER.Find(customerId);

                if (existingTempCustomer != null)
                {
                    customer = existingTempCustomer;
                    customer.BRANCHID = entity.branchId;
                    customer.CUSTOMERID = entity.customerId;
                    customer.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                    customer.CREATIONMAILSENT = entity.creationMailSent;
                    customer.CUSTOMERCODE = entity.customerCode;
                    customer.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
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

                    customer.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                    customer.ISREALATEDPARTY = entity.isRealatedParty;

                    customer.SPOUSE = entity.spouse;
                    customer.SUBSECTORID = entity.subSectorId;
                    customer.TAXNUMBER = entity.taxNumber;

                    customer.CUSTOMERBVN = entity.customerBVN;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;
                }
                else
                {
                    customer = new TBL_TEMP_CUSTOMER();
                    customer.COMPANYID = entity.companyId;
                    customer.BRANCHID = entity.branchId;
                    customer.CUSTOMERID = entity.customerId;
                    customer.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                    customer.CREATIONMAILSENT = entity.creationMailSent;
                    customer.CUSTOMERCODE = entity.customerCode;
                    customer.CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId;
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

                    customer.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                    customer.ISREALATEDPARTY = entity.isRealatedParty;

                    customer.SPOUSE = entity.spouse;
                    customer.SUBSECTORID = entity.subSectorId;
                    customer.TAXNUMBER = entity.taxNumber;

                    customer.CUSTOMERBVN = entity.customerBVN;

                    customer.CREATEDBY = entity.createdBy;
                    customer.DATETIMECREATED = DateTime.Now;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    customer.ISCURRENT = true;

                    context.TBL_TEMP_CUSTOMER.Add(customer);
                }

                var modified = new TBL_CUSTOMER_MODIFICATION
                {
                    CUSTOMERID = entity.customerId,
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
                    DETAIL = "Updated TBL_CUSTOMER: " + entity.customerName + " with code: " + entity.customerCode + " on" + " (" + entity.customerId + ") ",
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

                        var model = new ApprovalViewModel
                        {
                            staffId = entity.createdBy,
                            companyId = entity.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = targetId,
                            operationId = (int)OperationsEnum.CustomerInformationApproval,
                            BranchId = entity.userBranchId,
                            externalInitialization = true
                        };
                        var response = workflow.LogForApproval(model);

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
                allCustomers = GetCustomers().
                    Where(c => c.companyId == companyId)
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
                customers = customers.Where(x => x.CustomerPhoneContact.Where(o => o.phoneNumber == search.phoneNumber).Count() >= 1);
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
            var customers = (from x in GetCustomers()
                             where x.firstName.ToLower().Contains(searchQuery.ToLower())
                            || x.lastName.ToLower().Contains(searchQuery.ToLower())
                            || x.middleName.ToLower().Contains(searchQuery.ToLower())
                            || x.customerCode.Contains(searchQuery)
                            || x.branchName.Contains(searchQuery)
                             select x).ToList();
            if (customers.Count > 0)
            {
                return customers;
            }
            return null;
        }

        #region Single Customer Information By CustomerID
        public IEnumerable<CustomerViewModels> GetCustomerGeneralInfoByLoanId(int loanApplicationId)
        {
            var loanCust = (from a in context.TBL_LOAN_APPLICATION_DETAIL where a.LOANAPPLICATIONID == loanApplicationId select a.CUSTOMERID).ToList();
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
                            customerTypeId = (short)a.CUSTOMERTYPEID,
                            dateOfBirth = (DateTime)a.DATEOFBIRTH,
                            customerId = a.CUSTOMERID,
                            emailAddress = a.EMAILADDRESS,
                            firstName = a.FIRSTNAME,
                            gender = a.GENDER,
                            lastName = a.LASTNAME,
                            maidenName = a.MAIDENNAME,
                            maritalStatus = a.MARITALSTATUS.Value,
                            title = a.TITLE,
                            middleName = a.MIDDLENAME,
                            customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
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
                            subSectorId = (short)a.SUBSECTORID,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            taxNumber = a.TAXNUMBER,
                            relationshipOfficerName = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).FIRSTNAME + " "
                      + context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == a.RELATIONSHIPOFFICERID).LASTNAME,
                            riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
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
                                      identificationMode = context.TBL_CUSTOMER_IDENTI_MODE_TYPE.FirstOrDefault(r => r.IDENTIFICATIONMODEID == e.IDENTIFICATIONMODEID).IDENTIFICATIONMODE,
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
        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerDirectorInfo(int customerId, short directorTypeId)
        {
            var companyDirectors = (from s in context.TBL_CUSTOMER_COMPANY_DIRECTOR
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

                                        companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                        customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA.Where(a => a.COMPANYDIRECTORID == s.COMPANYDIRECTORID).Select(x => new CustomerCompanyBeneficiaryViewModels()
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
        public IEnumerable<CustomerCompanyDirectorsViewModels> GetSingleCustomerShareholderInfo(int customerId, short customerTypeId)
        {
            var companyDirectors = (from s in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                    where s.CUSTOMERID == customerId && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder && s.CUSTOMERTYPEID == customerTypeId
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
                                        companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS,
                                    }).ToList();
            return companyDirectors;
        }
        public IEnumerable<CustomerClientOrSupplierViewModels> GetSingleCustomerClientOrSupplierInfo(int customerId, short clientTypeId)
        {
            var clientOrSupplier = (from cs in context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                    where cs.CUSTOMERID == customerId && cs.CLIENT_SUPPLIERTYPEID == clientTypeId
                                    select new CustomerClientOrSupplierViewModels()
                                    {
                                        customerTypeId = cs.CUSTOMERTYPEID,
                                        customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(x => x.CUSTOMERTYPEID == cs.CUSTOMERTYPEID).NAME,
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
            var customerCompanyBeneficial = context.TBL_CUSTOMER_COMPANY_BENEFICIA.Where(a => a.COMPANYDIRECTORID == companyDirectorId).Select(x => new CustomerCompanyBeneficiaryViewModels()
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
            var casaInformation = context.TBL_CASA.Where(a => a.CUSTOMERID == customerId).Select(x => new CasaViewModel()
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

        public IEnumerable<CustomerNextOfKinViewModels> GetSingleCustomerNextOfKinInfo(int customerId)
        {
            var nextOfKin = context.TBL_CUSTOMER_NEXTOFKIN.Where(a => a.CUSTOMERID == customerId).Select(x => new CustomerNextOfKinViewModels()
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
                stateId = x.TBL_CITY.STATEID,
                cityId = x.CITYID,
                active = x.ACTIVE,
            }).ToList();
            return nextOfKin;
        }
        public dynamic GetCustomerAndType(int custormerId)
        {
            var data = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == custormerId).Select(c => new
            {
                custormerId = c.CUSTOMERID,
                customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                customerTypeId = c.CUSTOMERTYPEID,
                customerType = c.TBL_CUSTOMER_TYPE.NAME
            });
            return data;
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
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR where a.CUSTOMERID == customerId && a.CUSTOMERBVN == customerBvn select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }
            return itemExist;
        }
        public bool ValidateCustomerRCnumber(int customerId, string rcNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR where a.CUSTOMERID == customerId && a.REGISTRATION_NUMBER == rcNumber select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }
            return itemExist;
        }
        public bool ValidateCustomerTIN(int customerId, string tin)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR where a.CUSTOMERID == customerId && a.TAX_NUMBER == tin select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }
            return itemExist;
        }
        public bool ValidateCustomerEmail(int customerId, string email)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_COMPANY_DIRECTOR where a.CUSTOMERID == customerId && a.EMAILADDRESS == email select a).ToList();
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
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER where a.CUSTOMERID == customerId && a.EMAILADDRESS == email select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }
            return itemExist;
        }
        public bool ValidateClientSupplierRCnumber(int customerId, string rcNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER where a.CUSTOMERID == customerId && a.REGISTRATION_NUMBER == rcNumber select a).ToList();
            if (data.Count > 0)
            {
                itemExist = true;
            }
            return itemExist;
        }
        public bool ValidateClientSupplierTIN(int customerId, string taxNumber)
        {
            bool itemExist = false;
            var data = (from a in context.TBL_CUSTOMER_CLIENT_SUPPLIER where a.CUSTOMERID == customerId && a.TAX_NUMBER == taxNumber select a).ToList();
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges() != 0;
            return response;
        }
        #endregion

        public IEnumerable<CustomerInformationApprovalViemModel> GetAllCustomerInformationAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.CustomerInformationApproval);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from a in context.TBL_CUSTOMER_MODIFICATION
                    join c in context.TBL_APPROVAL_TRAIL on a.CUSTOMERMODIFICATIONID equals c.TARGETID
                    where
                        (c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                        && a.APPROVALCOMPLETED == false
                        && c.RESPONSESTAFFID == null
                        && c.OPERATIONID == (int)OperationsEnum.CustomerInformationApproval
                    && c.TOAPPROVALLEVELID == staffApprovalLevelId

                    select new CustomerInformationApprovalViemModel
                    {
                        customerId = a.CUSTOMERID,
                        customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                        customerModificationId = a.CUSTOMERMODIFICATIONID,
                        customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                        modificationTyepId = a.CUSTOMERMODIFICATIONID,
                        modificationType = a.TBL_CUSTOMER_MODIFICATN_TYPE.MODIFICATIONTYPENAME,
                        approvalStatus = c.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                        dateUpdated = a.DATETIMECREATED,
                        createdBy = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == a.CREATEDBY).FIRSTNAME + " "
                        + context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == a.CREATEDBY).LASTNAME,
                        customerBranch = a.TBL_CUSTOMER.TBL_BRANCH.BRANCHNAME,
                        operationId = (int)OperationsEnum.CustomerInformationApproval
                    }).ToList(); //.GroupBy(x => ).Select(g => g.FirstOrDefault());

        }
        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.CustomerInformationApproval;
            entity.externalInitialization = false;
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.LogForApproval(entity);
                    var b = workflow.NextLevelId ?? 0;
                    if (b == 0 && workflow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveCustomerInformation(entity.targetId, (short)workflow.StatusId, entity);

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
            var modifiedData = context.TBL_CUSTOMER_MODIFICATION.Find(modifiedId);
            if (modifiedData != null)
            {
                if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.General_Information)
                {
                    returnVal = ApproveGeneralInformation(modifiedData.CUSTOMERID, approvalStatusId, user);
                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Address_Modification)
                {

                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Phone_Number_Modification)
                {

                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Employment_History_Modification)
                {

                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Next_of_Kin_Modification)
                {

                }
                else if (modifiedData.MODIFICATIONTYPEID == (int)CustomerInformationTrackerEnum.Director_Modification)
                {

                }

            }
            return returnVal;
        }
        private bool ApproveGeneralInformation(int customerId, short approvalStatusId, UserInfo user)
        {
            TBL_CUSTOMER entity = null;
            var temp = context.TBL_TEMP_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == customerId);
            if (temp != null)
            {
                entity = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == customerId);
            }

            if (entity != null) //Update existing staff with tempStaff record
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
            }
            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Updated TBL_CUSTOMER:  with code: " + entity.CUSTOMERCODE,
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

    }
}


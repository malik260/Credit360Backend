using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        private int customerId;
        int status = 0;

        public CustomerRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository genSetup,
                                    FinTrakBankingContext _context)
        {
            context = _context;
            auditTrail = _auditTrail;
            _genSetup = genSetup;
        }

        double StackHoldersFund = 1000000000000;



        public dynamic GetCustomerRating(int custormerId)
        {

            var data = (from c in context.TBL_CUSTOMER
                        where c.CUSTOMERID == custormerId
                        select new
                        {

                            shFund = c.TBL_CUSTOMER_RISK_RATING.MAX_SHAREHOLDER_FUND_PERCENTAGE,
                            isInvestment = c.TBL_CUSTOMER_RISK_RATING.ISINVESTMENTGRADE,
                            rating = c.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                            limit = ((double)c.TBL_CUSTOMER_RISK_RATING.MAX_SHAREHOLDER_FUND_PERCENTAGE / 100.00) * StackHoldersFund
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
                address.ADDRESSTYPEID = ent.addressTypeId;
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
                            address.ADDRESSTYPEID = entity.addressTypeId;
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
                        address.ADDRESSTYPEID = entity.addressTypeId;
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
                            context.TBL_CUSTOMER_CHILDREN.Add(child);
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

                        var response = context.SaveChanges() != 0;
                        return response;
                    }
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
                    if(entity.companyDirectorTypeId != (int)CompanyDirectorTypeEnum.Shareholder)
                    {
                        entity.customerTypeId = 1;
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
                            directors.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                            directors.CUSTOMERBVN = entity.bankVerificationNumber;
                            directors.NUMBEROFSHARES = entity.numberOfShares;
                            directors.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                            directors.ADDRESS = entity.address;
                            directors.PHONENUMBER = entity.phoneNumber;
                            directors.EMAILADDRESS = entity.email;
                        }
                    }
                    else
                    {
                        directors = new TBL_CUSTOMER_COMPANY_DIRECTOR();

                        directors.CUSTOMERID = entity.customerId;
                        directors.SURNAME = entity.surname;
                        directors.FIRSTNAME = entity.firstname;
                        directors.CUSTOMERTYPEID = entity.customerTypeId;
                        directors.COMPANYDIRECTORTYPEID = entity.companyDirectorTypeId;
                        directors.CUSTOMERBVN = entity.bankVerificationNumber;
                        directors.NUMBEROFSHARES = entity.numberOfShares;
                        directors.ISPOLITICALLYEXPOSED = entity.isPoliticallyExposed;
                        directors.ADDRESS = entity.address;
                        directors.PHONENUMBER = entity.phoneNumber;
                        directors.EMAILADDRESS = entity.email;
                        directors.CREATEDBY = entity.createdBy;
                        directors.DATECREATED = DateTime.Now;

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
                        clientSupplier.HAS_CASA_ACCOUNT = entity.hasCASAAccount;
                        clientSupplier.CASA_ACCOUNTNO = entity.casaAccountNumber;
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
                       spouse = a.SPOUSE,
                       sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                       subSectorId = (short)a.SUBSECTORID,
                       subSectorName = a.TBL_SUB_SECTOR.NAME,
                       taxNumber = a.TAXNUMBER,
                       riskRatingId = a.RISKRATINGID,
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
                           authorizedCapital = d.AUTHORISEDCAPITAL

                       }).ToList(),
                       CustomerIdentification = context.TBL_CUSTOMER_IDENTIFICATION.Where(e => e.CUSTOMERID == a.CUSTOMERID).Select(e => new CustomerIdentificationViewModels()
                       {
                           identificationId = e.IDENTIFICATIONID,
                           identificationModeId = e.IDENTIFICATIONMODEID.Value,
                           identificationMode = context.TBL_CUSTOMER_IDENTI_MODE_TYPE.FirstOrDefault(r => r.IDENTIFICATIONMODEID == e.IDENTIFICATIONMODEID).IDENTIFICATIONMODE,
                           identificationNo = e.IDENTIFICATIONNO,
                           issueAuthority = e.ISSUEAUTHORITY,
                           issuePlace = e.ISSUEPLACE
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
                           numberOfShares = s.NUMBEROFSHARES,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                           numberOfShares = s.NUMBEROFSHARES,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                           numberOfShares = s.NUMBEROFSHARES,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                           clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                           firstName = cs.FIRSTNAME,
                           middleName = cs.MIDDLENAME,
                           lastName = cs.LASTNAME,
                           taxNumber = cs.TAX_NUMBER,
                           rcNumber = cs.REGISTRATION_NUMBER,
                           hasCASAAccount = (bool)cs.HAS_CASA_ACCOUNT,
                           casaAccountNumber = cs.CASA_ACCOUNTNO,
                           contactPerson = cs.CONTACT_PERSON,
                           client_SupplierAddress = cs.ADDRESS,
                           client_SupplierPhoneNumber = cs.PHONENUMBER,
                           client_SupplierEmail = cs.EMAILADDRESS,
                           client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYPE.CLIENT_SUPPLIERTYPENAME
                       }).ToList(),
                       CustomerSupplier = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == a.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                       .Select(cs => new CustomerSupplierViewModels()
                       {
                           client_SupplierId = cs.CLIENT_SUPPLIERID,
                           clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                           firstName = cs.FIRSTNAME,
                           middleName = cs.MIDDLENAME,
                           lastName = cs.LASTNAME,
                           client_SupplierAddress = cs.ADDRESS,
                           client_SupplierPhoneNumber = cs.PHONENUMBER,
                           client_SupplierEmail = cs.EMAILADDRESS,
                           client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                           client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYPE.CLIENT_SUPPLIERTYPENAME
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
            var type = from a in context.TBL_CUSTOMER_CLIENT_SUPPLR_TYPE
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
            var type = from a in context.TBL_CUSTOMER_COMPANY_DIREC_TYPE
                       select new CompanyDirectorTypeViewModels
                       {
                           name = a.COMPANYDIRECTORYTYPENAME,
                           companyDirectorTypeId = a.COMPANYDIRECTORYTYPEID
                       };
            return type;
        }

        public bool UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);

            if (customer != null)
            {
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
                customer.DATETIMEUPDATED = DateTime.Now;
                customer.LASTUPDATEDBY = entity.deletedBy;
                customer.RISKRATINGID = entity.riskRatingId;
                customer.CUSTOMERBVN = entity.customerBVN;
            }

            if (entity.CustomerCompanyInfomation.Count > 0)
            {
                UpdateCustomerCompanyInfomation(entity.CustomerCompanyInfomation);
            }
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

            auditTrail.AddAuditTrail(audit);

            return context.SaveChanges() != 0;

        }

        public IEnumerable<CustomerViewModels> CustomerSearch(int companyId, string search)
        {
            var customer = GetCustomerByCompanyId(companyId).ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                customer = customer.Where(x =>
               x.firstName.ToLower().Contains(search.ToLower())
               || x.lastName.ToLower().Contains(search.ToLower())
               || x.middleName.ToLower().Contains(search.ToLower())
               || x.customerCode.Contains(search.ToLower())
                ).ToList();
            }
            return customer;
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
            var customers = GetCustomerByCompanyId(companyId);

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
                            taxNumber = a.TAXNUMBER
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
                              authorizedCapital = d.AUTHORISEDCAPITAL

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
                               addressId = x.ADDRESSID
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
                                        numberOfShares = s.NUMBEROFSHARES,
                                        isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                                        bankVerificationNumber = s.CUSTOMERBVN,
                                        companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                        companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
                                        customerId = s.CUSTOMERID,
                                        customerName = s.FIRSTNAME + " " + s.SURNAME,
                                        address = s.ADDRESS,
                                        phoneNumber = s.PHONENUMBER,
                                        email = s.EMAILADDRESS
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
                                        customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(x=> x.CUSTOMERTYPEID == cs.CUSTOMERTYPEID).NAME,
                                        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                        firstName = cs.FIRSTNAME,
                                        middleName = cs.MIDDLENAME,
                                        lastName = cs.LASTNAME,
                                        taxNumber = cs.TAX_NUMBER,
                                        rcNumber = cs.REGISTRATION_NUMBER,
                                        hasCASAAccount = cs.HAS_CASA_ACCOUNT,
                                        casaAccountNumber = cs.CASA_ACCOUNTNO,
                                        contactPerson = cs.CONTACT_PERSON,
                                        client_SupplierAddress = cs.ADDRESS,
                                        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                        client_SupplierEmail = cs.EMAILADDRESS,
                                        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYPE.CLIENT_SUPPLIERTYPENAME
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
        #endregion
    }
}


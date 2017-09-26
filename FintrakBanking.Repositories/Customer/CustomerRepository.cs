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

        public async Task<bool> AddCustomer(CustomerViewModels entity)
        {
            var customer = new tbl_Customer
            {
                AccountCreationComplete = entity.accountCreationComplete,
                BranchId = entity.branchId,
                ChildDateOfBirth = entity.childDateOfBirth,
                CompanyId = entity.companyId,
                CreatedBy = (int)entity.createdBy,
                CreationMailSent = entity.creationMailSent,
                CustomerCode = entity.customerCode,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
                CustomerTypeId = entity.customerTypeId,
                DateOfBirth = entity.dateOfBirth,
                DateTimeCreated = DateTime.Now,
                EmailAddress = entity.emailAddress,
                FirstChildName = entity.firstChildName,
                FirstName = entity.firstName,
                Gender = entity.gender,
                LastName = entity.lastName,
                MaidenName = entity.maidenName,
                MaritalStatus = entity.maritalStatus,
                Title = entity.title,
                MiddleName = entity.middleName,
                MISCode = entity.misCode,
                MISStaff = entity.misStaff,
                Nationality = entity.nationality,
                Occupation = entity.occupation,
                PlaceOfBirth = entity.placeOfBirth,
                PoliticallyExposedPerson = entity.politicallyExposedPerson,
                RelationshipOfficerId = entity.relationshipOfficerId,
                Spouse = entity.spouse,
                SubSectorId = entity.subSectorId,
                TaxNumber = entity.taxNumber
            };
            context.tbl_Customer.Add(customer);
            customerId = customer.CustomerId;

            status = 1;
            if (entity.CustomerAddresses.Count > 0)
            {
                AddCustomerAddresses(entity.CustomerAddresses, status);
            }

            if (entity.CustomerBvn.Count > 0)
            {
                AddCustomerBvn(entity.CustomerBvn, status);
            }

            if (entity.CustomerPhoneContact.Count > 0)
            {
                AddCustomerPhoneContact(entity.CustomerPhoneContact);
            }

            if (entity.CustomerCompanyInfomation.Count > 0)
            {
                AddCustomerCompanyInfomation(entity.CustomerCompanyInfomation, status);
            }

            if (entity.CustomerIdentification.Count > 0)
            {
                AddCustomerIdentification(entity.CustomerIdentification);
            }

            if (entity.CustomerEmploymentHistory.Count > 0)
            {
                AddCustomerEmploymentHistory(entity.CustomerEmploymentHistory);
            }
            var response = await context.SaveChangesAsync() != 0;
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer  { entity.customerName } with Code: { entity.customerCode } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------


            return response;
        }


        private void AddCustomerAddresses(List<CustomerAddressViewModels> entity,
           int status)
        {
            var address = new tbl_Customer_Address();
            foreach (var ent in entity)
            {
                address.Active = ent.active;
                address.Address = ent.address;
                address.AddressTypeId = ent.addressTypeId;
                address.CityId = ent.cityId;
                address.CustomerId = ent.customerId;
                address.StateId = ent.stateId;
                address.HomeTown = ent.homeTown;
                address.POBox = ent.pobox;
                address.StateId = ent.stateId;

                context.tbl_Customer_Address.Add(address);

            }
        }

        public bool AddCustomerAddresses(CustomerAddressViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    tbl_Customer_Address address;
                    if (entity.addressId != 0 || entity.addressId < 0)
                    {
                        address = context.tbl_Customer_Address.Find(entity.addressId);
                        if (address != null)
                        {
                            address.Active = entity.active;
                            address.Address = entity.address;
                            address.AddressTypeId = entity.addressTypeId;
                            address.CityId = entity.cityId;
                            address.StateId = entity.stateId;
                            address.HomeTown = entity.homeTown;
                            address.POBox = entity.pobox;
                            address.StateId = entity.stateId;
                            address.ElectricMeterNumber = entity.electricMeterNumber;
                            address.NearestLandmark = entity.nearestLandmark;
                        }
                    }
                    else
                    {
                        address = new tbl_Customer_Address();
                        address.Active = entity.active;
                        address.Address = entity.address;
                        address.AddressTypeId = entity.addressTypeId;
                        address.CityId = entity.cityId;
                        address.CustomerId = entity.customerId;
                        address.StateId = entity.stateId;
                        address.HomeTown = entity.homeTown;
                        address.POBox = entity.pobox;
                        address.StateId = entity.stateId;
                        address.ElectricMeterNumber = entity.electricMeterNumber;
                        address.NearestLandmark = entity.nearestLandmark;
                        context.tbl_Customer_Address.Add(address);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_Address for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
            var customerBvn = new tbl_Customer_BVN();
            foreach (var bvn in entity)
            {
                customerBvn.BankVerificationNumber = bvn.bankVerificationNumber;
                customerBvn.CustomerId = bvn.companyId;
                customerBvn.CreatedBy = bvn.createdBy;
                customerBvn.CustomerId = bvn.customerId;
                customerBvn.Firstname = bvn.firstname;
                customerBvn.Surname = bvn.surname;
                customerBvn.IsValidBVN = bvn.isValidBvn;
                customerBvn.DateTimeCreated = bvn.dateTimeCreated;

                context.tbl_Customer_BVN.Add(customerBvn);
            }
        }
        public bool AddCustomerBvn(CustomerBvnViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    tbl_Customer_BVN customerBvn;
                    if (entity.customerBvnid != 0 || entity.customerBvnid < 0)
                    {
                        customerBvn = context.tbl_Customer_BVN.Find(entity.customerBvnid);

                        if (customerBvn != null)
                        {
                            customerBvn.BankVerificationNumber = entity.bankVerificationNumber;
                            customerBvn.CustomerId = entity.customerId;
                            customerBvn.CreatedBy = entity.createdBy;
                            customerBvn.Firstname = entity.firstname;
                            customerBvn.Surname = entity.surname;
                            customerBvn.IsValidBVN = entity.isValidBvn;
                            customerBvn.IsPoliticallyExposed = entity.isPoliticallyExposed;
                        }
                    }
                    else
                    {
                        customerBvn = new tbl_Customer_BVN
                        {
                            BankVerificationNumber = entity.bankVerificationNumber,
                            CustomerId = entity.customerId,
                            CreatedBy = entity.createdBy,
                            Firstname = entity.firstname,
                            Surname = entity.surname,
                            IsValidBVN = entity.isValidBvn,
                            DateTimeCreated = DateTime.Now,
                            IsPoliticallyExposed = entity.isPoliticallyExposed
                        };

                        context.tbl_Customer_BVN.Add(customerBvn);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_BVN for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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

        private void AddCustomerPhoneContact(IEnumerable<CustomerPhoneContactViewModels> entity)
        {
            var phone = new tbl_Customer_PhoneContact();
            foreach (var ent in entity)
            {
                phone.Active = ent.active;
                phone.CustomerId = customerId;
                phone.Phone = ent.phone;
                phone.PhoneNumber = ent.phoneNumber;
                context.tbl_Customer_PhoneContact.Add(phone);
            }
        }
        public bool AddCustomerPhoneContact(CustomerPhoneContactViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    tbl_Customer_PhoneContact phone;
                    if (entity.phoneContactId != 0 || entity.phoneContactId < 0)
                    {
                        phone = context.tbl_Customer_PhoneContact.Find(entity.phoneContactId);
                        if (phone != null)
                        {
                            phone.Active = entity.active;
                            phone.Phone = entity.phone;
                            phone.PhoneNumber = entity.phoneNumber;
                        }
                    }
                    else
                    {
                        phone = new tbl_Customer_PhoneContact
                        {
                            Active = entity.active,
                            CustomerId = entity.customerId,
                            Phone = entity.phone,
                            PhoneNumber = entity.phoneNumber
                        };
                        context.tbl_Customer_PhoneContact.Add(phone);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_Identification for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
            var info = new tbl_Customer_CompanyInfomation();
            foreach (var ent in entity)
            {
                info.AnnualTurnOver = ent.annualTurnOver;
                info.CompanyEmail = ent.companyEmail;
                info.CompanyName = ent.companyName;
                info.CompanyWebsite = ent.companyWebsite;
                info.CorporateBusinessCategory = ent.corporateBusinessCategory;
                info.CreditRating = ent.creditRating;
                info.CustomerId = customerId;
                info.PreviousCreditRating = ent.previousCreditRating;
                info.RegisteredOffice = ent.registeredOffice;
                info.RegistrationNumber = ent.registrationNumber;
               // info.paidUpCapital = ent.PaidUpCapital;
                //info.authorizedCapital = ent.AuthorisedCapital;
                context.tbl_Customer_CompanyInfomation.Add(info);

                // Audit Section ---------------------------
                var customer = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == info.CustomerId);
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                    StaffId = ent.createdBy,
                    BranchId = (short)ent.userBranchId,
                    Detail = "Added Customer's Customer Info for: " + customer.FirstName + " " + customer.LastName + " with Id: " + info.CustomerId + " to company" + " (" + info.CompanyName + ") " + " on " + info.CompanyInfomationId,
                    IPAddress = ent.userIPAddress,
                    Url = ent.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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
                    var info = context.tbl_Customer_CompanyInfomation.Find(ent.companyInfomationId);
                    info.AnnualTurnOver = ent.annualTurnOver;
                    info.CompanyEmail = ent.companyEmail;
                    info.CompanyName = ent.companyName;
                    info.CompanyWebsite = ent.companyWebsite;
                    info.CorporateBusinessCategory = ent.corporateBusinessCategory;
                    info.CreditRating = ent.creditRating;
                    info.CustomerId = ent.customerId;
                    info.PreviousCreditRating = ent.previousCreditRating;
                    info.RegisteredOffice = ent.registeredOffice;
                    info.RegistrationNumber = ent.registrationNumber;
                }
            }
               
        }
        
        private void AddCustomerIdentification(List<CustomerIdentificationViewModels> entity)
        {
            var identity = new tbl_Customer_Identification();
            foreach (var ent in entity)
            {
                identity.CustomerId = customerId;
                identity.IdentificationModeId = ent.identificationModeId;
                identity.IdentificationNo = ent.identificationNo;
                identity.IssueAuthority = ent.issueAuthority;
                identity.IssuePlace = ent.issuePlace;
                context.tbl_Customer_Identification.Add(identity);
            }

        }

        public bool AddCustomerCompanyDirector(CustomerCompanyDirectorsViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    tbl_Customer_Company_Director directors;
                    if (entity.companyDirectorId != 0 || entity.companyDirectorId < 0)
                    {
                        directors = context.tbl_Customer_Company_Director.Find(entity.companyDirectorId);
                        if (directors != null)
                        {
                            directors.CustomerId = entity.customerId;
                            directors.Surname = entity.surname;
                            directors.Firstname = entity.firstname;
                            directors.CompanyDirectorTypeId = entity.companyDirectorTypeId;
                            directors.CustomerBVN = entity.bankVerificationNumber;
                            directors.NumberOfShares = entity.numberOfShares;
                            directors.IsPoliticallyExposed = entity.isPoliticallyExposed;
                            directors.Address = entity.address;
                            directors.PhoneNumber = entity.phoneNumber;
                            directors.EmailAddress = entity.email;
                        }
                    }
                    else
                    {
                        directors = new tbl_Customer_Company_Director();

                        directors.CustomerId = entity.customerId;
                        directors.Surname = entity.surname;
                        directors.Firstname = entity.firstname;
                        directors.CompanyDirectorTypeId = entity.companyDirectorTypeId;
                        directors.CustomerBVN = entity.bankVerificationNumber;
                        directors.NumberOfShares = entity.numberOfShares;
                        directors.IsPoliticallyExposed = entity.isPoliticallyExposed;
                        directors.Address = entity.address;
                        directors.PhoneNumber = entity.phoneNumber;
                        directors.EmailAddress = entity.email;
                        directors.CreatedBy = entity.createdBy;
                        directors.DateCreated = DateTime.Now;

                        context.tbl_Customer_Company_Director.Add(directors);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_Identification for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
                    tbl_Customer_Identification identity;
                    if (entity.identificationId != 0 || entity.identificationId < 0)
                    {
                        identity = context.tbl_Customer_Identification.Find(entity.identificationId);
                        if (identity != null)
                        {
                            identity.CustomerId = entity.customerId;
                            identity.IdentificationModeId = entity.identificationModeId;
                            identity.IdentificationNo = entity.identificationNo;
                            identity.IssueAuthority = entity.issueAuthority;
                            identity.IssuePlace = entity.issuePlace;
                        }
                    }
                    else
                    {
                        identity = new tbl_Customer_Identification();
                        identity.CustomerId = entity.customerId;
                        identity.IdentificationModeId = entity.identificationModeId;
                        identity.IdentificationNo = entity.identificationNo;
                        identity.IssueAuthority = entity.issueAuthority;
                        identity.IssuePlace = entity.issuePlace;

                        context.tbl_Customer_Identification.Add(identity);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_Identification for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
            var history = new tbl_Customer_EmploymentHistory();
            foreach (var ent in entity)
            {
                history.Active = ent.active;
                history.CustomerId = customerId;
                history.EmployDate = ent.employDate;
                history.EmployerAddress = ent.employerAddress;
                history.EmployerCountryId = ent.employerCountryId;
                history.EmployerStateId = ent.employerStateId;
                history.EmployerName = ent.employerName;
                history.OfficePhone = ent.officePhone;
                history.PreviousEmployer = ent.previousEmployer;
                context.tbl_Customer_EmploymentHistory.Add(history);
            }
        }

        public bool AddCustomerClientSupplier(CustomerClientOrSupplierViewModels entity)
        {
            if (entity != null)
            {
                try
                {
                    tbl_Customer_Client_Supplier clientSupplier;
                    if (entity.client_SupplierId != 0 || entity.client_SupplierId < 0)
                    {
                        clientSupplier = context.tbl_Customer_Client_Supplier.Find(entity.client_SupplierId);
                        if (clientSupplier != null)
                        {
                            clientSupplier.CustomerId = entity.customerId;
                            clientSupplier.CustomerTypeId = entity.customerTypeId;
                            clientSupplier.FirstName = entity.firstName;
                            clientSupplier.MiddleName = entity.middleName;
                            clientSupplier.LastName = entity.lastName;
                            clientSupplier.Address = entity.client_SupplierAddress;
                            clientSupplier.PhoneNumber = entity.client_SupplierPhoneNumber;
                            clientSupplier.EmailAddress = entity.client_SupplierEmail;
                            clientSupplier.Client_SupplierTypeId = entity.client_SupplierTypeId;
                            clientSupplier.CreatedBy = entity.createdBy;
                        }
                    }
                    else
                    {
                        clientSupplier = new tbl_Customer_Client_Supplier();

                        clientSupplier.CustomerId = entity.customerId;
                        clientSupplier.CustomerTypeId = entity.customerTypeId;
                        clientSupplier.FirstName = entity.firstName;
                        clientSupplier.MiddleName = entity.middleName;
                        clientSupplier.LastName = entity.lastName;
                        clientSupplier.Address = entity.client_SupplierAddress;
                        clientSupplier.PhoneNumber = entity.client_SupplierPhoneNumber;
                        clientSupplier.EmailAddress = entity.client_SupplierEmail;
                        clientSupplier.Client_SupplierTypeId = entity.client_SupplierTypeId;
                        clientSupplier.CreatedBy = entity.createdBy;
                        clientSupplier.DateCreated = DateTime.Now;
                        context.tbl_Customer_Client_Supplier.Add(clientSupplier);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_Client_Supplier for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
                    tbl_Customer_EmploymentHistory history;
                    if (entity.placeOfWorkId != 0 || entity.placeOfWorkId < 0)
                    {
                        history = context.tbl_Customer_EmploymentHistory.Find(entity.placeOfWorkId);

                        if (history != null)
                        {
                            history.Active = entity.active;
                            history.CustomerId = entity.customerId;
                            history.EmployDate = entity.employDate;
                            history.EmployerAddress = entity.employerAddress;
                            history.EmployerCountryId = entity.employerCountryId;
                            history.EmployerStateId = entity.employerStateId;
                            history.EmployerName = entity.employerName;
                            history.OfficePhone = entity.officePhone;
                            history.PreviousEmployer = entity.previousEmployer;
                        }
                    }
                    else
                    {
                        history = new tbl_Customer_EmploymentHistory();

                        history.Active = entity.active;
                        history.CustomerId = entity.customerId;
                        history.EmployDate = entity.employDate;
                        history.EmployerAddress = entity.employerAddress;
                        history.EmployerCountryId = entity.employerCountryId;
                        history.EmployerStateId = entity.employerStateId;
                        history.EmployerName = entity.employerName;
                        history.OfficePhone = entity.officePhone;
                        history.PreviousEmployer = entity.previousEmployer;
                        context.tbl_Customer_EmploymentHistory.Add(history);
                    }

                    // Audit Section ----------------------------
                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                        StaffId = entity.createdBy,
                        BranchId = (short)entity.userBranchId,
                        Detail = "Added new tbl_Customer_EmploymentHistory for customer ID: + (" + entity.customerId + ") ",
                        IPAddress = entity.userIPAddress,
                        Url = entity.applicationUrl,
                        ApplicationDate = _genSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
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
            var customer = context.tbl_Customer.Find(customerId);

            if (customer != null)
            {
                customer.DateTimeDeleted = DateTime.Now;
                customer.Deleted = true;
                customer.DeletedBy = user.staffId;


                // Audit Section ---------------------------

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short) AuditTypeEnum.CustomerDeleted,
                    StaffId = user.staffId,
                    BranchId = (short) user.BranchId,
                    Detail = "Deleted Customer: " + customer.LastName + " with code: " + customer.CustomerCode,
                    IPAddress = user.userIPAddress,
                    Url = user.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
            }
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }


        IQueryable<CustomerViewModels> GetCustomers()
        {
            return from a in context.tbl_Customer
                   where a.Deleted == false
                   select

                   new CustomerViewModels
                   {
                       accountCreationComplete = a.AccountCreationComplete,
                       branchId = a.BranchId,
                       branchName = a.tbl_Branch.BranchName,
                       childDateOfBirth = a.ChildDateOfBirth.Value,
                       companyMainId = a.CompanyId,
                       createdBy = a.CreatedBy,
                       creationMailSent = a.CreationMailSent,
                       customerCode = a.CustomerCode,
                       customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                       customerTypeId = a.CustomerTypeId.Value,
                       dateOfBirth = a.DateOfBirth,
                       customerId = a.CustomerId,
                       emailAddress = a.EmailAddress,
                       firstChildName = a.FirstChildName,
                       firstName = a.FirstName,
                       gender = a.Gender,
                       lastName = a.LastName,
                       maidenName = a.MaidenName,
                       maritalStatus = a.MaritalStatus.Value,
                       title = a.Title,
                       middleName = a.MiddleName,
                       customerTypeName = context.tbl_Customer_Type.FirstOrDefault(c => c.CustomerTypeId == a.CustomerTypeId).Name,
                       misCode = a.MISCode,
                       misStaff = a.MISStaff,
                       nationality = a.Nationality,
                       occupation = a.Occupation,
                       placeOfBirth = a.PlaceOfBirth,
                       politicallyExposedPerson = a.PoliticallyExposedPerson,
                       relationshipOfficerId = a.RelationshipOfficerId.Value,
                       spouse = a.Spouse,
                       sectorId = a.tbl_Sub_Sector.tbl_Sector.SectorId,
                       sectorName = a.tbl_Sub_Sector.tbl_Sector.Name,
                       subSectorId = a.SubSectorId,
                       subSectorName = a.tbl_Sub_Sector.Name,
                       taxNumber = a.TaxNumber
                       ,
                       CustomerAddresses = context.tbl_Customer_Address.Where(x => x.CustomerId == a.CustomerId).Select(x => new CustomerAddressViewModels()
                       {
                           address = x.Address,
                           addressTypeId = x.AddressTypeId,
                           cityId = x.CityId,
                           customerId = x.CustomerId,
                           homeTown = x.HomeTown,
                           nearestLandmark = x.NearestLandmark,
                           electricMeterNumber = x.ElectricMeterNumber,
                           pobox = x.POBox,
                           stateId = x.StateId,
                           addressId = x.AddressId
                       }).ToList(),
                       CustomerBvn = context.tbl_Customer_BVN.Where(b => b.CustomerId == a.CustomerId).Select(b => new CustomerBvnViewModels()
                       {
                           bankVerificationNumber = b.BankVerificationNumber,
                           customerBvnid = b.CustomerBVNId,
                           firstname = b.Firstname,
                           isValidBvn = b.IsValidBVN,
                           isPoliticallyExposed = b.IsPoliticallyExposed,
                           surname = b.Surname
                       }).ToList(),
                       CustomerPhoneContact = context.tbl_Customer_PhoneContact.Where(c => c.CustomerId == a.CustomerId).Select(c => new CustomerPhoneContactViewModels
                       {
                           active = c.Active,
                           customerId = c.CustomerId,
                           phone = c.Phone,
                           phoneContactId = c.PhoneContactId,
                           phoneNumber = c.PhoneNumber
                       }).ToList(),
                       CustomerCompanyInfomation = context.tbl_Customer_CompanyInfomation.Where(d => d.CustomerId == a.CustomerId).Select(d => new CustomerCompanyInfomationViewModels()
                       {
                           annualTurnOver = d.AnnualTurnOver,
                           companyEmail = d.CompanyEmail,
                           companyId = d.CustomerId,
                           companyName = d.CompanyName,
                           companyWebsite = d.CompanyWebsite,
                           companyInfomationId = d.CompanyInfomationId,
                           corporateBusinessCategory = d.CorporateBusinessCategory,
                           createdBy = a.CreatedBy,
                           creditRating = d.CreditRating,
                           registeredOffice = d.RegisteredOffice,
                           previousCreditRating = d.PreviousCreditRating,
                           registrationNumber = d.RegistrationNumber,
                           paidUpCapital = d.PaidUpCapital,
                           authorizedCapital = d.AuthorisedCapital

                       }).ToList(),
                       CustomerIdentification = context.tbl_Customer_Identification.Where(e => e.CustomerId == a.CustomerId).Select(e => new CustomerIdentificationViewModels()
                       {
                           identificationId = e.IdentificationId,
                           identificationModeId = e.IdentificationModeId.Value,
                           identificationMode = context.tbl_Customer_IdentificationModeType.FirstOrDefault(r => r.IdentificationModeId == e.IdentificationModeId).IdentificationMode,
                           identificationNo = e.IdentificationNo,
                           issueAuthority = e.IssueAuthority,
                           issuePlace = e.IssuePlace
                       }).ToList(),
                       CustomerEmploymentHistory = context.tbl_Customer_EmploymentHistory.Where(s => s.CustomerId == a.CustomerId).Select(s => new CustomerEmploymentHistoryViewModels()
                       {
                           active = s.Active,
                           previousEmployer = s.PreviousEmployer,
                           customerId = s.CustomerId,
                           employDate = s.EmployDate,
                           placeOfWorkId = s.PlaceOfWorkId,
                           employerAddress = s.EmployerAddress,
                           employerCountryId = s.EmployerCountryId,
                           employerName = s.EmployerName,
                           officePhone = s.OfficePhone,
                           employerStateId = s.EmployerStateId
                       }).ToList(),
                       CustomerCompanyDirectors = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == a.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.BoardMember)
                       .Select(s => new CustomerCompanyDirectorsViewModels()
                       {
                           companyDirectorId = s.CompanyDirectorId,
                           surname = s.Surname,
                           firstname = s.Firstname,
                           numberOfShares = s.NumberOfShares,
                           isPoliticallyExposed = s.IsPoliticallyExposed,
                           bankVerificationNumber = s.CustomerBVN,
                           companyDirectorTypeId = s.CompanyDirectorTypeId,
                           companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                           customerId = s.CustomerId,
                           customerName = s.Firstname + " " + s.Surname,
                           address = s.Address,
                           phoneNumber = s.PhoneNumber,
                           email = s.EmailAddress
                       }).ToList(),
                       CustomerCompanyShareholder = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == a.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.Shareholder)
                       .Select(s => new CustomerCompanyShareholderViewModels()
                       {
                           companyDirectorId = s.CompanyDirectorId,
                           surname = s.Surname,
                           firstname = s.Firstname,
                           numberOfShares = s.NumberOfShares,
                           isPoliticallyExposed = s.IsPoliticallyExposed,
                           bankVerificationNumber = s.CustomerBVN,
                           companyDirectorTypeId = s.CompanyDirectorTypeId,
                           companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                           customerId = s.CustomerId,
                           customerName = s.Firstname + " " + s.Surname,
                           address = s.Address,
                           phoneNumber = s.PhoneNumber,
                           email = s.EmailAddress
                       }).ToList(),
                       CustomerClientOrSupplier = context.tbl_Customer_Client_Supplier.Where(cs => cs.CustomerId == a.CustomerId && cs.Client_SupplierTypeId ==(short)CompanyClientOrSupplierTypeEnum.Client)
                       .Select(cs => new CustomerClientOrSupplierViewModels()
                       {
                           client_SupplierId = cs.Client_SupplierId,
                           clientOrSupplierName = cs.FirstName + " " + cs.LastName,
                           firstName = cs.FirstName,
                           middleName = cs.MiddleName,
                           lastName = cs.LastName,
                           client_SupplierAddress = cs.Address,
                           client_SupplierPhoneNumber = cs.PhoneNumber,
                           client_SupplierEmail = cs.EmailAddress,
                           client_SupplierTypeId = cs.Client_SupplierTypeId,
                           client_SupplierTypeName = cs.tbl_Customer_Client_Supplier_Type.Client_SupplierTypeName
                       }).ToList(),
                       CustomerSupplier = context.tbl_Customer_Client_Supplier.Where(cs => cs.CustomerId == a.CustomerId && cs.Client_SupplierTypeId == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                       .Select(cs => new CustomerSupplierViewModels()
                       {
                           client_SupplierId = cs.Client_SupplierId,
                           clientOrSupplierName = cs.FirstName + " " + cs.LastName,
                           firstName = cs.FirstName,
                           middleName = cs.MiddleName,
                           lastName = cs.LastName,
                           client_SupplierAddress = cs.Address,
                           client_SupplierPhoneNumber = cs.PhoneNumber,
                           client_SupplierEmail = cs.EmailAddress,
                           client_SupplierTypeId = cs.Client_SupplierTypeId,
                           client_SupplierTypeName = cs.tbl_Customer_Client_Supplier_Type.Client_SupplierTypeName
                       }).ToList(),
                       CustomerCollateral = context.tbl_Collateral_Customer.Where(cc => cc.CustomerId == a.CustomerId)
                       .Select(x => new CollateralViewModel()
                       {
                           collateralId = x.CollateralCustomerId,
                           collateralTypeId = x.CollateralTypeId,
                           collateralSubTypeId = x.CollateralSubTypeId,
                           customerId = x.CustomerId,
                           currencyId = x.CurrencyId,
                           currency = x.tbl_Currency.CurrencyName,
                           collateralTypeName = x.tbl_Collateral_Type.CollateralTypeName,
                           collateralCode = x.CollateralCode,
                           camRefNumber = x.CamRefNumber,
                           allowSharing = x.AllowSharing,
                           isLocationBased = x.IsLocationBased,
                           valuationCycle = x.ValuationCycle,
                           haircut = x.HairCut,
                           approvalStatus = x.ApprovalStatus,
                       }).ToList(),
                   };

        }

        public IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId)
        {
            var data = (from cs in context.tbl_Customer_Group_Mapping
                        where cs.CustomerGroupId == groupId
                        select new CustomerViewModels()
                        {
                            customerId = cs.CustomerId,
                            fullName = cs.tbl_Customer.LastName + " " + cs.tbl_Customer.FirstName + "(" + cs.tbl_Customer.CustomerCode + ")",
                            firstName = cs.tbl_Customer.FirstName,
                            lastName = cs.tbl_Customer.LastName,
                            customerCode = cs.tbl_Customer.CustomerCode,
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
            var type = from a in context.tbl_Customer_Type
                       select new CustomerTypeViewModels
                       {
                           name = a.Name,
                           customerTypeId = a.CustomerTypeId
                       };
            return type;
        }
        public IEnumerable<CustomerSupplierTypeViewModels> GetClientSupplierType()
        {
            var type = from a in context.tbl_Customer_Client_Supplier_Type
                       select new CustomerSupplierTypeViewModels
                       {
                           name = a.Client_SupplierTypeName,
                           client_SupplierTypeId = a.Client_SupplierTypeId
                       };
            return type;
        }

        public IEnumerable<CustomerIdentificationModeTypeViewModels> GetIdentificationMode()
        {
            var type = from a in context.tbl_Customer_IdentificationModeType
                       select new CustomerIdentificationModeTypeViewModels
                       {
                           name = a.IdentificationMode,
                           identificationModeId = a.IdentificationModeId
                       };
            return type;
        }
        public IEnumerable<CompanyDirectorTypeViewModels> GetDirectorsTypes()
        {
            var type = from a in context.tbl_Customer_Company_DirectorType
                       select new CompanyDirectorTypeViewModels
                       {
                           name = a.CompanyDirectoryTypeName,
                           companyDirectorTypeId = a.CompanyDirectoryTypeId
                       };
            return type;
        }

        public bool UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            var customer = context.tbl_Customer.Find(customerId);

            if (customer != null)
            {
                customer.AccountCreationComplete = entity.accountCreationComplete;
                customer.BranchId = entity.branchId;
                customer.ChildDateOfBirth = entity.childDateOfBirth;
                customer.CompanyId = entity.companyMainId;
                customer.CreatedBy = (int) entity.createdBy;
                customer.CreationMailSent = entity.creationMailSent;
                customer.CustomerCode = entity.customerCode;
                customer.CustomerSensitivityLevelId = entity.customerSensitivityLevelId;
                customer.CustomerTypeId = entity.customerTypeId;
                customer.DateOfBirth = entity.dateOfBirth;
                customer.EmailAddress = entity.emailAddress;
                customer.FirstChildName = entity.firstChildName;
                customer.FirstName = entity.firstName;
                customer.Gender = entity.gender;
                customer.LastName = entity.lastName;
                customer.MaidenName = entity.maidenName;
                customer.MaritalStatus = entity.maritalStatus;
                customer.Title = entity.title;
                customer.MiddleName = entity.middleName;
                customer.MISCode = entity.misCode;
                customer.MISStaff = entity.misStaff;
                customer.Nationality = entity.nationality;
                customer.Occupation = entity.occupation;
                customer.PlaceOfBirth = entity.placeOfBirth;
                customer.PoliticallyExposedPerson = entity.politicallyExposedPerson;
                customer.RelationshipOfficerId = entity.relationshipOfficerId;
                customer.Spouse = entity.spouse;
                customer.SubSectorId = entity.subSectorId;
                customer.TaxNumber = entity.taxNumber;
                customer.DateTimeUpdated = DateTime.Now;
                customer.LastUpdatedBy = entity.deletedBy;
            }

            if (entity.CustomerCompanyInfomation.Count > 0)
            {
                UpdateCustomerCompanyInfomation(entity.CustomerCompanyInfomation);
            }
            // Audit Section ----------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = "Updated tbl_Customer: " + entity.customerName + " with code: " + entity.customerCode + " on" + " (" + entity.customerId + ") ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            return  context.SaveChanges() != 0;

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
            var data = (from cs in context.tbl_Sector
                        select new CustomerSectorViewModel()
                        {
                            sectorId = cs.SectorId,
                            sectorName = cs.Name,
                            sectorCode = cs.Code,
                        });

            return data;
        }

        public IEnumerable<CustomerSectorViewModel> GetCustomerSectorBySubSectorId(short ssId)
        {
            var data = (from s in context.tbl_Sub_Sector
                        where s.SubSectorId == ssId
                        select new CustomerSectorViewModel()
                        {
                            subSectorId = s.SubSectorId,
                            sectorId = s.tbl_Sector.SectorId,
                            sectorName = s.Name,
                            sectorCode = s.Code
                        });

            return data;
        }

    }
}


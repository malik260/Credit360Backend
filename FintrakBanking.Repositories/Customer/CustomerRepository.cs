using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
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
            this.context = _context;
            auditTrail = _auditTrail;
            this._genSetup = genSetup;
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
                AddCustomerPhoneContact(entity.CustomerPhoneContact, status);
            }

            if (entity.CustomerCompanyInfomation.Count > 0)
            {
                AddCustomerCompanyInfomation(entity.CustomerCompanyInfomation, status);
            }

            if (entity.CustomerIdentification.Count > 0)
            {
                AddCustomerIdentification(entity.CustomerIdentification, status);
            }

            if (entity.CustomerEmploymentHistory.Count > 0)
            {
                AddCustomerEmploymentHistory(entity.CustomerEmploymentHistory, status);
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

            this.auditTrail.AddAuditTrail(audit);

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

                // Audit Section ---------------------------
                //var audit = new tbl_Audit
                //{
                //    AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                //    StaffId = user.staffId,
                //    BranchId = (short) user.BranchId,
                //    Detail = status == 1 ? "Added tbl_Customer BVN: ": "Updated tbl_Customer BVN: " + bvn.bankVerificationNumber  + " to customer: " + bvn.customerId + " (" + bvn.surname + ")",
                //    IPAddress = user.userIPAddress,
                //    Url = user.applicationUrl,
                //    ApplicationDate = _genSetup.GetApplicaionDate(),
                //    SystemDateTime = DateTime.Now
                //};

                //this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
            }
        }

        private void AddCustomerPhoneContact(List<CustomerPhoneContactViewModels> entity,
           int status)
        {
            var phone = new tbl_Customer_PhoneContact();
            foreach (var ent in entity)
            {
                phone.Active = ent.active;
                phone.CustomerId = customerId;
                phone.Phone = ent.phone;
                phone.PhoneNumber = ent.phoneNumber;
                context.tbl_Customer_PhoneContact.Add(phone);
                // Audit Section ---------------------------
                //var CustomerCode = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == ent.customerId).CustomerCode;
                //var audit = new tbl_Audit
                //{
                //    AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                //    StaffId = user.staffId,
                //    BranchId = (short) user.BranchId,
                //    Detail = status == 1 ? "Added tbl_Customer Phone Contact: " : "Updated tbl_Customer Phone Contact: " + ent.phone + " to customer:  (" + CustomerCode + ")",
                //    IPAddress = user.userIPAddress,
                //    Url = user.applicationUrl,
                //    ApplicationDate = _genSetup.GetApplicaionDate(),
                //    SystemDateTime = DateTime.Now
                //};

                //this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
            }
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
                context.tbl_Customer_CompanyInfomation.Add(info);

                // Audit Section ---------------------------
                var customer = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == info.CustomerId);
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

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
            }

        }

        private void AddCustomerIdentification(List<CustomerIdentificationViewModels> entity,
           int status)
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

        private void AddCustomerEmploymentHistory(List<CustomerEmploymentHistoryViewModels> entity,
           int status)
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

                // Audit Section ----------------------------
                //var customer = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == history.CustomerId);
                //var audit = new tbl_Audit
                //{
                //    AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                //    StaffId = user.staffId,
                //    BranchId = (short) user.BranchId,
                //    Detail = status == 1 ? "Deleted tbl_Customer: " : "Updated tbl_Customer: " + customer.LastName + " with code: " + customer.CustomerCode,
                //    IPAddress = user.userIPAddress,
                //    Url = ent.applicationUrl,
                //    ApplicationDate = _genSetup.GetApplicaionDate(),
                //    SystemDateTime = DateTime.Now
                //};

                //this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
            }
        }

        public async Task<bool> DeleteCustomer(int customerId, UserInfo user)
        {
            var customer = context.tbl_Customer.Find(customerId);

            customer.DateTimeDeleted = DateTime.Now;
            customer.Deleted = true;
            customer.DeletedBy = user.staffId;


            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = "Deleted Customer: " + customer.LastName + " with code: " + customer.CustomerCode,
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
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
                       //branchName = a.tbl_Branch.BranchName,
                       childDateOfBirth = a.ChildDateOfBirth.Value,
                       companyId = a.CompanyId,
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
                           authorisedCapital = d.AuthorisedCapital

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
                       CustomerCompanyDirectors = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == a.CustomerId).Select(s => new CustomerCompanyDirectorsViewModels()
                       {
                           bankVerificationNumber = s.CustomerBVN,
                           companyDirectorTypeId = s.CompanyDirectorTypeId,
                           companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                           customerId = s.CustomerId,
                           customerName = s.Firstname + " " + s.Surname,
                           address = s.Address,
                           phoneNumber = s.PhoneNumber,
                           email = s.EmailAddress
                       }).ToList(),
   //                    CustomerClientOrSupplier = context.tbl_Customer_Client_Supplier.Where(cs => cs.CustomerId == a.CustomerId).Select(cs => new CustomerClientOrSupplierViewModels()
   //                    {
   //client_SupplierId = cs.Client_SupplierId,
   //clientOrSupplierName = cs.FirstName +" "+ cs.LastName,
   //     firstName  = cs.FirstName, 
   //    middleName = cs.MiddleName,
   //    lastName = cs.LastName,
   //    client_SupplierAddress = cs.Client_SupplierAddress,
   //    client_SupplierPhoneNumber = cs.Client_SupplierPhoneNumber,
   //    client_SupplierEmail = cs.Client_SupplierEmail,
   //    client_SupplierTypeId = cs.Client_SupplierTypeId,
   //    client_SupplierTypeName = cs.tbl_Customer_Client_Supplier_Type.Client_SupplierTypeName
   // }).ToList()
                   };

        }

        public CustomerViewModels GetCustomer(int custormerId)
        {

            var data = GetCustomers().Where(a => a.customerId == custormerId).FirstOrDefault();
            return data;
        }

        public IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId)
        {
            return GetCustomers().Where(a => a.branchId == branchId);
        }

        public IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId)
        {
            return GetCustomers().Where(a => a.companyId == companyId);
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

        public IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId)
        {
            var data = (from cs in context.tbl_Customer_Group_Mapping
                        where cs.CustomerGroupId == groupId
                        select new CustomerViewModels()
                        {
                            customerId = cs.CustomerId,
                            fullName = cs.tbl_Customer.LastName + " " + cs.tbl_Customer.FirstName + "(" + cs.tbl_Customer.CustomerCode +")",
                            firstName = cs.tbl_Customer.FirstName,
                            lastName = cs.tbl_Customer.LastName,
                            customerCode = cs.tbl_Customer.CustomerCode,
                        });

            return data;
        }

        public async Task<bool> UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            var customer = context.tbl_Customer.Find(customerId);

            customer.AccountCreationComplete = entity.accountCreationComplete;
            customer.BranchId = entity.branchId;
            customer.ChildDateOfBirth = entity.childDateOfBirth;
            customer.CompanyId = entity.companyId;
            customer.CreatedBy = (int)entity.createdBy;
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

            this.auditTrail.AddAuditTrail(audit);

            return await context.SaveChangesAsync() != 0;

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


            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
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


        public IEnumerable<SectorViewModel> GetCustomerSectors()
        {
            var data = (from cs in context.tbl_Sector
                        select new SectorViewModel()
                        {
                            sectorId = cs.SectorId,
                            sectorName = cs.Name,
                            sectorCode = cs.Code,
                        });

            return data;
        }


        public IEnumerable<SectorViewModel> GetCustomerSectorBySubSectorId(short ssId)
        {
            var data = (from s in context.tbl_Sub_Sector
                        where s.SubSectorId == ssId
                        select new SectorViewModel()
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


using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerGroupMappingViewModel : GeneralEntity
    {
        
        public CustomerGroupMappingViewModel()
        {
            customerBvnInformation = new List<CustomerBvnViewModels>();
            customerCompanyShareholders = new List<CustomerCompanyShareholdersViewModels>();
            customerCompanyDirectors = new List<CustomerCompanyDirectorsViewModels>();
            customerClients = new List<CustomerClientOrSupplierViewModels>();
            customerSuppliers = new List<CustomerSupplierViewModels>();
        }
        public string groupDescription { get; set; }
        public int customerGroupMappingId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string customerType { get; set; }
        public int customerGroupId { get; set; }
        public short relationshipTypeId { get; set; }
        public string relationshipTypeName { get; set; }
        public string customerGroupName { get; set; }
        public string customerGroupCode { get; set; }
        public string productAccountNumber { get; set; }
        public string accountHolder { get; set; }
        public short branchId { get; set; }
        public bool completedInformation { get; set; }
        public bool crdeitBureauCompleted { get; set; }
        public int? relationshipManagerId { get; set; }
        public int? relationshipOfficerId { get; set; }
        public List<CustomerBvnViewModels> customerBvnInformation { get; set; }
        public List<CustomerCompanyShareholdersViewModels> customerCompanyShareholders { get; set; }
        public List<CustomerCompanyDirectorsViewModels> customerCompanyDirectors { get; set; }
        public List<CustomerClientOrSupplierViewModels> customerClients { get; set; }
        public List<CustomerSupplierViewModels> customerSuppliers { get; set; }
        public string taxIdentificationNumber { get; set; }
        public string registrationNumber { get; set; }
        public bool isBlackList { get; set; }
        public bool isOnWatchList { get; set; }
        public bool isCamsol { get; set; }

        public int approvalStatusId { get; set; }
        public int operationId { get; set; }
        public short customerTypeId { get; set; }
    }


    public class GroupCustomerMembersViewModel
    {
        public bool isProspect { get; set; }

        public int customerId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string customerName { get { return lastName + ' ' + firstName; } }
        public string customerCode { get; set; }
        public string customerType { get; set; }
        public short? customerTypeId { get; set; }
        public string customerRating { get; set; }

        //public string productAccountNumber { get; set; }
        //public string productAccountName { get; set; }
        //public decimal balance { get; set; }
        //public int casaAccountId { get; set; }
        //  public string accountStatusId { get; set; }
    }


}

using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.CASA
{

    public class CasaViewModel : GeneralEntity
    {

        //public string accountNumber { get; set; }
        //public string accountName { get; set; }
        //public string product { get; set; }
        //public string productType { get; set; }
        //public string productCode { get; set; }
        //public string productName { get; set; }
        //public string accountDetail { get { return (this.accountNumber + "(" + this.accountName + ")"); } }
        //public string currencyType { get; set; }
        //public decimal balance { get; set; }
        //public string branch { get; set; }
        //public string accountStatus { get; set; }
        //public DateTime lastTransactionDate { get; set; }

        
        public string searchString { get; set; }
        public int casaAccountId { get; set; }
        public string productAccountNumber { get; set; }
        public string productAccountName { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public int productId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        //public int companyId { get; set; }
        public string accountDetail { get { return (productAccountNumber +"("+ customerCode +" " + productAccountName + ")"); } }
        public string productAccountDetail{ get { return (productAccountNumber + "(" + productName + " -" + currency + ")"); } }
        public short branchId { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public bool isCurrentAccount { get; set; }
        public int tenor { get; set; }
        public int currencyId { get; set; }
        public decimal interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime terminalDate { get; set; }
        public int actionBy { get; set; }
        public DateTime actionDate { get; set; }
        public short accountStatusId { get; set; }
        public string accountStatusName { get; set; }
        public int operationId { get; set; }
        public string currency { get; set; }
        public decimal availableBalance { get; set; }
        public decimal ledgerBalance { get; set; }
        public int relationshipManagerId { get; set; }
        public string relationshipManagerName { get; set; }
        public int relationshipOfficerId { get; set; }
        public string relationshipOfficerName { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public decimal overdraftAmount { get; set; }
        public decimal overdraftInterestRate { get; set; }
        public DateTime overdraftExpiryDate { get; set; }
        public bool? hasOverdraft { get; set; }
        public decimal lienAmount { get; set; }
        public bool hasLien { get; set; }
        public short postNoStatusId { get; set; }
        public string oldProductAccountNumber1 { get; set; }
        public string oldProductAccountNumber2 { get; set; }
        public string oldProductAccountNumber3 { get; set; }
        public string refreshBatchId { get; set; }
        public DateTime? lastRefreshDatetime { get; set; }
        public short? aprovalStatusId { get; set; }

        public string description { get; set; }
        public string lienReferenceNumber { get; set; }
        public string sourceReferenceNumber { get; set; }
        public short? lienTypeId { get; set; }
    }

    public class CasaCustomerSearchViewModel : CasaViewModel
    {
        public CasaCustomerSearchViewModel()
        {
            customerBvnInformation = new List<CustomerBvnViewModels>();
            customerCompanyDirectors = new List<CustomerCompanyDirectorsViewModels>();
            customerCompanyShareholders = new List<CustomerCompanyShareholdersViewModels>();
            customerClients = new List<CustomerClientOrSupplierViewModels>();
            customerSuppliers = new List<CustomerSupplierViewModels>();
        }

        public int productClassId { get; set; }
        public string productClassName { get; set; }
        public int customerSectorId { get; set; }
        public string customerSectorName { get; set; }
        public int subSectorId { get; set; }
        public string subSectorName { get; set; }
        //public string relationshipManagerName { get; set; }
        //public string relationshipOfficerName { get; set; }
        public string accountHolder { get; set; }

        public List<CustomerBvnViewModels> customerBvnInformation { get; set; }
        public List<CustomerCompanyDirectorsViewModels> customerCompanyDirectors { get; set; }
        public short? customerTypeId { get; set; }
        public string customerTypeName { get; set; }
        public string taxIdentificationNumber { get; set; }
        public string registrationNumber { get; set; }
        public List<CustomerCompanyShareholdersViewModels> customerCompanyShareholders { get; set; }
        public bool completedInformation { get; set; }
        public bool isBlackList { get; set; }
        public int? customerGroupId { get; set; }
        public bool isCamsol { get; set; }
        public string customerGroupName { get; set; }
        public List<CustomerClientOrSupplierViewModels> customerClients { get; set; }
        public List<CustomerSupplierViewModels> customerSuppliers { get; set; }
        public bool isOnWatchList { get; set; }
    }

    public class CustomerCasaAcountsViewModel
    {
        public int customerId { get; set; }
        public int casaAccountId { get; set; }
        public string productAccountNumber { get; set; }
        public string productAccountName { get; set; }
        public decimal availableBalance { get; set; }
    }

}

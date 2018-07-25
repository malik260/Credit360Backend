using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.Finance
{
    public class ChartOfAccountViewModel : GeneralEntity
    {

        public ChartOfAccountViewModel()
        {
            currencies = new List<ChartOfAccountCurrencyViewModel>();
        }
        public int accountId { get; set; }
        public string accountCode { get; set; }
        public string accountName { get; set; }
        public int accountTypeId { get; set; }
        public string accountTypeName { get; set; }
        public short accountCategoryId { get; set; }
        public string accountCategoryName { get; set; }
        
        public short branchId { get; set; }  
        public string branchName { get; set; }
        public string currencyName { get; set; }
        public bool systemUse { get; set; }
        public int? accountStatusId { get; set; }
        public bool branchSpecific { get; set; }
        public string oldAccountId { get; set; }
        public short fsCaptionId { get; set; }
        public string fsCaptionName { get; set; }
        public int approvalStatusId { get; set; }
        public string comment { get; set; }
        public int operationId { get; set; }
        public List<ChartOfAccountCurrencyViewModel> currencies { get; set; }

        public string accountDetail { get { return this.accountCode + " -- " + this.accountName + " -- " + accountCategoryName; } }

        public short glClassId { get; set; }
    }

    public class ChartOfAccountCurrencyViewModel: GeneralEntity
    {
        public int glaccountId { get; set; }
        public int glaccountCurrencyId { get; set; }
        public short currencyId { get; set; }
        public string currencyName { get; set; }
    }

    public class ChartOfAccountClassViewModel: GeneralEntity
    {
        public short glClassId { get; set; }
        public string glClassName { get; set; }
    }

    public class CustomChartOfAccountViewModel : GeneralEntity
    {
        public string detail { get; set; }

        public bool isNostroAccount { get; set; }

        public int customAccountId { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string currencyCode { get; set; }
        public string placeholderId { get; set; }
    }
}
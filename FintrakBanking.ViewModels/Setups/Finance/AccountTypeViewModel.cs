using System;
using System.ComponentModel.DataAnnotations;

namespace FintrakBanking.ViewModels.Setups.Finance
{
    public class AddAccountTypeViewModel : GeneralEntity
    {
        public int accountTypeCode { get; set; }
        public string accountTypeName { get; set; }
        public short accountCategoryId { get; set; }
    }

    public class AccountTypeViewModel : GeneralEntity
    {
        public int accountTypeId { get; set; }
        public int accountTypeCode { get; set; }

        [Required]
        public string accountTypeName { get; set; }

        public short accountCategoryId { get; set; }
        public string accountCategoryName { get; set; }


        public string accountTypeDetail { get { return this.accountTypeCode + " -- " + this.accountTypeName + " -- " + accountCategoryName; } }
    }
}
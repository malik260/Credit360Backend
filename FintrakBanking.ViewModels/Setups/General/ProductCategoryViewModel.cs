using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductCategoryViewModel
    {
        public short productCategoryId { get; set; }
        public string productCategoryName { get; set; }
    }

    public class RevolvingTypeViewModel
    {
        public short revolvingTypeId { get; set; }
        public string revolvingTypeName { get; set; }
    }


    public class productClassProcess
    {
        public short productClassProcessId { get; set; }
 
        public string productClassProscessName { get; set; }    

        public bool userAmountLimit { get; set; }

        public Decimal? maximumAmount { get; set; }
    }
}
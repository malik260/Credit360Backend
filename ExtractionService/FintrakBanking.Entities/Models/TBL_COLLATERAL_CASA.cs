namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_CASA")]
    public partial class TBL_COLLATERAL_CASA
    {
        [Key]
        public int COLLATERALCASAID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        //[Required]
        ////[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        public string ACCOUNTNAME { get; set; }

        public bool ISOWNEDBYCUSTOMER { get; set; }

        //[Column(TypeName = "money")]
        public decimal AVAILABLEBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal EXISTINGLIENAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal LIENAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal SECURITYVALUE { get; set; }

        ////[StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}

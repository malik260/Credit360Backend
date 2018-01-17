namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_CHARGE_FEE_CUSTMER")]
    public partial class TBL_PRODUCT_CHARGE_FEE_CUSTMER
    {
        [Key]
        public int CUSTOMER_PRODUCT_FEEID { get; set; }

        public int PRODUCTFEEID { get; set; }

        public int CUSTOMERID { get; set; }

        [Column(TypeName = "money")]
        public decimal RATEVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal? DEPENDENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT_CHARGE_FEE TBL_PRODUCT_CHARGE_FEE { get; set; }
    }
}

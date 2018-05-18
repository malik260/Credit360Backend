namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_PRODUCT_CHARGE_FEE")]
    public partial class TBL_TEMP_PRODUCT_CHARGE_FEE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTFEEID { get; set; }

        public int PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CHARGEFEEID { get; set; }

        public decimal RATEVALUE { get; set; }

        public decimal? DEPENDENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_TEMP_PRODUCT TBL_TEMP_PRODUCT { get; set; }
    }
}

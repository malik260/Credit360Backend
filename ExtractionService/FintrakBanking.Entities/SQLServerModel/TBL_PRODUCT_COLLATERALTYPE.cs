namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_PRODUCT_COLLATERALTYPE")]
    public partial class TBL_PRODUCT_COLLATERALTYPE
    {
        [Key]
        public int PRODUCTCOLLATERALTYPEID { get; set; }

        public short PRODUCTID { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }
    }
}

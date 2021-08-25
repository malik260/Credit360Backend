namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_PRODUCT_COLLATERALTYP")]
    public partial class TBL_TEMP_PRODUCT_COLLATERALTYP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTCOLLATERALTYPEID { get; set; }

        public int PRODUCTID { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_TEMP_PRODUCT TBL_TEMP_PRODUCT { get; set; }
    }
}

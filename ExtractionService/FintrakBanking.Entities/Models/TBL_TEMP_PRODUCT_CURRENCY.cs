namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_PRODUCT_CURRENCY")]
    public partial class TBL_TEMP_PRODUCT_CURRENCY
    {
        [Key]
        public int PRODUCTCURRENCYID { get; set; }

        public short TEMP_PRODUCTID { get; set; }

        public short CURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_TEMP_PRODUCT TBL_TEMP_PRODUCT { get; set; }
    }
}

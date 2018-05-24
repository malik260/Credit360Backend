namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_CHART_OF_ACCOUNT_CUR")]
    public partial class TBL_TEMP_CHART_OF_ACCOUNT_CUR
    {
        [Key]
        public int GLACCOUNTCURRENCYID { get; set; }

        public int GLACCOUNTID { get; set; }

        public short CURRENCYID { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_TEMP_CHART_OF_ACCOUNT TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
    }
}

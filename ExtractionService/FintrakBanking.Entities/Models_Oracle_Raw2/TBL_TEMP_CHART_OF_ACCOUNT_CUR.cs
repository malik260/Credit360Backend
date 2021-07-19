namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CHART_OF_ACCOUNT_CUR")]
    public partial class TBL_TEMP_CHART_OF_ACCOUNT_CUR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GLACCOUNTCURRENCYID { get; set; }

        public int GLACCOUNTID { get; set; }

        public int CURRENCYID { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_TEMP_CHART_OF_ACCOUNT TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
    }
}

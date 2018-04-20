namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_ACCOUNT_HISTORY_DTL")]
    public partial class TBL_STAFF_ACCOUNT_HISTORY_DTL
    {
        [Key]
        public int STAFFACCOUNTHISTORYDETAILID { get; set; }

        public int STAFFACCOUNTHISTORYID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public int TARGETID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_STAFF_ACCOUNT_HISTORY TBL_STAFF_ACCOUNT_HISTORY { get; set; }
    }
}

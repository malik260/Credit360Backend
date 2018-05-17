namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CALL_MEMO_LIMIT")]
    public partial class TBL_CALL_MEMO_LIMIT
    {
        [Key]
        public int CALLLIMITID { get; set; }

        public int JOBTITLEID { get; set; }

        //[Column(TypeName = "money")]
        public decimal MINIMUMAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal MAXIMUMAMOUNT { get; set; }

        public short FREQUENCYID { get; set; }

        public int CALLLIMITTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }
    }
}

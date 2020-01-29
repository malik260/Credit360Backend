namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_AUDIT")]
    public partial class TBL_AUDIT
    {
        [Key]
        public long AUDITID { get; set; }

        public short AUDITTYPEID { get; set; }

        public int STAFFID { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        //[Column(TypeName = "date")]
        public DateTime APPLICATIONDATE { get; set; }

       // [Required]
        ////[StringLength(4000)]
        public string DETAIL { get; set; }

        public short BRANCHID { get; set; }

        ////[StringLength(100)]
        public string IPADDRESS { get; set; }

        ////[StringLength(300)]
        public string URL { get; set; }

        public int TARGETID { get; set; }
        public string DEVICENAME { get; set; }
        public string OSNAME { get; set; }
        public virtual TBL_AUDIT_TYPE TBL_AUDIT_TYPE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}

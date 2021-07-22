namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CALL_MEMO")]
    public partial class TBL_CALL_MEMO
    {
        [Key]
        public int CALLMEMOID { get; set; }

        public int? LOANAPPLICATIONID { get; set; }

        public int STAFFID { get; set; }

        public DateTime MEMODATE { get; set; }

        public DateTime? NEXTCALLDATE { get; set; }

        //[Required]
        public string PURPOSE { get; set; }

        public string DISCUSION { get; set; }

        public string ACTION { get; set; }

        //[StringLength(500)]
        public string PARTICIPANTS { get; set; }
        public string LOCATION { get; set; }
        public string RECENTUPDATE { get; set; }
        public string BACKGROUND { get; set; }
        public string CC { get; set; }
        public int? APPROVALLEVELID { get; set; }
        public int CREATEDBY { get; set; }

        //[Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        public int CUSTOMERID { get; set; }

        public int OPERATIONID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public DateTime CALLTIME { get; set; }
        public DateTime? NEXTCALLTIME { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}

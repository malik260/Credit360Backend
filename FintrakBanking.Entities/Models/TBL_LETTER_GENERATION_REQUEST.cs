namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LETTER_GENERATION_REQUEST")]
    public partial class TBL_LETTER_GENERATION_REQUEST
    {
        [Key]
        public int LETTERGENERATIONREQUESTID { get; set; }

        public string REQUESTREF { get; set; }

        public int CUSTOMERID { get; set; }

        public string COMMENTS { get; set; }

        public int REQUESTTYPE { get; set; }

        public DateTime REQUESTDATE { get; set; }

        public DateTime ASATDATE { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
        
        public int? LASTUPDATEDBY { get; set; }
        
        public DateTime? DATETIMEUPDATED { get; set; }
        
        public bool DELETED { get; set; }
        
        public int? DELETEDBY { get; set; }
        
        public DateTime? DATETIMEDELETED { get; set; }

        public int? OPERATIONID { get; set; }

        public int? APPROVEDBY { get; set; }

        public bool? APPROVED { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        public short? APPLICATIONSTATUSID { get; set; }

        public int? FINALAPPROVAL_LEVELID { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public DateTime? APPROVEDDATE { get; set; }

        public decimal? LOANBALANCE { get; set; }
        

    }
}
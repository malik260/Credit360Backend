namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ORIGINAL_DOCUMENT_APPROVAL")]
    public partial class TBL_ORIGINAL_DOCUMENT_APPROVAL
    {

        [Key]
        public int ORIGINALDOCUMENTAPPROVALID { get; set; }

        public int LOANAPPLICATIONID { get; set; }
        public string APPLICATIONREFERNECENUMBER { get; set; }
        public string REFERENCENUMBER { get; set; }

        //[StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public short APPROVALSTATUSID { get; set; }
        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public DateTime? APPROVALDATE { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public bool ISORIGINALTITLEDOCUMENT { get; set; }
    }
}
/*

public virtual DbSet<TBL_ORIGINAL_DOCUMENT_APPROVAL> TBL_ORIGINAL_DOCUMENT_APPROVAL { get; set; }

*/

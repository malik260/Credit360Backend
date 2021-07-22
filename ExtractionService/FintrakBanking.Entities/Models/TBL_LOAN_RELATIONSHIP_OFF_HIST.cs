namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_RELATIONSHIP_OFF_HIST")]
    public partial class TBL_LOAN_RELATIONSHIP_OFF_HIST
    {
        [Key]
        public short LOANRELATIONSHIPOFFICERID { get; set; }

        public int LOANID { get; set; }

        public int STAFFID { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        public bool? ISCURRENT { get; set; }

        //[StringLength(100)]
        public string CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}

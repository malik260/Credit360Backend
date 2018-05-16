namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CALL_MEMO")]
    public partial class TBL_CALL_MEMO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CALLMEMOID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int STAFFID { get; set; }

        public int CALLLIMITTYPEID { get; set; }

        [Required]
        public string PURPOSE { get; set; }

        public string DISCUSION { get; set; }

        public string SUMMARY { get; set; }

        [Required]
        [StringLength(500)]
        public string ACTION { get; set; }

        [StringLength(500)]
        public string RECOMMENDATION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public DateTime? NEXTCALLDATE { get; set; }

        public DateTime MEMODATE { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_CALL_MEMO_TYPE TBL_CALL_MEMO_TYPE { get; set; }
    }
}

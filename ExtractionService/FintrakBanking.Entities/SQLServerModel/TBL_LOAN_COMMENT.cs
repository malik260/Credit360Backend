namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_COMMENT")]
    public partial class TBL_LOAN_COMMENT
    {
        [Key]
        public short LOANCOMMENTID { get; set; }

        public int? LOANID { get; set; }

        //[StringLength(20)]
        public string COMMENTTYPE { get; set; }

        //[StringLength(250)]
        public string COMMENT { get; set; }

        //[StringLength(100)]
        public string CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }
    }
}

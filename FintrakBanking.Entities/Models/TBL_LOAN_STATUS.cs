namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_STATUS")]
    public partial class TBL_LOAN_STATUS
    {
        [Key]
        public short LOANSTATUSID { get; set; }

        public short? OPERATIONID { get; set; }

        [Required]
        [StringLength(20)]
        public string ACCOUNTSTATUS { get; set; }

        public virtual TBL_LOAN_OPERATION TBL_LOAN_OPERATION { get; set; }
    }
}

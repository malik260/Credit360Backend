namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_REPAYMENT_TERMS")]
    public partial class TBL_REPAYMENT_TERMS
    {
        [Key]
        public long REPAYMENTTERMID { get; set; }

        [Required]
        [StringLength(4000)]
        public string REPAYMENTTERMDETAIL { get; set; }

        public bool DELETED { get; set; }
    }
}

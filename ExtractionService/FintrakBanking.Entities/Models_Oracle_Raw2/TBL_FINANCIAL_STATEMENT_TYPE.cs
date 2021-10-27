namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FINANCIAL_STATEMENT_TYPE")]
    public partial class TBL_FINANCIAL_STATEMENT_TYPE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FSTYPEID { get; set; }

        [Required]
        [StringLength(150)]
        public string FSTYPENAME { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_CASA_Overdraft")]
    public partial class tbl_CASA_Overdraft
    {
        [Key]
        public int OverdraftId { get; set; }

        public int CasaAccountId { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        [Column(TypeName = "money")]
        public decimal CreditAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal DebitAmount { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        public int CreatedBy { get; set; }

        public virtual tbl_CASA tbl_CASA { get; set; }
    }
}

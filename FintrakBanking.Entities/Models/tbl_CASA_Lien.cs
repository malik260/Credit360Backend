namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_CASA_Lien
    {
        [Key]
        public int LienId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductAccountNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string LienReferenceNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal LienCreditAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal LienDebitAmount { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public short SourceTypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        public int CreatedBy { get; set; }
    }
}

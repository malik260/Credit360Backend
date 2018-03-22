namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_SECTOR")]
    public partial class TBL_SECTOR
    {
        [Key]
        public short SECTORID { get; set; }

        [Required]
        [StringLength(200)]
        public string NAME { get; set; }

        [StringLength(10)]
        public string CODE { get; set; }

        [Column(TypeName = "money")]
        public decimal? LOAN_LIMIT { get; set; }
    }
}

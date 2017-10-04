namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Tenor_Mode")]
    public partial class tbl_Tenor_Mode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short TenorModeId { get; set; }

        [Required]
        [StringLength(50)]
        public string TenorModeName { get; set; }
    }
}

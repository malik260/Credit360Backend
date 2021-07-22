namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TENOR_MODE")]
    public partial class TBL_TENOR_MODE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TENORMODEID { get; set; }

        [Required]
        [StringLength(50)]
        public string TENORMODENAME { get; set; }
    }
}

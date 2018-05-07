namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CHARGE_FEE_DETAIL_CLASS")]
    public partial class TBL_CHARGE_FEE_DETAIL_CLASS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short DETAILCLASSID { get; set; }

        [Required]
        [StringLength(50)]
        public string DETAILCLASSNAME { get; set; }
    }
}

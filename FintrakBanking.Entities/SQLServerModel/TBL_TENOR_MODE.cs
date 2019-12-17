namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_TENOR_MODE")]
    public partial class TBL_TENOR_MODE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short TENORMODEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string TENORMODENAME { get; set; }
    }
}

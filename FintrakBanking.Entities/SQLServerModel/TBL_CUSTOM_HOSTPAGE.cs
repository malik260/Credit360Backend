namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOM_HOSTPAGE")]
    public partial class TBL_CUSTOM_HOSTPAGE
    {
        [Key]
        public int HOSTPAGEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string HOSTPAGE { get; set; }

        public int PARENTHOSTPAGEID { get; set; }

        public virtual TBL_CUSTOM_HOSTPAGE TBL_CUSTOM_HOSTPAGE1 { get; set; }

        public virtual TBL_CUSTOM_HOSTPAGE TBL_CUSTOM_HOSTPAGE2 { get; set; }
    }
}

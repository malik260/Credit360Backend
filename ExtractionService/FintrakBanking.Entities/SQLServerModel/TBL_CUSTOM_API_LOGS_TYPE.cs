namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_API_LOGS_TYPE")]
    public partial class TBL_CUSTOM_API_LOGS_TYPE
    {
        [Key]
        public short LOGTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string LOGTYPENAME { get; set; }
    }
}

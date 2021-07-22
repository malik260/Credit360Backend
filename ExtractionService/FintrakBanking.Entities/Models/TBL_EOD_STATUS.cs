namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_EOD_STATUS")]
    public partial class TBL_EOD_STATUS
    {
        [Key]
        public int EODSTATUSID { get; set; }

        [Required]
        public string EODSTATUSNAME { get; set; }
        
    }
}

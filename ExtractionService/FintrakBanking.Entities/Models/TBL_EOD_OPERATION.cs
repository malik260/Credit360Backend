namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_EOD_OPERATION")]
    public partial class TBL_EOD_OPERATION
    {
        [Key]
        public int EODOPERATIONID { get; set; }

        [Required]
        public string EODOPERATIONNAME { get; set; }

        [Required]
        public int POSITION { get; set; }

    }
}

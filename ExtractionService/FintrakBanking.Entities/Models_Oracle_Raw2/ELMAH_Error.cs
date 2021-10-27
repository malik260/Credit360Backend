namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.ELMAH_ERROR")]
    public partial class ELMAH_ERROR
    {
        [Key]
        [StringLength(36)]
        public string ERRORID { get; set; }

        [Required]
        [StringLength(60)]
        public string APPLICATION { get; set; }

        [Required]
        [StringLength(50)]
        public string HOST { get; set; }

        [Required]
        [StringLength(100)]
        public string TYPE { get; set; }

        [Required]
        [StringLength(60)]
        public string SOURCE { get; set; }

        [Required]
        [StringLength(500)]
        public string MESSAGE { get; set; }

        [Required]
        [StringLength(50)]
        public string USER_ { get; set; }

        public int STATUSCODE { get; set; }

        public DateTime TIMEUTC { get; set; }

        public int SEQUENCE { get; set; }

        [Required]
        public string ALLXML { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PUBLIC_HOLIDAY")]
    public partial class TBL_PUBLIC_HOLIDAY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PUBLICHOLIDAYID { get; set; }

        public int COUNTRYID { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public bool ISACTIVE { get; set; }

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }
    }
}

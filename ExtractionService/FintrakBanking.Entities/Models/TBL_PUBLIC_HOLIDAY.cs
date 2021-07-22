namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PUBLIC_HOLIDAY")]
    public partial class TBL_PUBLIC_HOLIDAY
    {
        [Key]
        public int PUBLICHOLIDAYID { get; set; }

        public int COUNTRYID { get; set; }

        //[Column(TypeName = "date")]

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }

        public bool ISACTIVE { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }
    }
}

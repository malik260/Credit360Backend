namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PROFILE_BUSINESS_UNIT")]
    public partial class TBL_PROFILE_BUSINESS_UNIT
    {
        [Key]
        public int BUSINESSUNITID    { get; set; }

        [Required]
        //[StringLength(200)]
        public string BUSINESSUNITNAME { get; set; }
        public string BUSINESSUNITINITIALS { get; set; }
        public string BUSINESSUNITSHORTCODE { get; set; }

        public string BUSINESSCOMMONNAME { get; set; }

    }
}
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_LIEN_PROCESS")]
    public partial class TBL_CUSTOM_LIEN_PROCESS
    {
        [Key]
        public int CUSTOMLIENID { get; set; }

        [StringLength(50)]
        public string LIENREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string ACCOUNTID { get; set; }

        [Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        [StringLength(10)]
        public string CURRENCYCODE { get; set; }

        [StringLength(50)]
        public string LIENTYPE { get; set; }

        [StringLength(50)]
        public string REASONCODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool CONSUMED { get; set; }

        public DateTime? DATETIMECONSUMED { get; set; }
    }
}

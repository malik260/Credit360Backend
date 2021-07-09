namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOM_OVERDRAFTEXTEND")]
    public partial class TBL_CUSTOM_OVERDRAFTEXTEND
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OVERDRAFTEXTENDID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [StringLength(50)]
        public string SANCTIONREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string SANCTIONLIMIT { get; set; }

        [StringLength(50)]
        public string APIURL { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool CONSUMED { get; set; }

        public DateTime? DATETIMECONSUMED { get; set; }

        public string EXPIRYDATE { get; set; }
    }
}

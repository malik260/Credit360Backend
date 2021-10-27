namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_BLACKLIST")]
    public partial class TBL_CUSTOMER_BLACKLIST
    {
        [Key]
        public int CUSTOMER_BLACKLISTID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public DateTime DATEBLACKLISTED { get; set; }

        [Required]
        [StringLength(2000)]
        public string REASON { get; set; }

        public bool ISCURRENT { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}

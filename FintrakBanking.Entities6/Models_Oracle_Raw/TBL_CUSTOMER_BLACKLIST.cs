namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_BLACKLIST")]
    public partial class TBL_CUSTOMER_BLACKLIST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMER_BLACKLISTID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public DateTime DATEBLACKLISTED { get; set; }

        [Required]
        [StringLength(2000)]
        public string REASON { get; set; }

        public int ISCURRENT { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}

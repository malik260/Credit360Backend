namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Blacklist")]
    public partial class tbl_Customer_Blacklist
    {
        [Key]
        public int Customer_BlacklistId { get; set; }

        public int? CompanyId { get; set; }

        public int? CustomerId { get; set; }

        public DateTime? DateBlacklisted { get; set; }

        [StringLength(2000)]
        public string Reason { get; set; }

        public bool? IsCurrent { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}

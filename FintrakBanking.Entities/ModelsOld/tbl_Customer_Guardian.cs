namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Guardian")]
    public partial class tbl_Customer_Guardian
    {
        [Key]
        public int GuardianId { get; set; }

        [StringLength(100)]
        public string GuardianName { get; set; }

        [StringLength(20)]
        public string GuardianPhone { get; set; }

        [StringLength(200)]
        public string GuardianAddress { get; set; }

        public int CustomerId { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}

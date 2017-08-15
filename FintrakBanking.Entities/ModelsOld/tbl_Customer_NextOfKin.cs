namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_NextOfKin")]
    public partial class tbl_Customer_NextOfKin
    {
        [Key]
        public int NextOfKinId { get; set; }

        [Required]
        [StringLength(200)]
        public string NextOfKinName { get; set; }

        [Required]
        [StringLength(200)]
        public string NextOfKinFirstName { get; set; }

        [StringLength(200)]
        public string NextOfKinPhoneNumber { get; set; }

        [StringLength(200)]
        public string NestOfKinEmail { get; set; }

        [Required]
        [StringLength(200)]
        public string NestOfKinAddress { get; set; }

        public int CustomerId { get; set; }

        public bool? Active { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}

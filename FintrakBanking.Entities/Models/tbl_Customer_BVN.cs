namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_BVN")]
    public partial class tbl_Customer_BVN
    {
        [Key]
        public int CustomerBVNId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }

        [Required]
        [StringLength(20)]
        public string BankVerificationNumber { get; set; }

        public bool IsValidBVN { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}

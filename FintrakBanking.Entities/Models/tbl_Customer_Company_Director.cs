namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Company_Director")]
    public partial class tbl_Customer_Company_Director
    {
        [Key]
        public short CompanyDirectorId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }

        public short CompanyDirectorTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string CustomerBVN { get; set; }

        public int NumberOfShares { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        [StringLength(500)]
        public string Others { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateCreated { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Customer_Company_DirectorType tbl_Customer_Company_DirectorType { get; set; }
    }
}

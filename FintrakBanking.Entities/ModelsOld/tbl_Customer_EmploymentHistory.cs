namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_EmploymentHistory")]
    public partial class tbl_Customer_EmploymentHistory
    {
        [Key]
        public int PlaceOfWorkId { get; set; }

        [Required]
        [StringLength(200)]
        public string EmployerName { get; set; }

        [Required]
        [StringLength(200)]
        public string EmployerAddress { get; set; }

        public int EmployerStateId { get; set; }

        public int EmployerCountryId { get; set; }

        [Required]
        [StringLength(200)]
        public string OfficePhone { get; set; }

        [Column(TypeName = "date")]
        public DateTime EmployDate { get; set; }

        [StringLength(200)]
        public string PreviousEmployer { get; set; }

        public int CustomerId { get; set; }

        public bool Active { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}

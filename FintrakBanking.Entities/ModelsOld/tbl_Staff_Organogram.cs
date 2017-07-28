namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Staff_Organogram")]
    public partial class tbl_Staff_Organogram
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StaffId { get; set; }

        [Key]
        [StringLength(50)]
        public string StaffCode { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        [StringLength(50)]
        public string Rank { get; set; }

        [StringLength(50)]
        public string JobTitle { get; set; }

        public short? StaffStatusId { get; set; }

        [StringLength(50)]
        public string ParentStaffCode { get; set; }

        public int? CompanyId { get; set; }

        public DateTime? DateTimeRefreshed { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}

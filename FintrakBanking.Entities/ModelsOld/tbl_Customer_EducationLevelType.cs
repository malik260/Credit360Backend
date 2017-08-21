namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_EducationLevelType")]
    public partial class tbl_Customer_EducationLevelType
    {
        [Key]
        public short EducationLevelTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string EducationLevel { get; set; }
    }
}

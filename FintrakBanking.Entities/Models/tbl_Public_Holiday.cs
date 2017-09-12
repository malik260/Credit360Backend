namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Public_Holiday")]
    public partial class tbl_Public_Holiday
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PublicHolidayId { get; set; }

        public int CountryId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public virtual tbl_Country tbl_Country { get; set; }
    }
}

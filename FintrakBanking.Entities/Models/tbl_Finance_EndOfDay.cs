namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Finance_EndOfDay")]
    public partial class tbl_Finance_EndOfDay
    {
        [Key]
        public int EndOfDayId { get; set; }

        public int CompanyId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public int CreatedBy { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}

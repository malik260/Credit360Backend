namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_FinanceCurrentDate")]
    public partial class tbl_FinanceCurrentDate
    {
        [Key]
        public int FinanceDateId { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}

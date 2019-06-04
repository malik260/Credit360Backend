namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_OFFICER_RATING")]
    public partial class TBL_CORR_OFFICER_RATING
    {
        [Key]
        public int OFFICERRATINGID { get; set; }

        public int STAFFID { get; set; }

        public int BRANCHID { get; set; }

        public int BUSINESSUNITID { get; set; }

        public int BORROWINGCUSTOMERS { get; set; }

        public decimal EXPOSURE { get; set; }

        [Required]
        [StringLength(20)]
        public string CORRCOMMENT { get; set; }

        public int CORRSCORE { get; set; }


        public DateTime DATERATED { get; set; }

        public DateTime FROMDATE { get; set; }

        public DateTime TODATE { get; set; }

    }
}

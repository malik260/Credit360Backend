namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_VISITATION")]
    public partial class TBL_COLLATERAL_VISITATION
    {
        [Key]
        public int COLLATERALVISITATIONID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime VISITATIONDATE { get; set; }

        [Required]
        [StringLength(2000)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}

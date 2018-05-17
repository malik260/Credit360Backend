namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_TYPE_SUB")]
    public partial class TBL_COLLATERAL_TYPE_SUB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALSUBTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string COLLATERALSUBTYPENAME { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public decimal HAIRCUT { get; set; }

        public int REVALUATIONDURATION { get; set; }

        public int? VISITATIONCYCLE { get; set; }

        public int ALLOWSHARING { get; set; }

        public int ISLOCATIONBASED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }
    }
}

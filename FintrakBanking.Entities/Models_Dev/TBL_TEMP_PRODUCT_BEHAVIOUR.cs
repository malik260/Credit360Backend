namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_PRODUCT_BEHAVIOUR")]
    public partial class TBL_TEMP_PRODUCT_BEHAVIOUR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPPRODUCT_BEHAVIOURID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCODE { get; set; }

        public double? COLLATERAL_LCY_LIMIT { get; set; }

        public double? COLLATERAL_FCY_LIMIT { get; set; }

        public decimal? CUSTOMER_LIMIT { get; set; }

        public double? PRODUCT_LIMIT { get; set; }

        public bool? ISINVOICEBASED { get; set; }

        public int? REQUIRECASAACCOUNT { get; set; }

        public bool? ALLOWFUNDUSAGE { get; set; }

        public bool? ISTEMPORARYOVERDRAFT { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }
    }
}

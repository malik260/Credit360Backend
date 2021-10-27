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

        public decimal? COLLATERAL_LCY_LIMIT { get; set; }

        public decimal? COLLATERAL_FCY_LIMIT { get; set; }

        public decimal? CUSTOMER_LIMIT { get; set; }

        public decimal? PRODUCT_LIMIT { get; set; }

        public int? ISINVOICEBASED { get; set; }

        public int? REQUIRECASAACCOUNT { get; set; }

        public int? ALLOWFUNDUSAGE { get; set; }

        public int? ISTEMPORARYOVERDRAFT { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }
    }
}

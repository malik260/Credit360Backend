namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CASA_OVERDRAFT")]
    public partial class TBL_CASA_OVERDRAFT
    {
        [Key]
        public int OVERDRAFTID { get; set; }

        public int CASAACCOUNTID { get; set; }

        [Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal CREDITAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal DEBITAMOUNT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        public int CREATEDBY { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }
    }
}

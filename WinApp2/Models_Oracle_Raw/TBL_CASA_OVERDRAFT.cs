namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CASA_OVERDRAFT")]
    public partial class TBL_CASA_OVERDRAFT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OVERDRAFTID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public decimal CREDITAMOUNT { get; set; }

        public decimal DEBITAMOUNT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime? DATECREATED { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_PRICEINDEX_EXCEPTION")]
    public partial class TBL_LOAN_PRICEINDEX_EXCEPTION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANPRICEEXCEPTIONID { get; set; }

        public int LOANID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
    }
}

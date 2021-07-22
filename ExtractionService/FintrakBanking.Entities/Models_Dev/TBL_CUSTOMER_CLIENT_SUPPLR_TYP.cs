namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_CLIENT_SUPPLR_TYP")]
    public partial class TBL_CUSTOMER_CLIENT_SUPPLR_TYP
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CLIENT_SUPPLIERTYPEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string CLIENT_SUPPLIERTYPENAME { get; set; }
    }
}

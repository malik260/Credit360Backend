namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_KYC_DOCUMENTTYPE")]
    public partial class TBL_KYC_DOCUMENTTYPE
    {
        [Key]
        public int DOCUMENTTYPEID { get; set; }

        [StringLength(100)]
        public string DOCUMENTTYPENAME { get; set; }
    }
}

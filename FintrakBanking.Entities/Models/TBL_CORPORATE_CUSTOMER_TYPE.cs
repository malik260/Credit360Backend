

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORPORATE_CUSTOMER_TYPE")]
    public partial class TBL_CORPORATE_CUSTOMER_TYPE
    {
        [Key]
        public int CORPORATECUSTOMERTYPEID { get; set; }
        public string CORPORATECUSTOMERTYPENAME { get; set; }
    }
}

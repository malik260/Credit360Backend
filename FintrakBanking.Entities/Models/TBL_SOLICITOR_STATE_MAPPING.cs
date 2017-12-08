namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_SOLICITOR_STATE_MAPPING")]
    public partial class TBL_SOLICITOR_STATE_MAPPING
    {
        [Key]
        public int SOLICITORSTATEID { get; set; }

        public int SOLICITORID { get; set; }

        public int STATEID { get; set; }

        [Column(TypeName = "money")]
        public decimal COLLATERALSEARCHCHARGEAMOUNT { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }

        public virtual TBL_SOLICITOR TBL_SOLICITOR { get; set; }
    }
}

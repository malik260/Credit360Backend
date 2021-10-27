namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_RISK_RATING")]
    public partial class TBL_CUSTOMER_RISK_RATING
    {
        public TBL_CUSTOMER_RISK_RATING()
        {
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RISKRATINGID { get; set; }

        [Required]
        [StringLength(200)]
        public string RISKRATING { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public int ISINVESTMENTGRADE { get; set; }

        public decimal MAX_SHAREHOLDER_FUND_PERCENTAG { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }
    }
}

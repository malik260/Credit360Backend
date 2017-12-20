namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_RISK_RATING")]
    public partial class TBL_CUSTOMER_RISK_RATING
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_RISK_RATING()
        {
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RISKRATINGID { get; set; }

        [Required]
        [StringLength(200)]
        public string RISKRATING { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public double MAX_SHAREHOLDER_FUND_PERCENTAG { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_RISK_RATING")]
    public partial class TBL_CUSTOMER_RISK_RATING
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_RISK_RATING()
        {
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RISKRATINGID { get; set; }

        [Required]
        //[StringLength(200)]
        public string RISKRATING { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(500)]
        public string DESCRIPTION { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public double MAX_SHAREHOLDER_FUND_PERCENTAG { get; set; }
        public string CLASSIFICATION { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        //public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }
        // TBL_CUSTOMER_RISK_RATING
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CREDIT_BUREAU")]
    public partial class TBL_CREDIT_BUREAU
    {
        public TBL_CREDIT_BUREAU()
        {
            TBL_CUSTOMER_CREDIT_BUREAU = new HashSet<TBL_CUSTOMER_CREDIT_BUREAU>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREDITBUREAUID { get; set; }

        [Required]
        [StringLength(300)]
        public string CREDITBUREAUNAME { get; set; }

        public decimal CORPORATE_CHARGEAMOUNT { get; set; }

        public decimal INDIVIDUAL_CHARGEAMOUNT { get; set; }

        public int INUSE { get; set; }

        public int GLACCOUNTID { get; set; }

        public int ISMANDATORY { get; set; }

        public int USEINTEGRATION { get; set; }

        [StringLength(50)]
        public string USERNAME { get; set; }

        [StringLength(50)]
        public string PASSWORD { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual ICollection<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
    }
}

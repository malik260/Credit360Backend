namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FEE")]
    public partial class TBL_FEE
    {
        public TBL_FEE()
        {
            TBL_TEMP_PRODUCT_FEE = new HashSet<TBL_TEMP_PRODUCT_FEE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FEEID { get; set; }

        [Required]
        [StringLength(150)]
        public string FEENAME { get; set; }

        public short ACCOUNTCATEGORYID { get; set; }

        public int FEETYPEID { get; set; }

        public int FEEINTERVALID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public int FEETARGETID { get; set; }

        public int GLACCOUNTID { get; set; }

        public int? FEEAMORTISATIONTYPEID { get; set; }

        public int ISINTEGRALFEE { get; set; }

        public int INCLUDECUTOFFDAY { get; set; }

        public int? CUTOFFDAY { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? FEEDATE { get; set; }

        public virtual TBL_ACCOUNT_CATEGORY TBL_ACCOUNT_CATEGORY { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_FEE_AMORTISATION_TYPE TBL_FEE_AMORTISATION_TYPE { get; set; }

        public virtual TBL_FEE_INTERVAL TBL_FEE_INTERVAL { get; set; }

        public virtual TBL_FEE_TARGET TBL_FEE_TARGET { get; set; }

        public virtual TBL_FEE_TYPE TBL_FEE_TYPE { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_FEE> TBL_TEMP_PRODUCT_FEE { get; set; }
    }
}

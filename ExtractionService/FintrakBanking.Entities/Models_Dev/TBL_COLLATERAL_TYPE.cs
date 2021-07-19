namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_TYPE")]
    public partial class TBL_COLLATERAL_TYPE
    {
        public TBL_COLLATERAL_TYPE()
        {
            TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
            TBL_COLLATERAL_TYPE_SUB = new HashSet<TBL_COLLATERAL_TYPE_SUB>();
            TBL_COLLATERAL_VALUEBASE_TYPE = new HashSet<TBL_COLLATERAL_VALUEBASE_TYPE>();
            TBL_TEMP_PRODUCT_COLLATERALTYP = new HashSet<TBL_TEMP_PRODUCT_COLLATERALTYP>();
            TBL_PRODUCT_COLLATERALTYPE = new HashSet<TBL_PRODUCT_COLLATERALTYPE>();
            TBL_TEMP_COLLATERAL_CUSTOMER = new HashSet<TBL_TEMP_COLLATERAL_CUSTOMER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALTYPEID { get; set; }

        [Required]
        [StringLength(250)]
        public string COLLATERALTYPENAME { get; set; }

        public int COMPANYID { get; set; }

        [StringLength(500)]
        public string DETAILS { get; set; }

        public bool REQUIREINSURANCEPOLICY { get; set; }

        public bool REQUIREVISITATION { get; set; }

        public int? CHARGEGLACCOUNTID { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_COLLATERAL_TYPE_SUB> TBL_COLLATERAL_TYPE_SUB { get; set; }

        public virtual ICollection<TBL_COLLATERAL_VALUEBASE_TYPE> TBL_COLLATERAL_VALUEBASE_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_COLLATERALTYP> TBL_TEMP_PRODUCT_COLLATERALTYP { get; set; }

        public virtual ICollection<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}

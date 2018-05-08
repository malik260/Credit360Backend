namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CHARGE_FEE")]
    public partial class TBL_TEMP_CHARGE_FEE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_TEMP_CHARGE_FEE()
        {
            TBL_TEMP_CHARGE_FEE_DETAIL = new HashSet<TBL_TEMP_CHARGE_FEE_DETAIL>();
        }

        [Key]
        public int TEMPCHARGEFEEID { get; set; }

        [Required]
        [StringLength(150)]
        public string CHARGEFEENAME { get; set; }

        public short ACCOUNTCATEGORYID { get; set; }

        public short FEEINTERVALID { get; set; }

        public short? PRODUCTTYPEID { get; set; }

        public short FEETARGETID { get; set; }

        public int GLACCOUNTID { get; set; }

        public short? FEEAMORTISATIONTYPEID { get; set; }

        public bool ISINTEGRALFEE { get; set; }

        public bool INCLUDECUTOFFDAY { get; set; }

        public short? CUTOFFDAY { get; set; }

        public int? OPERATIONID { get; set; }

        [Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        public double? RATE { get; set; }

        public short FEETYPEID { get; set; }

        public bool? RECURRING { get; set; }

        public int? PRIMARYTAXID { get; set; }

        public int? SECONDARYTAXID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int? CHARGEFEEID { get; set; }

        public int? TEMP_ROOT_ID { get; set; }

        public bool ISUPDATESTATUS { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_FEE_AMORTISATION_TYPE TBL_FEE_AMORTISATION_TYPE { get; set; }

        public virtual TBL_FEE_INTERVAL TBL_FEE_INTERVAL { get; set; }

        public virtual TBL_FEE_TARGET TBL_FEE_TARGET { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_TAX TBL_TAX { get; set; }

        public virtual TBL_TAX TBL_TAX1 { get; set; }

        public virtual TBL_ACCOUNT_CATEGORY TBL_ACCOUNT_CATEGORY { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHARGE_FEE_DETAIL> TBL_TEMP_CHARGE_FEE_DETAIL { get; set; }
    }
}

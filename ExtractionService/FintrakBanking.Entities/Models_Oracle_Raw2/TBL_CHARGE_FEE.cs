namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHARGE_FEE")]
    public partial class TBL_CHARGE_FEE
    {
        public TBL_CHARGE_FEE()
        {
            TBL_CHARGE_FEE_DETAIL = new HashSet<TBL_CHARGE_FEE_DETAIL>();
            TBL_CHARGE_RANGE = new HashSet<TBL_CHARGE_RANGE>();
            TBL_CUSTOMER_PRODUCT_FEE = new HashSet<TBL_CUSTOMER_PRODUCT_FEE>();
            TBL_LOAN_APPLICATION_DETL_FEE = new HashSet<TBL_LOAN_APPLICATION_DETL_FEE>();
            TBL_LOAN_FEE = new HashSet<TBL_LOAN_FEE>();
            TBL_LOAN_FEE_ARCHIVE = new HashSet<TBL_LOAN_FEE_ARCHIVE>();
            TBL_PRODUCT_CHARGE_FEE = new HashSet<TBL_PRODUCT_CHARGE_FEE>();
            TBL_TEMP_PRODUCT_CHARGE_FEE = new HashSet<TBL_TEMP_PRODUCT_CHARGE_FEE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHARGEFEEID { get; set; }

        [Required]
        [StringLength(150)]
        public string CHARGEFEENAME { get; set; }

        public int FEEINTERVALID { get; set; }

        public int? PRODUCTTYPEID { get; set; }

        public int FEETARGETID { get; set; }

        public int? FEEAMORTISATIONTYPEID { get; set; }

        public int ISINTEGRALFEE { get; set; }

        public int INCLUDECUTOFFDAY { get; set; }

        public int? CUTOFFDAY { get; set; }

        public int? OPERATIONID { get; set; }

        public decimal? AMOUNT { get; set; }

        public decimal? RATE { get; set; }

        public int FEETYPEID { get; set; }

        public int? RECURRING { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_FEE_TYPE TBL_FEE_TYPE { get; set; }

        public virtual TBL_FEE_TARGET TBL_FEE_TARGET { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_FEE_AMORTISATION_TYPE TBL_FEE_AMORTISATION_TYPE { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }

        public virtual TBL_FEE_INTERVAL TBL_FEE_INTERVAL { get; set; }

        public virtual ICollection<TBL_CHARGE_RANGE> TBL_CHARGE_RANGE { get; set; }

        public virtual ICollection<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_FEE> TBL_LOAN_APPLICATION_DETL_FEE { get; set; }

        public virtual ICollection<TBL_LOAN_FEE> TBL_LOAN_FEE { get; set; }

        public virtual ICollection<TBL_LOAN_FEE_ARCHIVE> TBL_LOAN_FEE_ARCHIVE { get; set; }

        public virtual ICollection<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }
    }
}

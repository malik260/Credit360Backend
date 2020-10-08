namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_REVIEW_OPERATION")]
    public partial class TBL_LOAN_REVIEW_OPERATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_REVIEW_OPERATION()
        {
            TBL_LOAN_REVIEW_OPRATN_IREG_SC = new HashSet<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
        }

        [Key]
        public int LOANREVIEWOPERATIONID { get; set; }

        public int? LOANREVIEWAPPLICATIONID { get; set; }
        public short? MATURITYINSTRUCTIONTYPEID { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int OPERATIONTYPEID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        public DateTime? OPERATIONDATE { get; set; }

        public int? PREPAYMENTMETHODID { get; set; }

        [Required]
        public string REVIEWDETAILS { get; set; }

        public double? INTERATERATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal? PREPAYMENT { get; set; }

        public int? PRINCIPALFREQUENCYTYPEID { get; set; }

        public int? INTERESTFREQUENCYTYPEID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? PRINCIPALFIRSTPAYMENTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? INTERESTFIRSTPAYMENTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? MATURITYDATE { get; set; }

        public int? TENOR { get; set; }

        public int? CASA_ACCOUNTID { get; set; }

        //[Column(TypeName = "money")]
        public decimal? OVERDRAFTTOPUP { get; set; }

        //[Column(TypeName = "money")]
        public decimal? FEE_CHARGES { get; set; }

        public short? SCHEDULETYPEID { get; set; }

        public short? SCHEDULEDAYCOUNTCONVENTIONID { get; set; }

        public short? SCHEDULEDAYINTERESTTYPEID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public bool ISMANAGEMENTINTERESTRATE { get; set; }

        public bool OPERATIONCOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        //[Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }
        public DateTime? REBOOKDATE { get; set; }

        //[StringLength(20)] 
        public string LEGALCONTINGENTCODE { get; set; }
        public decimal? CONTINGENTOUTSTANDINGPRINCIPAL { get; set; }

        public bool ISMARKETINDUCED { get; set; }

        public int? PRODUCTPRICEINDEXGLOBALID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVIEW_OPRATN_IREG_SC> TBL_LOAN_REVIEW_OPRATN_IREG_SC { get; set; }
        public int? TARGETID { get; set; }
        public bool ISPRINTED { get; set; }

        //public short? FEETYPEID { get; set; }

    }
}

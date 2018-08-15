namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVIEW_OPERATION")]
    public partial class TBL_LOAN_REVIEW_OPERATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_REVIEW_OPERATION()
        {
            TBL_LOAN_REVIEW_OPRATN_IREG_SC = new HashSet<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
        }

        [Key]
        public int LOANREVIEWOPERATIONID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int OPERATIONTYPEID { get; set; }

        [Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        [Required]
        public string REVIEWDETAILS { get; set; }

        public double? INTERATERATE { get; set; }

        [Column(TypeName = "money")]
        public decimal? PREPAYMENT { get; set; }

        public int? PRINCIPALFREQUENCYTYPEID { get; set; }

        public int? INTERESTFREQUENCYTYPEID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PRINCIPALFIRSTPAYMENTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime? INTERESTFIRSTPAYMENTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime? MATURITYDATE { get; set; }

        public int? TENOR { get; set; }

        public int? CASA_ACCOUNTID { get; set; }

        [Column(TypeName = "money")]
        public decimal? OVERDRAFTTOPUP { get; set; }

        [Column(TypeName = "money")]
        public decimal? FEE_CHARGES { get; set; }

        public short? SCHEDULETYPEID { get; set; }

        public short? SCHEDULEDAYCOUNTCONVENTIONID { get; set; }

        public short? SCHEDULEDAYINTERESTTYPEID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public bool ISMANAGEMENTINTERESTRATE { get; set; }

        public bool OPERATIONCOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVIEW_OPRATN_IREG_SC> TBL_LOAN_REVIEW_OPRATN_IREG_SC { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVIEW_APPLICATION")]
    public partial class TBL_LOAN_REVIEW_APPLICATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_REVIEW_APPLICATION()
        {
            TBL_LOAN_REVIEW_APPLICATN_CAM = new HashSet<TBL_LOAN_REVIEW_APPLICATN_CAM>();
        }

        [Key]
        public int LOANREVIEWAPPLICATIONID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public short BRANCHID { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        public string REVIEWDETAILS { get; set; }
        /*
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


        public bool ISMANAGEMENTINTERESTRATE { get; set; }*/

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }
    }
}

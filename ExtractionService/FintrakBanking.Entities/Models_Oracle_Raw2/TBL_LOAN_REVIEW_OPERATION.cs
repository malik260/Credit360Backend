namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_REVIEW_OPERATION")]
    public partial class TBL_LOAN_REVIEW_OPERATION
    {
        public TBL_LOAN_REVIEW_OPERATION()
        {
            TBL_LOAN_REVIEW_OPRATN_IREG_SC = new HashSet<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANREVIEWOPERATIONID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int OPERATIONTYPEID { get; set; }

        [Required]
        public string REVIEWDETAILS { get; set; }

        public decimal? INTERATERATE { get; set; }

        public decimal? PREPAYMENT { get; set; }

        public int? PRINCIPALFREQUENCYTYPEID { get; set; }

        public int? INTERESTFREQUENCYTYPEID { get; set; }

        public int? TENOR { get; set; }

        public int? CASA_ACCOUNTID { get; set; }

        public decimal? OVERDRAFTTOPUP { get; set; }

        public decimal? FEE_CHARGES { get; set; }

        public int? SCHEDULETYPEID { get; set; }

        public int? SCHEDULEDAYCOUNTCONVENTIONID { get; set; }

        public int? SCHEDULEDAYINTERESTTYPEID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int ISMANAGEMENTINTERESTRATE { get; set; }

        public int OPERATIONCOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public DateTime? MATURITYDATE { get; set; }

        public DateTime? INTERESTFIRSTPAYMENTDATE { get; set; }

        public DateTime? PRINCIPALFIRSTPAYMENTDATE { get; set; }

        public DateTime EFFECTIVEDATE { get; set; }

        public virtual ICollection<TBL_LOAN_REVIEW_OPRATN_IREG_SC> TBL_LOAN_REVIEW_OPRATN_IREG_SC { get; set; }
    }
}

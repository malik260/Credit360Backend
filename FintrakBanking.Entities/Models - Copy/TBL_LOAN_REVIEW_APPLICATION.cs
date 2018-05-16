namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_REVIEW_APPLICATION")]
    public partial class TBL_LOAN_REVIEW_APPLICATION
    {
        public TBL_LOAN_REVIEW_APPLICATION()
        {
            TBL_LOAN_REVIEW_APPLICATN_CAM = new HashSet<TBL_LOAN_REVIEW_APPLICATN_CAM>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANREVIEWAPPLICATIONID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int BRANCHID { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        public string REVIEWDETAILS { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime? DATECREATED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}

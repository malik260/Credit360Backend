namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVIEW_APPLICATN_CAM")]
    public partial class TBL_LOAN_REVIEW_APPLICATN_CAM
    {
        [Key]
        public int LOANREVIEWCAMID { get; set; }

        public int LOANREVIEWAPPLICATIONID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CAMREF { get; set; }

        public bool ISCOMPLETED { get; set; }

        public bool RISKRATED { get; set; }

        public string DOCUMENTATION { get; set; }

        public int APPROVALLEVELID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_LOAN_REVIEW_APPLICATION TBL_LOAN_REVIEW_APPLICATION { get; set; }
    }
}

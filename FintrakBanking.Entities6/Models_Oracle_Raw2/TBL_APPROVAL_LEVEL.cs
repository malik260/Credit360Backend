namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_LEVEL")]
    public partial class TBL_APPROVAL_LEVEL
    {
        public TBL_APPROVAL_LEVEL()
        {
            TBL_APPROVAL_LEVEL_STAFF = new HashSet<TBL_APPROVAL_LEVEL_STAFF>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL1 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_CHECKLIST_TYPE_APROV_LEVL = new HashSet<TBL_CHECKLIST_TYPE_APROV_LEVL>();
            TBL_CREDIT_APPRAISAL_MEMO_DOCU = new HashSet<TBL_CREDIT_APPRAISAL_MEMO_DOCU>();
            TBL_CREDIT_TEMPLATE = new HashSet<TBL_CREDIT_TEMPLATE>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_REVIEW_APPLICATN_CAM = new HashSet<TBL_LOAN_REVIEW_APPLICATN_CAM>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALLEVELID { get; set; }

        [Required]
        [StringLength(150)]
        public string LEVELNAME { get; set; }

        public int GROUPID { get; set; }

        public int? STAFFROLEID { get; set; }

        public int POSITION { get; set; }

        public int? TENOR { get; set; }

        public decimal MAXIMUMAMOUNT { get; set; }

        public decimal? INVESTMENTGRADEAMOUNT { get; set; }

        public int NUMBEROFUSERS { get; set; }

        public int NUMBEROFAPPROVALS { get; set; }

        public int SLAINTERVAL { get; set; }

        public decimal? FEERATE { get; set; }

        public decimal? INTERESTRATE { get; set; }

        public int ISPOLITICALLYEXPOSED { get; set; }

        public int ISACTIVE { get; set; }

        public int CANVIEWDOCUMENT { get; set; }

        public int CANVIEWUPLOAD { get; set; }

        public int CANVIEWAPPROVAL { get; set; }

        public int CANAPPROVE { get; set; }

        public int CANUPLOAD { get; set; }

        public int CANEDIT { get; set; }

        public int CANRECIEVEEMAIL { get; set; }

        public int CANRECIEVESMS { get; set; }

        public int CANESCALATE { get; set; }

        public int CANAPPROVEUNTENORED { get; set; }

        public int CANRESOLVEDISPUTE { get; set; }

        public int ROUTEVIASTAFFORGANOGRAM { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_GROUP TBL_APPROVAL_GROUP { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }

        public virtual ICollection<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL1 { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual ICollection<TBL_CHECKLIST_TYPE_APROV_LEVL> TBL_CHECKLIST_TYPE_APROV_LEVL { get; set; }

        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMO_DOCU> TBL_CREDIT_APPRAISAL_MEMO_DOCU { get; set; }

        public virtual ICollection<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }
    }
}

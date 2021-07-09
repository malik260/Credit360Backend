namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_APPROVAL_LEVEL")]
    public partial class TBL_APPROVAL_LEVEL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_APPROVAL_LEVEL()
        {
            TBL_APPROVAL_LEVEL_STAFF = new HashSet<TBL_APPROVAL_LEVEL_STAFF>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL1 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_CHECKLIST_TYPE_APROV_LEVL = new HashSet<TBL_CHECKLIST_TYPE_APROV_LEVL>();
            TBL_CREDIT_APPRAISAL_MEMO_DOCU = new HashSet<TBL_CREDIT_APPRAISAL_MEMO_DOCU>();
            TBL_CREDIT_TEMPLATE = new HashSet<TBL_CREDIT_TEMPLATE>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_LOAN_REVIEW_APPLICATN_CAM = new HashSet<TBL_LOAN_REVIEW_APPLICATN_CAM>();
        }

        [Key]
        public int APPROVALLEVELID { get; set; }

        [Required]
        [StringLength(150)]
        public string LEVELNAME { get; set; }

        public int GROUPID { get; set; }

        public int? STAFFROLEID { get; set; }

        public int POSITION { get; set; }

        public int? TENOR { get; set; }

        //[Column(TypeName = "money")]
        public decimal MAXIMUMAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal? INVESTMENTGRADEAMOUNT { get; set; }

        public int NUMBEROFUSERS { get; set; }

        public int NUMBEROFAPPROVALS { get; set; }

        public int SLAINTERVAL { get; set; }

        public double? FEERATE { get; set; }

        public double? INTERESTRATE { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISACTIVE { get; set; }

        public bool CANVIEWDOCUMENT { get; set; }

        public bool CANVIEWUPLOAD { get; set; }

        public bool CANVIEWAPPROVAL { get; set; }

        public bool CANAPPROVE { get; set; }

        public bool CANUPLOAD { get; set; }

        public bool CANEDIT { get; set; }

        public bool CANRECIEVEEMAIL { get; set; }

        public bool CANRECIEVESMS { get; set; }

        public bool CANESCALATE { get; set; }

        public bool CANAPPROVEUNTENORED { get; set; }

        public bool CANRESOLVEDISPUTE { get; set; }

        public bool ROUTEVIASTAFFORGANOGRAM { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_GROUP TBL_APPROVAL_GROUP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_TYPE_APROV_LEVL> TBL_CHECKLIST_TYPE_APROV_LEVL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMO_DOCU> TBL_CREDIT_APPRAISAL_MEMO_DOCU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }
    }
}

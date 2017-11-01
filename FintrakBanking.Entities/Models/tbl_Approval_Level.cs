namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_APPROVAL_LEVEL")]
    public partial class TBL_APPROVAL_LEVEL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_APPROVAL_LEVEL()
        {
            TBL_APPROVAL_LEVEL_STAFF = new HashSet<TBL_APPROVAL_LEVEL_STAFF>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL1 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT = new HashSet<TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT>();
            TBL_CREDIT_TEMPLATE = new HashSet<TBL_CREDIT_TEMPLATE>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
        }

        [Key]
        public int APPROVALLEVELID { get; set; }

        [Required]
        [StringLength(150)]
        public string LEVELNAME { get; set; }

        public int GROUPID { get; set; }

        public int POSITION { get; set; }

        public int? TENOR { get; set; }

        [Column(TypeName = "money")]
        public decimal MAXIMUMAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal? INVESTMENTGRADEAMOUNT { get; set; }

        public int NUMBEROFUSERS { get; set; }

        public int NUMBEROFAPPROVALS { get; set; }

        public int SLAINTERVAL { get; set; }

        public bool CANROUTEBACK { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISACTIVE { get; set; }

        public bool CANEDIT { get; set; }

        public bool CANDORISKASSESSMENT { get; set; }

        public bool CANRECIEVEADJUSTMENT { get; set; }

        public bool CANRECIEVEEMAIL { get; set; }

        public bool CANRECIEVESMS { get; set; }

        public bool HASCHECKLIST { get; set; }

        public bool CANPERFORMFINANCIALANALYSIS { get; set; }

        public bool REQUIREAUTHORISATION { get; set; }

        public bool CANOVERIDEAUTHORISATION { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT> TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }
    }
}

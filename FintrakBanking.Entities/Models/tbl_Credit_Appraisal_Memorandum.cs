namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CREDIT_APPRAISAL_MEMORANDUM")]
    public partial class TBL_CREDIT_APPRAISAL_MEMORANDUM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CREDIT_APPRAISAL_MEMORANDUM()
        {
            TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT = new HashSet<TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT>();
            TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL = new HashSet<TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL>();
        }

        [Key]
        public int APPRAISALMEMORANDUMID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CAMREF { get; set; }

        public bool ISCOMPLETED { get; set; }

        public bool RISKRATED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT> TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL> TBL_CREDIT_APPRAISAL_MEMORANDUM_LOAN_DETAIL { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}

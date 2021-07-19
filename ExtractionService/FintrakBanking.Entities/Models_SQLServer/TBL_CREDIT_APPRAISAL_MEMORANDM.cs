namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CREDIT_APPRAISAL_MEMORANDM")]
    public partial class TBL_CREDIT_APPRAISAL_MEMORANDM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CREDIT_APPRAISAL_MEMORANDM()
        {
            TBL_CREDIT_APPRAISAL_MEMO_DETL = new HashSet<TBL_CREDIT_APPRAISAL_MEMO_DETL>();
            TBL_CREDIT_APPRAISAL_MEMO_DOCU = new HashSet<TBL_CREDIT_APPRAISAL_MEMO_DOCU>();
            TBL_CREDIT_APPRAISAL_MEMO_LOG = new HashSet<TBL_CREDIT_APPRAISAL_MEMO_LOG>();
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
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMO_DETL> TBL_CREDIT_APPRAISAL_MEMO_DETL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMO_DOCU> TBL_CREDIT_APPRAISAL_MEMO_DOCU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMO_LOG> TBL_CREDIT_APPRAISAL_MEMO_LOG { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}

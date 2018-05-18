namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_OPERATIONS")]
    public partial class TBL_OPERATIONS
    {
        public TBL_OPERATIONS()
        {
            TBL_APPROVAL_GROUP_MAPPING = new HashSet<TBL_APPROVAL_GROUP_MAPPING>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            TBL_CHARGES = new HashSet<TBL_CHARGES>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_CUSTOM_FIANCE_TRANSACTION = new HashSet<TBL_CUSTOM_FIANCE_TRANSACTION>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_REVIEW_APPLICATION = new HashSet<TBL_LOAN_REVIEW_APPLICATION>();
            TBL_MESSAGE_LOG = new HashSet<TBL_MESSAGE_LOG>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OPERATIONID { get; set; }

        [Required]
        [StringLength(150)]
        public string OPERATIONNAME { get; set; }

        public int OPERATIONTYPEID { get; set; }

        public int TERMINATEIFDISAPPROVED { get; set; }

        [StringLength(300)]
        public string OPERATIONURL { get; set; }

        public int ISDISABLED { get; set; }

        public virtual ICollection<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_CHARGES> TBL_CHARGES { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual ICollection<TBL_CUSTOM_FIANCE_TRANSACTION> TBL_CUSTOM_FIANCE_TRANSACTION { get; set; }

        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATION> TBL_LOAN_REVIEW_APPLICATION { get; set; }

        public virtual ICollection<TBL_MESSAGE_LOG> TBL_MESSAGE_LOG { get; set; }

        public virtual TBL_OPERATIONS_TYPE TBL_OPERATIONS_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
    }
}

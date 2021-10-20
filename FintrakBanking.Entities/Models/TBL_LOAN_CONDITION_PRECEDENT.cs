namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_CONDITION_PRECEDENT")]
    public partial class TBL_LOAN_CONDITION_PRECEDENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_CONDITION_PRECEDENT()
        {
            TBL_LOAN_CONDITION_DEFERRAL = new HashSet<TBL_LOAN_CONDITION_DEFERRAL>();
        }

        [Key]
        public int LOANCONDITIONID { get; set; }

        public int? CONDITIONID { get; set; }

        [Required]
        //[StringLength(1000)]
        public string CONDITION { get; set; }

        public bool ISEXTERNAL { get; set; }

        public bool ISSUBSEQUENT { get; set; }

        public int CREATEDBY { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public short RESPONSE_TYPEID { get; set; }

        public short? CHECKLISTSTATUSID { get; set; }

        public bool? CHECKLISTVALIDATED { get; set; }
      

        //[Column(TypeName = "date")]
        public DateTime? DEFEREDDATE { get; set; }
        public int? DEFEREDDAYS { get; set; }
        public short APPROVALSTATUSID { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? TIMELINEID { get; set; }
        public int? SECTORID { get; set; }
        public int? SUBSECTORID { get; set; }

        //public bool DELETED { get; set; }


        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHECKLIST_RESPONSE_TYPE TBL_CHECKLIST_RESPONSE_TYPE { get; set; }

        public virtual TBL_CHECKLIST_STATUS TBL_CHECKLIST_STATUS { get; set; }

        public virtual TBL_CONDITION_PRECEDENT TBL_CONDITION_PRECEDENT { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_DEFERRAL> TBL_LOAN_CONDITION_DEFERRAL { get; set; }
    }
}

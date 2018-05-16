namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_CONDITION_PRECEDENT")]
    public partial class TBL_LOAN_CONDITION_PRECEDENT
    {
        public TBL_LOAN_CONDITION_PRECEDENT()
        {
            TBL_LOAN_CONDITION_DEFERRAL = new HashSet<TBL_LOAN_CONDITION_DEFERRAL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANCONDITIONID { get; set; }

        public int? CONDITIONID { get; set; }

        [Required]
        [StringLength(1000)]
        public string CONDITION { get; set; }

        public int ISEXTERNAL { get; set; }

        public int ISSUBSEQUENT { get; set; }

        public int CREATEDBY { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int RESPONSE_TYPEID { get; set; }

        public int? CHECKLISTSTATUSID { get; set; }

        public int? CHECKLISTVALIDATED { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? TIMELINEID { get; set; }

        public DateTime? DEFEREDDATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHECKLIST_RESPONSE_TYPE TBL_CHECKLIST_RESPONSE_TYPE { get; set; }

        public virtual TBL_CHECKLIST_STATUS TBL_CHECKLIST_STATUS { get; set; }

        public virtual TBL_CONDITION_PRECEDENT TBL_CONDITION_PRECEDENT { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual ICollection<TBL_LOAN_CONDITION_DEFERRAL> TBL_LOAN_CONDITION_DEFERRAL { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CONDITION_PRECEDENT")]
    public partial class TBL_CONDITION_PRECEDENT
    {
        public TBL_CONDITION_PRECEDENT()
        {
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONDITIONID { get; set; }

        [Required]
        [StringLength(1000)]
        public string CONDITION { get; set; }

        public int ISEXTERNAL { get; set; }

        public int ISSUBSEQUENT { get; set; }

        public int CORPORATE { get; set; }

        public int RETAIL { get; set; }

        public int? PRODUCTID { get; set; }

        public int RESPONSE_TYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? TIMELINEID { get; set; }

        public virtual TBL_CHECKLIST_RESPONSE_TYPE TBL_CHECKLIST_RESPONSE_TYPE { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }
    }
}

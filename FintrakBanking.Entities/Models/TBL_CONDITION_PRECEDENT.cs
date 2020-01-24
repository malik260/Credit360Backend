namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CONDITION_PRECEDENT")]
    public partial class TBL_CONDITION_PRECEDENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CONDITION_PRECEDENT()
        {
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
        }

        [Key]
        public int CONDITIONID { get; set; }

        [Required]
        //[StringLength(1000)]
        public string CONDITION { get; set; }

        public bool ISEXTERNAL { get; set; }

        public bool ISSUBSEQUENT { get; set; }

        public bool CORPORATE { get; set; }

        public bool RETAIL { get; set; }

        public short? PRODUCTID { get; set; }

        public short RESPONSE_TYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? TIMELINEID { get; set; }

        public bool DELETED { get; set; }

        public int? OPERATIONID { get; set; }

        public int? SECTORID { get; set; }
        public int? SUBSECTORID { get; set; }

        public virtual TBL_CHECKLIST_RESPONSE_TYPE TBL_CHECKLIST_RESPONSE_TYPE { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }
    }
}

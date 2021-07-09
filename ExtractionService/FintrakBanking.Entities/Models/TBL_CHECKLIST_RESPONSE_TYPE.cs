namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHECKLIST_RESPONSE_TYPE")]
    public partial class TBL_CHECKLIST_RESPONSE_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHECKLIST_RESPONSE_TYPE()
        {
            TBL_CHECKLIST_ITEM = new HashSet<TBL_CHECKLIST_ITEM>();
            TBL_CHECKLIST_STATUS = new HashSet<TBL_CHECKLIST_STATUS>();
            TBL_CONDITION_PRECEDENT = new HashSet<TBL_CONDITION_PRECEDENT>();
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RESPONSE_TYPEID { get; set; }

        [Required]
        //[StringLength(160)]
        public string RESPONSE_TYPE_NAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_ITEM> TBL_CHECKLIST_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_STATUS> TBL_CHECKLIST_STATUS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CONDITION_PRECEDENT> TBL_CONDITION_PRECEDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }
    }
}

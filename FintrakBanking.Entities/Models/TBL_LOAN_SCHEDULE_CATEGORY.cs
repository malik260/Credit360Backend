namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_SCHEDULE_CATEGORY")]
    public partial class TBL_LOAN_SCHEDULE_CATEGORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_SCHEDULE_CATEGORY()
        {
            TBL_LOAN_SCHEDULE_TYPE = new HashSet<TBL_LOAN_SCHEDULE_TYPE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SCHEDULECATEGORYID { get; set; }

        //[StringLength(50)]
        public string SCHEDULECATEGORYNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_SCHEDULE_TYPE> TBL_LOAN_SCHEDULE_TYPE { get; set; }
    }
}

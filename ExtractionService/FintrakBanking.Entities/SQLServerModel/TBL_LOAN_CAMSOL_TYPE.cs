namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_CAMSOL_TYPE")]
    public partial class TBL_LOAN_CAMSOL_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_CAMSOL_TYPE()
        {
            TBL_LOAN_CAMSOL = new HashSet<TBL_LOAN_CAMSOL>();
            TBL_TEMP_LOAN_CAMSOL = new HashSet<TBL_TEMP_LOAN_CAMSOL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CAMSOLTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CAMSOLTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN_CAMSOL> TBL_TEMP_LOAN_CAMSOL { get; set; }
    }
}

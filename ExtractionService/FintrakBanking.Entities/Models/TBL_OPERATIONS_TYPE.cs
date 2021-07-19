namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_OPERATIONS_TYPE")]
    public partial class TBL_OPERATIONS_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_OPERATIONS_TYPE()
        {
            TBL_OPERATIONS = new HashSet<TBL_OPERATIONS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short OPERATIONTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string OPERATIONTYPENAME { get; set; }

        public short? BINDINGTYPEID { get; set; }

        public bool? INUSE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_OPERATIONS> TBL_OPERATIONS { get; set; }
    }
}

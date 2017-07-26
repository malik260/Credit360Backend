namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_IdentificationModeType")]
    public partial class tbl_Customer_IdentificationModeType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_IdentificationModeType()
        {
            tbl_Customer_Identification = new HashSet<tbl_Customer_Identification>();
        }

        [Key]
        public int IdentificationModeId { get; set; }

        [Required]
        [StringLength(50)]
        public string IdentificationMode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Identification> tbl_Customer_Identification { get; set; }
    }
}

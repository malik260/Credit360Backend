namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Daily_Accural_Category")]
    public partial class tbl_Daily_Accural_Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Daily_Accural_Category()
        {
            tbl_Daily_Accural = new HashSet<tbl_Daily_Accural>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Daily_Accural> tbl_Daily_Accural { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Financial_Statement_Type")]
    public partial class tbl_Financial_Statement_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Financial_Statement_Type()
        {
            tbl_Customer_FS_Caption = new HashSet<tbl_Customer_FS_Caption>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FSTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public string FSTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption> tbl_Customer_FS_Caption { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Company_DirectorType")]
    public partial class tbl_Customer_Company_DirectorType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_Company_DirectorType()
        {
            tbl_Customer_Company_Director = new HashSet<tbl_Customer_Company_Director>();
        }

        [Key]
        public short CompanyDirectoryTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyDirectoryTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Company_Director> tbl_Customer_Company_Director { get; set; }
    }
}

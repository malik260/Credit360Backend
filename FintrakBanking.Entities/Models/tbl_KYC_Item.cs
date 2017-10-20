namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_KYC_Item")]
    public partial class tbl_KYC_Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_KYC_Item()
        {
            tbl_Customer_Account_KYC_Item = new HashSet<tbl_Customer_Account_KYC_Item>();
        }

        [Key]
        public short KYCItemId { get; set; }

        public short? ProductId { get; set; }

        public int DisplayOrder { get; set; }

        [Required]
        [StringLength(500)]
        public string Item { get; set; }

        public bool IsMandatory { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Account_KYC_Item> tbl_Customer_Account_KYC_Item { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }
    }
}

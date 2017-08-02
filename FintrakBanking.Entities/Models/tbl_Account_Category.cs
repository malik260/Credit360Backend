namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Account_Category")]
    public partial class tbl_Account_Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Account_Category()
        {
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
            tbl_Customer_FS_Caption = new HashSet<tbl_Customer_FS_Caption>();
            tbl_Fee = new HashSet<tbl_Fee>();
            tbl_Account_Type = new HashSet<tbl_Account_Type>();
            tbl_Financial_Statement_Caption = new HashSet<tbl_Financial_Statement_Caption>();
        }

        [Key]
        public short AccountCategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountCategoryName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption> tbl_Customer_FS_Caption { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Account_Type> tbl_Account_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Financial_Statement_Caption> tbl_Financial_Statement_Caption { get; set; }
    }
}

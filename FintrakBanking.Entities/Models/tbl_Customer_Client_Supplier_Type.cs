namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Client_Supplier_Type")]
    public partial class tbl_Customer_Client_Supplier_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_Client_Supplier_Type()
        {
            tbl_Customer_Client_Supplier = new HashSet<tbl_Customer_Client_Supplier>();
        }

        [Key]
        public short Client_SupplierTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Client_SupplierTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Client_Supplier> tbl_Customer_Client_Supplier { get; set; }
    }
}

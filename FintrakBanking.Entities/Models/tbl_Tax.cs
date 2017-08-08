namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Tax")]
    public partial class tbl_Tax
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Tax()
        {
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
            tbl_Charge_Fee1 = new HashSet<tbl_Charge_Fee>();
            tbl_Temp_Charge_Fee = new HashSet<tbl_Temp_Charge_Fee>();
            tbl_Temp_Charge_Fee1 = new HashSet<tbl_Temp_Charge_Fee>();
        }

        [Key]
        public int TaxId { get; set; }

        [Required]
        [StringLength(150)]
        public string TaxName { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        public double? Rate { get; set; }

        public int GLAccountId { get; set; }

        public bool? UseAmount { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee1 { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee1 { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Account_Type")]
    public partial class tbl_Account_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Account_Type()
        {
            tbl_Chart_Of_Account = new HashSet<tbl_Chart_Of_Account>();
            tbl_Temp_Chart_Of_Account = new HashSet<tbl_Temp_Chart_Of_Account>();
        }

        [Key]
        public int AccountTypeId { get; set; }

        public int AccountTypeCode { get; set; }

        [Required]
        [StringLength(100)]
        public string AccountTypeName { get; set; }

        public short AccountCategoryId { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Account_Category tbl_Account_Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Chart_Of_Account> tbl_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Chart_Of_Account> tbl_Temp_Chart_Of_Account { get; set; }
    }
}

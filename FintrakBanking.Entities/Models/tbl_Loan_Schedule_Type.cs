namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Schedule_Type")]
    public partial class tbl_Loan_Schedule_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Schedule_Type()
        {
            tbl_Product = new HashSet<tbl_Product>();
            tbl_Loan = new HashSet<tbl_Loan>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ScheduleTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string ScheduleTypeName { get; set; }

        public short ScheduleCategoryId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product> tbl_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        public virtual tbl_Loan_Schedule_Category tbl_Loan_Schedule_Category { get; set; }
    }
}

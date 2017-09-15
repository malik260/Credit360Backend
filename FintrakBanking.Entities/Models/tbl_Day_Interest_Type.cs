namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Day_Interest_Type")]
    public partial class tbl_Day_Interest_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Day_Interest_Type()
        {
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan = new HashSet<tbl_Loan>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short DayInterestTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string DayInterestTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }
    }
}

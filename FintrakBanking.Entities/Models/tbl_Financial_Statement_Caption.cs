namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Financial_Statement_Caption")]
    public partial class tbl_Financial_Statement_Caption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Financial_Statement_Caption()
        {
            tbl_Chart_Of_Account = new HashSet<tbl_Chart_Of_Account>();
        }

        [Key]
        public short FSCaptionId { get; set; }

        [Required]
        [StringLength(50)]
        public string FSCaptionCode { get; set; }

        [Required]
        [StringLength(200)]
        public string FSCaption { get; set; }

        public int Position { get; set; }

        [StringLength(50)]
        public string RefNote { get; set; }

        [Required]
        [StringLength(20)]
        public string FinType { get; set; }

        public short? AccountCategoryId { get; set; }

        public int? ParentId { get; set; }

        public bool IsTotalLine { get; set; }

        [StringLength(50)]
        public string CaptionColor { get; set; }

        public double? Multiplier { get; set; }

        public virtual tbl_Account_Category tbl_Account_Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Chart_Of_Account> tbl_Chart_Of_Account { get; set; }
    }
}

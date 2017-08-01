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
        [Key]
        [Column(Order = 0)]
        public int TaxId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(150)]
        public string TaxName { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        public double? Rate { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GLAccountId { get; set; }

        public bool? UseAmount { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyId { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [Key]
        [Column(Order = 6)]
        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Charge_Fee")]
    public partial class tbl_Charge_Fee
    {
        [Key]
        [Column(Order = 0)]
        public int ChargeFeeId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(150)]
        public string ChargeFeeName { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AccountCategoryId { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeTypeId { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeIntervalId { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ProductTypeId { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeTargetId { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GLAccountId { get; set; }

        public short? FeeAmortisationTypeId { get; set; }

        [Key]
        [Column(Order = 8)]
        public bool IsIntegralFee { get; set; }

        [Key]
        [Column(Order = 9)]
        public bool IncludeCutOffDay { get; set; }

        public short? CutOffDay { get; set; }

        [Key]
        [Column(Order = 10, TypeName = "date")]
        public DateTime FeeDate { get; set; }

        [Key]
        [Column(Order = 11)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyId { get; set; }

        [Key]
        [Column(Order = 12)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        [Key]
        [Column(Order = 13)]
        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [Key]
        [Column(Order = 14)]
        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Fee_Amortisation_Type tbl_Fee_Amortisation_Type { get; set; }

        public virtual tbl_Fee_Interval tbl_Fee_Interval { get; set; }

        public virtual tbl_Fee_Target tbl_Fee_Target { get; set; }

        public virtual tbl_Fee_Type tbl_Fee_Type { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Account_Category tbl_Account_Category { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }
    }
}

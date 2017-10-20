namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class view_Approval_Setup
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GroupId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(150)]
        public string GroupName { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyId { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ApprovalLevelId { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(150)]
        public string LevelName { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Position { get; set; }

        [Key]
        [Column(Order = 6, TypeName = "money")]
        public decimal LevelMaximumAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal? InvestmentGradeAmount { get; set; }

        public int? Tenor { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StaffId { get; set; }

        [Key]
        [Column(Order = 8, TypeName = "money")]
        public decimal StaffMaximumAmount { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string StaffCode { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(50)]
        public string StaffFirstName { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(50)]
        public string StaffLastName { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(50)]
        public string Username { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OperationId { get; set; }

        [Key]
        [Column(Order = 14)]
        [StringLength(150)]
        public string OperationName { get; set; }
    }
}

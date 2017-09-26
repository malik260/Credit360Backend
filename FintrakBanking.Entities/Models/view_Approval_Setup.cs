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
        public int StaffLevelId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StaffId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ApprovalLevelId { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "money")]
        public decimal MaximumAmount { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(50)]
        public string StaffCode { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(50)]
        public string LastName { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(50)]
        public string Username { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(150)]
        public string LevelName { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(150)]
        public string GroupName { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(150)]
        public string OperationName { get; set; }

        [Key]
        [Column(Order = 11)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OperationId { get; set; }

        [Key]
        [Column(Order = 12)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LevelPosition { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GroupPosition { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; }

        [StringLength(50)]
        public string ProductName { get; set; }

        [StringLength(50)]
        public string ProductClassName { get; set; }

        public short? ProductId { get; set; }

        public short? ProductClassId { get; set; }

        [Key]
        [Column(Order = 14, TypeName = "money")]
        public decimal MinimumAmount { get; set; }

        [Key]
        [Column(Order = 15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NumberOfUsers { get; set; }

        [Key]
        [Column(Order = 16)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NumberOfApprovals { get; set; }
    }
}

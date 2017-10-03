namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Call_Memo")]
    public partial class tbl_Call_Memo
    {
        [Key]
        public int CallMemoId { get; set; }

        public int LoanApplicationId { get; set; }

        public int StaffId { get; set; }

        public short CallLimitTypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime MemoDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NextCallDate { get; set; }

        [Required]
        public string Purpose { get; set; }

        public string Discusion { get; set; }

        public string Summary { get; set; }

        [Required]
        [StringLength(500)]
        public string Action { get; set; }

        [StringLength(500)]
        public string Recommendation { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Call_Memo_Type tbl_Call_Memo_Type { get; set; }
    }
}

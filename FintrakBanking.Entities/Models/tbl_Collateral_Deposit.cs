namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Deposit")]
    public partial class tbl_Collateral_Deposit
    {
        [Key]
        public int TermDepositTranAccId { get; set; }

        public int ColleralCustomerId { get; set; }

<<<<<<< HEAD
=======
        public short? CollateralSubTypeId { get; set; }

        [StringLength(50)]
        public string DealReferenceNumber { get; set; }

        [Required]
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba
        [StringLength(50)]
        public string AccountType { get; set; }

        [StringLength(50)]
        public string AccountNo { get; set; }

        [Column(TypeName = "money")]
<<<<<<< HEAD
        public decimal? AccountBalance { get; set; }

        [StringLength(5)]
        public string Contribution { get; set; }

        public DateTime? MaturityDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }
=======
        public decimal ExistingLienAmount { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

        [Column(TypeName = "money")]
        public decimal LienAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        public DateTime MaturityDate { get; set; }

        [Column(TypeName = "money")]
        public decimal MaturityAmount { get; set; }

<<<<<<< HEAD
        public DateTime? DateTimeDeleted { get; set; }
=======
        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Collateral_Type_Sub tbl_Collateral_Type_Sub { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba
    }
}

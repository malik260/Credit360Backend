namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_CAMSOL")]
    public partial class TBL_LOAN_CAMSOL
    {
        [Key]
        public int LOAN_CAMSOLID { get; set; }
        [Required]
        public int COMPANYID { get; set; }
        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }
        public int? LOANID { get; set; }
        [Required]  
        public decimal BALANCE { get; set; }

        public int LOANSYSTEMTYPEID { get; set; }

        public string CUSTOMERNAME { get; set; }

        public decimal PRINCIPAL { get; set; }

        public decimal INTERESTINSUSPENSE { get; set; }

        public int CAMSOLTYPEID { get; set; }

        public string ACCOUNTNUMBER { get; set; }

        public string ACCOUNTNAME { get; set; }

        public string REMARK { get; set; }
        [Required]
        public bool CANTAKELOAN { get; set; }

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }
        [Required]
        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
        [Required]
        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
        public decimal AMOUNTAFFECTED { get; set; }
        public string TYPE { get; set; }
    }
}

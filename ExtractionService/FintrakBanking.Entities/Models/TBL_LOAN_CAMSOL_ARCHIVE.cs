namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_CAMSOL_ARCHIVE")]
    public partial class TBL_LOAN_CAMSOL_ARCHIVE
    {

        [Key]
        public int ARCHIVELOAN_CAMSOLID { get; set; }

        public int LOAN_CAMSOLID { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        [Required]
        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }
        public int? LOANID { get; set; }
        public decimal BALANCE { get; set; }

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        public DateTime ARCHIVEDATE { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }
        public string CUSTOMERNAME { get; set; }
        public decimal PRINCIPAL { get; set; }
        public decimal INTERESTINSUSPENSE { get; set; }
        public int CAMSOLTYPEID { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string ACCOUNTNAME { get; set; }
        public string REMARK { get; set; }
        public bool CANTAKELOAN { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }

        public decimal WRITTENOFFACCRUALAMOUNT { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

    }
}

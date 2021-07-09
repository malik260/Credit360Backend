namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_LOAN_CAMSOL")]
    public partial class TBL_TEMP_LOAN_CAMSOL
    {
        [Key]
        public int TEMPLOAN_CAMSOLID { get; set; }

        public int LOAN_CAMSOLID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public int? LOANID { get; set; }

        [Column(TypeName = "money")]
        public decimal BALANCE { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public short? LOANSYSTEMTYPEID { get; set; }

        [Required]
        //[StringLength(300)]
        public string CUSTOMERNAME { get; set; }

        [Column(TypeName = "money")]
        public decimal PRINCIPAL { get; set; }

        [Column(TypeName = "money")]
        public decimal INTERESTINSUSPENSE { get; set; }

        public short CAMSOLTYPEID { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        //[StringLength(300)]
        public string ACCOUNTNAME { get; set; }

        //[StringLength(300)]
        public string REMARK { get; set; }

        public bool CANTAKELOAN { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_LOAN_CAMSOL_TYPE TBL_LOAN_CAMSOL_TYPE { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}

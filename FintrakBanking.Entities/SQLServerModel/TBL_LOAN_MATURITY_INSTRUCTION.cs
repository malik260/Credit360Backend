namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_MATURITY_INSTRUCTION")]
    public partial class TBL_LOAN_MATURITY_INSTRUCTION
    {
        [Key]
        public int MATURITYINSTRUCTIONID { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int TENOR { get; set; }

        public short INSTRUCTIONTYPEID { get; set; }

        public bool ISUSED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_LOAN_MATURITY_INSTRU_TYPE TBL_LOAN_MATURITY_INSTRU_TYPE { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }
    }
}

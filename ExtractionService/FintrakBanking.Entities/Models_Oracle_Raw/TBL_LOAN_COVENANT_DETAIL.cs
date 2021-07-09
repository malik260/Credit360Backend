namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_COVENANT_DETAIL")]
    public partial class TBL_LOAN_COVENANT_DETAIL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANCOVENANTDETAILID { get; set; }

        [Required]
        [StringLength(2000)]
        public string COVENANTDETAIL { get; set; }

        public int LOANID { get; set; }

        public int LOANSYSTEMTYPEID { get; set; }

        public int COVENANTTYPEID { get; set; }

        public int? FREQUENCYTYPEID { get; set; }

        public decimal? COVENANTAMOUNT { get; set; }

        public int ISPERCENTAGE { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? NEXTCOVENANTDATE { get; set; }

        public DateTime? COVENANTDATE { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }

        public virtual TBL_LOAN_COVENANT_TYPE TBL_LOAN_COVENANT_TYPE { get; set; }
    }
}

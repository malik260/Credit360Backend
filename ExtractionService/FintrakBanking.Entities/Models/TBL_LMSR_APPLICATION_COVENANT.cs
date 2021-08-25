namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LMSR_APPLICATION_COVENANT")]
    public partial class TBL_LMSR_APPLICATION_COVENANT
    {
        [Key]
        public int LOANCOVENANTDETAILID { get; set; }

        public int LOANREVIEWAPPLICATIONID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string COVENANTDETAIL { get; set; }

        public short COVENANTTYPEID { get; set; }

        public short? FREQUENCYTYPEID { get; set; }

        //[Column(TypeName = "money")]
        public decimal? COVENANTAMOUNT { get; set; }

        public bool ISPERCENTAGE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime COVENANTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? NEXTCOVENANTDATE { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_LMSR_APPLICATION_DETAIL TBL_LMSR_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_COVENANT_TYPE TBL_LOAN_COVENANT_TYPE { get; set; }
    }
}

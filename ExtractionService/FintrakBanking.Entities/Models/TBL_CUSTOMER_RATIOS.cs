namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_RATIOS")]
    public partial class TBL_CUSTOMER_RATIOS
    {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]

        [Key]
        public int CUSTOMERRATIOID { get; set; }

        [Required]
        //[StringLength(50)]financial_Period
        public string DESCRIPTION { get; set; }
        public string VALUE { get; set; }
        public string FINANCIALPERIOD { get; set; }

        public int CUSTOMERID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int? CUSTOMERGROUPID { get; set; }
        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        //public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? CATEGORYID { get; set; }
        public int? CATEGORYIDFI { get; set; }
        public DateTime? COMPILATIONDATE { get; set; }
        public string AUDITORNAME { get; set; }

    }
}

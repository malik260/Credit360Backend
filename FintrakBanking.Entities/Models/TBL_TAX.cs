namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TAX")]
    public partial class TBL_TAX
    {
        [Key]
        public int TAXID { get; set; }

        [Required]
        //[StringLength(150)]
        public string TAXNAME { get; set; }

        //[Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        public double? RATE { get; set; }

        public int GLACCOUNTID { get; set; }

        public bool? USEAMOUNT { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }
    }
}

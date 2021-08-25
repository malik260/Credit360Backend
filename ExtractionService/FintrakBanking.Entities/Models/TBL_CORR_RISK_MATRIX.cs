namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_RISK_MATRIX")]
    public partial class TBL_CORR_RISK_MATRIX
    {
        [Key]
        public int RISKMATRIXID { get; set; }

        //[StringLength(20)]
        public string RATING { get; set; }

        public int GRADINGMINIMUM { get; set; }

        public int GRADINGMAXIMUM { get; set; }

        //[StringLength(20)]
        public string DESCRIPTION { get; set; }

        public int RISKTYPEID { get; set; }


    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_ACCREDITEDCONSL_STATE")]
    public partial class TBL_TEMP_ACCREDITEDCONSULTANT_STATE
    {
        [Key]
        public int TEMP_CONSULT_STATE_COVREDID { get; set; }

        public int STATEID { get; set; }

        public int TEMPACCREDITEDCONSULTANTID { get; set; }

    }
}

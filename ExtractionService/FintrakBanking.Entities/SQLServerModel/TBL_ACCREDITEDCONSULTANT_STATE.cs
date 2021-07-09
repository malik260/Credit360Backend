namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_ACCREDITEDCONSULTANT_STATE")]
    public partial class TBL_ACCREDITEDCONSULTANT_STATE
    {
        [Key]
        public int CONSULT_STATE_COVREDID { get; set; }

        public int STATEID { get; set; }

        public int ACCREDITEDCONSULTANTID { get; set; }

        public virtual TBL_ACCREDITEDCONSULTANT TBL_ACCREDITEDCONSULTANT { get; set; }
    }
}

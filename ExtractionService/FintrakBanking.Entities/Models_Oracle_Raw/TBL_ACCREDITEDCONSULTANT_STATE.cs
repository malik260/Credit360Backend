namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_ACCREDITEDCONSULTANT_STATE")]
    public partial class TBL_ACCREDITEDCONSULTANT_STATE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONSULT_STATE_COVREDID { get; set; }

        public int STATEID { get; set; }

        public int ACCREDITEDCONSULTANTID { get; set; }

        public virtual TBL_ACCREDITEDCONSULTANT TBL_ACCREDITEDCONSULTANT { get; set; }
    }
}

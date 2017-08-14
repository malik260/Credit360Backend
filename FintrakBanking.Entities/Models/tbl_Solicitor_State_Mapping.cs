namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Solicitor_State_Mapping")]
    public partial class tbl_Solicitor_State_Mapping
    {
        [Key]
        public int SolicitorStateId { get; set; }

        public int SolicitorId { get; set; }

        public int StateId { get; set; }

        [Column(TypeName = "money")]
        public decimal CollateralSearchChargeAmount { get; set; }

        public virtual tbl_State tbl_State { get; set; }

        public virtual tbl_Solicitor tbl_Solicitor { get; set; }
    }
}

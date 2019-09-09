namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_AUTHORISED_SIGNATORY
    {
        [Key]
        public int SIGNATORYID { get; set; }

        public string SIGNATORYNAME { get; set; }
        public string SIGNATORYTITLE { get; set; }
        public string SIGNATORYINITIALS { get; set; }
    }
}

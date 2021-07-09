namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_FINANCECURRENTDATE")]
    public partial class TBL_FINANCECURRENTDATE
    {
        [Key]
        public int FINANCEDATEID { get; set; }

        public int COMPANYID { get; set; }

        public DateTime CURRENTDATE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}

using System;
using System.Collections.Generic;
namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_CURRENCY_TBL")]
    public partial class STG_CURRENCY_TBL
    {
        [Key]
        //[StringLength(3)]
        public string CURRENCY { get; set; }
        //[StringLength(20)]
        public string CURRENCY_NAME { get; set; }
        //[StringLength(5)]
        public string COUNTRY_CODE { get; set; }
        public int POINTS { get; set; }
        public string ENTITY_CRE_FLAG { get; set; }

    }
}




namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_SETUP_GLOBAL")]
    public partial class TBL_SETUP_GLOBAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short APPLICATIONSETUPID { get; set; }

        //[StringLength(500)]
        public string REPORTPATH { get; set; }

        public bool USE_ACTIVE_DIRECTORY { get; set; }

        //[StringLength(100)]
        public string ACTIVE_DIRECTORY_DOMAIN_NAME { get; set; }

        //[StringLength(50)]
        public string ACTIVE_DIRECTORY_USERNAME { get; set; }

        //[StringLength(50)]
        public string ACTIVE_DIRECTORY_PASSWORD { get; set; }

        public bool REQUIRE_ADUSER { get; set; }

        public bool USE_THIRD_PARTY_INTEGRATION { get; set; }

        public int MAXIMUMUPLOADFILESIZE { get; set; }
    }
}

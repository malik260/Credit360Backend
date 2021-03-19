namespace FintrakBanking.Entities.StagingModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_TEAM
    {
        [Key]

        //[StringLength(20)]
        public int TEAMID { get; set; }

        //[StringLength(10)]
        public string TEAMKEY { get; set; }

        //[StringLength(20)]
        public string YEAR { get; set; }

        //[StringLength(50)]
        public string ACCOUNTOFFICERCODE { get; set; }

        //[StringLength(50)]
        public string ACCOUNTOFFICERNAME { get; set; }

        //[StringLength(50)]
        public string TEAMCODE { get; set; }

        //[StringLength(20)]
        public string TEAMNAME { get; set; }

        //[StringLength(200)]
        public string BRANCHCODE { get; set; }
        public string BRANCHNAME { get; set; }

        //[StringLength(500)]
        public string ZONECODE { get; set; }

        //[StringLength(500)]
        public string ZONENAME { get; set; }

        //[StringLength(10)]
        public string UNITCODE { get; set; }

        [Required]
        //[StringLength(10)]
        public string UNITNAME { get; set; }

        public string GEOGRAPHIALCODE { get; set; }

        //[StringLength(10)]
        public string GEOGRAPHIALNAME { get; set; }

        public string DIVISIONCODE { get; set; }

        //[StringLength(20)]
        public string DIVISIONNAME { get; set; }

        //[StringLength(500)]
        public string REGIONCODE { get; set; }

        //[StringLength(20)]
        public string REGIONNAME { get; set; }
        public string STAFFID { get; set; }
        public string  SECTOR_CODE { get; set; }
        public string SECTORNAME { get; set; }
        public string SRATEGY_CODE { get; set; }
        public string STRATEGYNAME { get; set; }
        public string SUPERSEGMENT_CODE { get; set; }
        public string SUPERSEGMENTNAME { get; set; }
        public int INSERTAUDITKEY { get; set; }
        public int UPDATEAUDITKEY { get; set; }
    }
}

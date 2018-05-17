namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOM_TEMPORARYOVERDRAFT")]
    public partial class TBL_CUSTOM_TEMPORARYOVERDRAFT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPORARYOVERDRAFTID { get; set; }

        [StringLength(50)]
        public string TEMPORARYOVERDRAFTFLAG { get; set; }

        [StringLength(50)]
        public string TEMPORARYOVERDRAFTAMOUNT { get; set; }

        [StringLength(50)]
        public string TEMPORARYOVERDRAFTNARATION { get; set; }

        [StringLength(50)]
        public string APIURL { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int CONSUMED { get; set; }

        public DateTime? DATETIMECONSUMED { get; set; }

        public DateTime? TEMPORARYOVERDRAFTDATE { get; set; }
    }
}

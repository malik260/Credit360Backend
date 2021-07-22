namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHARGES")]
    public partial class TBL_CHARGES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CHARGEID { get; set; }

        [StringLength(50)]
        public string CHARGENAME { get; set; }

        public int? OPERATIONID { get; set; }

        public decimal? SETVALUE { get; set; }

        public int? GLACCOUNTID { get; set; }

        public int? FREQUENCY { get; set; }

        public int? APPLYVAT { get; set; }

        public int? APPLYWHT { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}

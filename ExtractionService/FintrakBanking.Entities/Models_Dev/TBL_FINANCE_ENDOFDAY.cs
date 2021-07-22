namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FINANCE_ENDOFDAY")]
    public partial class TBL_FINANCE_ENDOFDAY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ENDOFDAYID { get; set; }

        public int COMPANYID { get; set; }

        public DateTime STARTDATETIME { get; set; }

        public DateTime? ENDDATETIME { get; set; }

        public int CREATEDBY { get; set; }

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}

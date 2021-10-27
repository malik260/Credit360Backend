namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CHARGES_VALUESOURCE")]
    public partial class TBL_CHARGES_VALUESOURCE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VALUESOURCEID { get; set; }

        [Required]
        [StringLength(50)]
        public string VALUESOURCENAME { get; set; }

        public int ISFIXED { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}

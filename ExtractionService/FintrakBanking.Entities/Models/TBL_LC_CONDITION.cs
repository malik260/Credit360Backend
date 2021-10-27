namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LC_CONDITION")]
    public partial class TBL_LC_CONDITION
    {
        [Key]
        public int LCCONDITIONID { get; set; }

        public int LCISSUANCEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string CONDITION { get; set; }

        public bool ISSATISFIED { get; set; }
        public bool ISTRANSACTIONDYNAMICS { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LC_ISSUANCE TBL_LC_ISSUANCE { get; set; }

    }
}
        /*


            

        */

namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_RELATED_PARTY")]
    public partial class TBL_CUSTOMER_RELATED_PARTY
    {
        [Key]
        public int RELATEDPARTYID { get; set; }

        public int COMPANYDIRECTORID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        //[StringLength(200)]
        public string RELATIONSHIPTYPE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY_DIRECTOR TBL_COMPANY_DIRECTOR { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}

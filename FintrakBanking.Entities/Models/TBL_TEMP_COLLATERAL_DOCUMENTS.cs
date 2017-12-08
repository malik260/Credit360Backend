namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_DOCUMENTS")]
    public partial class TBL_TEMP_COLLATERAL_DOCUMENTS
    {
        [Key]
        public long DOCUMENTID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(100)]
        public string DOCUMENTCATEGORY { get; set; }

        [Required]
        [StringLength(500)]
        public string DOCUMENTREF { get; set; }

        [Required]
        [StringLength(100)]
        public string DOCUMENTCODE { get; set; }

        [StringLength(100)]
        public string DOCUMENTTYPE { get; set; }

        public bool ISMANDATORY { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_GROUP_MAPPING")]
    public partial class TBL_APPROVAL_GROUP_MAPPING
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GROUPOPERATIONMAPPINGID { get; set; }

        public int OPERATIONID { get; set; }

        public int GROUPID { get; set; }

        public short? PRODUCTCLASSID { get; set; }

        public short? PRODUCTID { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_GROUP TBL_APPROVAL_GROUP { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}

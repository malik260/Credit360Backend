using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_TEMP_APPROVAL_GRP_MAPPING")]
    public partial class TBL_TEMP_APPROVAL_GRP_MAPPING
    {
        [Key]
        public int TEMPGROUPOPERATIONMAPPINGID { get; set; }

        public int GROUPOPERATIONMAPPINGID { get; set; }

        public int OPERATIONID { get; set; }

        public int GROUPID { get; set; }

        public short? PRODUCTCLASSID { get; set; }

        public short? PRODUCTID { get; set; }

        public int POSITION { get; set; }

        public bool ALLOWMULTIPLEINITIATOR { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public string OPERATION { get; set; }

        public bool ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_GROUP TBL_APPROVAL_GROUP { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }


    }
}

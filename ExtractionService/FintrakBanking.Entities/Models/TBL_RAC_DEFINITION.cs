namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_DEFINITION")]
    public partial class TBL_RAC_DEFINITION
    {
        public TBL_RAC_DEFINITION()
        {
            TBL_RAC_DETAIL = new HashSet<TBL_RAC_DETAIL>();
        }

        [Key]
        public int RACDEFINITIONID { get; set; }

        public int RACITEMID { get; set; }

        public int? PRODUCTID { get; set; }
        public int? PRODUCTCLASSID { get; set; }

        public int RACCATEGORYID { get; set; }

        public bool ISACTIVE { get; set; }

        public bool ISREQUIRED { get; set; }

        public int RACINPUTTYPEID { get; set; }

        public int? RACOPTIONID { get; set; }

        public int CONDITIONALOPERATORID { get; set; }

        public int DEFINEDFUNCTIONID { get; set; }

        public bool REQUIREUPLOAD { get; set; }

        public int? OPERATIONID { get; set; }

        public int? APPROVALLEVELID { get; set; }

        public int? RACCATEGORYTYPEID { get; set; }

        public bool? SHOWATDRAWDOWN { get; set; }

        public int? ROLEID { get; set; }

        public decimal? CONTROLAMOUNT { get; set; }
        public decimal? CONTROLAMOUNTMAX { get; set; }
        public int? CONTROLOPTIONID { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public bool? REQUIRECOMMENT { get; set; }
        public short? CURRENCYID { get; set; }
        public string CURRENCYTYPE { get; set; }
        public bool? ISRACTIERCONTROLKEY { get; set; }
        public string SEARCHPLACEHOLDER { get; set; }
        public string EMPLOYMENTTYPE { get; set; }
        public short? CUSTOMERTYPEID { get; set; }
        public virtual ICollection<TBL_RAC_DETAIL> TBL_RAC_DETAIL { get; set; }

        public virtual TBL_RAC_INPUT_TYPE TBL_RAC_INPUT_TYPE { get; set; }

        public virtual TBL_RAC_ITEM TBL_RAC_ITEM { get; set; }

    }
}

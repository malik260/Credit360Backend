namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_DOC_TEMPLATE_DETAIL")]
    public partial class TBL_DOC_TEMPLATE_DETAIL
    {
        [Key]
        public int DOCUMENTDETAILID { get; set; }

        public int OPERATIONID { get; set; }

        public int TARGETID { get; set; }

        public int TEMPLATESECTIONID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string TITLE { get; set; }

        [Required]
        public string TEMPLATEDOCUMENT { get; set; }

        public int POSITION { get; set; }

        public bool CANEDIT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_DOC_TEMPLATE_SECTION TBL_DOC_TEMPLATE_SECTION { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}

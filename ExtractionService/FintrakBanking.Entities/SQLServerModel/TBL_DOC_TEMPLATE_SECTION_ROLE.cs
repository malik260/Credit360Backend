namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_DOC_TEMPLATE_SECTION_ROLE")]
    public partial class TBL_DOC_TEMPLATE_SECTION_ROLE
    {
        [Key]
        public int SECTIONROLEID { get; set; }

        public int TEMPLATESECTIONID { get; set; }

        public int STAFFROLEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_DOC_TEMPLATE_SECTION TBL_DOC_TEMPLATE_SECTION { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}

namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_DOC_TEMPLATE_SECTION")]
    public partial class TBL_DOC_TEMPLATE_SECTION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_DOC_TEMPLATE_SECTION()
        {
            TBL_DOC_TEMPLATE_DETAIL = new HashSet<TBL_DOC_TEMPLATE_DETAIL>();
            TBL_DOC_TEMPLATE_SECTION_ROLE = new HashSet<TBL_DOC_TEMPLATE_SECTION_ROLE>();
        }

        [Key]
        public int TEMPLATESECTIONID { get; set; }

        public int TEMPLATEID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string TITLE { get; set; }

        [Required]
        public string TEMPLATEDOCUMENT { get; set; }

        public int POSITION { get; set; }

        public bool ISDISABLED { get; set; }

        public bool CANEDIT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_DOC_TEMPLATE TBL_DOC_TEMPLATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DOC_TEMPLATE_DETAIL> TBL_DOC_TEMPLATE_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DOC_TEMPLATE_SECTION_ROLE> TBL_DOC_TEMPLATE_SECTION_ROLE { get; set; }
    }
}

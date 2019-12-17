namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_DOC_TEMPLATE")]
    public partial class TBL_DOC_TEMPLATE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_DOC_TEMPLATE()
        {
            TBL_DOC_TEMPLATE_SECTION = new HashSet<TBL_DOC_TEMPLATE_SECTION>();
        }

        [Key]
        public int TEMPLATEID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        //[StringLength(100)]
        public string TEMPLATENAME { get; set; }

        public int STAFFROLEID { get; set; }

        public int OPERATIONID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DOC_TEMPLATE_SECTION> TBL_DOC_TEMPLATE_SECTION { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}

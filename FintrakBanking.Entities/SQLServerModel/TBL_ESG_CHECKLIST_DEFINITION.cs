namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_ESG_CHECKLIST_DEFINITION")]
    public partial class TBL_ESG_CHECKLIST_DEFINITION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_ESG_CHECKLIST_DEFINITION()
        {
            TBL_ESG_CHECKLIST_DETAIL = new HashSet<TBL_ESG_CHECKLIST_DETAIL>();
        }

        [Key]
        public int ESGCHECKLISTDEFINITIONID { get; set; }

        public int CHECKLISTITEMID { get; set; }

        public int ESGCATEGORYID { get; set; }

        public int? ESGSUBCATEGORYID { get; set; }

        public bool ISCOMPULSORY { get; set; }

        //[StringLength(800)]
        public string ITEMDESCRIPTION { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHECKLIST_ITEM TBL_CHECKLIST_ITEM { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_ESG_CATEGORY TBL_ESG_CATEGORY { get; set; }

        public virtual TBL_ESG_SUB_CATEGORY TBL_ESG_SUB_CATEGORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_ESG_CHECKLIST_DETAIL> TBL_ESG_CHECKLIST_DETAIL { get; set; }
    }
}

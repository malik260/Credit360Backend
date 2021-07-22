namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHECKLIST_DEFINITION")]
    public partial class TBL_CHECKLIST_DEFINITION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHECKLIST_DEFINITION()
        {
            TBL_CHECKLIST_DETAIL = new HashSet<TBL_CHECKLIST_DETAIL>();
        }

        [Key]
        public int CHECKLISTDEFINITIONID { get; set; }

        public short? PRODUCTID { get; set; }

        public int? APPROVALLEVELID { get; set; }

        public int CHECKLISTITEMID { get; set; }

        public int OPERATIONID { get; set; }

        public short CHECKLIST_TYPEID { get; set; }

        //[StringLength(2000)]
        public string ITEMDESCRIPTION { get; set; }

        public bool ISREQUIRED { get; set; }

        public int COMPANYID { get; set; }

        public bool ISACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_DETAIL> TBL_CHECKLIST_DETAIL { get; set; }

        public virtual TBL_CHECKLIST_TYPE TBL_CHECKLIST_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_CHECKLIST_ITEM TBL_CHECKLIST_ITEM { get; set; }
    }
}

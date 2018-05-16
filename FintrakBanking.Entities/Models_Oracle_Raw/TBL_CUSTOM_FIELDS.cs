namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOM_FIELDS")]
    public partial class TBL_CUSTOM_FIELDS
    {
        public TBL_CUSTOM_FIELDS()
        {
            TBL_CUSTOM_FIELDS_DATA = new HashSet<TBL_CUSTOM_FIELDS_DATA>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMFIELDID { get; set; }

        public int COMPANYID { get; set; }

        public int HOSTPAGEID { get; set; }

        [Required]
        [StringLength(50)]
        public string LABELNAME { get; set; }

        [Required]
        [StringLength(20)]
        public string CONTROLKEY { get; set; }

        [Required]
        [StringLength(20)]
        public string CONTROLTYPE { get; set; }

        public int REQUIRED { get; set; }

        public int ITEMORDER { get; set; }

        public int ISUPLOAD { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_CUSTOM_FIELDS_DATA> TBL_CUSTOM_FIELDS_DATA { get; set; }
    }
}

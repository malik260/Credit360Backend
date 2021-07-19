namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOM_FIELDS_DATA")]
    public partial class TBL_CUSTOM_FIELDS_DATA
    {
        public TBL_CUSTOM_FIELDS_DATA()
        {
            TBL_CUSTOM_FIELD_DATA_UPLOAD = new HashSet<TBL_CUSTOM_FIELD_DATA_UPLOAD>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMFIELDSDATAID { get; set; }

        public int CUSTOMFIELDID { get; set; }

        [Required]
        public string DATADETAILS { get; set; }

        public int CREATEDBY { get; set; }

        public int OWNERID { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public virtual ICollection<TBL_CUSTOM_FIELD_DATA_UPLOAD> TBL_CUSTOM_FIELD_DATA_UPLOAD { get; set; }

        public virtual TBL_CUSTOM_FIELDS TBL_CUSTOM_FIELDS { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}

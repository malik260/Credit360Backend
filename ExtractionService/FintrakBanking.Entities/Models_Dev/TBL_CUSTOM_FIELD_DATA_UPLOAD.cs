namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOM_FIELD_DATA_UPLOAD")]
    public partial class TBL_CUSTOM_FIELD_DATA_UPLOAD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMFIELDDATAUPLOADID { get; set; }

        public byte[] CUSTOMFIELDDATAUPLOAD { get; set; }

        public int CUSTOMFIELDSDATAID { get; set; }

        [StringLength(50)]
        public string CONTENTTYPE { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public virtual TBL_CUSTOM_FIELDS_DATA TBL_CUSTOM_FIELDS_DATA { get; set; }
    }
}

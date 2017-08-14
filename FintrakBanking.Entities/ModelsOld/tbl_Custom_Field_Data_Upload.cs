namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Custom_Field_Data_Upload")]
    public partial class tbl_Custom_Field_Data_Upload
    {
        [Key]
        public int CustomFieldDataUploadId { get; set; }

        public byte[] CustomFieldDataUpload { get; set; }

        public int CustomFieldsDataId { get; set; }

        [StringLength(50)]
        public string ContentType { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public virtual tbl_Custom_Fields_Data tbl_Custom_Fields_Data { get; set; }
    }
}

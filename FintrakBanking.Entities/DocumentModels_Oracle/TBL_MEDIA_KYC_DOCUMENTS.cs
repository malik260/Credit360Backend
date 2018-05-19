namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKINGDOCUMENTS.TBL_MEDIA_KYC_DOCUMENTS")]
    public partial class TBL_MEDIA_KYC_DOCUMENTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DOCUMENTID { get; set; }

        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public int CUSTOMERID { get; set; }

        public int DOCUMENTTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string DOCUMENTTITLE { get; set; }

        [Required]
        [StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        [StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        [StringLength(100)]
        public string PHYSICALFILENUMBER { get; set; }

        [StringLength(200)]
        public string PHYSICALLOCATION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }
    }
}

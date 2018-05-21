namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_MEDIA_LOAN_MATURITY_INSTR
    {
        [Key]
        [Column(Order = 0)]
        public int DOCUMENTID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MATURITYINSTRUCTIONID { get; set; }

        [StringLength(250)]
        public string DOCUMENT_TITLE { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(400)]
        public string FILENAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Key]
        [Column(Order = 4)]
        public byte[] FILEDATA { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime SYSTEMDATETIME { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATEDBY { get; set; }

        [Key]
        [Column(Order = 7)]
        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}

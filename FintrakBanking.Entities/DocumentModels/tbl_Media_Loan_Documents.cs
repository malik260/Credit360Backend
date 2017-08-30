namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Media_Loan_Documents
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanApplicationNumber { get; set; }

        [StringLength(50)]
        public string LoanReferenceNumber { get; set; }

        [Required]
        [StringLength(250)]
        public string DocumentTitle { get; set; }

        public short DocumentTypeId { get; set; }

        [Required]
        [StringLength(400)]
        public string FileName { get; set; }

        [Required]
        [StringLength(10)]
        public string FileExtension { get; set; }

        //[Required]
        public byte[] FileData { get; set; }

        public DateTime SystemDateTime { get; set; }

        [StringLength(50)]
        public string PhysicalFileNumber { get; set; }

        [StringLength(250)]
        public string PhysicalLocation { get; set; }

        public int CreatedBy { get; set; }
    }
}

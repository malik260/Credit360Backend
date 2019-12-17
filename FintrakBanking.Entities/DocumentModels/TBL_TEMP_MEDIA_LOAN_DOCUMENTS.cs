namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_TEMP_MEDIA_LOAN_DOCUMENTS
    {
        [Key]
        public int DOCUMENTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string LOANAPPLICATIONNUMBER { get; set; }

        //[StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        public int? LOAN_BOOKING_REQUESTID { get; set; }

        [Required]
        //[StringLength(250)]
        public string DOCUMENTTITLE { get; set; }

        public short DOCUMENTTYPEID { get; set; }

        [Required]
        //[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        //[StringLength(50)]
        public string PHYSICALFILENUMBER { get; set; }

        //[StringLength(250)]
        public string PHYSICALLOCATION { get; set; }

        public int CREATEDBY { get; set; }
        public bool ISPRIMARYDOCUMENT { get; set; }
        public int? COMPANYID { get; set; }
        public short? LOANSYSTEMTYPEID { get; set; }
        public int? TEMPLOANREVIEWOPERATIONID { get; set; }

    }
}

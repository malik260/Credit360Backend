namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_MEDIA_CHECKLIST_DOCUMENTS
    {
        [Key]
        public int DOCUMENTID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int LOANDETAILSID { get; set; }

        public int CHECKLISTDEFINITIONID { get; set; }

        public int CHECKLISTSTATUSID { get; set; }

        [Required]
        //[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        [Required]
        //[StringLength(100)]
        public string PHYSICALFILENUMBER { get; set; }

        [Required]
        //[StringLength(200)]
        public string PHYSICALLOCATION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }
    }
}

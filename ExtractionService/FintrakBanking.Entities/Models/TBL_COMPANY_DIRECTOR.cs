namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("TBL_COMPANY_DIRECTOR")]
    public partial class TBL_COMPANY_DIRECTOR
    {
        [Key] public int COMPANYDIRECTORID { get; set; }

        public int COMPANYID { get; set; }

        [Required] //[StringLength(50)] 
        public string TITLE { get; set; }

        [Required] //[StringLength(200)] 
        public string FIRSTNAME { get; set; }

        //[StringLength(200)] 
        public string MIDDLENAME { get; set; }

        [Required] //[StringLength(200)] 
        public string LASTNAME { get; set; }

        //[StringLength(10)] 
        public string GENDER { get; set; }

        //[StringLength(50)] 
        public string BVN { get; set; }

        //[StringLength(500)] 
        public string ADDRESS { get; set; }

        //[StringLength(50)] 
        public string EMAIL { get; set; }

        //[StringLength(150)] 
        public string PHONENUMBER { get; set; }

        public bool ISACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? SHAREHOLDINGPERCENTAGE { get; set; }
    }
}
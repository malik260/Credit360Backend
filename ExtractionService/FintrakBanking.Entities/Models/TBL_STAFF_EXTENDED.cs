using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FintrakBanking.Entities.Models
{
    public partial class TBL_STAFF_EXTENDED
    {
        [Key]
        public int EXTENDEDSTAFFID { get; set; }

        public int STAFFID { get; set; }

        //[StringLength(200)]
        public string FIELD1 { get; set; }

        //[StringLength(200)]
        public string FIELD2 { get; set; }

        //[StringLength(200)]
        public string FIELD3 { get; set; }

        //[StringLength(200)]
        public string FIELD4 { get; set; }

        //[StringLength(200)]
        public string FIELD5 { get; set; }

        //[StringLength(200)]
        public string FIELD6 { get; set; }

        //[StringLength(200)]
        public string FIELD7 { get; set; }

        //[StringLength(200)]
        public string FIELD8 { get; set; }

        //[StringLength(200)]
        public string FIELD9 { get; set; }

        //[StringLength(200)]
        public string FIELD10 { get; set; }
    }
}

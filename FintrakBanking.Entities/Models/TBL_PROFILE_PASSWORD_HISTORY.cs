namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PROFILE_PASSWORD_HISTORY")]
    public partial class TBL_PROFILE_PASSWORD_HISTORY
    {
        [Key]
        public int PASSWORDHISTORYID { get; set; }

        public int USERID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public string PASSWORD { get; set; }
    }
}

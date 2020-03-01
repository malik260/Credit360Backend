namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_PASSWORD_HISTORY")]
    public partial class TBL_PROFILE_PASSWORD_HISTORY
    {
        [Key]
        public int PASSWORDHISTORYID { get; set; }

        public int USERID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string PASSWORD { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }
    }
}

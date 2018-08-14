namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_SETTING")]
    public partial class TBL_PROFILE_SETTING
    {
        [Key]
        public int PROFILESETTINGID { get; set; }

        public int MINREQUIREDPASSWORDLENGTH { get; set; }

        public int MINREQUIREDNONALPHANUMERICCHAR { get; set; }

        public bool? ENABLEPASSWORDRETRIEVAL { get; set; }

        public bool? ENABLEPASSWORDRESET { get; set; }

        public bool? REQUIRESQUESTIONANDANSWER { get; set; }

        public bool? REQUIRESUNIQUEEMAIL { get; set; }

        public int MAXINVALIDPASSWORDATTEMPTS { get; set; }

        public int ALLOWPASSWORDREUSEAFTER { get; set; }

        public int EXPIREPASSWORDAFTER { get; set; }

        public int MAXPERIODOFUSERINACTIVITY { get; set; }

        public int SESSIONTIMEOUT { get; set; }
    }
}

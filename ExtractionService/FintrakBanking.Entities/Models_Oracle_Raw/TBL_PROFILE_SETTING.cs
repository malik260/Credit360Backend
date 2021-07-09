namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_SETTING")]
    public partial class TBL_PROFILE_SETTING
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PROFILESETTINGID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MINREQUIREDPASSWORDLENGTH { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MINREQUIREDNONALPHANUMERICCHAR { get; set; }

        public int? ENABLEPASSWORDRETRIEVAL { get; set; }

        public int? ENABLEPASSWORDRESET { get; set; }

        public int? REQUIRESQUESTIONANDANSWER { get; set; }

        public int? REQUIRESUNIQUEEMAIL { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAXINVALIDPASSWORDATTEMPTS { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ALLOWPASSWORDREUSEAFTER { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EXPIREPASSWORDAFTER { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAXPERIODOFUSERINACTIVITY { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SESSIONTIMEOUT { get; set; }
    }
}

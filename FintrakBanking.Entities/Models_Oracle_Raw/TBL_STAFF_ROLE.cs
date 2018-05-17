namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STAFF_ROLE")]
    public partial class TBL_STAFF_ROLE
    {
        public TBL_STAFF_ROLE()
        {
            TBL_APPROVAL_LEVEL = new HashSet<TBL_APPROVAL_LEVEL>();
            TBL_PROFILE_STAFF_ROLE_ADT_ACT = new HashSet<TBL_PROFILE_STAFF_ROLE_ADT_ACT>();
            TBL_PROFILE_STAFF_ROLE_GROUP = new HashSet<TBL_PROFILE_STAFF_ROLE_GROUP>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TEMP_PROFILE_STAFF_ROLE_AA = new HashSet<TBL_TEMP_PROFILE_STAFF_ROLE_AA>();
            TBL_TEMP_PROFILE_STAFF_ROL_GRP = new HashSet<TBL_TEMP_PROFILE_STAFF_ROL_GRP>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STAFFROLEID { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFROLECODE { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFROLENAME { get; set; }

        public int COMPANYID { get; set; }

        public virtual ICollection<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }

        public virtual ICollection<TBL_PROFILE_STAFF_ROLE_ADT_ACT> TBL_PROFILE_STAFF_ROLE_ADT_ACT { get; set; }

        public virtual ICollection<TBL_PROFILE_STAFF_ROLE_GROUP> TBL_PROFILE_STAFF_ROLE_GROUP { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_STAFF_ROLE_AA> TBL_TEMP_PROFILE_STAFF_ROLE_AA { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_STAFF_ROL_GRP> TBL_TEMP_PROFILE_STAFF_ROL_GRP { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}

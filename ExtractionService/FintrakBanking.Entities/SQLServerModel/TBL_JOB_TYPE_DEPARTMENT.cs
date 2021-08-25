namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_JOB_TYPE_DEPARTMENT")]
    public partial class TBL_JOB_TYPE_DEPARTMENT
    {
        [Key]
        public int JOB_TYPE_DEPARTMENTID { get; set; }

        public short JOBTYPEID { get; set; }

        public short DEPARTMENTID { get; set; }

        public virtual TBL_DEPARTMENT TBL_DEPARTMENT { get; set; }

        public virtual TBL_JOB_TYPE TBL_JOB_TYPE { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CUSTOMER_GROUP_MAPPING_ARC")]
    public partial class TBL_CUSTOMER_GROUP_MAPPING_ARC
    {
        [Key]
        public int CUSTOMERGROUPMAPPINGARCID { get; set; }
        public int CUSTOMERGROUPMAPPINGID { get; set; }

        public int CUSTOMERID { get; set; }

        public int CUSTOMERGROUPID { get; set; }

        public short RELATIONSHIPTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

    }
}

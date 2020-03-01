using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_SYNDICATION_PARTY_TYP")]
    public partial class TBL_LOAN_SYNDICATION_PARTY_TYP
    {
       
        [Key]
        public int PARTY_TYPEID { get; set; }

        //[StringLength(50)]
        public string PARTY_TYPENAME { get; set; }
    }
}

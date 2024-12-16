using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PROPERTY_TYPE")]

    public class TBL_PROPERTY_TYPE
    {
        [Key]
        public short PROPERTYTYPEID { get; set; }
        public string PROPERTYNAME { get; set; }

    }
}

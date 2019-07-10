using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   public class TBL_INSURANCE_TYPE
    {
        [Key]
        public int INSURANCETYPEID { get; set; }
        public string INSURANCETYPE { get; set; }
    }
}

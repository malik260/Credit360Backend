using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_CONDITION")]
    public partial class TBL_ALERT_CONDITION
    {
        [Key]
        public int ALERTCONDITIONID { get; set; }
        public string FORMULAR { get; set; }
        public DateTime LASTRUNDATE { get; set; }
<<<<<<< HEAD
        public DateTime? NEXTRUNDATE { get; set; }
        // public string LASTRUNTIME { get; set; }
=======
        public DateTime NEXTRUNDATE { get; set; }
        //public DateTime LASTRUNTIME { get; set; }
>>>>>>> 19b565f82e067473fede390c2a92c5a094618277
        public short? OPERATIONID { get; set; }
        public string TRIGGERSOURCE { get; set; }
        public string ACTIONFORTRIGGER { get; set; }
        public string TYPE { get; set; }
        public short? ALERTINTERVAL { get; set; }
<<<<<<< HEAD
=======
        public int? TITLE { get; set; }
>>>>>>> 19b565f82e067473fede390c2a92c5a094618277
    }
}

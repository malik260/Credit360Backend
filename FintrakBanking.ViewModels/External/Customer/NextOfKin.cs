using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class NextOfKin
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string gender { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string relationship { get; set; }
        public string nearestLandmark { get; set; }
        public int cityId { get; set; }
        public string contactAddress { get; set; }
        public string mobilePhoneNo { get; set; }
    }
}

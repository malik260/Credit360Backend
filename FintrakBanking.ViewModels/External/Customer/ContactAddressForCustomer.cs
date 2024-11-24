using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class ContactAddressForCustomer
    {
        public int stateId { get; set; }
        //public int lgaId { get; set; }
        public string nearestLandmark { get; set; }
        public int cityId { get; set; }
        public string utilityBillNo { get; set; }
        public string mailingAddress { get; set; }
        public string contactAddress { get; set; }
        public short addressTypeId { get; set; }
    }
}

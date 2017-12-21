using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
   public class ScheduledCollateralModel
    {
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string surname { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.surname; } }
        public string cutomerAddress { get; set; }
        public string phoneNumber { get; set; }
        public string emailAddress { get; set; }
        public string principalSum { get; set; }
        public string outstandingBalance { get; set; }
        public string collateralType { get; set; }
        public string descriptionOfTheCollateral { get; set; }
        public string locationOfTheCollateral { get; set; }
        public string collateralValue { get; set; }
        public string Status { get; set; }
        public string gAddress { get; set; }
        public string guarantorPhoneNumber { get { return this.PhoneNumber1 + " " + this.PhoneNumber2; } }
        public string guarantorEmail { get; set; }
        public string guarantorRemark { get; set; }
        public string guarantorAddress { get { return gAddress + " " + homeTown + " " + landMark + " " + poBox; } }
        public string homeTown { get; set; }
        public string poBox { get; set; }
        public string landMark { get; set; }
        public string gFirstName { get; set; }
        public string gMiddleName { get; set; }
        public string gLastName { get; set; }
        public string guarantorName { get { return gFirstName + " " + gMiddleName + " " + gLastName; } }
        public string PhoneNumber2 { get; set; }
        public string PhoneNumber1 { get; set; }
    }
}

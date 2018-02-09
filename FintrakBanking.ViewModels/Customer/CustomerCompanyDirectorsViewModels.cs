using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerCompanyDirectorsViewModels : GeneralEntity
    {
        public CustomerCompanyDirectorsViewModels()
        {
            customerCompanyBeneficial = new List<CustomerCompanyBeneficiaryViewModels>();
        }
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public short customerTypeId { get; set; }
        public string customerName { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string fullname { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public string rcNumber { get; set; }
        public string taxNumber { get; set; }
        public double numberOfShares { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public List<CustomerCompanyBeneficiaryViewModels> customerCompanyBeneficial { get; set; }
    }

    public class CustomerCompanyBeneficiaryViewModels
    {
       public int companyBeneficiaryId { get; set; }
        public int companyDirectorId { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string bankVerificationNumber { get; set; }
        public int numberOfShares { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
}

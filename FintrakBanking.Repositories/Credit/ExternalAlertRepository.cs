using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
   public class ExternalAlertRepository
    {
        // place holders
        private readonly string customerNameHolder = "@{{customerName}}";
        private readonly string branchNameHolder = "@{{branchName}}";
        private readonly string descriptionNameHolder = "@{{description}}";
        private readonly string expiryDateHolder = "@{{expiryDate}}";
        private readonly string meetingDateHolder = "@{{meetingDate}}";
        private readonly string provideHolder = "@{{provideHolder}}";
        private readonly string dueDateHolder = "@{{dueDate}}";
        private readonly string accountNumberHolder = "@{{accountNumber}}";
        private readonly string accountOfficerNameHolder = "@{{accountOfficerName}}";
        
        
        // properties to have getter methods for interfacing
        private string customerName;
        private string branchName;
        private string description;
        private string expiryDate;
        private string meetingDate;
        private string provider;
        private string dueDate;
        private string accountNumber;
        private string accountOfficerName;
        

        public string Replace(string content) // placeholders replace
        {
            content = content.Replace(customerNameHolder, customerName);
            content = content.Replace(branchNameHolder, branchName);
            content = content.Replace(descriptionNameHolder, description);
            content = content.Replace(expiryDateHolder, expiryDate);
            content = content.Replace(meetingDateHolder, meetingDate);
            content = content.Replace(provideHolder, provideHolder);
            content = content.Replace(dueDateHolder, dueDate);
            content = content.Replace(accountNumberHolder, accountNumber);
            content = content.Replace(accountOfficerNameHolder, accountOfficerName);

           return content;
        }
    }


}

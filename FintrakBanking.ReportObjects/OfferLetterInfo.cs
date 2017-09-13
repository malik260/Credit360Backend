using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects
{
    public class OfferLetterInfo
    {
        public static OfferLetterViewModel GenerateOfferLetter(int customerId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            //var loanApplicant = context.tbl_Loan_Application.Where(x => x.CustomerId == customerId).FirstOrDefault();
            var targetCustomer = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId);

            if (targetCustomer != null)
            {
                var offerLetterDetails = context.tbl_Loan_Application
                    .Where(x => x.CustomerId == customerId && x.SubmittedForAppraisal == true)
                    .Select(x => new OfferLetterViewModel
                    {
                        customerId = (int)x.CustomerId,
                        customerName = targetCustomer.Title + ". " + targetCustomer.FirstName + " " + targetCustomer.LastName,
                        customerAddress = context.tbl_Customer_Address.FirstOrDefault(cAddr => cAddr.CustomerId == customerId).Address ?? string.Empty,
                        loanAmount = context.tbl_Loan_Preliminary_Evaluation.FirstOrDefault(pen => pen.CustomerId == customerId).LoanAmount,
                        interestRate = x.InterestRate,
                        tenor = x.Tenor,
                        maturityDate = DateTime.Now
                    });

                return offerLetterDetails.FirstOrDefault();
            }

            return new OfferLetterViewModel();
        }
    }
}

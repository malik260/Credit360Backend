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
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var offerLetterDetails = (from a in context.tbl_Loan_Application
                                      join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                      where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() &&
                                      a.ApprovalStatusId == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          customerId = (int)a.CustomerId,
                                          customerName = b.Title + ". " + b.FirstName + " " + b.LastName,
                                          customerAddress = context.tbl_Customer_Address.FirstOrDefault(cAddr => cAddr.CustomerId == b.CustomerId).Address ?? string.Empty,
                                          loanAmount = context.tbl_Loan_Preliminary_Evaluation.FirstOrDefault(pen => pen.CustomerId == b.CustomerId).LoanAmount,
                                          interestRate = a.InterestRate,
                                          tenor = a.Tenor,
                                          maturityDate = a.ApplicationDate
                                     }).FirstOrDefault();

            if (offerLetterDetails != null)
            {
                return offerLetterDetails;
            }

            return new OfferLetterViewModel();
        }
    }
}

using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.ReportObjects
{

    public  class LoanReportObjects 
    {
      

        private IQueryable<LoanInformation> Loans(int companyId , DateTime startDate, DateTime endDate)
        {
            IQueryable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                loan = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Schedule_Periodic on a.TermLoanId equals b.LoanId
                        where a.CompanyId == companyId && a.IsDisbursed == true &&( a.DisburseDate >= startDate && a.DisburseDate <= endDate)
                        select new LoanInformation()
                        { customerId = a.CustomerId, tearmLoanId = a.TermLoanId ,
                            branchName = a.tbl_Branch.BranchName,
                            branchCode = a.tbl_Branch.BranchCode,
                            customerCode = a.tbl_Customer.CustomerCode,
                            firstName = a.tbl_Customer.FirstName,
                            lastName = a.tbl_Customer.LastName,
                            middleName = a.tbl_Customer.MiddleName,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            productName = a.tbl_Product.ProductName,
                            loanRefrenceNumber = a.LoanReferenceNumber,
                            frequancy = context.tbl_Loan_Schedule_Periodic .Where(c=> c.LoanId == a.TermLoanId ).Count()-1,
                            frequencyType = a.tbl_Frequency_Type.Mode,
                            companyName = a.tbl_Company.Name,
                            effectiveDate = a.EffectiveDate,
                            interestRate = a.InterestRate,
                            maturityDate = a.MaturityDate,
                            principalAmount = a.PrincipalAmount,          
                            closePrincipalAmount = b.EndPrincipalAmount,
                            startingBalance = b.StartPrincipalAmount ,
                            paymentDate = b.PaymentDate,
                            periodInterestAmount = b.PeriodInterestAmount,
                            principalRepaymentAmount = b.PeriodPaymentAmount,
                            outstandingPrincipal =a.OutstandingPrincipal,
                            outstandingInterest   = a.OutstandingInterest
                        });
                return loan;
            }

            
        }

        public IEnumerable<LoanInformation> GetLoanSchedule( int companyId, int tearmLoanId)
        {
            IEnumerable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();
                loan = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Schedule_Periodic on a.TermLoanId equals b.LoanId
                        where a.CompanyId == companyId  && a.TermLoanId == tearmLoanId
                        select new LoanInformation()
                        {
                             accountNumber = context.tbl_CASA .FirstOrDefault(c=> c.CasaAccountId == a.CasaAccountId ).ProductAccountNumber ,
                            customerId = a.CustomerId,
                            tearmLoanId = a.TermLoanId,
                            branchName = a.tbl_Branch.BranchName,
                            branchCode = a.tbl_Branch.BranchCode,
                            customerCode = a.tbl_Customer.CustomerCode,
                            firstName = a.tbl_Customer.FirstName,
                            lastName = a.tbl_Customer.LastName,
                            middleName = a.tbl_Customer.MiddleName,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            productName = a.tbl_Product.ProductName,
                            loanRefrenceNumber = a.LoanReferenceNumber,
                            frequancy = context.tbl_Loan_Schedule_Periodic.Where(c => c.LoanId == a.TermLoanId).Count() - 1,
                            frequencyType = a.tbl_Frequency_Type.Mode,
                            companyName = company.Name,
                            effectiveDate = a.EffectiveDate,
                            interestRate = a.InterestRate,
                            maturityDate = a.MaturityDate,
                            principalAmount = a.PrincipalAmount,
                            closePrincipalAmount = b.EndPrincipalAmount,
                            startingBalance = b.StartPrincipalAmount,
                            paymentDate = b.PaymentDate,
                            periodInterestAmount = b.PeriodInterestAmount,
                            principalRepaymentAmount = b.PeriodPaymentAmount,
                            outstandingPrincipal = a.OutstandingPrincipal,
                            outstandingInterest = a.OutstandingInterest
                        });
                return loan.ToList();
            }
             
        }
    }

  
}

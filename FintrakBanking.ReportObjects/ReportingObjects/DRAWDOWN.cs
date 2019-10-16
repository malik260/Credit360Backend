using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class DRAWDOWN
    {
        FinTrakBankingContext context = new FinTrakBankingContext();

        public IEnumerable<DrawdownViewModel> GetDrawdown(string loanRefNo)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                StringBuilder sb = new StringBuilder();
                var data = from a in context.TBL_LOAN_APPLICATION
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           where a.APPLICATIONREFERENCENUMBER.Trim() == loanRefNo.Trim()

                           select new DrawdownViewModel
                           {
                               branch = context.TBL_BRANCH.Where(h => h.BRANCHID == a.BRANCHID).Select(h => h.BRANCHNAME).FirstOrDefault() == null ? "" : context.TBL_BRANCH.Where(h => h.BRANCHID == a.BRANCHID).Select(h => h.BRANCHNAME).FirstOrDefault(),
                               misCode = a.MISCODE,
                               facilityType = context.TBL_PRODUCT.Where(pr=>pr.PRODUCTID == a.PRODUCTID).Select(pr=>pr.PRODUCTNAME).FirstOrDefault()==null? "": context.TBL_PRODUCT.Where(pr => pr.PRODUCTID == a.PRODUCTID).Select(pr => pr.PRODUCTNAME).FirstOrDefault(), //p.PRODUCTNAME,
                               interestRate = a.INTERESTRATE,
                               drawdownAmount = b.APPROVEDAMOUNT,
                               accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                               processingFee = 0.00,
                               tenor = b.APPROVEDTENOR,
                               approvedAmount = b.APPROVEDAMOUNT,
                               mgtFee = 0.00,
                               moratorium = b.MORATORIUMDURATION,
                               commitmentFee = 0.00,
                               customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                               principalRepayment = a.APPROVEDAMOUNT,
                               effectiveDate = b.EFFECTIVEDATE,
                               otherFeeSpecify = 0.00,
                               interestRepayment = b.REPAYMENTTERMS,
                               amountUtilized = 0.00,
                               newRequest = context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(O => O.AMOUNT_REQUESTED).FirstOrDefault(),//: context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(O=>O.AMOUNT_REQUESTED).FirstOrDefault(),
                               inPlace = "",
                               perfected = "",
                               deffered = "",
                               relationshipOfficer = context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPOFFICERID).Select(re => re.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPOFFICERID).Select(re => re.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPOFFICERID).Select(re => re.LASTNAME).FirstOrDefault(),
                               relationshipManager = context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPMANAGERID).Select(re => re.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPMANAGERID).Select(re => re.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(re => re.STAFFID == a.RELATIONSHIPMANAGERID).Select(re => re.LASTNAME).FirstOrDefault(),
                               riskManagement = "",
                               legal = "",
                               treasury = "",
                               coo = "",
                               crmInternation = "",
                               otherInPlace = "",
                               otherPerfected = "",
                               otherDeffered = "",
                           };

                return data.ToList();

            }
        }
    }
}

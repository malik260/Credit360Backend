using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class ScheduledCollateralObject
    {
        public IEnumerable<ScheduledCollateralModel> ScheduledCollateralData(DateTime startDate, DateTime endDate, int companyId)
        {

            using (FinTrakBankingContext db = new FinTrakBankingContext())
            {

                var data = from l in db.TBL_LOAN
                           join m in db.TBL_LOAN_COLLATERAL_MAPPING on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals m.LOANID
                           //join g in db.TBL_LOAN_GUARANTOR on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals g.LOANAPPLICATIONID
                           where l.COMPANYID == companyId && l.LOANSTATUSID == 1
                                && DbFunctions.TruncateTime(l.DATEAPPROVED) >= DbFunctions.TruncateTime(startDate)
                                 && DbFunctions.TruncateTime(l.DATEAPPROVED) <= DbFunctions.TruncateTime(endDate)
                           orderby l.DATEAPPROVED descending

                           select new ScheduledCollateralModel()
                           {
                               lastName = l.TBL_CUSTOMER.LASTNAME,
                               firstName = l.TBL_CUSTOMER.FIRSTNAME,
                               middleName = l.TBL_CUSTOMER.MIDDLENAME,
                               cutomerAddress = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.ADDRESS).ToString(),
                               homeTown = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.HOMETOWN).ToString(),
                               poBox = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.POBOX).ToString(),
                               landMark = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.NEARESTLANDMARK).ToString(),
                               phoneNumber = l.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.PHONENUMBER).ToString(),
                               emailAddress = l.TBL_CUSTOMER.EMAILADDRESS,
                               principalSum = l.PRINCIPALAMOUNT.ToString(),
                               outstandingBalance = l.OUTSTANDINGPRINCIPAL.ToString(),
                               collateralType = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               descriptionOfTheCollateral = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.DETAILS,
                               locationOfTheCollateral = "",
                               collateralValue = l.TBL_CUSTOMER.TBL_COLLATERAL_CUSTOMER.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.COLLATERALVALUE).ToString(),
                              // Status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                              //TODO: Please refactor
                               //gFirstName = g.FIRSTNAME,
                               //gMiddleName = g.MIDDLENAME,
                               //gLastName = g.LASTNAME,
                               //gAddress = g.ADDRESS,
                               //PhoneNumber1 = g.PHONENUMBER1,
                               //PhoneNumber2 = g.PHONENUMBER2,
                               //guarantorEmail = g.EMAILADDRESS

                           };

                return data;
            }


        }

        public IEnumerable<ScheduledCollateralModel> ScheduledCollateralData(string accountNo, int companyId)
        {

            using (FinTrakBankingContext db = new FinTrakBankingContext())
            {

                var data = from l in db.TBL_LOAN
                           join m in db.TBL_LOAN_COLLATERAL_MAPPING on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals m.LOANID
                          // join g in db.TBL_LOAN_GUARANTOR on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals g.LOANAPPLICATIONID
                           where l.COMPANYID == companyId && l.LOANSTATUSID == 1  && l.TBL_CASA.PRODUCTACCOUNTNUMBER==accountNo

                           select new ScheduledCollateralModel()
                           {
                               lastName = l.TBL_CUSTOMER.LASTNAME,
                               firstName = l.TBL_CUSTOMER.FIRSTNAME,
                               middleName = l.TBL_CUSTOMER.MIDDLENAME,
                               cutomerAddress = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.ADDRESS).ToString(),
                               homeTown = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.HOMETOWN).ToString(),
                               poBox = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.POBOX).ToString(),
                               landMark = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.NEARESTLANDMARK).ToString(),
                               phoneNumber = l.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.PHONENUMBER).ToString(),
                               emailAddress = l.TBL_CUSTOMER.EMAILADDRESS,
                               principalSum = l.PRINCIPALAMOUNT.ToString(),
                               outstandingBalance = l.OUTSTANDINGPRINCIPAL.ToString(),
                               collateralType = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               descriptionOfTheCollateral = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.DETAILS,
                               locationOfTheCollateral = "",
                               collateralValue = l.TBL_CUSTOMER.TBL_COLLATERAL_CUSTOMER.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.COLLATERALVALUE).ToString(),
                             //  Status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                             //TODO: please refactor
                               //gFirstName = g.FIRSTNAME,
                               //gMiddleName = g.MIDDLENAME,
                               //gLastName = g.LASTNAME,
                               //gAddress = g.ADDRESS,
                               //PhoneNumber1 = g.PHONENUMBER1,
                               //PhoneNumber2 = g.PHONENUMBER2,
                               //guarantorEmail = g.EMAILADDRESS

                           };

                return data;
            }


        }
        public IEnumerable<ScheduledCollateralModel> ScheduledCollateralDataByAccount(string accountNo, int companyId)
        {

            using (FinTrakBankingContext db = new FinTrakBankingContext())
            {

                var data = from l in db.TBL_LOAN
                           join m in db.TBL_LOAN_COLLATERAL_MAPPING on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals m.LOANID
                          // join g in db.TBL_LOAN_GUARANTOR on l.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals g.LOANAPPLICATIONID
                           where l.COMPANYID == companyId && l.LOANSTATUSID == 1 && l.TBL_CASA.PRODUCTACCOUNTNUMBER == accountNo

                           select new ScheduledCollateralModel()
                           {
                               lastName = l.TBL_CUSTOMER.LASTNAME,
                               firstName = l.TBL_CUSTOMER.FIRSTNAME,
                               middleName = l.TBL_CUSTOMER.MIDDLENAME,
                               cutomerAddress = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.ADDRESS).ToString(),
                               homeTown = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.HOMETOWN).ToString(),
                               poBox = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.POBOX).ToString(),
                               landMark = db.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.NEARESTLANDMARK).ToString(),
                               phoneNumber = l.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.PHONENUMBER).ToString(),
                               emailAddress = l.TBL_CUSTOMER.EMAILADDRESS,
                               principalSum = l.PRINCIPALAMOUNT.ToString(),
                               outstandingBalance = l.OUTSTANDINGPRINCIPAL.ToString(),
                               collateralType = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               descriptionOfTheCollateral = m.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.DETAILS,
                               locationOfTheCollateral = "",
                               collateralValue = l.TBL_CUSTOMER.TBL_COLLATERAL_CUSTOMER.Where(x => x.CUSTOMERID == l.CUSTOMERID).Select(x => x.COLLATERALVALUE).ToString(),
                             //  Status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                             //TODO : please refactor
                               //gFirstName = g.FIRSTNAME,
                               //gMiddleName = g.MIDDLENAME,
                               //gLastName = g.LASTNAME,
                               //gAddress = g.ADDRESS,
                               //PhoneNumber1 = g.PHONENUMBER1,
                               //PhoneNumber2 = g.PHONENUMBER2,
                               //guarantorEmail = g.EMAILADDRESS

                           };

                return data;
            }


        }
    }
}

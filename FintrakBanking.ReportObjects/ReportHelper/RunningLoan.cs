using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FintrakBanking.ReportObjects.ReportHelper
{
    public class RunningLoan
    {
        public List<RuniningLoanViewModel> GetRunningLoan(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {
            //List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                //subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var runiningLoanList = (from l in context.TBL_LOAN
                                            join al in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals al.LOANAPPLICATIONDETAILID
                                            join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                            join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                            join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                            join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID
                                            join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                            join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                            join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                            //join ch in context.TBL_CHART_OF_ACCOUNT on p.PRODUCTCODE equals ch.pr
                                            //join ld in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID

                                            join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                            join pgt in context.TBL_LOAN_PRUDENT_GUIDE_TYPE on pg.PRUDENTIALGUIDELINETYPEID equals pgt.PRUDENTIALGUIDELINETYPEID
                                            join d in context.TBL_COLLATERAL_CUSTOMER on l.CUSTOMERID equals d.CUSTOMERID
                                           




                                            //let staffInfo =(from u in subList where u.staffCode == st.STAFFCODE select new { u.subHead,u.teamUnit,u.deptName}).ToList()

                                            let glInfo = (from gl in context.TBL_CHART_OF_ACCOUNT
                                                          join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                                          join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                                          where pr.PRODUCTID == p.PRODUCTID && cst.CURRENCYCODE == cur.CURRENCYCODE
                                                          select cst.ACCOUNTID).FirstOrDefault()


                                            where l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                            && DbFunctions.TruncateTime(l.MATURITYDATE) >= DbFunctions.TruncateTime(startDate)
                                               && DbFunctions.TruncateTime(l.MATURITYDATE) <= DbFunctions.TruncateTime(endDate)
                                               && (b.BRANCHID == branchId || branchId == null || branchId == 0)
                                                && l.COMPANYID == companyId
                                            orderby l.MATURITYDATE descending

                                            select new RuniningLoanViewModel
                                            {
                                                rmCode = rm.STAFFCODE,
                                                rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                                branchName = b.BRANCHNAME,
                                                branchCode = b.BRANCHCODE,
                                                loanRefNo = l.LOANREFERENCENUMBER,
                                                customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                                currencyType = cur.CURRENCYNAME,
                                                loanId = l.TERMLOANID,
                                                loanSytemTypeId = l.LOANSYSTEMTYPEID,
                                                transactionDateBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                                schemeType = pt.PRODUCTTYPENAME,
                                                schemeDescription = p.PRODUCTNAME,
                                                sanctionLimit = l.PRINCIPALAMOUNT,
                                                expiryDate = l.MATURITYDATE,
                                                customerId = l.CUSTOMERID,
                                                schemeCode = p.PRODUCTCODE,
                                                subUserClassification = pg.STATUSNAME,
                                                userClassification = pgt.PRUDENTIALGUIDELINETYPENAME,
                                                glSubHeadCode = glInfo,
                                                classificationDate = DateTime.Now,
                                                limitExpiryDate = l.MATURITYDATE,
                                                pastDueDate = l.PASTDUEDATE,
                                                staffCode = st.STAFFCODE,
                                                teamCode = "",
                                                deskCode = "",
                                                groupCode = "",
                                                buCode = "",
                                                pastDueDays = (int)DbFunctions.DiffDays((l.PASTDUEDATE.Value == null ? default(DateTime) : l.PASTDUEDATE.Value), DateTime.Now),

                                                //groupDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.subHead).FirstOrDefault()),
                                                //teamDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.teamUnit).FirstOrDefault()),
                                                // deskDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.deptName).FirstOrDefault()),



                                            }).ToList().Select(x =>
                                            {


                                                //var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                                //if (checkForGroupHead == null)
                                                //{
                                                //    x.groupDescription = "";
                                                //}
                                                //else if (checkForGroupHead != null)
                                                //{
                                                //    x.groupDescription = checkForGroupHead;
                                                //}
                                                //var checkForTeamDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.teamUnit).FirstOrDefault();
                                                //if (checkForTeamDescription == null)
                                                //{
                                                //    x.teamDescription = "";
                                                //}
                                                //else if (checkForTeamDescription != null)
                                                //{
                                                //    x.teamDescription = checkForTeamDescription;
                                                //}
                                                //var checkForDeskDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.deptName).FirstOrDefault();
                                                //if (checkForDeskDescription == null)
                                                //{
                                                //    x.deskDescription = "";
                                                //}
                                                //else if (checkForDeskDescription != null)
                                                //{
                                                //    x.deskDescription = checkForDeskDescription;
                                                //}
                                                //var checkForBusinessDevelopment = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.businessDevelopmentManger).FirstOrDefault();

                                                //if (checkForBusinessDevelopment == null)
                                                //{
                                                //    x.businessDevelopmentManger = "";
                                                //}
                                                //else if (checkForBusinessDevelopment != null)
                                                //{
                                                //    x.businessDevelopmentManger = checkForBusinessDevelopment;
                                                //}

                                                //var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                                //if (checkForBuDescription == null)
                                                //{
                                                //    x.buDescription = "";
                                                //}
                                                //else if (checkForBuDescription != null)
                                                //{
                                                //    x.buDescription = checkForBuDescription;
                                                //}

                                                return x;


                                            }).ToList();
                    

                    return runiningLoanList;

                }
            }

        }

        public List<RuniningLoanViewModel> GetRunningInsuranceLoan(int companyId)
        {
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var runiningLoanList = (from l in context.TBL_LOAN
                                                //join al in context.TBL_LOAN_APPLICATION on l.CUSTOMERID equals al.CUSTOMERID
                                            join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                            join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                            join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                            join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID
                                            join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                            join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                            join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                            //join ch in context.TBL_CHART_OF_ACCOUNT on p.PRODUCTCODE equals ch.pr
                                            //join ld in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID

                                            join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                            join pgt in context.TBL_LOAN_PRUDENT_GUIDE_TYPE on pg.PRUDENTIALGUIDELINETYPEID equals pgt.PRUDENTIALGUIDELINETYPEID
                                            join d in context.TBL_COLLATERAL_CUSTOMER on l.CUSTOMERID equals d.CUSTOMERID





                                            //let staffInfo =(from u in subList where u.staffCode == st.STAFFCODE select new { u.subHead,u.teamUnit,u.deptName}).ToList()

                                            let glInfo = (from gl in context.TBL_CHART_OF_ACCOUNT
                                                          join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                                          join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                                          where pr.PRODUCTID == p.PRODUCTID && cst.CURRENCYCODE == cur.CURRENCYCODE
                                                          select cst.ACCOUNTID).FirstOrDefault()


                                            where l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                  && l.COMPANYID == companyId
                                            orderby l.MATURITYDATE descending

                                            select new RuniningLoanViewModel
                                            {


                                                rmCode = rm.STAFFCODE,
                                                rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                                branchName = b.BRANCHNAME,
                                                branchCode = b.BRANCHCODE,
                                                loanRefNo = l.LOANREFERENCENUMBER,
                                                customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,

                                                currencyType = cur.CURRENCYNAME,

                                                loanId = l.TERMLOANID,
                                                loanSytemTypeId = l.LOANSYSTEMTYPEID,

                                                transactionDateBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                                schemeType = pt.PRODUCTTYPENAME,

                                                schemeDescription = p.PRODUCTNAME,


                                                sanctionLimit = l.PRINCIPALAMOUNT,


                                                expiryDate = l.MATURITYDATE,

                                                customerId = l.CUSTOMERID,

                                                schemeCode = p.PRODUCTCODE,

                                                subUserClassification = pg.STATUSNAME,
                                                userClassification = pgt.PRUDENTIALGUIDELINETYPENAME,

                                                glSubHeadCode = glInfo,
                                                classificationDate = DateTime.Now,

                                                limitExpiryDate = l.MATURITYDATE,
                                                pastDueDate = l.PASTDUEDATE,

                                                staffCode = st.STAFFCODE,

                                                teamCode = "",
                                                deskCode = "",
                                                groupCode = "",
                                                buCode = "",

                                                pastDueDays = (int)DbFunctions.DiffDays((l.PASTDUEDATE.Value == null ? default(DateTime) : l.PASTDUEDATE.Value), DateTime.Now),

                                                //groupDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.subHead).FirstOrDefault()),
                                                //teamDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.teamUnit).FirstOrDefault()),
                                                // deskDescription = (staffInfo == null ? "" : staffInfo.Select(x=>x.deptName).FirstOrDefault()),



                                            }).ToList().Select(x =>
                                            {


                                                var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                                if (checkForGroupHead == null)
                                                {
                                                    x.groupDescription = "";
                                                }
                                                else if (checkForGroupHead != null)
                                                {
                                                    x.groupDescription = checkForGroupHead;
                                                }
                                                var checkForTeamDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.teamUnit).FirstOrDefault();
                                                if (checkForTeamDescription == null)
                                                {
                                                    x.teamDescription = "";
                                                }
                                                else if (checkForTeamDescription != null)
                                                {
                                                    x.teamDescription = checkForTeamDescription;
                                                }
                                                var checkForDeskDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.deptName).FirstOrDefault();
                                                if (checkForDeskDescription == null)
                                                {
                                                    x.deskDescription = "";
                                                }
                                                else if (checkForDeskDescription != null)
                                                {
                                                    x.deskDescription = checkForDeskDescription;
                                                }
                                                var checkForBusinessDevelopment = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.businessDevelopmentManger).FirstOrDefault();

                                                if (checkForBusinessDevelopment == null)
                                                {
                                                    x.businessDevelopmentManger = "";
                                                }
                                                else if (checkForBusinessDevelopment != null)
                                                {
                                                    x.businessDevelopmentManger = checkForBusinessDevelopment;
                                                }

                                                var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                                if (checkForBuDescription == null)
                                                {
                                                    x.buDescription = "";
                                                }
                                                else if (checkForBuDescription != null)
                                                {
                                                    x.buDescription = checkForBuDescription;
                                                }

                                                return x;


                                            }).ToList();


                    return runiningLoanList;

                }
            }

        }
    }
}
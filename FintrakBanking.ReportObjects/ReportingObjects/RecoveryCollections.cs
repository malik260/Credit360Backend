using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class RecoveryCollections
    {
        public IEnumerable<RecoveryCollectionsViewModel> DelinquentAccounts(DateTime startDate, DateTime endDate)
        {
            List<SubHead> staffmisi = new List<SubHead>();
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext())
                {
                    staffmisi = (from sl in stagingContext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                }
                var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                             join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                             where
                                             (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && DbFunctions.DiffDays(lr.DATEASSIGNED, DateTime.UtcNow).Value >= 30
                                             && lr.TOTALAMOUNTRECOVERY < 50000000
                                             && lr.ISFULLYRECOVERED == false
                                             && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && lr.SOURCE.ToLower() == "retail"
                                             && lr.DELETED == false

                                             orderby ln.DATETIMECREATED descending
                                             select new RecoveryCollectionsViewModel
                                             {
                                                 dateAssigned = lr.DATEASSIGNED,
                                                 loanReference = lr.LOANREFERENCE,
                                                 accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 customerCode = cu.CUSTOMERCODE,
                                                 customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                 referenceNumber = ln.LOANREFERENCENUMBER,
                                                 productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                 main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                 businessLine = "Nil",
                                                 subBusinessLine = "Nil",
                                                 groupHeadName = "Nil",
                                                 regionName = "Nil",
                                                 groupName = "Nil",
                                                 teamName = "Nil",
                                                 productCode = pr.PRODUCTCODE,
                                                 maturityDate = ln.MATURITYDATE,
                                                 principalAmount = ln.PRINCIPALAMOUNT,
                                                 interest = ln.INTERESTONPASTDUEINTEREST,
                                                 penalCharges = ln.PENALCHARGEAMOUNT,
                                                 amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                 loanAmountLcy = ld.APPROVEDAMOUNT,
                                                 totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                 collections = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANID == ln.TERMLOANID).Sum(c => c.AMOUNTRECOVERED),
                                                 facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                 staffCode = st.STAFFCODE,
                                                 supervisorId = st.SUPERVISOR_STAFFID,
                                                 location = br.BRANCHNAME,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                  join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                  where
                                                  (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                  && DbFunctions.DiffDays(lr.DATEASSIGNED, DateTime.UtcNow).Value >= 30
                                                  && lr.TOTALAMOUNTRECOVERY < 50000000
                                                  && lr.ISFULLYRECOVERED == false
                                                  && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && lr.SOURCE.ToLower() == "retail"
                                                  && lr.DELETED == false

                                                  orderby ln.DATETIMECREATED descending
                                                  select new RecoveryCollectionsViewModel
                                                  {
                                                      dateAssigned = lr.DATEASSIGNED,
                                                      loanReference = lr.LOANREFERENCE,
                                                      accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                      customerCode = cu.CUSTOMERCODE,
                                                      customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                      referenceNumber = ln.LOANREFERENCENUMBER,
                                                      productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                      main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                      businessLine = "Nil",
                                                      subBusinessLine = "Nil",
                                                      groupHeadName = "Nil",
                                                      regionName = "Nil",
                                                      groupName = "Nil",
                                                      teamName = "Nil",
                                                      productCode = pr.PRODUCTCODE,
                                                      maturityDate = ln.MATURITYDATE,
                                                      principalAmount = ln.OVERDRAFTLIMIT,
                                                      interest = ln.INTERESTONPASTDUEINTEREST,
                                                      penalCharges = ln.PENALCHARGEAMOUNT,
                                                      amountDue = ln.PASTDUEPRINCIPAL,
                                                      loanAmountLcy = ld.APPROVEDAMOUNT,
                                                      totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                      collections = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANID == ln.REVOLVINGLOANID).Sum(c => c.AMOUNTRECOVERED),
                                                      facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                      staffCode = st.STAFFCODE,
                                                      supervisorId = st.SUPERVISOR_STAFFID,
                                                      location = br.BRANCHNAME,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();

                var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                var allData = unionAll.ToList();

                if (allData.Count() > 0)
                {
                    foreach (var i in allData)
                    {
                        var rm = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == i.supervisorId).FirstOrDefault();
                        if (rm != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                i.groupHeadName = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == zonalHead.SUPERVISOR_STAFFID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                            }
                        }
                        i.regionName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.region).FirstOrDefault();
                        i.groupName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.businessUnit).FirstOrDefault();
                        i.teamName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.deptName).FirstOrDefault();
                    }
                }

                return allData;
            }

        }
            public IEnumerable<RecoveryCollectionsViewModel> PaydayLoanAllocation(DateTime startDate, DateTime endDate)
            {
                List<SubHead> staffmisi = new List<SubHead>();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    using (FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext())
                    {
                        staffmisi = (from sl in stagingContext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                    }
                    var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                 join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                 join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                 join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                 join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                 join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                 join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                 join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                 join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                 where
                                                 (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                 && lr.ISFULLYRECOVERED == false
                                                 && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                 && lr.SOURCE.ToLower() == "retail"
                                                 && lr.DELETED == false

                                                 orderby ln.DATETIMECREATED descending
                                                 select new RecoveryCollectionsViewModel
                                                 {

                                                     dateAssigned = lr.DATEASSIGNED,
                                                     agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a=>a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a=>a.FIRMNAME).FirstOrDefault(),
                                                     loanReference = lr.LOANREFERENCE,
                                                     settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                     customerCode = cu.CUSTOMERCODE,
                                                     customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                     referenceNumber = ln.LOANREFERENCENUMBER,
                                                     productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                     main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                     businessLine = "Nil",
                                                     subBusinessLine = "Nil",
                                                     groupHeadName = "Nil",
                                                     regionName = "Nil",
                                                     groupName = "Nil",
                                                     teamName = "Nil",
                                                     mobileNumber = "Nil",
                                                     divisionName = "Nil",
                                                     productCode = pr.PRODUCTCODE,
                                                     productName = pr.PRODUCTNAME,
                                                     principalOutstandingBalLcy = ln.OUTSTANDINGPRINCIPAL,
                                                     bookingDate = ln.BOOKINGDATE,
                                                     valueDate = ln.DATETIMECREATED,
                                                     referenceDate = ln.EFFECTIVEDATE,
                                                     maturityDate = ln.MATURITYDATE,
                                                     principalAmount = ln.PRINCIPALAMOUNT,
                                                     accountOfficerCode = st.STAFFCODE,
                                                     accountOfficerName = st.FIRSTNAME +" "+ st.MIDDLENAME +" "+ st.LASTNAME,
                                                     processDate = ln.BOOKINGDATE,
                                                     interest = ln.INTERESTONPASTDUEINTEREST,
                                                     penalCharges = ln.PENALCHARGEAMOUNT,
                                                     amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                     loanAmountLcy = ld.APPROVEDAMOUNT,
                                                     totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                     collections = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANID == ln.TERMLOANID).Sum(c => c.AMOUNTRECOVERED),
                                                     facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                     staffCode = st.STAFFCODE,
                                                     supervisorId = st.SUPERVISOR_STAFFID,
                                                     location = br.BRANCHNAME,
                                                 }).ToList();

                    var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                      join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                      join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                      join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                      join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                      join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                      join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                      join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                      join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                      where
                                                      (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                      && lr.ISFULLYRECOVERED == false
                                                      && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                      && lr.SOURCE.ToLower() == "retail"
                                                      && lr.DELETED == false

                                                      orderby ln.DATETIMECREATED descending
                                                      select new RecoveryCollectionsViewModel
                                                      {
                                                          dateAssigned = lr.DATEASSIGNED,
                                                          agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                          loanReference = lr.LOANREFERENCE,
                                                          accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                          customerCode = cu.CUSTOMERCODE,
                                                          customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                          referenceNumber = ln.LOANREFERENCENUMBER,
                                                          productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                          main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                          businessLine = "Nil",
                                                          subBusinessLine = "Nil",
                                                          groupHeadName = "Nil",
                                                          regionName = "Nil",
                                                          groupName = "Nil",
                                                          teamName = "Nil",
                                                          mobileNumber = "Nil",
                                                          divisionName = "Nil",
                                                          productCode = pr.PRODUCTCODE,
                                                          accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                          processDate = ln.BOOKINGDATE,
                                                          productName = pr.PRODUCTNAME,
                                                          principalOutstandingBalLcy = ln.PASTDUEPRINCIPAL,
                                                          bookingDate = ln.BOOKINGDATE,
                                                          valueDate = ln.DATETIMECREATED,
                                                          referenceDate = ln.EFFECTIVEDATE,
                                                          maturityDate = ln.MATURITYDATE,
                                                          principalAmount = ln.OVERDRAFTLIMIT,
                                                          interest = ln.INTERESTONPASTDUEINTEREST,
                                                          penalCharges = ln.PENALCHARGEAMOUNT,
                                                          amountDue = ln.PASTDUEPRINCIPAL,
                                                          loanAmountLcy = ld.APPROVEDAMOUNT,
                                                          totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                          collections = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANID == ln.REVOLVINGLOANID).Sum(c => c.AMOUNTRECOVERED),
                                                          facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                          staffCode = st.STAFFCODE,
                                                          supervisorId = st.SUPERVISOR_STAFFID,
                                                          location = br.BRANCHNAME,
                                                      }).ToList();

                    var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();
                    var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();

                    var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                    var allData = unionAll.ToList();

                    if (allData.Count() > 0)
                    {
                        foreach (var i in allData)
                        {
                            var rm = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == i.supervisorId).FirstOrDefault();
                            if (rm != null)
                            {
                                var zonalHead = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                                if (zonalHead != null)
                                {
                                    i.groupHeadName = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == zonalHead.SUPERVISOR_STAFFID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                                }
                            }
                            i.regionName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.region).FirstOrDefault();
                            i.groupName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.businessUnit).FirstOrDefault();
                            i.teamName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.deptName).FirstOrDefault();
                        }
                    }

                    return allData;
                }


            }


        public IEnumerable<RecoveryCollectionsViewModel> ComputationForExternalAgents(DateTime startDate, DateTime endDate)
        {
            List<SubHead> staffmisi = new List<SubHead>();
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext())
                {
                    staffmisi = (from sl in stagingContext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                }
                var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                             join ra in context.TBL_ACCREDITEDCONSULTANT on lr.ACCREDITEDCONSULTANT equals ra.ACCREDITEDCONSULTANTID
                                             join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                             where
                                             (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && lr.ISFULLYRECOVERED == false
                                             && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && lr.SOURCE.ToLower() == "retail"
                                             && ra.CATEGORY.ToLower() == "external"
                                             && lr.DELETED == false

                                             orderby ln.DATETIMECREATED descending
                                             select new RecoveryCollectionsViewModel
                                             {
                                                 accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 dateAssigned = lr.DATEASSIGNED,
                                                 agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                 loanReference = lr.LOANREFERENCE,
                                                 settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 customerCode = cu.CUSTOMERCODE,
                                                 customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                 referenceNumber = ln.LOANREFERENCENUMBER,
                                                 productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                 main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                 businessLine = "Nil",
                                                 subBusinessLine = "Nil",
                                                 groupHeadName = "Nil",
                                                 regionName = "Nil",
                                                 groupName = "Nil",
                                                 teamName = "Nil",
                                                 mobileNumber = "Nil",
                                                 divisionName = "Nil",
                                                 productCode = pr.PRODUCTCODE,
                                                 productName = pr.PRODUCTNAME,
                                                 principalOutstandingBalLcy = ln.OUTSTANDINGPRINCIPAL,
                                                 bookingDate = ln.BOOKINGDATE,
                                                 valueDate = ln.DATETIMECREATED,
                                                 referenceDate = ln.EFFECTIVEDATE,
                                                 maturityDate = ln.MATURITYDATE,
                                                 principalAmount = ln.PRINCIPALAMOUNT,
                                                 accountOfficerCode = st.STAFFCODE,
                                                 accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                 processDate = ln.BOOKINGDATE,
                                                 interest = ln.INTERESTONPASTDUEINTEREST,
                                                 penalCharges = ln.PENALCHARGEAMOUNT,
                                                 amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                 loanAmountLcy = ld.APPROVEDAMOUNT,
                                                 totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                 collections = lr.TOTALAMOUNTRECOVERY, 
                                                 actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                 commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                 facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                 staffCode = st.STAFFCODE,
                                                 supervisorId = st.SUPERVISOR_STAFFID,
                                                 location = br.BRANCHNAME,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                  join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                  where
                                                  (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                  && lr.ISFULLYRECOVERED == false
                                                  && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && lr.SOURCE.ToLower() == "retail"
                                                  && lr.DELETED == false

                                                  orderby ln.DATETIMECREATED descending
                                                  select new RecoveryCollectionsViewModel
                                                  {
                                                      dateAssigned = lr.DATEASSIGNED,
                                                      agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                      loanReference = lr.LOANREFERENCE,
                                                      accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                      customerCode = cu.CUSTOMERCODE,
                                                      customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                      referenceNumber = ln.LOANREFERENCENUMBER,
                                                      productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                      main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                      businessLine = "Nil",
                                                      subBusinessLine = "Nil",
                                                      groupHeadName = "Nil",
                                                      regionName = "Nil",
                                                      groupName = "Nil",
                                                      teamName = "Nil",
                                                      mobileNumber = "Nil",
                                                      divisionName = "Nil",
                                                      productCode = pr.PRODUCTCODE,
                                                      accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                      processDate = ln.BOOKINGDATE,
                                                      productName = pr.PRODUCTNAME,
                                                      principalOutstandingBalLcy = ln.PASTDUEPRINCIPAL,
                                                      bookingDate = ln.BOOKINGDATE,
                                                      valueDate = ln.DATETIMECREATED,
                                                      referenceDate = ln.EFFECTIVEDATE,
                                                      maturityDate = ln.MATURITYDATE,
                                                      principalAmount = ln.OVERDRAFTLIMIT,
                                                      interest = ln.INTERESTONPASTDUEINTEREST,
                                                      penalCharges = ln.PENALCHARGEAMOUNT,
                                                      amountDue = ln.PASTDUEPRINCIPAL,
                                                      loanAmountLcy = ld.APPROVEDAMOUNT,
                                                      totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                      collections = lr.TOTALAMOUNTRECOVERY,
                                                      actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                      commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                      facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                      staffCode = st.STAFFCODE,
                                                      supervisorId = st.SUPERVISOR_STAFFID,
                                                      location = br.BRANCHNAME,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();

                var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                var allData = unionAll.ToList();

                if (allData.Count() > 0)
                {
                    foreach (var i in allData)
                    {
                        var rm = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == i.supervisorId).FirstOrDefault();
                        if (rm != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                i.groupHeadName = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == zonalHead.SUPERVISOR_STAFFID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                            }
                        }
                        i.regionName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.region).FirstOrDefault();
                        i.groupName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.businessUnit).FirstOrDefault();
                        i.teamName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.deptName).FirstOrDefault();
                    }
                }

                return allData;
            }


        }

        public IEnumerable<RecoveryCollectionsViewModel> RecoveryCollectionReport(DateTime startDate, DateTime endDate)
        {
            List<SubHead> staffmisi = new List<SubHead>();
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext())
                {
                    staffmisi = (from sl in stagingContext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                }
                var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                             join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                             where
                                             (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && lr.ISFULLYRECOVERED == false
                                             && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && lr.SOURCE.ToLower() == "retail"
                                             && lr.DELETED == false

                                             orderby ln.DATETIMECREATED descending
                                             select new RecoveryCollectionsViewModel
                                             {
                                                 accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 dateAssigned = lr.DATEASSIGNED,
                                                 agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                 loanReference = lr.LOANREFERENCE,
                                                 settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 customerCode = cu.CUSTOMERCODE,
                                                 customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                 referenceNumber = ln.LOANREFERENCENUMBER,
                                                 productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                 main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                 businessLine = "Nil",
                                                 subBusinessLine = "Nil",
                                                 groupHeadName = "Nil",
                                                 regionName = "Nil",
                                                 groupName = "Nil",
                                                 teamName = "Nil",
                                                 mobileNumber = "Nil",
                                                 divisionName = "Nil",
                                                 productCode = pr.PRODUCTCODE,
                                                 productName = pr.PRODUCTNAME,
                                                 principalOutstandingBalLcy = ln.OUTSTANDINGPRINCIPAL,
                                                 bookingDate = ln.BOOKINGDATE,
                                                 valueDate = ln.DATETIMECREATED,
                                                 referenceDate = ln.EFFECTIVEDATE,
                                                 maturityDate = ln.MATURITYDATE,
                                                 principalAmount = ln.PRINCIPALAMOUNT,
                                                 accountOfficerCode = st.STAFFCODE,
                                                 accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                 processDate = ln.BOOKINGDATE,
                                                 interest = ln.INTERESTONPASTDUEINTEREST,
                                                 penalCharges = ln.PENALCHARGEAMOUNT,
                                                 amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                 loanAmountLcy = ld.APPROVEDAMOUNT,
                                                 totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                 collections = lr.TOTALAMOUNTRECOVERY,
                                                 actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                 commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                 facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                 staffCode = st.STAFFCODE,
                                                 supervisorId = st.SUPERVISOR_STAFFID,
                                                 location = br.BRANCHNAME,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                  join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                  where
                                                  (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                  && lr.ISFULLYRECOVERED == false
                                                  && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && lr.SOURCE.ToLower() == "retail"
                                                  && lr.DELETED == false

                                                  orderby ln.DATETIMECREATED descending
                                                  select new RecoveryCollectionsViewModel
                                                  {
                                                      dateAssigned = lr.DATEASSIGNED,
                                                      agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                      loanReference = lr.LOANREFERENCE,
                                                      accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                      customerCode = cu.CUSTOMERCODE,
                                                      customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                      referenceNumber = ln.LOANREFERENCENUMBER,
                                                      productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                      main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                      businessLine = "Nil",
                                                      subBusinessLine = "Nil",
                                                      groupHeadName = "Nil",
                                                      regionName = "Nil",
                                                      groupName = "Nil",
                                                      teamName = "Nil",
                                                      mobileNumber = "Nil",
                                                      divisionName = "Nil",
                                                      productCode = pr.PRODUCTCODE,
                                                      accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                      processDate = ln.BOOKINGDATE,
                                                      productName = pr.PRODUCTNAME,
                                                      principalOutstandingBalLcy = ln.PASTDUEPRINCIPAL,
                                                      bookingDate = ln.BOOKINGDATE,
                                                      valueDate = ln.DATETIMECREATED,
                                                      referenceDate = ln.EFFECTIVEDATE,
                                                      maturityDate = ln.MATURITYDATE,
                                                      principalAmount = ln.OVERDRAFTLIMIT,
                                                      interest = ln.INTERESTONPASTDUEINTEREST,
                                                      penalCharges = ln.PENALCHARGEAMOUNT,
                                                      amountDue = ln.PASTDUEPRINCIPAL,
                                                      loanAmountLcy = ld.APPROVEDAMOUNT,
                                                      totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                      collections = lr.TOTALAMOUNTRECOVERY,
                                                      actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                      commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                      facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                      staffCode = st.STAFFCODE,
                                                      supervisorId = st.SUPERVISOR_STAFFID,
                                                      location = br.BRANCHNAME,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();

                var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                var allData = unionAll.ToList();

                if (allData.Count() > 0)
                {
                    foreach (var i in allData)
                    {
                        var rm = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == i.supervisorId).FirstOrDefault();
                        if (rm != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                i.groupHeadName = context.TBL_STAFF.Where(s => s.SUPERVISOR_STAFFID == zonalHead.SUPERVISOR_STAFFID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                            }
                        }
                        i.regionName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.region).FirstOrDefault();
                        i.groupName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.businessUnit).FirstOrDefault();
                        i.teamName = staffmisi.Where(z => z.staffCode == i.staffCode).Select(z => z.deptName).FirstOrDefault();
                    }
                }

                return allData;
            }
        }

        public IEnumerable<RecoveryCollectionsViewModel> ComputationForInternalAgents(DateTime startDate, DateTime endDate)
        {
           
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                
                var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                             join ra in context.TBL_ACCREDITEDCONSULTANT on lr.ACCREDITEDCONSULTANT equals ra.ACCREDITEDCONSULTANTID
                                             join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                             where
                                             (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && lr.ISFULLYRECOVERED == false
                                             && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && lr.SOURCE.ToLower() == "retail"
                                             && ra.CATEGORY.ToLower() == "internal"
                                             && lr.DELETED == false

                                             orderby ln.DATETIMECREATED descending
                                             select new RecoveryCollectionsViewModel
                                             {
                                                 accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 dateAssigned = lr.DATEASSIGNED,
                                                 agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                 loanReference = lr.LOANREFERENCE,
                                                 newCountReferenceNumber = lr.LOANREFERENCE,
                                                 settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                 customerCode = cu.CUSTOMERCODE,
                                                 customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                 referenceNumber = ln.LOANREFERENCENUMBER,
                                                 productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                 main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                 businessLine = "Nil",
                                                 subBusinessLine = "Nil",
                                                 groupHeadName = "Nil",
                                                 regionName = "Nil",
                                                 groupName = "Nil",
                                                 teamName = "Nil",
                                                 mobileNumber = "Nil",
                                                 divisionName = "Nil",
                                                 productCode = pr.PRODUCTCODE,
                                                 productName = pr.PRODUCTNAME,
                                                 principalOutstandingBalLcy = ln.OUTSTANDINGPRINCIPAL,
                                                 minimumAmountDueUnpaid = ln.OUTSTANDINGPRINCIPAL,
                                                 totalOutstanding = lr.TOTALAMOUNTRECOVERY - (context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED)),
                                                 bookingDate = ln.BOOKINGDATE,
                                                 valueDate = ln.DATETIMECREATED,
                                                 referenceDate = ln.EFFECTIVEDATE,
                                                 maturityDate = ln.MATURITYDATE,
                                                 principalAmount = ln.PRINCIPALAMOUNT,
                                                 accountOfficerCode = st.STAFFCODE,
                                                 accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                 processDate = ln.BOOKINGDATE,
                                                 interest = ln.INTERESTONPASTDUEINTEREST,
                                                 penalCharges = ln.PENALCHARGEAMOUNT,
                                                 amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                 loanAmountLcy = ld.APPROVEDAMOUNT,
                                                 totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                 collections = lr.TOTALAMOUNTRECOVERY,
                                                 actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                 commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                 facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                 staffCode = st.STAFFCODE,
                                                 supervisorId = st.SUPERVISOR_STAFFID,
                                                 location = br.BRANCHNAME,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                  join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                  where
                                                  (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                  && lr.ISFULLYRECOVERED == false
                                                  && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && lr.SOURCE.ToLower() == "retail"
                                                  && lr.DELETED == false

                                                  orderby ln.DATETIMECREATED descending
                                                  select new RecoveryCollectionsViewModel
                                                  {
                                                      dateAssigned = lr.DATEASSIGNED,
                                                      agentAssigned = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(a => a.FIRMNAME).FirstOrDefault(),
                                                      loanReference = lr.LOANREFERENCE,
                                                      newCountReferenceNumber = lr.LOANREFERENCE,
                                                      accountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                      customerCode = cu.CUSTOMERCODE,
                                                      customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                      referenceNumber = ln.LOANREFERENCENUMBER,
                                                      productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                      main = cu.CUSTOMERTYPEID == 1 ? "Retail" : "Non Retail",
                                                      businessLine = "Nil",
                                                      subBusinessLine = "Nil",
                                                      groupHeadName = "Nil",
                                                      regionName = "Nil",
                                                      groupName = "Nil",
                                                      teamName = "Nil",
                                                      mobileNumber = "Nil",
                                                      divisionName = "Nil",
                                                      productCode = pr.PRODUCTCODE,
                                                      totalOutstanding = lr.TOTALAMOUNTRECOVERY - (context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED)),
                                                      accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                      processDate = ln.BOOKINGDATE,
                                                      productName = pr.PRODUCTNAME,
                                                      principalOutstandingBalLcy = ln.PASTDUEPRINCIPAL,
                                                      minimumAmountDueUnpaid = ln.PASTDUEPRINCIPAL,
                                                      bookingDate = ln.BOOKINGDATE,
                                                      valueDate = ln.DATETIMECREATED,
                                                      referenceDate = ln.EFFECTIVEDATE,
                                                      maturityDate = ln.MATURITYDATE,
                                                      principalAmount = ln.OVERDRAFTLIMIT,
                                                      interest = ln.INTERESTONPASTDUEINTEREST,
                                                      penalCharges = ln.PENALCHARGEAMOUNT,
                                                      amountDue = ln.PASTDUEPRINCIPAL,
                                                      loanAmountLcy = ld.APPROVEDAMOUNT,
                                                      totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                      collections = lr.TOTALAMOUNTRECOVERY,
                                                      actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                      commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                      facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                      staffCode = st.STAFFCODE,
                                                      supervisorId = st.SUPERVISOR_STAFFID,
                                                      location = br.BRANCHNAME,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanReference).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReference).ToList();

                var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                var allData = unionAll.ToList();
                return allData;
            }
        }

        public IEnumerable<RecoveryCollectionsViewModel> SummaryComputationForInternalAgents(DateTime startDate, DateTime endDate)
        {
           
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                
                var dataLoanNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                             join ra in context.TBL_ACCREDITEDCONSULTANT on lr.ACCREDITEDCONSULTANT equals ra.ACCREDITEDCONSULTANTID
                                             join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                             where
                                             (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && lr.ISFULLYRECOVERED == false
                                             && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && lr.SOURCE.ToLower() == "retail"
                                             && ra.CATEGORY.ToLower() == "internal"
                                             && lr.DELETED == false

                                             orderby ln.DATETIMECREATED descending
                                             select new RecoveryCollectionsViewModel
                                             {
                                                 dateAssigned = lr.DATEASSIGNED,
                                                 agentAssigned = ra.FIRMNAME,
                                                 accreditedConsultant = ra.ACCREDITEDCONSULTANTID,
                                                 loanReference = lr.LOANREFERENCE,
                                                 accountNumber = ra.ACCOUNTNUMBER,
                                                 customerCode = cu.CUSTOMERCODE,
                                                 customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                 referenceNumber = ln.LOANREFERENCENUMBER,
                                                 productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                 productCode = pr.PRODUCTCODE,
                                                 productName = pr.PRODUCTNAME,
                                                 principalOutstandingBalLcy = ln.OUTSTANDINGPRINCIPAL,
                                                 bookingDate = ln.BOOKINGDATE,
                                                 valueDate = ln.DATETIMECREATED,
                                                 referenceDate = ln.EFFECTIVEDATE,
                                                 maturityDate = ln.MATURITYDATE,
                                                 principalAmount = ln.PRINCIPALAMOUNT,
                                                 accountOfficerCode = st.STAFFCODE,
                                                 accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                 processDate = ln.BOOKINGDATE,
                                                 interest = ln.INTERESTONPASTDUEINTEREST,
                                                 penalCharges = ln.PENALCHARGEAMOUNT,
                                                 amountDue = ln.PRINCIPALINSTALLMENTLEFT,
                                                 loanAmountLcy = ld.APPROVEDAMOUNT,
                                                 totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                 collections = lr.TOTALAMOUNTRECOVERY,
                                                 actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                 commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                 facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                 staffCode = st.STAFFCODE,
                                                 supervisorId = st.SUPERVISOR_STAFFID,
                                                 location = br.BRANCHNAME,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                                  join ra in context.TBL_ACCREDITEDCONSULTANT on lr.ACCREDITEDCONSULTANT equals ra.ACCREDITEDCONSULTANTID
                                                  join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                                  where
                                                  (DbFunctions.TruncateTime(lr.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(lr.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                                  && lr.ISFULLYRECOVERED == false
                                                  && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && lr.SOURCE.ToLower() == "retail"
                                                  && lr.DELETED == false

                                                  orderby ln.DATETIMECREATED descending
                                                  select new RecoveryCollectionsViewModel
                                                  {
                                                      dateAssigned = lr.DATEASSIGNED,
                                                      agentAssigned = ra.FIRMNAME,
                                                      accreditedConsultant = ra.ACCREDITEDCONSULTANTID,
                                                      loanReference = lr.LOANREFERENCE,
                                                      accountNumber = ra.ACCOUNTNUMBER,
                                                      customerCode = cu.CUSTOMERCODE,
                                                      customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                      referenceNumber = ln.LOANREFERENCENUMBER,
                                                      productClass = context.TBL_PRODUCT_CLASS.Where(p => p.PRODUCTCLASSID == pr.PRODUCTCLASSID).Select(p => p.PRODUCTCLASSNAME).FirstOrDefault(),
                                                      productCode = pr.PRODUCTCODE,
                                                      accountOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                      processDate = ln.BOOKINGDATE,
                                                      productName = pr.PRODUCTNAME,
                                                      principalOutstandingBalLcy = ln.PASTDUEPRINCIPAL,
                                                      bookingDate = ln.BOOKINGDATE,
                                                      valueDate = ln.DATETIMECREATED,
                                                      referenceDate = ln.EFFECTIVEDATE,
                                                      maturityDate = ln.MATURITYDATE,
                                                      principalAmount = ln.OVERDRAFTLIMIT,
                                                      interest = ln.INTERESTONPASTDUEINTEREST,
                                                      penalCharges = ln.PENALCHARGEAMOUNT,
                                                      amountDue = ln.PASTDUEPRINCIPAL,
                                                      loanAmountLcy = ld.APPROVEDAMOUNT,
                                                      totalExposureLcy = lp.TOTALEXPOSUREAMOUNT,
                                                      collections = lr.TOTALAMOUNTRECOVERY,
                                                      actualRecovery = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.AMOUNTRECOVERED),
                                                      commission = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.LOANREFERENCE == ln.LOANREFERENCENUMBER).Sum(c => c.COMMISSIONPAYABLE),
                                                      facilityType = context.TBL_PRODUCT_TYPE.Where(f => f.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(f => f.PRODUCTTYPENAME).FirstOrDefault(),
                                                      staffCode = st.STAFFCODE,
                                                      supervisorId = st.SUPERVISOR_STAFFID,
                                                      location = br.BRANCHNAME,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.accreditedConsultant).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.agentAssigned).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.accreditedConsultant).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.agentAssigned).ToList();

                var unionAll = termLoanDataNon.Union(revolvingLoanDataNon);
                var allData = unionAll.ToList();

                foreach(var consultant in allData)
                {
                    consultant.orlMinimumAssigned = 0.0;
                    consultant.amountRecoveredOrl = 0.0;
                    consultant.commissionOne = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber).Sum(c => c.COMMISSIONPAYABLE);
                    consultant.commissionTwo = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber).Sum(c => c.COMMISSIONPAYABLE);
                    consultant.target = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber).Sum(c => c.TOTALRECOVERYAMOUNT);
                    consultant.totalAmountRecovered = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber).Sum(c => c.AMOUNTRECOVERED);
                    consultant.totalAmountAssigned = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber).Sum(c => c.TOTALRECOVERYAMOUNT);
                    consultant.amountRecoveredCreditCard = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber && c.PRODUCTCLASSID == (int)ProductClassEnum.Creditcards).Sum(c => c.AMOUNTRECOVERED);
                    consultant.creditCardMinimumAssigned = context.TBL_LOAN_RECOVERY_COMMISSION_RETAIL.Where(c => c.AGENTACCOUNTNUMBER == consultant.accountNumber && c.PRODUCTCLASSID == (int)ProductClassEnum.Creditcards).Sum(c => c.TOTALRECOVERYAMOUNT);
                                                      
                }

                return allData;
            }
        }

    }
}

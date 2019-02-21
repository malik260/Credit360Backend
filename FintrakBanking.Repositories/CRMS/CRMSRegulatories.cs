using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.CRMS
{
    public class CRMSRegulatories : ICRMSRegulatories
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ICRMSCodeBookRepository codeBook;

        public CRMSRegulatories(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        IAuditTrailRepository _audit,
                                        ICRMSCodeBookRepository _codeBook)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.codeBook = _codeBook;

        }

        public string AddCRMSCode(CRMSViewModel param)
        {
            //if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            //{
                var loan = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == param.loanId).Select(x => x).FirstOrDefault();
                if (loan == null)
                    throw new ConditionNotMetException("This Facility does not exist");

                var codeExist = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.CRMSCODE == param.crmsCode).Any();
                if (codeExist == true)
                    throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has aleady been Assigned, Kindly Provide Another Code..");

                loan.CRMSCODE = param.crmsCode;
                loan.CRMSDATE = DateTime.Now;
            loan.CRMSVALIDATED = true;

            //}

            //else if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            //{
            //    var loan = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == param.loanId).Select(x => x).FirstOrDefault();
            //    if (loan == null)
            //        throw new ConditionNotMetException("This loan does not exist");

            //    var codeExist = context.TBL_LOAN_REVOLVING.Where(x => x.CRMSCODE == param.crmsCode).Any();
            //    if (codeExist == true)
            //        throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has aleady been Assigned, Kindly Provide Another Code..");

            //    loan.CRMSCODE = param.crmsCode;
            //    loan.CRMSDATE = DateTime.Now;

            //}
            //else if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            //{
            //    var loan = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == param.loanId).Select(x => x).FirstOrDefault();
            //    if (loan == null)
            //        throw new ConditionNotMetException("This loan does not exist");

            //    var codeExist = context.TBL_LOAN_CONTINGENT.Where(x => x.CRMSCODE == param.crmsCode).Any();
            //    if (codeExist == true)
            //        throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has aleady been Assigned, Kindly Provide Another Code..");

            //    loan.CRMSCODE = param.crmsCode;
            //    loan.CRMSDATE = DateTime.Now;

            //}
            if (context.SaveChanges() > 0)
            {
                return "Successful";
            }
            return "Failed";

        }

        private IQueryable<CRMSTemplateViewModel> GetFee(CRMSViewModel param)
        {
            var term = from x in context.TBL_LOAN_APPLICATION_DETAIL
                           //join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                       //join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                       join f in context.TBL_LOAN_APPLICATION_DETL_FEE on x.LOANAPPLICATIONDETAILID equals f.LOANAPPLICATIONDETAILID
                       join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
                      where (x.CRMSVALIDATED == false || x.CRMSVALIDATED == null)
                       select new CRMSTemplateViewModel
                       {
                           ACCOUNT = x.CASAACCOUNTID != null ? context.TBL_CASA.Where(a => a.CASAACCOUNTID == x.CASAACCOUNTID).Select(g => g.PRODUCTACCOUNTNUMBER).FirstOrDefault():"n/a", //c.PRODUCTACCOUNTNUMBER,
                           FEE_TYPE = cf.CRMSREGULATORYID,
                           FEE_AMOUNT = f.RECOMMENDED_FEERATEVALUE * x.APPROVEDAMOUNT,
                           CRMSLEGALSTATUSID = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == x.CUSTOMERID).Select(q => q.CRMSLEGALSTATUSID).FirstOrDefault() != null ? context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == x.CUSTOMERID).Select(q => q.CRMSLEGALSTATUSID).FirstOrDefault() : 0,// b.CRMSLEGALSTATUSID,
                           DATETIMECREATED = x.DATETIMECREATED,
                           LOANAPPLICATIONDETAILID =x.LOANAPPLICATIONDETAILID,
                          
                       };
            //var OD = from x in context.TBL_LOAN_REVOLVING
            //         join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
            //         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //         join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
            //         join f in context.TBL_LOAN_FEE on x.REVOLVINGLOANID equals f.LOANID
            //         join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
            //         where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //         && ld.CRMSCODE == null
            //         select new CRMSTemplateViewModel
            //         {
            //             ACCOUNT = c.PRODUCTACCOUNTNUMBER,
            //             FEE_TYPE = cf.CRMSREGULATORYID,
            //             FEE_AMOUNT = f.FEEAMOUNT,
            //             CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID

            //         };
            //var contingent = from x in context.TBL_LOAN_CONTINGENT
            //                 join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
            //                 join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //                 join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
            //                 join f in context.TBL_LOAN_FEE on x.CONTINGENTLOANID equals f.LOANID
            //                 join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
            //                 where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //                 && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //                 && ld.CRMSCODE == null
            //                 select new CRMSTemplateViewModel
            //                 {
            //                     ACCOUNT = c.PRODUCTACCOUNTNUMBER,
            //                     FEE_TYPE = cf.CRMSREGULATORYID,
            //                     FEE_AMOUNT = f.FEEAMOUNT,
            //                     CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID
            //                 };


            return term.OrderBy(x => x.ACCOUNT);//.Union(OD).Union(contingent).OrderBy(x => x.ACCOUNT).ToList();
        }
        private IQueryable<CRMSTemplateViewModel> GetDirectors(CRMSViewModel param)
        {
             var term = from x in context.TBL_LOAN_APPLICATION_DETAIL
                       //join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                       //join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                       join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
                        where (x.CRMSVALIDATED == false || x.CRMSVALIDATED == null)

                        select new CRMSTemplateViewModel
                       {

                           ACCOUNT = x.CASAACCOUNTID != null ? context.TBL_CASA.Where(a => a.CASAACCOUNTID == x.CASAACCOUNTID).Select(g => g.PRODUCTACCOUNTNUMBER).FirstOrDefault() : "n/a", //c.PRODUCTACCOUNTNUMBER,
                           ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                           ID_DETAIL = d.TAX_NUMBER != null ? d.TAX_NUMBER : d.CUSTOMERBVN,
                           CRMSLEGALSTATUSID = context.TBL_CUSTOMER.Where(a=>a.CUSTOMERID == x.CUSTOMERID).Select(q=>q.CRMSLEGALSTATUSID).FirstOrDefault() != null ? context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == x.CUSTOMERID).Select(q => q.CRMSLEGALSTATUSID).FirstOrDefault() : 0,// b.CRMSLEGALSTATUSID,
                           EMAIL = d.EMAILADDRESS,
                           DATETIMECREATED = x.DATETIMECREATED,
                           LOANAPPLICATIONDETAILID = x.LOANAPPLICATIONDETAILID,

                        };
            //var OD = from x in context.TBL_LOAN_REVOLVING
            //         join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
            //         join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
            //         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //         join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
            //         where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //         && ld.CRMSCODE == null
            //         select new CRMSTemplateViewModel
            //         {
            //             ACCOUNT = c.PRODUCTACCOUNTNUMBER,
            //             ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
            //             ID_DETAIL = d.TAX_NUMBER != null ? d.TAX_NUMBER : d.CUSTOMERBVN,
            //             CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
            //             EMAIL = d.EMAILADDRESS
            //         };
            //var contingent = from x in context.TBL_LOAN_CONTINGENT
            //                 join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
            //                 join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
            //                 join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //                 join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
            //                 where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //                 && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //                 && ld.CRMSCODE == null
            //                 select new CRMSTemplateViewModel
            //                 {
            //                     ACCOUNT = c.PRODUCTACCOUNTNUMBER,
            //                     ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
            //                     ID_DETAIL = d.TAX_NUMBER != null ? d.TAX_NUMBER : d.CUSTOMERBVN,
            //                     CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
            //                     EMAIL = d.EMAILADDRESS
            //                 };

            return term.OrderBy(x => x.ACCOUNT);//.Union(OD).Union(contingent).OrderBy(x => x.ACCOUNT).ToList();
        }
        public List<CRMSRegulatoryViewModel> GetAllLoansForCRMS(CRMSViewModel param)
        {

            List<CRMSRegulatoryViewModel> tLoan = new List<CRMSRegulatoryViewModel>();
            List<CRMSRegulatoryViewModel> revolving = new List<CRMSRegulatoryViewModel>();

            List<CRMSRegulatoryViewModel> contingent = new List<CRMSRegulatoryViewModel>();
            var operationRollOver = CommonHelpers.GetRolloverOperations();
            var operationRestructure = CommonHelpers.GetRestructureOperations();
            var operations = operationRollOver.Union(operationRestructure).ToList();
            if (param.isLms)
            {
                tLoan = (from x in context.TBL_LOAN
                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                         join op in context.TBL_LOAN_REVIEW_OPERATION on x.TERMLOANID equals op.LOANID
                         join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                         && operations.Contains((short)op.OPERATIONTYPEID)
                         select new CRMSRegulatoryViewModel
                         {
                             accountNumber = c.PRODUCTACCOUNTNUMBER,
                             beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                             crmsCode = ld.CRMSCODE,
                             crmsDate = ld.CRMSDATE,
                             effectiveDate = x.EFFECTIVEDATE,
                             facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                             grantedAmount = x.PRINCIPALAMOUNT,
                             interestRate = x.INTERESTRATE,
                             loanId = x.TERMLOANID,
                             loanSystemTypeId = x.LOANSYSTEMTYPEID,
                             crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                             //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                             operationName = opn.OPERATIONNAME
                         }).ToList();

                revolving = (from x in context.TBL_LOAN_REVOLVING
                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                             join op in context.TBL_LOAN_REVIEW_OPERATION on x.REVOLVINGLOANID equals op.LOANID
                             join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                             where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                         && operations.Contains((short)op.OPERATIONTYPEID)
                             select new CRMSRegulatoryViewModel
                             {
                                 accountNumber = c.PRODUCTACCOUNTNUMBER,
                                 beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                 crmsCode = ld.CRMSCODE,
                                 crmsDate = ld.CRMSDATE,
                                 effectiveDate = x.EFFECTIVEDATE,
                                 facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                 grantedAmount = x.OVERDRAFTLIMIT,
                                 interestRate = x.INTERESTRATE,
                                 loanId = x.REVOLVINGLOANID,
                                 loanSystemTypeId = x.LOANSYSTEMTYPEID,
                                 crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                                 //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                                 operationName = opn.OPERATIONNAME
                             }).ToList();

                contingent = (from x in context.TBL_LOAN_CONTINGENT
                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                              join op in context.TBL_LOAN_REVIEW_OPERATION on x.CONTINGENTLOANID equals op.LOANID
                              join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                              join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                              join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                              where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                         && operations.Contains((short)op.OPERATIONTYPEID)
                              select new CRMSRegulatoryViewModel
                              {
                                  accountNumber = c.PRODUCTACCOUNTNUMBER,
                                  beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                  crmsCode = ld.CRMSCODE,
                                  crmsDate = ld.CRMSDATE,
                                  effectiveDate = x.EFFECTIVEDATE,
                                  facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                  grantedAmount = x.CONTINGENTAMOUNT,
                                  interestRate = 0,
                                  loanId = x.CONTINGENTLOANID,
                                  loanSystemTypeId = x.LOANSYSTEMTYPEID,
                                  crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                                  //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                                  operationName = opn.OPERATIONNAME
                                  //loansCount
                              }).ToList();
            }
            else
            {
                tLoan = (from x in context.TBL_LOAN
                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                         select new CRMSRegulatoryViewModel
                         {
                             accountNumber = c.PRODUCTACCOUNTNUMBER,
                             beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                             crmsCode = ld.CRMSCODE,
                             crmsDate = ld.CRMSDATE,
                             effectiveDate = x.EFFECTIVEDATE,
                             facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                             grantedAmount = x.PRINCIPALAMOUNT,
                             interestRate = x.INTERESTRATE,
                             loanId = x.TERMLOANID,
                             loanSystemTypeId = x.LOANSYSTEMTYPEID,
                             crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                             //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,

                         }).ToList();

                revolving = (from x in context.TBL_LOAN_REVOLVING
                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                             where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                             select new CRMSRegulatoryViewModel
                             {
                                 accountNumber = c.PRODUCTACCOUNTNUMBER,
                                 beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                 crmsCode = ld.CRMSCODE,
                                 crmsDate = ld.CRMSDATE,
                                 effectiveDate = x.EFFECTIVEDATE,
                                 facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                 grantedAmount = x.OVERDRAFTLIMIT,
                                 interestRate = x.INTERESTRATE,
                                 loanId = x.REVOLVINGLOANID,
                                 loanSystemTypeId = x.LOANSYSTEMTYPEID,
                                 crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                                 //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,

                             }).ToList();

                contingent = (from x in context.TBL_LOAN_CONTINGENT
                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                              join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                              join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                              where x.COMPANYID == param.companyId && ld.CRMSCODE == null
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                              select new CRMSRegulatoryViewModel
                              {
                                  accountNumber = c.PRODUCTACCOUNTNUMBER,
                                  beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                  crmsCode = ld.CRMSCODE,
                                  crmsDate = ld.CRMSDATE,
                                  effectiveDate = x.EFFECTIVEDATE,
                                  facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                  grantedAmount = x.CONTINGENTAMOUNT,
                                  interestRate = 0,
                                  loanId = x.CONTINGENTLOANID,
                                  loanSystemTypeId = x.LOANSYSTEMTYPEID,
                                  crmsLegalStatusId = b.CRMSLEGALSTATUSID,
                                  //tenor = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,

                                  //loansCount
                              }).ToList();
            }


            var loans = tLoan.Union(revolving).Union(contingent).ToList();


            return loans;
        }

        public List<LoansCount> LoanCountsByLegalStatus(List<CRMSRegulatoryViewModel> loans)
        {
            var groupLaon = from x in loans
                            group x by new { x.crmsLegalStatusId } into xx
                            select new LoansCount
                            {
                                count = xx.Count(),
                                crmsLegalStatusName = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSTYPEID == xx.Key.crmsLegalStatusId).Select(o => o.DESCRIPTION).FirstOrDefault(),
                                code = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSTYPEID == xx.Key.crmsLegalStatusId).Select(o => o.CODE).FirstOrDefault(),
                            };
            return groupLaon.ToList();

        }

        private CRMSRecord GenerateCRMS300Template(List<CRMSTemplateViewModel> loanInput, CRMSViewModel param, int forSingle = 0)
        {
            Byte[] fileBytes = null;
            CRMSRecord excel = new CRMSRecord();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FACILITIES");

                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    ws.Cells[1, 1].Value = "UNIQUE_IDENTIFICATION_TYPE";
                    ws.Cells[1, 2].Value = "UNIQUE_IDENTIFICATION_NO";
                    ws.Cells[1, 3].Value = "CREDIT_TYPE";
                    ws.Cells[1, 4].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES";
                    ws.Cells[1, 5].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR";
                    ws.Cells[1, 6].Value = "CREDIT_LIMIT";
                    ws.Cells[1, 7].Value = "OUTSTANDING_AMOUNT";
                    ws.Cells[1, 8].Value = "FEES (FEE_TYPE & FEE_AMOUNT)";
                    ws.Cells[1, 9].Value = "EFFECTIVE_DATE";
                    ws.Cells[1, 10].Value = "TENOR";
                    ws.Cells[1, 11].Value = "EXPIRY_DATE";
                    ws.Cells[1, 12].Value = "REPAYMENT_AGREEMENT_MODE";
                    ws.Cells[1, 13].Value = "INTEREST_RATE";
                    ws.Cells[1, 14].Value = "BENEFICIARY_ACCOUNT_NUMBER";
                    ws.Cells[1, 15].Value = "LOCATION_OF_BENEFICIARY";
                    ws.Cells[1, 16].Value = "RELATIONSHIP_TYPE";
                    ws.Cells[1, 17].Value = "COMPANY_SIZE";
                    ws.Cells[1, 18].Value = "FUNDING_SOURCE_CATEGORY";
                    ws.Cells[1, 19].Value = "ECCI_NUMBER";
                    ws.Cells[1, 20].Value = "FUNDING_SOURCE";
                    ws.Cells[1, 21].Value = "LEGAL_STATUS";
                    ws.Cells[1, 22].Value = "CLASSIFICATION_BY_BUSINESS_LINES";
                    ws.Cells[1, 23].Value = "CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR";
                    ws.Cells[1, 24].Value = "SPECIALISED_LOAN";
                    ws.Cells[1, 25].Value = "SPECIALISED_LOAN_MORATORIUM_PERIOD";
                    ws.Cells[1, 26].Value = "DIRECTOR_UNIQUE_IDENTIFIER";
                    ws.Cells[1, 27].Value = "SYNDICATION";
                    ws.Cells[1, 28].Value = "SYNDICATION_STATUS";
                    ws.Cells[1, 29].Value = "SYNDICATION_REF_NUMBER";
                    ws.Cells[1, 30].Value = "COLLATERAL_PRESENT";
                    ws.Cells[1, 31].Value = "COLLATERAL_SECURE";
                    ws.Cells[1, 32].Value = "SECURITY_TYPE";
                    ws.Cells[1, 33].Value = "ADDRESS_OF_SECURITY";
                    ws.Cells[1, 34].Value = "OWNER_OF_SECURITY";
                    ws.Cells[1, 35].Value = "UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER";
                    ws.Cells[1, 36].Value = "UNIQUE_IDENTIFIER_OF_SECURITY_OWNER";
                    ws.Cells[1, 37].Value = "GUARANTEE";
                    ws.Cells[1, 38].Value = "GUARANTEE_TYPE";
                    ws.Cells[1, 39].Value = "GUARANTOR_UNIQUE_IDENTIFICATION_TYPE";
                    ws.Cells[1, 40].Value = "GUARANTOR_UNIQUE_IDENTIFICATION";
                    ws.Cells[1, 41].Value = "AMOUNT_GUARANTEED";
                    ws.Cells[1, 42].Value = "LOAN_REFERENCE_NUMBER";

                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];

                       // var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();

                        //ws.Cells[i, 1].Value = i - 1;
                        ws.Cells[i, 1].Value = record.UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 2].Value = record.UNIQUE_IDENTIFICATION_NO;
                        ws.Cells[i, 3].Value = record.CREDIT_TYPE;
                        ws.Cells[i, 4].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES;
                        ws.Cells[i, 5].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR;
                        ws.Cells[i, 6].Value = record.CREDIT_LIMIT;
                        ws.Cells[i, 7].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 8].Value = record.FEES;
                        ws.Cells[i, 9].Value = record.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 10].Value = (record.EXPIRY_DATE - record.EFFECTIVE_DATE).TotalDays; //temor
                        ws.Cells[i, 11].Value = record.EXPIRY_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 12].Value = record.REPAYMENT_AGREEMENT_MODE;
                        ws.Cells[i, 13].Value = record.INTEREST_RATE;
                        ws.Cells[i, 14].Value = record.BENEFICIARY_ACCOUNT_NUMBER;
                        ws.Cells[i, 15].Value = record.LOCATION_OF_BENEFICIARY;
                        ws.Cells[i, 16].Value = record.RELATIONSHIP_TYPE;
                        ws.Cells[i, 17].Value = record.COMPANY_SIZE;
                        ws.Cells[i, 18].Value = record.FUNDING_SOURCE_CATEGORY;
                        ws.Cells[i, 19].Value = record.ECCI_NUMBER;
                        ws.Cells[i, 20].Value = record.FUNDING_SOURCE;
                        ws.Cells[i, 21].Value = record.LEGAL_STATUS;
                        ws.Cells[i, 22].Value = record.CLASSIFICATION_BY_BUSINESS_LINES;
                        ws.Cells[i, 23].Value = record.CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR;
                        ws.Cells[i, 24].Value = record.SPECIALISED_LOAN;
                        ws.Cells[i, 25].Value = record.MORATORIUMDURATION;//record.FIRSTPRINCIPALPAYMENTDATE != null ? ((DateTime)record.FIRSTPRINCIPALPAYMENTDATE - record.EFFECTIVE_DATE).TotalDays : 0;// record.SPECIALISED_LOAN_MORATORIUM_PERIOD;
                        ws.Cells[i, 26].Value = record.DIRECTOR_UNIQUE_IDENTIFIER;
                        ws.Cells[i, 27].Value = record.SYNDICATION;
                        ws.Cells[i, 28].Value = record.SYNDICATION_STATUS;
                        ws.Cells[i, 29].Value = record.SYNDICATION_REF_NUMBER;
                        ws.Cells[i, 30].Value = record.COLLATERAL_PRESENT;
                        ws.Cells[i, 31].Value = record.COLLATERAL_SECURE;
                        ws.Cells[i, 32].Value = record.SECURITY_TYPE;
                        ws.Cells[i, 33].Value = record.ADDRESS_OF_SECURITY;
                        ws.Cells[i, 34].Value = record.OWNER_OF_SECURITY;
                        if (record.UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER == 1){  ws.Cells[i, 35].Value = "BVN";}
                        else if (record.UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER == 2) { ws.Cells[i, 35].Value = "TIN"; }
                        else {  ws.Cells[i, 35].Value = ""; }
                        ws.Cells[i, 36].Value = record.UNIQUE_IDENTIFIER_OF_SECURITY_OWNER;
                        ws.Cells[i, 37].Value = record.GUARANTEE != null ? "YES" : "NO";
                        if (record.GUARANTEE_TYPE == 1) {  ws.Cells[i, 38].Value = "INDIVIDUAL"; }
                        else if (record.GUARANTEE_TYPE == 2) {  ws.Cells[i, 38].Value = "NON_INDIVIDUAL"; }
                        else { ws.Cells[i, 38].Value = "";}
                        ws.Cells[i, 39].Value = record.GUARANTOR_UNIQUE_IDENTIFICATION_TYPE != null ? "BVN" : "TIN"; //record.GUARANTOR_UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 40].Value = record.GUARANTOR_UNIQUE_IDENTIFICATION ;// record.GUARANTOR_UNIQUE_IDENTIFICATION;
                        ws.Cells[i, 41].Value = record.AMOUNT_GUARANTEED;// record.AMOUNT_GUARANTEED;
                        ws.Cells[i, 42].Value = record.REFERENCENUMBER; ;
                    }
                    var output = new List<CRMSTemplateViewModel>();
                    var directors = new List<CRMSTemplateViewModel>();

                    if (forSingle == 1)
                    {
                        output = GetFee(param).Where(a => a.LOANAPPLICATIONDETAILID == param.loanId).ToList(); 
                    }
                    else
                    {
                        output = GetFee(param).Where(x => DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)).ToList();
                    }


                    if (output != null)
                    {
                        output = output.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).Select(x => x).ToList();

                        ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("FEE");
                        ws2.Cells[1, 1].Value = "ACCOUNT";
                        ws2.Cells[1, 2].Value = "FEE_TYPE";
                        ws2.Cells[1, 3].Value = "FEE_AMOUNT";

                        for (int i = 2; i <= output.Count + 1; i++)
                        {
                            var feeRecord = output[i - 2];

                            ws2.Cells[i, 1].Value = feeRecord.ACCOUNT;
                            ws2.Cells[i, 2].Value = feeRecord.FEE_TYPE;
                            ws2.Cells[i, 3].Value = feeRecord.FEE_AMOUNT;
                        }


                    }

                    //var directors = GetDirectors(param);
                    if (forSingle == 1)
                    {
                        directors = GetDirectors(param).Where(a => a.LOANAPPLICATIONDETAILID == param.loanId).ToList(); 
                    }
                    else
                    {
                        directors = GetDirectors(param).Where(x => DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)).ToList();
                    }

                    if (directors != null)
                    {
                        directors = directors.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).Select(x => x).ToList();

                        ExcelWorksheet ws3 = pck.Workbook.Worksheets.Add("DIRECTORS");
                        ws3.Cells[1, 1].Value = "ACCOUNT";
                        ws3.Cells[1, 2].Value = "ID_TTPE";
                        ws3.Cells[1, 3].Value = "ID_DETAIL";
                        ws3.Cells[1, 4].Value = "EMAIL";

                        for (int i = 2; i <= directors.Count + 1; i++)
                        {
                            var record = directors[i - 2];

                            ws3.Cells[i, 1].Value = record.ACCOUNT;
                            ws3.Cells[i, 2].Value = record.ID_TTPE;
                            ws3.Cells[i, 3].Value = record.ID_DETAIL;
                            ws3.Cells[i, 4].Value = record.EMAIL;
                        }
                    }

                    fileBytes = pck.GetAsByteArray();
                    excel.reportData = fileBytes;
                    excel.templateTypeName = "CRMS_T300";

                }

            }

            return excel;
        }
        private CRMSRecord GenerateCRMS400BTemplate(List<CRMSTemplateViewModel> loanInput, CRMSViewModel param)
        {
            Byte[] fileBytes = null;
            CRMSRecord excel = new CRMSRecord();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Restructuring Existing Facility");
                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("DIRECTORS");

                    ws.Cells[1, 1].Value = "UNIQUE_IDENTIFICATION_TYPE";
                    ws.Cells[1, 2].Value = "UNIQUE_IDENTIFICATION_NO";
                    ws.Cells[1, 3].Value = "CRMS_REFERENCE_NUMBER";
                    ws.Cells[1, 4].Value = "CREDIT_TYPE";
                    ws.Cells[1, 5].Value = "CREDIT_LIMIT";
                    ws.Cells[1, 6].Value = "OUTSTANDING_AMOUNT";
                    ws.Cells[1, 7].Value = "FEES (FEE_TYPE & FEE_AMOUNT)";
                    ws.Cells[1, 8].Value = "EFFECTIVE_DATE";
                    ws.Cells[1, 9].Value = "TENOR";
                    ws.Cells[1, 10].Value = "EXPIRY_DATE";
                    ws.Cells[1, 11].Value = "REPAYMENT_AGREEMENT_MODE";
                    ws.Cells[1, 12].Value = "PERFORMANCE_REPAYMENT_STATUS";
                    ws.Cells[1, 13].Value = "REASON_FOR_RESTRUCTURING";
                    ws.Cells[1, 14].Value = "INTEREST_RATE";
                    ws.Cells[1, 15].Value = "BENEFICIARY_ACCOUNT_NUMBER";
                    ws.Cells[1, 16].Value = "LOCATION_OF_BENEFICIARY";
                    ws.Cells[1, 17].Value = "RELATIONSHIP_TYPE";
                    ws.Cells[1, 18].Value = "FUNDING_SOURCE_CATEGORY";
                    ws.Cells[1, 19].Value = "ECCI_NUMBER";
                    ws.Cells[1, 20].Value = "FUNDING_SOURCE";
                    ws.Cells[1, 21].Value = "SPECIALISED_LOAN";
                    ws.Cells[1, 22].Value = "SPECIALISED_LOAN_MORATORIUM_PERIOD";
                    ws.Cells[1, 23].Value = "DIRECTOR_UNIQUE_IDENTIFIER";
                    ws.Cells[1, 24].Value = "COLLATERAL_PRESENT";
                    ws.Cells[1, 25].Value = "COLLATERAL_SECURE";
                    ws.Cells[1, 26].Value = "SECURITY_TYPE";
                    ws.Cells[1, 27].Value = "ADDRESS_OF_SECURITY";
                    ws.Cells[1, 28].Value = "OWNER_OF_SECURITY";
                    ws.Cells[1, 29].Value = "UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER";
                    ws.Cells[1, 30].Value = "UNIQUE_IDENTIFIER_OF_SECURITY_OWNER";
                    ws.Cells[1, 31].Value = "GUARANTEE";
                    ws.Cells[1, 32].Value = "GUARANTEE_TYPE";
                    ws.Cells[1, 33].Value = "GUARANTOR_UNIQUE_IDENTIFICATION_TYPE";
                    ws.Cells[1, 34].Value = "GUARANTOR_UNIQUE_IDENTIFICATION";
                    ws.Cells[1, 35].Value = "OPERATION_NAME";
                    ws.Cells[1, 36].Value = "DATE CREATED";
                    ws.Cells[1, 37].Value = "LOAN_REFERENCE_NUMBER";
                    //ws.Cells[1, 38].Value = "LOANREVIEWOPERATIONID";
                    //ws.Cells[1, 39].Value = "CUSTOMERID";




                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];

                      //  var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();
                        ws.Cells[i, 1].Value = record.UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 2].Value = record.UNIQUE_IDENTIFICATION_NO;
                        ws.Cells[i, 3].Value = record.CRMSCODE;
                        ws.Cells[i, 4].Value = record.CREDIT_TYPE;
                        ws.Cells[i, 5].Value = record.CREDIT_LIMIT;
                        ws.Cells[i, 6].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 7].Value = record.FEES;
                        ws.Cells[i, 8].Value = record.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        if (record.MATURITYDATE != null)
                        {
                            var proposedTenor = ((DateTime)record.MATURITYDATE - record.EFFECTIVE_DATE).TotalDays;
                            var result = "";
                            var units = proposedTenor == 1 ? " day" : " days";
                            if (proposedTenor < 15)
                            {
                                result = "1";//proposedTenor.ToString() + units;
                            }
                            else
                            {
                                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                                units = months == 1 ? " month" : " months";
                                result = months.ToString(); //+ " " + units;
                            }


                            ws.Cells[i, 9].Value = result;

                            ws.Cells[i, 10].Value = ((DateTime)record.MATURITYDATE).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            ws.Cells[i, 9].Value = "NULL";
                            ws.Cells[i, 10].Value = "NULL"; ;
                        }
                        ws.Cells[i, 11].Value = record.REPAYMENT_AGREEMENT_MODE;
                        ws.Cells[i, 12].Value = record.PERFORMANCE_REPAYMENT_STATUS;
                        ws.Cells[i, 13].Value = record.REASON_FOR_RESTRUCTURING;
                        ws.Cells[i, 14].Value = (record.INTEREST_RATE / 100);
                        ws.Cells[i, 15].Value = record.BENEFICIARY_ACCOUNT_NUMBER;
                        ws.Cells[i, 16].Value = record.LOCATION_OF_BENEFICIARY;
                        ws.Cells[i, 17].Value = record.RELATIONSHIP_TYPE;
                        ws.Cells[i, 18].Value = record.FUNDING_SOURCE_CATEGORY;
                        ws.Cells[i, 19].Value = record.ECCI_NUMBER;
                        ws.Cells[i, 20].Value = record.FUNDING_SOURCE;
                        ws.Cells[i, 21].Value = record.SPECIALISED_LOAN;
                        if (record.SPECIALISED_LOAN == "NO")
                        {
                            ws.Cells[i, 22].Value = 0;
                        }
                        else
                        {
                            ws.Cells[i, 22].Value = record.FIRSTPRINCIPALPAYMENTDATE != null ? ((DateTime)record.FIRSTPRINCIPALPAYMENTDATE - record.EFFECTIVE_DATE).TotalDays : 0;
                        }
                        ws.Cells[i, 23].Value = record.DIRECTOR_UNIQUE_IDENTIFIER;
                        ws.Cells[i, 24].Value = record.COLLATERAL_PRESENT;

                        if (record.COLLATERAL_PRESENT == "NO")
                        {
                            ws.Cells[i, 25].Value = "NO";
                        }
                        else
                        {
                            ws.Cells[i, 25].Value = record.COLLATERAL_SECURE;
                        }
                        ws.Cells[i, 26].Value = record.SECURITY_TYPE;
                        ws.Cells[i, 27].Value = record.ADDRESS_OF_SECURITY;
                        ws.Cells[i, 28].Value = record.OWNER_OF_SECURITY;
                        if (record.UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER==1)
                        {
                            ws.Cells[i, 29].Value = "BVN";
                        }
                        else if (record.UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER == 2)
                        {
                            ws.Cells[i, 29].Value = "TIN";
                        }
                        else{
                            ws.Cells[i, 29].Value = "";
                        }
                        ws.Cells[i, 30].Value = record.UNIQUE_IDENTIFIER_OF_SECURITY_OWNER;
                        ws.Cells[i, 31].Value = record.GUARANTEE;
                        if (record.GUARANTEE_TYPE == 1)
                        {
                            ws.Cells[i, 32].Value = "INDIVIDUAL";
                        }
                        else if (record.GUARANTEE_TYPE == 2)
                        {
                            ws.Cells[i, 32].Value = "NON_INDIVIDUAL";
                        }
                        else
                        {
                            ws.Cells[i, 32].Value = "";
                        }
                        ws.Cells[i, 33].Value = record.GUARANTOR_UNIQUE_IDENTIFICATION_TYPE; //record.GUARANTOR_UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 34].Value = record.GUARANTOR_UNIQUE_IDENTIFICATION;// record.GUARANTOR_UNIQUE_IDENTIFICATION;
                        ws.Cells[i, 35].Value = record.OPERATION_NAME;
                        ws.Cells[i, 36].Value = record.DATETIMECREATED.ToString("dd/MM/yyyy");
                        ws.Cells[i, 37].Value = record.REFERENCENUMBER;
                        //ws.Cells[i, 38].Value = record.LOANREVIEWOPERATIONID;
                        //ws.Cells[i, 39].Value = record.CUSTOMERID;



                    }
                    var director = GetDirectors400B(param);
                    ws2.Cells[1, 1].Value = "ACCOUNT";
                    ws2.Cells[1, 2].Value = "ID_TTPE";
                    ws2.Cells[1, 3].Value = "ID_DETAIL";
                    ws2.Cells[1, 4].Value = "EMAIL";
                    //   ws2.Cells[1, 5].Value = "CRMSCODE";
                    //ws2.Cells[1, 6].Value = "LOANREVIEWOPERATIONID";
                    //ws2.Cells[1, 7].Value = "CUSTOMERID";

                    for (int j = 2; j <= director.Count + 1; j++)
                    {
                        var directorRecord = director[j - 2];

                        ws2.Cells[j, 1].Value = directorRecord.ACCOUNT;
                        ws2.Cells[j, 2].Value = directorRecord.ID_TTPE;
                        ws2.Cells[j, 3].Value = directorRecord.ID_DETAIL;
                        ws2.Cells[j, 4].Value = directorRecord.EMAIL;
                        //  ws2.Cells[j, 5].Value = directorRecord.CRMSCODE;
                        //ws2.Cells[j, 6].Value = directorRecord.LOANREVIEWOPERATIONID;
                        //ws2.Cells[j, 7].Value = directorRecord.CUSTOMERID;

                    }



                    var fee = GetFee400B(param);
                    ExcelWorksheet ws3 = pck.Workbook.Worksheets.Add("FEE");
                    ws3.Cells[1, 1].Value = "ACCOUNT";
                    ws3.Cells[1, 2].Value = "FEE_TYPE";
                    ws3.Cells[1, 3].Value = "FEE_AMOUNT";


                    for (int i = 2; i <= fee.Count + 1; i++)
                    {
                        var feeRecord = fee[i - 2];

                        ws3.Cells[i, 1].Value = feeRecord.ACCOUNT;
                        ws3.Cells[i, 2].Value = feeRecord.FEE_TYPE_NAME;
                        ws3.Cells[i, 3].Value = feeRecord.FEE_AMOUNT;

                    }
                    fileBytes = pck.GetAsByteArray();
                    excel.reportData = fileBytes;
                    excel.templateTypeName = "CRMS_T400B";
                }
            }

            return excel;
        }
        private List<CRMSTemplateViewModel> GetFee400B(CRMSViewModel param)
        {
            var term = from x in context.TBL_LOAN
                       join op in context.TBL_LOAN_REVIEW_OPERATION on x.TERMLOANID equals op.LOANID
                       join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                       join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                       join f in context.TBL_LOAN_FEE on op.LOANREVIEWOPERATIONID equals f.LOANREVIEWOPERATIONID
                       join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                       join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                       join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
                       join crms in context.TBL_CRMS_REGULATORY on cf.CRMSREGULATORYID equals crms.CRMSREGULATORYID
                       where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                       && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                       && f.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                       select new CRMSTemplateViewModel
                       {
                           ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                           FEE_TYPE = cf.CRMSREGULATORYID,
                           FEE_TYPE_NAME = crms.CODE,
                           FEE_AMOUNT = f.FEEAMOUNT,
                           CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID
                       };
            var OD = from x in context.TBL_LOAN_REVOLVING
                     join op in context.TBL_LOAN_REVIEW_OPERATION on x.REVOLVINGLOANID equals op.LOANID
                     join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                     join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                     join f in context.TBL_LOAN_FEE on op.LOANREVIEWOPERATIONID equals f.LOANREVIEWOPERATIONID
                     join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                     join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
                     join crms in context.TBL_CRMS_REGULATORY on cf.CRMSREGULATORYID equals crms.CRMSREGULATORYID
                     where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                     && f.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

                     select new CRMSTemplateViewModel
                     {
                         ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                         FEE_TYPE = cf.CRMSREGULATORYID,
                         FEE_TYPE_NAME = crms.CODE,
                         FEE_AMOUNT = f.FEEAMOUNT,
                         CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID

                     };
            var contingent = from x in context.TBL_LOAN_CONTINGENT
                             join op in context.TBL_LOAN_REVIEW_OPERATION on x.CONTINGENTLOANID equals op.LOANID
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                             join f in context.TBL_LOAN_FEE on op.LOANREVIEWOPERATIONID equals f.LOANREVIEWOPERATIONID
                             join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                             join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                             join cf in context.TBL_CHARGE_FEE on f.CHARGEFEEID equals cf.CHARGEFEEID
                             join crms in context.TBL_CRMS_REGULATORY on cf.CRMSREGULATORYID equals crms.CRMSREGULATORYID

                             where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                             && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                             && f.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

                             select new CRMSTemplateViewModel
                             {
                                 ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                                 FEE_TYPE = cf.CRMSREGULATORYID,
                                 FEE_TYPE_NAME = crms.CODE,
                                 FEE_AMOUNT = f.FEEAMOUNT,
                                 CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID
                             };


            return term.Union(OD).Union(contingent).OrderBy(x => x.ACCOUNT).ToList();
        }
        private List<CRMSTemplateViewModel> GetDirectors400B(CRMSViewModel param)
        {
            var operationRestructure = CommonHelpers.GetRestructureOperations();

            var term = from x in context.TBL_LOAN
                       join op in context.TBL_LOAN_REVIEW_OPERATION on x.TERMLOANID equals op.LOANID
                       join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                       join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                       join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                       join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                       join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                       join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                       join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                       join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID
                       join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID


                       where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                      && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                      && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                      && operationRestructure.Contains((short)op.OPERATIONTYPEID)
                       select new CRMSTemplateViewModel
                       {

                           ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                           ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                           ID_DETAIL = d.CUSTOMERTYPEID == 2 ? d.TAX_NUMBER : d.CUSTOMERBVN,
                           CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                           EMAIL = d.EMAILADDRESS,
                           CRMSCODE = ld.CRMSCODE,
                           LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID,
                           CUSTOMERID = x.CUSTOMERID,


                       };
            var OD = from x in context.TBL_LOAN_REVOLVING
                     join op in context.TBL_LOAN_REVIEW_OPERATION on x.REVOLVINGLOANID equals op.LOANID
                     join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                     join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                     join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                     join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                     join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                     join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                     join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID
                     join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID

                     where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                    && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                    && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                    && operationRestructure.Contains((short)op.OPERATIONTYPEID)

                     select new CRMSTemplateViewModel
                     {
                         //ACCOUNT = d.CUSTOMERBVN,

                         ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                         ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                         ID_DETAIL = d.CUSTOMERTYPEID == 2 ? d.TAX_NUMBER : d.CUSTOMERBVN,
                         CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                         EMAIL = d.EMAILADDRESS,
                         CRMSCODE = ld.CRMSCODE,
                         LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID,
                         CUSTOMERID = x.CUSTOMERID,



                     };
            var contingent = from x in context.TBL_LOAN_CONTINGENT
                             join op in context.TBL_LOAN_REVIEW_OPERATION on x.CONTINGENTLOANID equals op.LOANID
                             join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                             join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                             join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                             join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                             join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID
                             join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID


                             where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                            && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                            && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                            && operationRestructure.Contains((short)op.OPERATIONTYPEID)
                             select new CRMSTemplateViewModel
                             {
                                 ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                                 ID_TTPE = d.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                                 ID_DETAIL = d.CUSTOMERTYPEID == 2 ? d.TAX_NUMBER : d.CUSTOMERBVN,
                                 CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                                 EMAIL = d.EMAILADDRESS,
                                 CRMSCODE = ld.CRMSCODE,
                                 LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID,
                                 CUSTOMERID = x.CUSTOMERID,

                             };

            return term.Union(OD).Union(contingent).OrderBy(x => x.ACCOUNT).ToList();
        }
        private IQueryable<CRMSTemplateViewModel> GenerateCRMSReport400B(CRMSViewModel param)
        {
            // int[] crmsRegulatoryIds = { (int)CRMSRegulatory.Government, (int)CRMSRegulatory.Parastatals_MDA };

            var tLoan = new List<CRMSTemplateViewModel>();
            var revolving = new List<CRMSTemplateViewModel>();
            var contingent = new List<CRMSTemplateViewModel>();
            var operationRestructure = CommonHelpers.GetRestructureOperations();
            var year = DateTime.Parse("01/01/0001");
            tLoan = (from x in context.TBL_LOAN
                     join op in context.TBL_LOAN_REVIEW_OPERATION on x.TERMLOANID equals op.LOANID
                     join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                     join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                     join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                     join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                     join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                     join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                     join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

                     let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                     let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                     let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()

                     let collateralCustomer = context.TBL_LOAN_COLLATERAL_MAPPING.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
                     new { mp, col }).Where(dd => dd.mp.LOANID == x.TERMLOANID).OrderByDescending(map => map.mp.LOANCOLLATERALMAPPINGID).Select(dd => dd).FirstOrDefault()

                     let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                     let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

                     let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                     where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                     && operationRestructure.Contains((short)op.OPERATIONTYPEID)

                     select new CRMSTemplateViewModel
                     {
                         CUSTOMERID = x.CUSTOMERID,
                         BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                         EFFECTIVE_DATE = op.EFFECTIVEDATE == null || op.EFFECTIVEDATE.Year == year.Year ? x.EFFECTIVEDATE : op.EFFECTIVEDATE,
                         MATURITYDATE = op.MATURITYDATE == null || op.MATURITYDATE.Value.Year == year.Year ? x.MATURITYDATE : op.MATURITYDATE,

                         CREDIT_LIMIT = x.PRINCIPALAMOUNT,
                         INTEREST_RATE = op.INTERATERATE == null ? x.INTERESTRATE : op.INTERATERATE,
                         UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                         UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                         CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                         // CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                         // CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
                         CRMSCODE = ld.CRMSCODE,

                         FEES = "",
                         //TENOR = (op.MATURITYDATE - op.EFFECTIVEDATE).TotalDays,
                         REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                         LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                         RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                         COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                         FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                         ECCI_NUMBER = ld.CRMS_ECCI_NUMBER,
                         REASON_FOR_RESTRUCTURING = op.REVIEWDETAILS,
                         FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                         // LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                         // CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         // CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         SPECIALISED_LOAN = ld.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                    // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                         DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),

                         //SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                         //SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                         // SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                         COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.TERMLOANID).Any() ? "YES" : "NO",
                         COLLATERAL_SECURE = ld.SECUREDBYCOLLATERAL ? "YES" : "NO",
                         SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == context.TBL_LOAN_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(y => y.CRMSCOLLATERALTYPEID).FirstOrDefault()).Select(o => o.CODE).FirstOrDefault(),
                         ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
                         OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
                         UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
                         UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
                         GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
                         GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID, //if has collteral guarantee
                         GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
                         GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
                         AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
                         FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                         LOANID = x.TERMLOANID,
                         CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                         //100
                         GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                         REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                         //200
                         GOVERNMENT_MDA_TIN = b.TAXNUMBER,
                         PERFORMANCE_REPAYMENT_STATUS = x.USER_PRUDENTIAL_GUIDE_STATUSID == 1 ? "100" : "103",
                         //600
                         //SYNDICATION_NAME = a.FIELD2,
                         //SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                         PARTICIPATING_BANK_CODE = "",
                         OPERATION_NAME = opn.OPERATIONNAME,
                         DATETIMECREATED = op.DATECREATED,
                         REFERENCENUMBER = x.LOANREFERENCENUMBER,
                         LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID

                     }).ToList();

            revolving = (from x in context.TBL_LOAN_REVOLVING
                         join op in context.TBL_LOAN_REVIEW_OPERATION on x.REVOLVINGLOANID equals op.LOANID
                         join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                         join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                         join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                         join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

                         let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                         let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                         let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()

                         let collateralCustomer = context.TBL_LOAN_COLLATERAL_MAPPING.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
                    new { mp, col }).Where(dd => dd.mp.LOANID == x.REVOLVINGLOANID).OrderByDescending(map => map.mp.LOANCOLLATERALMAPPINGID).Select(dd => dd).FirstOrDefault()

                         let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                         let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

                         let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                         where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                     && operationRestructure.Contains((short)op.OPERATIONTYPEID)
                         select new CRMSTemplateViewModel
                         {
                             CUSTOMERID = x.CUSTOMERID,

                             BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                             EFFECTIVE_DATE = op.EFFECTIVEDATE == null || op.EFFECTIVEDATE.Year == year.Year ? x.EFFECTIVEDATE : op.EFFECTIVEDATE,
                             MATURITYDATE = op.MATURITYDATE == null || op.MATURITYDATE.Value.Year == year.Year ? x.MATURITYDATE : op.MATURITYDATE,
                             CREDIT_LIMIT = x.OVERDRAFTLIMIT,
                             INTEREST_RATE = op.INTERATERATE == null ? x.INTERESTRATE : op.INTERATERATE,
                             UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                             UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                             CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                             // CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                             // CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             //OUTSTANDING_AMOUNT = x.,
                             CRMSCODE = ld.CRMSCODE,

                             FEES = "",
                             //TENOR = (op.MATURITYDATE - op.EFFECTIVEDATE).TotalDays,
                             REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                             LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                             RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                             COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                             FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                             ECCI_NUMBER = ld.CRMS_ECCI_NUMBER,
                             REASON_FOR_RESTRUCTURING = op.REVIEWDETAILS,
                             FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                             // LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                             // CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             // CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             SPECIALISED_LOAN = ld.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                        // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                             DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),

                             //SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                             //SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                             // SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                             COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.REVOLVINGLOANID).Any() ? "YES" : "NO",
                             COLLATERAL_SECURE = ld.SECUREDBYCOLLATERAL ? "YES" : "NO",
                             SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == context.TBL_LOAN_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(y => y.CRMSCOLLATERALTYPEID).FirstOrDefault()).Select(o => o.CODE).FirstOrDefault(),
                             ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
                             OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
                             UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
                             UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
                             GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
                             GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID, //if has collteral guarantee
                             GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
                             GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
                             AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
                             // FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                             LOANID = x.REVOLVINGLOANID,
                             CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                             //100
                             GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                             REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                             //200
                             GOVERNMENT_MDA_TIN = b.TAXNUMBER,
                             PERFORMANCE_REPAYMENT_STATUS = x.USER_PRUDENTIAL_GUIDE_STATUSID == 1 ? "100" : "103",

                             //600
                             //SYNDICATION_NAME = a.FIELD2,
                             //SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                             PARTICIPATING_BANK_CODE = "",
                             OPERATION_NAME = opn.OPERATIONNAME,
                             DATETIMECREATED = op.DATECREATED,
                             REFERENCENUMBER = x.LOANREFERENCENUMBER,
                             LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID

                         }).ToList();


            contingent = (from x in context.TBL_LOAN_CONTINGENT
                          join op in context.TBL_LOAN_REVIEW_OPERATION on x.CONTINGENTLOANID equals op.LOANID
                          join opn in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals opn.OPERATIONID
                          join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                          join a in context.TBL_LMSR_APPLICATION_DETAIL on op.LOANREVIEWAPPLICATIONID equals a.LOANREVIEWAPPLICATIONID
                          join l in context.TBL_LMSR_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                          join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                          join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                          join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

                          join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                          let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                          let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                          let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()

                          let collateralCustomer = context.TBL_LOAN_COLLATERAL_MAPPING.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
                    new { mp, col }).Where(dd => dd.mp.LOANID == x.CONTINGENTLOANID).OrderByDescending(map => map.mp.LOANCOLLATERALMAPPINGID).Select(dd => dd).FirstOrDefault()

                          let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                          let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

                          let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                          where x.COMPANYID == param.companyId && x.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                          && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                     && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                     && operationRestructure.Contains((short)op.OPERATIONTYPEID)
                          select new CRMSTemplateViewModel
                          {
                              CUSTOMERID = x.CUSTOMERID,

                              BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                              EFFECTIVE_DATE = op.EFFECTIVEDATE == null || op.EFFECTIVEDATE.Year == year.Year ? x.EFFECTIVEDATE : op.EFFECTIVEDATE,
                              MATURITYDATE = op.MATURITYDATE == null || op.MATURITYDATE.Value.Year == year.Year ? x.MATURITYDATE : op.MATURITYDATE,
                              CREDIT_LIMIT = x.CONTINGENTAMOUNT,
                              INTEREST_RATE = op.INTERATERATE == null ? 0 : op.INTERATERATE,
                              UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                              UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                              CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                              // CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                              // CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              //OUTSTANDING_AMOUNT = x.,
                              CRMSCODE = ld.CRMSCODE,

                              FEES = "",
                              //TENOR = (op.MATURITYDATE - op.EFFECTIVEDATE).TotalDays,
                              REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                              LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                              RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                              COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                              FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                              ECCI_NUMBER = ld.CRMS_ECCI_NUMBER,
                              REASON_FOR_RESTRUCTURING = op.REVIEWDETAILS,
                              FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == ld.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                              // LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                              // CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              // CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              SPECIALISED_LOAN = ld.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                         // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                              DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),

                              //SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                              //SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                              // SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                              COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.CONTINGENTLOANID).Any() ? "YES" : "NO",
                              COLLATERAL_SECURE = ld.SECUREDBYCOLLATERAL ? "YES" : "NO",
                              SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == context.TBL_LOAN_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(y => y.CRMSCOLLATERALTYPEID).FirstOrDefault()).Select(o => o.CODE).FirstOrDefault(),
                              ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
                              OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
                              UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
                              UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
                              GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
                              GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID, //if has collteral guarantee
                              GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
                              GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
                              AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
                              // FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                              LOANID = x.CONTINGENTLOANID,
                              CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                              //100
                              GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                              REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                              //200
                              GOVERNMENT_MDA_TIN = b.TAXNUMBER,
                              PERFORMANCE_REPAYMENT_STATUS = "100",

                              //600
                              //SYNDICATION_NAME = a.FIELD2,
                              //SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                              PARTICIPATING_BANK_CODE = "",
                              OPERATION_NAME = opn.OPERATIONNAME,
                              DATETIMECREATED = op.DATECREATED,
                              REFERENCENUMBER = x.LOANREFERENCENUMBER,
                              LOANREVIEWOPERATIONID = op.LOANREVIEWOPERATIONID

                          }).ToList();


            var data = tLoan.Union(revolving).Union(contingent).AsQueryable();

            return data;
        }
        private CRMSRecord GenerateCRMS400CTemplate(List<CRMSTemplateViewModel> loanInput, CRMSViewModel param)
        {
            Byte[] fileBytes = null;
            CRMSRecord excel = new CRMSRecord();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Restructuring Existing Facility");

                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    ws.Cells[1, 1].Value = "UNIQUE_IDENTIFICATION_TYPE";
                    ws.Cells[1, 2].Value = "UNIQUE_IDENTIFICATION_NO";
                    ws.Cells[1, 3].Value = "CRMS_REFERENCE_NUMBER";
                    ws.Cells[1, 4].Value = "OUTSTANDING_AMOUNT";
                    ws.Cells[1, 5].Value = "PERFORMANCE_REPAYMENT_STATUS";
                    ws.Cells[1, 6].Value = "TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES";
                    ws.Cells[1, 7].Value = "TOTAL_BANK_INDUCED_CREDIT_WRITEOFF";
                    ws.Cells[1, 8].Value = "TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN";
                    ws.Cells[1, 9].Value = "TOTAL_CUSTOMER_INDUCED_CREDIT";
                    ws.Cells[1, 10].Value = "TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE";
                    ws.Cells[1, 11].Value = "TOTAL_CUSTOMER_INDUCED_DEBIT_AMT";
                    ws.Cells[1, 12].Value = "TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE";
                    ws.Cells[1, 13].Value = "UNAMORTIZED_CREDIT_CHARGES";
                    ws.Cells[1, 14].Value = "LIQUIDATION ";
                    ws.Cells[1, 15].Value = "LOAN_REFERENCE_NUMBER";


                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];

                        var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();
                        ws.Cells[i, 1].Value = record.UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 2].Value = record.UNIQUE_IDENTIFICATION_NO;
                        ws.Cells[i, 3].Value = record.CRMSCODE;
                        ws.Cells[i, 4].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 5].Value = record.PERFORMANCE_REPAYMENT_STATUS;
                        ws.Cells[i, 6].Value = record.TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES;
                        ws.Cells[i, 7].Value = record.TOTAL_BANK_INDUCED_CREDIT_WRITEOFF;
                        ws.Cells[i, 8].Value = record.TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN;
                        ws.Cells[i, 9].Value = record.TOTAL_CUSTOMER_INDUCED_CREDIT;
                        ws.Cells[i, 10].Value = record.TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE;
                        ws.Cells[i, 11].Value = record.TOTAL_CUSTOMER_INDUCED_DEBIT_AMT;
                        ws.Cells[i, 12].Value = record.TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE;
                        ws.Cells[i, 13].Value = record.UNAMORTIZED_CREDIT_CHARGES;
                        if (record.OUTSTANDING_AMOUNT == 0)
                        {
                            ws.Cells[i, 14].Value = "YES";
                        }
                        else
                        {
                            ws.Cells[i, 14].Value = "NO";
                        }
                        ws.Cells[i, 15].Value = record.REFERENCENUMBER;
                    }
                    fileBytes = pck.GetAsByteArray();
                    excel.reportData = fileBytes;
                    excel.templateTypeName = "CRMS_T400C";
                }
            }

            return excel;
        }

        private CRMSRecord GenerateCRMS400CTemplate(CRMSViewModel param)
        {
            var result = GenerateCRMSReport400C(param);
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA);
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T300");

            return GenerateCRMS400CTemplate(result.ToList(), param);
        }
        private IQueryable<CRMSTemplateViewModel> GenerateCRMSReport400C(CRMSViewModel param)
        {
            // int[] crmsRegulatoryIds = { (int)CRMSRegulatory.Government, (int)CRMSRegulatory.Parastatals_MDA };

            var tLoan = new List<CRMSTemplateViewModel>();
            var revolving = new List<CRMSTemplateViewModel>();
            var contingent = new List<CRMSTemplateViewModel>();

            tLoan = (from x in context.TBL_LOAN
                     join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                     join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                     join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                     join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                     join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID
                     let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                     let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                     let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()
                     where x.COMPANYID == param.companyId



                     select new CRMSTemplateViewModel
                     {
                         BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                         EFFECTIVE_DATE = x.EFFECTIVEDATE,
                         CREDIT_LIMIT = x.PRINCIPALAMOUNT,
                         INTEREST_RATE = x.INTERESTRATE,
                         UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                         UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                         CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL + x.PASTDUEPRINCIPAL,
                         FEES = "",
                         CRMSCODE = a.CRMSCODE,
                         // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                         EXPIRY_DATE = x.MATURITYDATE,

                         REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                         LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                         RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                         COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                         FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                         ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                         FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                         LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                         CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                   // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                         DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                         SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                         SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                         SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                         COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.TERMLOANID).Any() ? "YES" : "NO",
                         COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                         SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSCOLLATERALTYPEID).Select(o => o.CODE).FirstOrDefault(),
                         ADDRESS_OF_SECURITY = "",
                         OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                                                 //  UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                         UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                         GUARANTEE = "", //if has collteral guarantee
                         GUARANTEE_TYPE = null, //if has collteral guarantee
                         GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                         GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                         AMOUNT_GUARANTEED = null, //amount
                         FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                         LOANID = x.TERMLOANID,
                         CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                         //100
                         GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                         REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                         //200
                         GOVERNMENT_MDA_TIN = b.TAXNUMBER,
                         PERFORMANCE_REPAYMENT_STATUS = x.USER_PRUDENTIAL_GUIDE_STATUSID == 1 ? "100" : "103",

                         //PERFORMANCE_REPAYMENT_STATUS = context.TBL_CRMS_REGULATORY.Where(cr=>cr.CRMSREGULATORYID == x.CRMSREPAYMENTAGREEMENTID).Select(s=>s.DESCRIPTION).FirstOrDefault(),
                         TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES = 0,
                         TOTAL_BANK_INDUCED_CREDIT_WRITEOFF = 0,
                         TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN = 0,
                         TOTAL_CUSTOMER_INDUCED_CREDIT = 0,
                         TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE = "TRF",
                         TOTAL_CUSTOMER_INDUCED_DEBIT_AMT = 0,
                         TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE = "TRF",
                         UNAMORTIZED_CREDIT_CHARGES = 0,
                         //LIQUIDATION = x.OUTSTANDINGPRINCIPAL + x.PASTDUEPRINCIPAL == 0 ? "NO": "YES",
                         REFERENCENUMBER = x.LOANREFERENCENUMBER,

                         //600
                         SYNDICATION_NAME = a.FIELD2,
                         SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                         PARTICIPATING_BANK_CODE = "",

                     }).ToList();

            revolving = (from x in context.TBL_LOAN_REVOLVING
                         join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                         join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

                         let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                         let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                         let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()
                         where x.COMPANYID == param.companyId

                         select new CRMSTemplateViewModel
                         {
                             BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                             EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
                             CREDIT_LIMIT = x.OVERDRAFTLIMIT,
                             INTEREST_RATE = x.INTERESTRATE,
                             UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                             UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                             CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                             CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                             CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             OUTSTANDING_AMOUNT = c.AVAILABLEBALANCE < 0 ? Math.Abs(c.AVAILABLEBALANCE) : 0,
                             FEES = "",
                             CRMSCODE = a.CRMSCODE,

                             // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                             EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
                             REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                             LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                             RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                             COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                             FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                             ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                             FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                             LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                             CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                       // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                             DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                             SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                             SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                             SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                             COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.REVOLVINGLOANID).Any() ? "YES" : "NO",
                             COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                             SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSCOLLATERALTYPEID).Select(o => o.CODE).FirstOrDefault(),
                             ADDRESS_OF_SECURITY = "",
                             OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                                                     // UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                             UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                             GUARANTEE = "", //if has collteral guarantee
                             GUARANTEE_TYPE = null, //if has collteral guarantee
                             GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                             GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                             AMOUNT_GUARANTEED = null, //amount
                             //FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                             LOANID = x.REVOLVINGLOANID,
                             CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                             //100
                             GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                             REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                             //200
                             GOVERNMENT_MDA_TIN = b.TAXNUMBER,

                             //600
                             SYNDICATION_NAME = a.FIELD2,
                             SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                             PARTICIPATING_BANK_CODE = "",
                             PERFORMANCE_REPAYMENT_STATUS = x.USER_PRUDENTIAL_GUIDE_STATUSID == 1 ? "100" : "103",

                             //PERFORMANCE_REPAYMENT_STATUS = context.TBL_CRMS_REGULATORY.Where(cr => cr.CRMSREGULATORYID == x.CRMSREPAYMENTAGREEMENTID).Select(s => s.DESCRIPTION).FirstOrDefault(),
                             TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES = 0,
                             TOTAL_BANK_INDUCED_CREDIT_WRITEOFF = 0,
                             TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN = 0,
                             TOTAL_CUSTOMER_INDUCED_CREDIT = 0,
                             TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE = "TRF",
                             TOTAL_CUSTOMER_INDUCED_DEBIT_AMT = 0,
                             TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE = "TRF",
                             UNAMORTIZED_CREDIT_CHARGES = 0,
                             //LIQUIDATION = x.OUTSTANDINGPRINCIPAL + x.PASTDUEPRINCIPAL == 0 ? "NO" : "YES",
                             REFERENCENUMBER = x.LOANREFERENCENUMBER,
                         }).ToList();

            contingent = (from x in context.TBL_LOAN_CONTINGENT
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                          join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                          join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                          join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

                          join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                          let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                          let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                          let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()
                          where x.COMPANYID == param.companyId

                          select new CRMSTemplateViewModel
                          {
                              BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                              EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
                              CREDIT_LIMIT = x.CONTINGENTAMOUNT,
                              // INTEREST_RATE = x.INTERESTRATE.ToString(),
                              UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                              UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                              CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                              CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                              CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              // OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
                              OUTSTANDING_AMOUNT = x.CONTINGENTAMOUNT,
                              FEES = "",
                              CRMSCODE = a.CRMSCODE,

                              // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                              EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
                              REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                              LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                              RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                              COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                              FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                              ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                              FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                              LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                              CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                        // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                              DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                              SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                              SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                              SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                              COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.CONTINGENTLOANID).Any() ? "YES" : "NO",
                              COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                              SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSCOLLATERALTYPEID).Select(o => o.CODE).FirstOrDefault(),
                              ADDRESS_OF_SECURITY = "",
                              OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                                                      //  UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                              UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                              GUARANTEE = "", //if has collteral guarantee
                              GUARANTEE_TYPE = null, //if has collteral guarantee
                              GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                              GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                              AMOUNT_GUARANTEED = null, //amount
                              //FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                              LOANID = x.CONTINGENTLOANID,
                              CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                              //100
                              GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                              REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                              //200
                              GOVERNMENT_MDA_TIN = b.TAXNUMBER,

                              //600
                              SYNDICATION_NAME = a.FIELD2,
                              SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                              PARTICIPATING_BANK_CODE = "",
                              PERFORMANCE_REPAYMENT_STATUS = "100",
                              TOTAL_BANK_INDUCED_DEBIT_BANK_CHARGES = 0,
                              TOTAL_BANK_INDUCED_CREDIT_WRITEOFF = 0,
                              TOTAL_BANK_INDUCED_CREDIT_DRAWDOWN = 0,
                              TOTAL_CUSTOMER_INDUCED_CREDIT = 0,
                              TOTAL_CUSTOMER_INDUCED_CREDIT_TRN_TYPE = "TRF",
                              TOTAL_CUSTOMER_INDUCED_DEBIT_AMT = 0,
                              TOTAL_CUSTOMER_INDUCED_DEBIT_TRN_TYPE = "TRF",
                              UNAMORTIZED_CREDIT_CHARGES = 0,
                              //LIQUIDATION = x.CONTINGENTAMOUNT == 0 ? "NO" : "YES",
                              REFERENCENUMBER = x.LOANREFERENCENUMBER,
                          }).ToList();

            var data = tLoan.Union(revolving).Union(contingent).AsQueryable();

            return data;
        }

        private CRMSRecord GenerateCRMS400A(CRMSViewModel param)
        {
            Byte[] fileBytes = null;
            CRMSRecord rollOverFacility = new CRMSRecord();
            var output = GetRecords400A(param);
            if (output != null)
            {
                output = output.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).Select(x => x).ToList();

                using (ExcelPackage facility = new ExcelPackage())
                {
                    ExcelWorksheet sheet = facility.Workbook.Worksheets.Add("Roll Over Facility");
                    sheet.Cells[1, 1].Value = "UNIQUE_IDENTIFICATION_TYPE";
                    sheet.Cells[1, 2].Value = "UNIQUE_IDENTIFICATION_NO";
                    sheet.Cells[1, 3].Value = "CRMS_REFERENCE_NUMBER";
                    sheet.Cells[1, 4].Value = "EFFECTIVE_DATE";
                    sheet.Cells[1, 5].Value = "TENOR";
                    sheet.Cells[1, 6].Value = "EXPIRY_DATE";
                    sheet.Cells[1, 7].Value = "OPERATION_NAME";
                    sheet.Cells[1, 8].Value = "DATE CREATED";
                    sheet.Cells[1, 9].Value = "LOAN_REFERENCE_NUMBER";


                    for (int i = 2; i <= output.Count + 1; i++)
                    {
                        var facilityRecord = output[i - 2];

                        sheet.Cells[i, 1].Value = facilityRecord.ID_TTPE;
                        sheet.Cells[i, 2].Value = facilityRecord.ID_DETAIL;
                        sheet.Cells[i, 3].Value = facilityRecord.CRMSCODE;
                        sheet.Cells[i, 4].Value = facilityRecord.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        if (facilityRecord.MATURITYDATE != null)
                        {
                            var proposedTenor = ((DateTime)facilityRecord.MATURITYDATE - facilityRecord.EFFECTIVE_DATE).TotalDays;
                            var result = "";
                            var units = proposedTenor == 1 ? " day" : " days";
                            if (proposedTenor < 15)
                            {
                                result = "1";//proposedTenor.ToString() + units;
                            }
                            else
                            {
                                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                                units = months == 1 ? " month" : " months";
                                result = months.ToString(); //+ " " + units;
                            }


                            sheet.Cells[i, 5].Value = result;

                            sheet.Cells[i, 6].Value = ((DateTime)facilityRecord.MATURITYDATE).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            sheet.Cells[i, 5].Value = "NULL";
                            sheet.Cells[i, 6].Value = "NULL"; ;
                        }


                        //if (facilityRecord.MATURITYDATE != null)
                        //{
                        //    sheet.Cells[i, 5].Value = ((DateTime)facilityRecord.MATURITYDATE - facilityRecord.EFFECTIVE_DATE).TotalDays;
                        //    sheet.Cells[i, 6].Value = ((DateTime)facilityRecord.MATURITYDATE).ToString("dd/MM/yyyy"); 
                        //}
                        //else
                        //{
                        //    sheet.Cells[i, 5].Value = "NULL";
                        //    sheet.Cells[i, 6].Value = "NULL";
                        //}

                        sheet.Cells[i, 7].Value = facilityRecord.OPERATION_NAME;
                        sheet.Cells[i, 8].Value = facilityRecord.DATETIMECREATED.ToString("dd/MM/yyyy");
                        sheet.Cells[i, 9].Value = facilityRecord.REFERENCENUMBER;

                        //sheet.Cells[i, 6].Value = facilityRecord.TENOR;

                    }
                    fileBytes = facility.GetAsByteArray();


                    rollOverFacility.reportData = fileBytes;
                    rollOverFacility.templateTypeName = "CRMS_T400A_DIRECTORS";
                }

            }


            return rollOverFacility;
        }
        private List<CRMSTemplateViewModel> GetRecords400A(CRMSViewModel param)
        {
            var operationRollOver = CommonHelpers.GetRolloverOperations();
            var term = (from x in context.TBL_LOAN
                        join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                        join l in context.TBL_LOAN_REVIEW_OPERATION on x.TERMLOANID equals l.LOANID
                        join opn in context.TBL_OPERATIONS on l.OPERATIONTYPEID equals opn.OPERATIONID
                        join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                        join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                        //join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
                        where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                        && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                        && x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
                        && operationRollOver.Contains((short)l.OPERATIONTYPEID)
                        select new CRMSTemplateViewModel
                        {
                            ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                            MATURITYDATE = l.MATURITYDATE == null ? x.MATURITYDATE : l.MATURITYDATE,
                            EFFECTIVE_DATE = l.EFFECTIVEDATE == null ? x.EFFECTIVEDATE : l.EFFECTIVEDATE,
                            //TENOR = x.EFFECTIVEDATE - x.MATURITYDATE,
                            ID_TTPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                            ID_DETAIL = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                            CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                            CRMSCODE = ld.CRMSCODE,
                            OPERATION_NAME = opn.OPERATIONNAME,
                            DATETIMECREATED = l.DATECREATED,
                            REFERENCENUMBER = x.LOANREFERENCENUMBER,

                        }).ToList();
            var OD = (from x in context.TBL_LOAN_REVOLVING
                      join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                      join l in context.TBL_LOAN_REVIEW_OPERATION on x.REVOLVINGLOANID equals l.LOANID
                      join opn in context.TBL_OPERATIONS on l.OPERATIONTYPEID equals opn.OPERATIONID
                      join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                      join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                      // join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
                      where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                      && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                        && x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility
                       && operationRollOver.Contains((short)l.OPERATIONTYPEID)
                      select new CRMSTemplateViewModel
                      {
                          ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                          ID_TTPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                          ID_DETAIL = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                          CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                          MATURITYDATE = l.MATURITYDATE == null ? x.MATURITYDATE : l.MATURITYDATE,
                          EFFECTIVE_DATE = l.EFFECTIVEDATE == null ? x.EFFECTIVEDATE : l.EFFECTIVEDATE,
                          //TENOR = x.EFFECTIVEDATE - x.MATURITYDATE,
                          CRMSCODE = ld.CRMSCODE,
                          OPERATION_NAME = opn.OPERATIONNAME,
                          DATETIMECREATED = l.DATECREATED,
                          REFERENCENUMBER = x.LOANREFERENCENUMBER,
                      }).ToList();
            var contingent = (from x in context.TBL_LOAN_CONTINGENT
                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                              join l in context.TBL_LOAN_REVIEW_OPERATION on x.CONTINGENTLOANID equals l.LOANID
                              join opn in context.TBL_OPERATIONS on l.OPERATIONTYPEID equals opn.OPERATIONID
                              join b in context.TBL_CUSTOMER on x.CUSTOMERID equals b.CUSTOMERID
                              join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                              // join d in context.TBL_CUSTOMER_COMPANY_DIRECTOR on x.CUSTOMERID equals d.CUSTOMERID
                              where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                              && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
                              && x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability
                              && operationRollOver.Contains((short)l.OPERATIONTYPEID)
                              select new CRMSTemplateViewModel
                              {
                                  ACCOUNT = c.PRODUCTACCOUNTNUMBER,
                                  ID_TTPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                                  ID_DETAIL = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                                  CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,
                                  MATURITYDATE = l.MATURITYDATE == null ? x.MATURITYDATE : l.MATURITYDATE,
                                  EFFECTIVE_DATE = l.EFFECTIVEDATE == null ? x.EFFECTIVEDATE : l.EFFECTIVEDATE,
                                  //TENOR = x.EFFECTIVEDATE - x.MATURITYDATE,
                                  CRMSCODE = ld.CRMSCODE,
                                  OPERATION_NAME = opn.OPERATIONNAME,
                                  DATETIMECREATED = l.DATECREATED,
                                  REFERENCENUMBER = x.LOANREFERENCENUMBER,
                              }).ToList();

            return term.Union(OD).Union(contingent).OrderBy(x => x.ACCOUNT).ToList();
        }

        private CRMSRecord GenerateCRMS100Template(List<CRMSTemplateViewModel> loanInput)
        {

            Byte[] fileBytes = null;
            CRMSRecord data = new CRMSRecord();

            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");

                    ws.Cells[1, 1].Value = "GOVERNMENT_CODE";
                    ws.Cells[1, 2].Value = "LEGAL_STATUS";
                    ws.Cells[1, 3].Value = "CREDIT_TYPE";
                    ws.Cells[1, 4].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES";
                    ws.Cells[1, 5].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR";
                    ws.Cells[1, 6].Value = "CREDIT_LIMIT";
                    ws.Cells[1, 7].Value = "OUTSTANDING_AMOUNT";
                    ws.Cells[1, 8].Value = "FEES (FEE_TYPE & FEE_AMOUNT)";
                    ws.Cells[1, 9].Value = "EFFECTIVE_DATE";
                    ws.Cells[1, 10].Value = "TENOR";
                    ws.Cells[1, 11].Value = "EXPIRY_DATE";
                    ws.Cells[1, 12].Value = "REPAYMENT_AGREEMENT_MODE";
                    ws.Cells[1, 13].Value = "SPECIALISED_LOAN";
                    ws.Cells[1, 14].Value = "SPECIALISED_LOAN_PERIOD";
                    ws.Cells[1, 15].Value = "INTEREST_RATE";
                    ws.Cells[1, 16].Value = "COLLATERAL_PRESENT";
                    ws.Cells[1, 17].Value = "COLLATERAL_SECURE";
                    ws.Cells[1, 18].Value = "SECURITY_TYPE";
                    ws.Cells[1, 19].Value = "REPAYMENT_SOURCE";
                    ws.Cells[1, 20].Value = "SYNDICATION";
                    ws.Cells[1, 21].Value = "SYNDICATION_STATUS";
                    ws.Cells[1, 22].Value = "SYNDICATION_REF_NUMBER";

                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];

                        var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();

                        ws.Cells[i, 1].Value = record.GOVERNMENT_CODE;
                        ws.Cells[i, 2].Value = record.LEGAL_STATUS;
                        ws.Cells[i, 3].Value = record.CREDIT_TYPE;
                        ws.Cells[i, 4].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES;
                        ws.Cells[i, 5].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR;
                        ws.Cells[i, 6].Value = record.CREDIT_LIMIT;
                        ws.Cells[i, 7].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 8].Value = record.FEES;
                        ws.Cells[i, 8].Value = record.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 10].Value = record.TENOR; //temor
                        ws.Cells[i, 11].Value = record.EXPIRY_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 12].Value = record.REPAYMENT_AGREEMENT_MODE;
                        ws.Cells[i, 13].Value = record.SPECIALISED_LOAN;
                        ws.Cells[i, 14].Value = record.SPECIALISED_LOAN_PERIOD;
                        ws.Cells[i, 15].Value = record.INTEREST_RATE;
                        ws.Cells[i, 16].Value = record.COLLATERAL_PRESENT;
                        ws.Cells[i, 17].Value = record.COLLATERAL_SECURE;
                        ws.Cells[i, 18].Value = record.SECURITY_TYPE;
                        ws.Cells[i, 19].Value = record.FUNDING_SOURCE;
                        ws.Cells[i, 20].Value = record.SYNDICATION;
                        ws.Cells[i, 21].Value = record.SYNDICATION_STATUS;
                        ws.Cells[i, 22].Value = record.SYNDICATION_REF_NUMBER;
                    }
                    fileBytes = pck.GetAsByteArray();
                    data.reportData = fileBytes;
                    data.templateTypeName = "CRMS_T100";
                }


            }

            return data;
        }

        private CRMSRecord GenerateCRMS600Template(List<CRMSTemplateViewModel> loanInput)
        {

            Byte[] fileBytes = null;
            CRMSRecord data = new CRMSRecord();
            List<CRMSRecord> result = new List<CRMSRecord>();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");

                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    ws.Cells[1, 1].Value = "SYNDICATION_REF_NUMBER";
                    ws.Cells[1, 2].Value = "SYNDICATION_NAME";
                    ws.Cells[1, 3].Value = "SYNDICATION_TOTAL_AMOUNT";
                    ws.Cells[1, 4].Value = "PARTICIPATING_BANK_CODE";

                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {

                        var record = loanInput[i - 2];

                        var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();

                        ws.Cells[i, 1].Value = record.SYNDICATION_REF_NUMBER;
                        ws.Cells[i, 2].Value = record.SYNDICATION_NAME;
                        ws.Cells[i, 3].Value = record.SYNDICATION_TOTAL_AMOUNT;
                        ws.Cells[i, 4].Value = record.PARTICIPATING_BANK_CODE;

                    }
                    fileBytes = pck.GetAsByteArray();
                    data.reportData = fileBytes;
                    data.templateTypeName = "CRMS_T600";
                }


            }

            return data;
        }
        private CRMSRecord GenerateCRMS200Template(List<CRMSTemplateViewModel> loanInput)
        {

            Byte[] fileBytes = null;
            CRMSRecord data = new CRMSRecord();
            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("something");

                    using (var range = ws.Cells[1, 1, 1, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                    }
                    // ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    //ws.Cells[1, 1, 1, count].AutoFilter = true;

                    ws.Cells[1, 1].Value = "GOVERNMENT_MDA_TIN";
                    ws.Cells[1, 2].Value = "LEGAL_STATUS";
                    ws.Cells[1, 3].Value = "CREDIT_TYPE";
                    ws.Cells[1, 4].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES";
                    ws.Cells[1, 5].Value = "CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR";
                    ws.Cells[1, 6].Value = "CREDIT_LIMIT";
                    ws.Cells[1, 7].Value = "OUTSTANDING_AMOUNT";
                    ws.Cells[1, 8].Value = "FEES (FEE_TYPE & FEE_AMOUNT)";
                    ws.Cells[1, 9].Value = "EFFECTIVE_DATE";
                    ws.Cells[1, 10].Value = "TENOR";
                    ws.Cells[1, 11].Value = "EXPIRY_DATE";
                    ws.Cells[1, 12].Value = "REPAYMENT_AGREEMENT_MODE";
                    ws.Cells[1, 13].Value = "PERFORMANCE_REPAYMENT_STATUS";
                    ws.Cells[1, 14].Value = "INTEREST_RATE";
                    ws.Cells[1, 15].Value = "SPECIALISED_LOAN";
                    ws.Cells[1, 16].Value = "SPECIALISED_LOAN_PERIOD";
                    ws.Cells[1, 17].Value = "COLLATERAL_PRESENT";
                    ws.Cells[1, 18].Value = "COLLATERAL_SECURE";
                    ws.Cells[1, 19].Value = "SECURITY_TYPE";
                    ws.Cells[1, 20].Value = "REPAYMENT_SOURCE";
                    ws.Cells[1, 21].Value = "SYNDICATION";
                    ws.Cells[1, 22].Value = "SYNDICATION_STATUS";
                    ws.Cells[1, 23].Value = "SYNDICATION_REF_NUMBER";


                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {

                        var record = loanInput[i - 2];

                        var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();

                        ws.Cells[i, 1].Value = record.GOVERNMENT_MDA_TIN;
                        ws.Cells[i, 2].Value = record.LEGAL_STATUS;
                        ws.Cells[i, 3].Value = record.CREDIT_TYPE;
                        ws.Cells[i, 4].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES;
                        ws.Cells[i, 5].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR;
                        ws.Cells[i, 6].Value = record.CREDIT_LIMIT;
                        ws.Cells[i, 7].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 8].Value = record.FEES;
                        ws.Cells[i, 9].Value = record.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 10].Value = (record.EXPIRY_DATE - record.EFFECTIVE_DATE).TotalDays; //temor
                        ws.Cells[i, 11].Value = record.EXPIRY_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 12].Value = record.REPAYMENT_AGREEMENT_MODE;
                        ws.Cells[i, 13].Value = record.PERFORMANCE_REPAYMENT_STATUS;
                        ws.Cells[i, 14].Value = record.INTEREST_RATE;
                        ws.Cells[i, 15].Value = record.SPECIALISED_LOAN;
                        ws.Cells[i, 16].Value = record.SPECIALISED_LOAN_PERIOD;
                        ws.Cells[i, 17].Value = record.COLLATERAL_PRESENT;
                        ws.Cells[i, 18].Value = record.COLLATERAL_SECURE;
                        ws.Cells[i, 19].Value = record.SECURITY_TYPE;
                        ws.Cells[i, 20].Value = record.REPAYMENT_SOURCE;
                        ws.Cells[i, 21].Value = record.SYNDICATION;
                        ws.Cells[i, 22].Value = record.SYNDICATION_STATUS;
                        ws.Cells[i, 23].Value = record.SYNDICATION_REF_NUMBER;


                    }
                    fileBytes = pck.GetAsByteArray();
                    data.reportData = fileBytes;
                    data.templateTypeName = "CRMS_T200";
                }


            }

            return data;
        }


        private IQueryable<CollateralViewModel> CollateralGuarantee(int loanId)
        {
            List<CollateralViewModel> guaranteeInfo = new List<CollateralViewModel>();

            var collateralCostomerId = context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANID == loanId).Select(x => x.COLLATERALCUSTOMERID).FirstOrDefault();
            guaranteeInfo = (from x in context.TBL_COLLATERAL_GAURANTEE
                             where x.COLLATERALCUSTOMERID == collateralCostomerId
                             select new CollateralViewModel
                             {
                                 collateralId = x.COLLATERALCUSTOMERID,
                                 collateralGauranteeId = x.COLLATERALGAURANTEEID,
                                 collateralCustomerId = x.COLLATERALCUSTOMERID,
                                 institutionName = x.INSTITUTIONNAME,
                                 guarantorAddress = x.GUARANTORADDRESS,
                                 guaranteeValue = x.GUARANTEEVALUE,
                                 cStartDate = x.STARTDATE,
                                 endDate = x.ENDDATE,
                                 remark = x.REMARK,
                                 firstName = x.FIRSTNAME,
                                 middleName = x.MIDDLENAME,
                                 lastName = x.LASTNAME,
                                 bvn = x.BVN,
                                 rcNumber = x.RCNUMBER,
                                 phoneNumber1 = x.PHONENUMBER1,
                                 phoneNumber2 = x.PHONENUMBER2,
                                 emailAddress = x.EMAILADDRESS,
                                 relationship = x.RELATIONSHIP,
                                 relationshipDuration = x.RELATIONSHIPDURATION,
                                 taxNumber = x.TAXNUMBER
                             }).ToList();


            return guaranteeInfo.AsQueryable();
        }
        private IQueryable<CRMSTemplateViewModel> GenerateCRMSReport(CRMSViewModel param, int forSingle = 0)
        {
            // int[] crmsRegulatoryIds = { (int)CRMSRegulatory.Government, (int)CRMSRegulatory.Parastatals_MDA };

            var tLoan = new List<CRMSTemplateViewModel>();
            var revolving = new List<CRMSTemplateViewModel>();
            var contingent = new List<CRMSTemplateViewModel>();

            tLoan = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                     join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                     join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                     join co in context.TBL_COMPANY on l.COMPANYID equals co.COMPANYID

                     let lgaId = context.TBL_CUSTOMER_ADDRESS
                     .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
                     .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
                     let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
                     let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()

                     let collateralCustomer = context.TBL_LOAN_APPLICATION_COLLATERL.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
                   new { mp, col }).Where(dd => dd.mp.LOANAPPLICATIONID == a.LOANAPPLICATIONID && dd.col.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved).OrderByDescending(map => map.mp.LOANAPPCOLLATERALID).Select(dd => dd).FirstOrDefault()

                     let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                     let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

                     let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

                     where (a.CRMSVALIDATED == false || a.CRMSVALIDATED == null) && b.COMPANYID == param.companyId && a.STATUSID == (int)ApprovalStatusEnum.Approved



                     select new CRMSTemplateViewModel
                     {
                         DATETIMECREATED = a.DATETIMECREATED,
                         LOANAPPLICATIONDETAILID = a.LOANAPPLICATIONDETAILID,
                         BENEFICIARY_ACCOUNT_NUMBER = a.CASAACCOUNTID != null ? context.TBL_CASA.Where(o=>o.CASAACCOUNTID == a.CASAACCOUNTID).Select(q=>q.PRODUCTACCOUNTNUMBER).FirstOrDefault(): "n/a",
                         EFFECTIVE_DATE = a.EFFECTIVEDATE != null ? (DateTime)a.EFFECTIVEDATE : DateTime.Today.Date,
                         CREDIT_LIMIT = a.APPROVEDAMOUNT,
                         INTEREST_RATE = a.APPROVEDINTERESTRATE,
                         UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
                         UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
                         CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         OUTSTANDING_AMOUNT = a.APPROVEDAMOUNT,
                         FEES = "",
                         // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                         EXPIRY_DATE = a.EXPIRYDATE != null ? (DateTime)a.EXPIRYDATE : DateTime.Today.Date,
                         REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
                         LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
                         RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
                         COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
                         FUNDING_SOURCE_CATEGORY = co.CURRENCYID == a.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
                         ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                         FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
                         LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
                         CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
                                                                                                                                                   // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                         DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                         SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                         SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                         SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
                         COLLATERAL_PRESENT = collateralMpping != null ? "YES": "NO", //context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.TERMLOANID).Any() ? "YES" : "NO",
                         COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                         SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSCOLLATERALTYPEID).Select(o => o.CODE).FirstOrDefault(),
                         ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
                         OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
                         UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
                         UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
                         GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
                         GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID, //if has collteral guarantee
                         GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
                         GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
                         AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
                         //FIRSTPRINCIPALPAYMENTDATE = a.MORATORIUMDURATION,
                         MORATORIUMDURATION = a.MORATORIUMDURATION,
                         //LOANID = x.TERMLOANID,
                         CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

                         //100
                         GOVERNMENT_CODE = stateCode + "-" + lgaCode,
                         REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

                         //200
                         GOVERNMENT_MDA_TIN = b.TAXNUMBER,
                         PERFORMANCE_REPAYMENT_STATUS = "",

                         //600
                         SYNDICATION_NAME = a.FIELD2,
                         SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
                         PARTICIPATING_BANK_CODE = "",
                         REFERENCENUMBER = a.LOANAPPLICATIONDETAILID.ToString(),

                     }).ToList();

            //revolving = (from x in context.TBL_LOAN_REVOLVING
            //             join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
            //             join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
            //             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //             join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
            //             join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

            //             join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
            //             let lgaId = context.TBL_CUSTOMER_ADDRESS
            //         .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
            //         .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
            //             let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
            //             let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()
            //             let collateralCustomer = context.TBL_LOAN_COLLATERAL_MAPPING.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
            //       new { mp, col }).Where(dd => dd.mp.LOANID == x.REVOLVINGLOANID).OrderByDescending(map => map.mp.LOANCOLLATERALMAPPINGID).Select(dd => dd).FirstOrDefault()

            //             let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

            //             let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

            //             let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

            //             where a.CRMSCODE == null && x.COMPANYID == param.companyId
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //             select new CRMSTemplateViewModel
            //             {
            //                 LOANAPPLICATIONDETAILID = a.LOANAPPLICATIONDETAILID,

            //                 BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
            //                 EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
            //                 CREDIT_LIMIT = x.OVERDRAFTLIMIT,
            //                 INTEREST_RATE = x.INTERESTRATE,
            //                 UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
            //                 UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
            //                 CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
            //                 CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                 CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                 // OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
            //                 FEES = "",
            //                 // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
            //                 EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
            //                 REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
            //                 LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
            //                 RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
            //                 COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
            //                 FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
            //                 ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
            //                 FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
            //                 LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
            //                 CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                 CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                 SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
            //                                                                                                                                           // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
            //                 DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
            //                 SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
            //                 SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
            //                 SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
            //                 COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.REVOLVINGLOANID).Any() ? "YES" : "NO",
            //                 COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
            //                 SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == context.TBL_LOAN_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(y => y.CRMSCOLLATERALTYPEID).FirstOrDefault()).Select(o => o.CODE).FirstOrDefault(),
            //                 ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
            //                 OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
            //                 UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
            //                 UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
            //                 GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
            //                 GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID , //if has collteral guarantee
            //                 GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
            //                 GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
            //                 AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
            //                 CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

            //                 //100
            //                 GOVERNMENT_CODE = stateCode + "-" + lgaCode,
            //                 REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

            //                 //200
            //                 GOVERNMENT_MDA_TIN = b.TAXNUMBER,
            //                 PERFORMANCE_REPAYMENT_STATUS = "",

            //                 //600
            //                 SYNDICATION_NAME = a.FIELD2,
            //                 SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
            //                 PARTICIPATING_BANK_CODE = "",
            //                 REFERENCENUMBER = x.LOANREFERENCENUMBER,

            //             }).ToList();

            //contingent = (from x in context.TBL_LOAN_CONTINGENT
            //              join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
            //              join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
            //              join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
            //              join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
            //              join co in context.TBL_COMPANY on x.COMPANYID equals co.COMPANYID

            //              join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
            //              let lgaId = context.TBL_CUSTOMER_ADDRESS
            //         .Join(context.TBL_CITY, q => q.CITYID, ci => ci.CITYID, (q, ci) => new { q, ci }).Where(f => f.q.CUSTOMERID == b.CUSTOMERID)
            //         .Select(q => q.ci.LOCALGOVERNMENTID).FirstOrDefault()
            //              let lgaCode = context.TBL_LOCALGOVERNMENT.Where(aa => aa.LOCALGOVERNMENTID == lgaId).Select(aa => aa.LGACODE).FirstOrDefault()
            //              let stateCode = context.TBL_STATE.Where(x => x.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(x => x.STATECODE).FirstOrDefault()

            //              let collateralCustomer = context.TBL_LOAN_COLLATERAL_MAPPING.Join(context.TBL_COLLATERAL_CUSTOMER, mp => mp.COLLATERALCUSTOMERID, col => col.COLLATERALCUSTOMERID, (mp, col) =>
            //                                 new { mp, col }).Where(dd => dd.mp.LOANID == x.CONTINGENTLOANID).OrderByDescending(map => map.mp.LOANCOLLATERALMAPPINGID).Select(dd => dd).FirstOrDefault()

            //              let collateralMpping = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

            //              let securityOwnerCustomerDetal = context.TBL_CUSTOMER.Where(cus => cus.CUSTOMERID == collateralCustomer.col.CUSTOMERID).Select(cus => cus).FirstOrDefault()

            //              let collateralGuarantee = context.TBL_COLLATERAL_GAURANTEE.Where(mo => mo.COLLATERALCUSTOMERID == collateralCustomer.mp.COLLATERALCUSTOMERID).Select(mo => mo).FirstOrDefault()

            //              where a.CRMSCODE == null && x.COMPANYID == param.companyId
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
            //         && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)
            //              select new CRMSTemplateViewModel
            //              {
            //                  LOANAPPLICATIONDETAILID = a.LOANAPPLICATIONDETAILID,

            //                  BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
            //                  EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
            //                  CREDIT_LIMIT = x.CONTINGENTAMOUNT,
            //                  // INTEREST_RATE = x.INTERESTRATE.ToString(),
            //                  UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERTYPEID == 2 ? "TIN" : "BVN",
            //                  UNIQUE_IDENTIFICATION_NO = b.TAXNUMBER != null ? b.TAXNUMBER : b.CUSTOMERBVN,
            //                  CREDIT_TYPE = context.TBL_CRMS_REGULATORY.Where(s => s.CRMSREGULATORYID == p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault()).Select(s => s.CODE).FirstOrDefault(),
            //                  CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                  CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                  // OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
            //                  FEES = "",
            //                  // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
            //                  EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
            //                  REPAYMENT_AGREEMENT_MODE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSREPAYMENTAGREEMENTID).Select(o => o.CODE).FirstOrDefault(),
            //                  LOCATION_OF_BENEFICIARY = context.TBL_CITY.Where(g => g.CITYID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CITYID).FirstOrDefault()).Select(g => g.CRMSCODE).FirstOrDefault(),
            //                  RELATIONSHIP_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSRELATIONSHIPTYPEID).Select(o => o.CODE).FirstOrDefault(),
            //                  COMPANY_SIZE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSCOMPANYSIZEID).Select(o => o.CODE).FirstOrDefault(),
            //                  FUNDING_SOURCE_CATEGORY = co.CURRENCYID == x.CURRENCYID ? "LCY" : "FCY",//a.CRMSFUNDINGSOURCECATEGORY,
            //                  ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
            //                  FUNDING_SOURCE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == a.CRMSFUNDINGSOURCEID).Select(o => o.CODE).FirstOrDefault(),
            //                  LEGAL_STATUS = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == b.CRMSLEGALSTATUSID).Select(o => o.CODE).FirstOrDefault(),
            //                  CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                  CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
            //                  SPECIALISED_LOAN = a.ISSPECIALISED ? "YES" : "NO",                                                                        // pending
            //                                                                                                                                            // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
            //                  DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
            //                  SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
            //                  SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
            //                  SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? a.FIELD1 : "NIL",// Pending,
            //                  COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.CONTINGENTLOANID).Any() ? "YES" : "NO",
            //                  COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
            //                  SECURITY_TYPE = context.TBL_CRMS_REGULATORY.Where(o => o.CRMSREGULATORYID == context.TBL_LOAN_APPLICATION_DETAIL.Where(y => y.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(y => y.CRMSCOLLATERALTYPEID).FirstOrDefault()).Select(o => o.CODE).FirstOrDefault(),
            //                  ADDRESS_OF_SECURITY = collateralMpping.PROPERTYADDRESS,
            //                  OWNER_OF_SECURITY = securityOwnerCustomerDetal.FIRSTNAME + " " + securityOwnerCustomerDetal.LASTNAME + "" + securityOwnerCustomerDetal.MIDDLENAME,
            //                  UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID,
            //                  UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = securityOwnerCustomerDetal.CUSTOMERTYPEID == 1 ? securityOwnerCustomerDetal.CUSTOMERBVN : securityOwnerCustomerDetal.TAXNUMBER,
            //                  GUARANTEE = collateralGuarantee != null ? "YES" : "NO",
            //                  GUARANTEE_TYPE = securityOwnerCustomerDetal.CUSTOMERTYPEID, //if has collteral guarantee
            //                  GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = collateralGuarantee.BVN != null ? "BVN" : "TIN",
            //                  GUARANTOR_UNIQUE_IDENTIFICATION = collateralGuarantee.BVN != null ? collateralGuarantee.BVN : collateralGuarantee.TAXNUMBER,
            //                  AMOUNT_GUARANTEED = context.TBL_COLLATERAL_CUSTOMER.Where(p => p.COLLATERALCUSTOMERID == collateralGuarantee.COLLATERALCUSTOMERID).Select(p => p.COLLATERALVALUE).FirstOrDefault(),
            //                  //FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
            //                  LOANID = x.CONTINGENTLOANID,
            //                  CRMSLEGALSTATUSID = b.CRMSLEGALSTATUSID,

            //                  //100
            //                  GOVERNMENT_CODE = stateCode + "-" + lgaCode,
            //                  REPAYMENT_SOURCE = a.REPAYMENTSCHEDULE,

            //                  //200
            //                  GOVERNMENT_MDA_TIN = b.TAXNUMBER,
            //                  PERFORMANCE_REPAYMENT_STATUS = "",

            //                  //600
            //                  SYNDICATION_NAME = a.FIELD2,
            //                  SYNDICATION_TOTAL_AMOUNT = a.FIELD3,
            //                  PARTICIPATING_BANK_CODE = "",
            //                  REFERENCENUMBER = x.LOANREFERENCENUMBER,

            //              }).ToList();

            var data = tLoan.AsQueryable();//Union(revolving).Union(contingent).AsQueryable();

            return data;
        }
        private CRMSRecord GenerateCRMS400BTemplate(CRMSViewModel param)
        {


            var result = GenerateCRMSReport400B(param);
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA);
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T300");

            return GenerateCRMS400BTemplate(result.ToList(), param);

        }
        private CRMSRecord GenerateCRMS300TemplateByLoanAppId(CRMSViewModel param)
        {
            int forSingle = 1;
            var record = GenerateCRMSReport(param, forSingle).ToList();
            var result = record.Where(a=>a.LOANAPPLICATIONDETAILID == param.loanId).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T300");

            return GenerateCRMS300Template(result.ToList(), param, forSingle);
        }
        private CRMSRecord GenerateCRMS300Template(CRMSViewModel param)
        {
            int forSingle = 0;
            var record = GenerateCRMSReport(param, forSingle).ToList();

            var result = record.Where(x => x.DATETIMECREATED.Date >= param.startDate.Date
                     && x.DATETIMECREATED.Date <= param.endDate.Date).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T300");

            return GenerateCRMS300Template(result.ToList(), param, forSingle);
        }


        private CRMSRecord GenerateCRMS100TemplateByLoanAppId(CRMSViewModel param)
        {
            int forSingle = 1;
            var record = GenerateCRMSReport(param, forSingle).ToList();
            var result = record.Where(a => a.LOANAPPLICATIONDETAILID == param.loanId).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Government).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T100");

            return GenerateCRMS100Template(result.ToList());
        }
        private CRMSRecord GenerateCRMS100Template(CRMSViewModel param)
        {
            int forSingle = 0;
            var record = GenerateCRMSReport(param, forSingle).ToList();

            var result = record.Where(x => x.DATETIMECREATED.Date >= param.startDate.Date
                     && x.DATETIMECREATED.Date <= param.endDate.Date).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Government).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T100");

            return GenerateCRMS100Template(result.ToList());
        }

     private CRMSRecord GenerateCRMS200TemplateByLoanAppId(CRMSViewModel param)
        {
            int forSingle = 1;
            var record = GenerateCRMSReport(param, forSingle).ToList();
            var result = record.Where(a => a.LOANAPPLICATIONDETAILID == param.loanId).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T200");

            return GenerateCRMS200Template(result.ToList());
        }
        private CRMSRecord GenerateCRMS200Template(CRMSViewModel param)
        {
            int forSingle = 0;
            var record = GenerateCRMSReport(param, forSingle).ToList();

            var result = record.Where(x => x.DATETIMECREATED.Date >= param.startDate.Date
                     && x.DATETIMECREATED.Date <= param.endDate.Date).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T200");

            return GenerateCRMS200Template(result.ToList());
        }

     private CRMSRecord GenerateCRMS600TemplateByLoanAppId(CRMSViewModel param)
        {
            int forSingle = 1;
            var record = GenerateCRMSReport(param, forSingle).ToList();
            var result = record.Where(a => a.LOANAPPLICATIONDETAILID == param.loanId).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T600");

            return GenerateCRMS600Template(result.ToList());
        }
        private CRMSRecord GenerateCRMS600Template(CRMSViewModel param)
        {
            int forSingle = 0;
            var record = GenerateCRMSReport(param, forSingle).ToList();

            var result = record.Where(x => x.DATETIMECREATED.Date >= param.startDate.Date
                     && x.DATETIMECREATED.Date <= param.endDate.Date).ToList();
            result = result.Where(x => x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Government && x.CRMSLEGALSTATUSID != (int)CRMSRegulatory.Parastatals_MDA).ToList();
            if (result == null)
                throw new ConditionNotMetException("Record Not Found For T600");

            return GenerateCRMS600Template(result.ToList());
        }

         public CRMSRecord GenerateCBNReport(CRMSViewModel param)
        {
            if (param.templateTypeId == (int)CRMSTemplate.Template100)
            {
                return GenerateCRMS100Template(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template200)
            {
                return GenerateCRMS200Template(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template300)
            {
                return GenerateCRMS300Template(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template600)
            {
                return GenerateCRMS600Template(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400A)
            {
                return GenerateCRMS400A(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400B)
            {
                return GenerateCRMS400BTemplate(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400C)
            {
                return GenerateCRMS400CTemplate(param);
            }
            return new CRMSRecord();
        }
         public CRMSRecord GenerateCBNReportByLoanAppId(CRMSViewModel param)
        {
            if (param.templateTypeId == (int)CRMSTemplate.Template100)
            {
                return GenerateCRMS100TemplateByLoanAppId(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template200)
            {
                return GenerateCRMS200TemplateByLoanAppId(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template300)
            {
                return GenerateCRMS300TemplateByLoanAppId(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template600)
            {
                return GenerateCRMS600TemplateByLoanAppId(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400A)
            {
                return GenerateCRMS400A(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400B)
            {
                return GenerateCRMS400BTemplate(param);
            }
            else if (param.templateTypeId == (int)CRMSTemplate.Template400C)
            {
                return GenerateCRMS400CTemplate(param);
            }
            return new CRMSRecord();
        }
    }
}

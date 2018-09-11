using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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
            if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                var loan = context.TBL_LOAN.Where(x => x.TERMLOANID == param.loanId).Select(x => x).FirstOrDefault();
                if (loan == null)
                    throw new ConditionNotMetException("This loan does not exist");

                var codeExist = context.TBL_LOAN.Where(x => x.CRMSCODE == param.crmsCode).Any();
                if (codeExist == true)
                    throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has been assigned");

                loan.CRMSCODE = param.crmsCode;
                loan.CRMSDATE = DateTime.Now;

            }
            else if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                var loan = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == param.loanId).Select(x => x).FirstOrDefault();
                if (loan == null)
                    throw new ConditionNotMetException("This loan does not exist");

                var codeExist = context.TBL_LOAN_REVOLVING.Where(x => x.CRMSCODE == param.crmsCode).Any();
                if (codeExist == true)
                    throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has been assigned");

                loan.CRMSCODE = param.crmsCode;
                loan.CRMSDATE = DateTime.Now;

            }
            else if (param.loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                var loan = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == param.loanId).Select(x => x).FirstOrDefault();
                if (loan == null)
                    throw new ConditionNotMetException("This loan does not exist");

                var codeExist = context.TBL_LOAN_CONTINGENT.Where(x => x.CRMSCODE == param.crmsCode).Any();
                if (codeExist == true)
                    throw new ConditionNotMetException($"This CRMS {param.crmsCode} code has been assigned");

                loan.CRMSCODE = param.crmsCode;
                loan.CRMSDATE = DateTime.Now;

            }
            if (context.SaveChanges() > 0)
            {
                return "Successful";
            }
            return "Failed";

        }


        public List<CRMSRegulatoryViewModel> GetAllLoansWithCRMSCode(CRMSViewModel param)
        {
            var tLoan = (from x in context.TBL_LOAN
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                         select new CRMSRegulatoryViewModel
                         {
                             accountNumber = c.PRODUCTACCOUNTNUMBER,
                             beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                             crmsCode = x.CRMSCODE,
                             crmsDate = x.CRMSDATE,
                             effectiveDate = x.EFFECTIVEDATE,
                             facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                             grantedAmount = x.PRINCIPALAMOUNT,
                             interestRate = x.INTERESTRATE,
                             loanId = x.TERMLOANID,
                             loanSystemTypeId = x.LOANSYSTEMTYPEID
                         });

            var revolving = (from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                             where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                             select new CRMSRegulatoryViewModel
                             {
                                 accountNumber = c.PRODUCTACCOUNTNUMBER,
                                 beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                 crmsCode = x.CRMSCODE,
                                 crmsDate = x.CRMSDATE,
                                 effectiveDate = x.EFFECTIVEDATE,
                                 facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                 grantedAmount = x.OVERDRAFTLIMIT,
                                 interestRate = x.INTERESTRATE,
                                 loanId = x.REVOLVINGLOANID,
                                 loanSystemTypeId = x.LOANSYSTEMTYPEID
                             });

            var contingent = (from x in context.TBL_LOAN_CONTINGENT
                              join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                              join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                              where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                              select new CRMSRegulatoryViewModel
                              {
                                  accountNumber = c.PRODUCTACCOUNTNUMBER,
                                  beneficiary = b.FIRSTNAME + " " + b.LASTNAME,
                                  crmsCode = x.CRMSCODE,
                                  crmsDate = x.CRMSDATE,
                                  effectiveDate = x.EFFECTIVEDATE,
                                  facilityType = x.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                                  grantedAmount = x.CONTINGENTAMOUNT,
                                  interestRate = 0,
                                  loanId = x.CONTINGENTLOANID,
                                  loanSystemTypeId = x.LOANSYSTEMTYPEID,
                              });

            return tLoan.Union(revolving).Union(contingent).ToList();
        }

        private byte[] GenerateExportTemplate(List<CRMS300TemplateViewModel> loanInput)
        {

            Byte[] fileBytes = null;

            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");

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

                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                       

                        var record = loanInput[i - 2];

                        var guarantee = CollateralGuarantee(record.LOANID).Select(x => x).FirstOrDefault();

                        ws.Cells[i, 1].Value = i - 1;
                        ws.Cells[i, 2].Value = record.UNIQUE_IDENTIFICATION_TYPE;
                        ws.Cells[i, 3].Value = record.UNIQUE_IDENTIFICATION_NO;
                        ws.Cells[i, 4].Value = record.CREDIT_TYPE;
                        ws.Cells[i, 5].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES;
                        ws.Cells[i, 6].Value = record.CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR;
                        ws.Cells[i, 7].Value = record.CREDIT_LIMIT;
                        ws.Cells[i, 8].Value = record.OUTSTANDING_AMOUNT;
                        ws.Cells[i, 9].Value = record.FEES;
                        ws.Cells[i, 10].Value = record.EFFECTIVE_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 11].Value = (record.EXPIRY_DATE - record.EFFECTIVE_DATE).TotalDays; //temor
                        ws.Cells[i, 12].Value = record.EXPIRY_DATE.ToString("dd/MM/yyyy");
                        ws.Cells[i, 13].Value = record.REPAYMENT_AGREEMENT_MODE;
                        ws.Cells[i, 14].Value = record.INTEREST_RATE;
                        ws.Cells[i, 15].Value = record.BENEFICIARY_ACCOUNT_NUMBER;
                        ws.Cells[i, 16].Value = record.LOCATION_OF_BENEFICIARY;
                        ws.Cells[i, 17].Value = record.RELATIONSHIP_TYPE;
                        ws.Cells[i, 18].Value = record.COMPANY_SIZE;
                        ws.Cells[i, 19].Value = record.FUNDING_SOURCE_CATEGORY;
                        ws.Cells[i, 20].Value = record.ECCI_NUMBER;
                        ws.Cells[i, 21].Value = record.FUNDING_SOURCE;
                        ws.Cells[i, 22].Value = record.LEGAL_STATUS;
                        ws.Cells[i, 23].Value = record.CLASSIFICATION_BY_BUSINESS_LINES;
                        ws.Cells[i, 24].Value = record.CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR;
                        ws.Cells[i, 25].Value = record.SPECIALISED_LOAN;
                        ws.Cells[i, 26].Value = record.FIRSTPRINCIPALPAYMENTDATE!=null ? ((DateTime)record.FIRSTPRINCIPALPAYMENTDATE - record.EFFECTIVE_DATE).TotalDays : 0;// record.SPECIALISED_LOAN_MORATORIUM_PERIOD;
                        ws.Cells[i, 27].Value = record.DIRECTOR_UNIQUE_IDENTIFIER;
                        ws.Cells[i, 28].Value = record.SYNDICATION;
                        ws.Cells[i, 29].Value = record.SYNDICATION_STATUS;
                        ws.Cells[i, 30].Value = record.SYNDICATION_REF_NUMBER;
                        ws.Cells[i, 31].Value = record.COLLATERAL_PRESENT;
                        ws.Cells[i, 32].Value = record.COLLATERAL_SECURE;
                        ws.Cells[i, 33].Value = record.SECURITY_TYPE;
                        ws.Cells[i, 34].Value = record.ADDRESS_OF_SECURITY;
                        ws.Cells[i, 35].Value = record.OWNER_OF_SECURITY;
                        ws.Cells[i, 36].Value = record.UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER;
                        ws.Cells[i, 37].Value = record.UNIQUE_IDENTIFIER_OF_SECURITY_OWNER;

                        if (guarantee!=null)
                        {
                            ws.Cells[i, 38].Value = guarantee.collateralType != null ? guarantee.collateralType : ""; //GUARANTEE_TYPE
                            ws.Cells[i, 39].Value = guarantee.bvn != null ? "BVN" : "TIN"; //record.GUARANTOR_UNIQUE_IDENTIFICATION_TYPE;
                            ws.Cells[i, 40].Value = guarantee.bvn != null ? guarantee.bvn : guarantee.rcNumber;// record.GUARANTOR_UNIQUE_IDENTIFICATION;
                            ws.Cells[i, 41].Value = guarantee.guaranteeValue;// record.AMOUNT_GUARANTEED;

                        }

                    }
                    fileBytes = pck.GetAsByteArray();
                }


            }

            return fileBytes;
        }


        private IQueryable< CollateralViewModel> CollateralGuarantee(int loanId)
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

        //private string GuaranteeType(CollateralViewModel collateralGuarantee)
        //{
        //    if (collateralGuarantee.)
        //    {

        //    }
        //}

        public byte[] GenerateCRMS300Template(CRMSViewModel param)
        {
            var tLoan = new List<CRMS300TemplateViewModel>();
            var revolving = new List<CRMS300TemplateViewModel>();
            var contingent = new List<CRMS300TemplateViewModel>();

            tLoan = (from x in context.TBL_LOAN
                     join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                     join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                     join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                     join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                     join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                     where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                     select new CRMS300TemplateViewModel
                     {
                         BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                         EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
                         CREDIT_LIMIT = x.PRINCIPALAMOUNT,
                         INTEREST_RATE = x.INTERESTRATE.ToString(),
                         UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERBVN != null ? "BVN" : "TIN",
                         UNIQUE_IDENTIFICATION_NO = b.CUSTOMERBVN != null ? b.CUSTOMERBVN : b.TAXNUMBER,
                         CREDIT_TYPE = p.TBL_PRODUCT_BEHAVIOUR.Where(o=>o.PRODUCTID==p.PRODUCTID).Select(o=>o.CRMSREGULATORYID).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                         CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o=>o.SUBSECTORID==a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
                         FEES = "",
                        // TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                         EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
                         REPAYMENT_AGREEMENT_MODE = a.CRMSREPAYMENTSOURCEID,
                         LOCATION_OF_BENEFICIARY =context.TBL_STATE.Where(g=>g.STATEID== context.TBL_CUSTOMER_ADDRESS.Where(o=>o.CUSTOMERID==b.CUSTOMERID).Select(o=>o.STATEID).FirstOrDefault()).Select(g=>g.STATECODE).FirstOrDefault(),
                         RELATIONSHIP_TYPE = x.CRMSREPAYMENTAGREEMENTID,
                         COMPANY_SIZE = b.CRMSCOMPANYSIZEID,
                         FUNDING_SOURCE_CATEGORY = a.CRMSFUNDINGSOURCECATEGORY,
                         ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                         FUNDING_SOURCE = a.CRMSFUNDINGSOURCEID,
                         LEGAL_STATUS = b.CRMSLEGALSTATUSID,
                         CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o=>o.SECTORID==a.TBL_SUB_SECTOR.SUBSECTORID).Select(o=>o.CODE).FirstOrDefault(),
                         CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                         SPECIALISED_LOAN = "If(LPD)",                                                                        // pending
                        // SPECIALISED_LOAN_MORATORIUM_PERIOD =  ((DateTime)x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).TotalDays,    // pending
                         DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID==b.CUSTOMERID).Select(o=>o.CUSTOMERBVN).FirstOrDefault(),
                         SYNDICATION =  (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                         SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL" ,//"IF(product tye is syndicationa by the product type (Austine))",
                         SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "SYN NUMBER" : "NIL",// "IF(product tye is syndicationa by the product type (Austine)) ",
                         COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o=>o.LOANID==x.TERMLOANID).Any() ? "YES" : "NO",
                         COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                         SECURITY_TYPE = a.CRMSCOLLATERALTYPEID,
                         ADDRESS_OF_SECURITY = "",
                         OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                         UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                         UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                         GUARANTEE = "", //if has collteral guarantee
                         GUARANTEE_TYPE = "", //if has collteral guarantee
                         GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                         GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                         AMOUNT_GUARANTEED = "", //amount
                         FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE,
                         LOANID = x.TERMLOANID

                     }).ToList();
            revolving = (from x in context.TBL_LOAN_REVOLVING
                         join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                         join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                         join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                         where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                         select new CRMS300TemplateViewModel
                         {
                             BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                             EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
                             CREDIT_LIMIT = x.OVERDRAFTLIMIT,
                             INTEREST_RATE = x.INTERESTRATE.ToString(),
                             UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERBVN != null ? "BVN" : "TIN",
                             UNIQUE_IDENTIFICATION_NO = b.CUSTOMERBVN != null ? b.CUSTOMERBVN : b.TAXNUMBER,
                             CREDIT_TYPE = p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault(),
                             CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                             CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             //OUTSTANDING_AMOUNT = x.ba,
                             FEES = "",
                             //TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                             EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
                             REPAYMENT_AGREEMENT_MODE = a.CRMSREPAYMENTSOURCEID,
                             LOCATION_OF_BENEFICIARY = context.TBL_STATE.Where(g => g.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(g => g.STATECODE).FirstOrDefault(),
                             RELATIONSHIP_TYPE = x.CRMSREPAYMENTAGREEMENTID,
                             COMPANY_SIZE = b.CRMSCOMPANYSIZEID,
                             FUNDING_SOURCE_CATEGORY = a.CRMSFUNDINGSOURCECATEGORY,
                             ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                             FUNDING_SOURCE = a.CRMSFUNDINGSOURCEID,
                             LEGAL_STATUS = b.CRMSLEGALSTATUSID,
                             CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                             SPECIALISED_LOAN = "If(LPD)",                                                                        // pending
                            // SPECIALISED_LOAN_MORATORIUM_PERIOD = (x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).Value.Days,    // pending
                             DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                             SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                             SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                             SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "SYN NUMBER" : "NIL",// "IF(product tye is syndicationa by the product type (Austine)) ",
                             COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.REVOLVINGLOANID).Any() ? "YES" : "NO",
                             COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                             SECURITY_TYPE = a.CRMSCOLLATERALTYPEID,
                             ADDRESS_OF_SECURITY = "",
                             OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                             UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                             UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                             GUARANTEE = "", //if has collteral guarantee
                             GUARANTEE_TYPE = "", //if has collteral guarantee
                             GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                             GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                             AMOUNT_GUARANTEED = "", //amount
                             //FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE
                             LOANID = x.REVOLVINGLOANID
                         }).ToList();
            contingent = (from x in context.TBL_LOAN_CONTINGENT
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join l in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                          join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                          join b in context.TBL_CUSTOMER on c.CUSTOMERID equals b.CUSTOMERID
                          join p in context.TBL_PRODUCT on x.PRODUCTID equals p.PRODUCTID
                          where x.CRMSCODE != null && x.CRMSDATE >= param.startDate && x.CRMSDATE <= param.endDate
                          select new CRMS300TemplateViewModel
                          {
                              BENEFICIARY_ACCOUNT_NUMBER = c.PRODUCTACCOUNTNUMBER,
                              EFFECTIVE_DATE = x.EFFECTIVEDATE,//.ToString("dd/MM/yyyy"),
                              CREDIT_LIMIT = x.CONTINGENTAMOUNT,
                             // INTEREST_RATE = x.in.ToString(),
                              UNIQUE_IDENTIFICATION_TYPE = b.CUSTOMERBVN != null ? "BVN" : "TIN",
                              UNIQUE_IDENTIFICATION_NO = b.CUSTOMERBVN != null ? b.CUSTOMERBVN : b.TAXNUMBER,
                              CREDIT_TYPE = p.TBL_PRODUCT_BEHAVIOUR.Where(o => o.PRODUCTID == p.PRODUCTID).Select(o => o.CRMSREGULATORYID).FirstOrDefault(),
                              CREDIT_PURPOSE_BY_BUSINESSLINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SECTORID).Select(o => o.CODE).FirstOrDefault(),
                              CREDIT_PURPOSE_BY_BUSINESSLINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              //OUTSTANDING_AMOUNT = x.OUTSTANDINGPRINCIPAL,
                              FEES = "",
                              //TENOR = (x.MATURITYDATE - x.EFFECTIVEDATE).TotalDays,
                              EXPIRY_DATE = x.MATURITYDATE,//.ToString("dd/MM/yyyy"),
                              REPAYMENT_AGREEMENT_MODE = a.CRMSREPAYMENTSOURCEID,
                              LOCATION_OF_BENEFICIARY = context.TBL_STATE.Where(g => g.STATEID == context.TBL_CUSTOMER_ADDRESS.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.STATEID).FirstOrDefault()).Select(g => g.STATECODE).FirstOrDefault(),
                              RELATIONSHIP_TYPE = x.CRMSREPAYMENTAGREEMENTID,
                              COMPANY_SIZE = b.CRMSCOMPANYSIZEID,
                              FUNDING_SOURCE_CATEGORY = a.CRMSFUNDINGSOURCECATEGORY,
                              ECCI_NUMBER = a.CRMS_ECCI_NUMBER,
                              FUNDING_SOURCE = a.CRMSFUNDINGSOURCEID,
                              LEGAL_STATUS = b.CRMSLEGALSTATUSID,
                              CLASSIFICATION_BY_BUSINESS_LINES = context.TBL_SECTOR.Where(o => o.SECTORID == a.TBL_SUB_SECTOR.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              CLASSIFICATION_BY_BUSINESS_LINES_SUB_SECTOR = context.TBL_SUB_SECTOR.Where(o => o.SUBSECTORID == a.SUBSECTORID).Select(o => o.CODE).FirstOrDefault(),
                              SPECIALISED_LOAN = "If(LPD)",                                                                        // pending
                             // SPECIALISED_LOAN_MORATORIUM_PERIOD = (x.FIRSTPRINCIPALPAYMENTDATE - x.EFFECTIVEDATE).Value.Days,    // pending
                              DIRECTOR_UNIQUE_IDENTIFIER = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(o => o.CUSTOMERID == b.CUSTOMERID).Select(o => o.CUSTOMERBVN).FirstOrDefault(),
                              SYNDICATION = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "YES" : "NO",
                              SYNDICATION_STATUS = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "MEMBER" : "NIL",//"IF(product tye is syndicationa by the product type (Austine))",
                              SYNDICATION_REF_NUMBER = (a.PROPOSEDPRODUCTID == (int)LoanProductTypeEnum.SyndicatedTermLoan) ? "SYN NUMBER" : "NIL",// "IF(product tye is syndicationa by the product type (Austine)) ",
                              COLLATERAL_PRESENT = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.CONTINGENTLOANID).Any() ? "YES" : "NO",
                              COLLATERAL_SECURE = a.SECUREDBYCOLLATERAL ? "YES" : "NO",
                              SECURITY_TYPE = a.CRMSCOLLATERALTYPEID,
                              ADDRESS_OF_SECURITY = "",
                              OWNER_OF_SECURITY = "", //cusmerId map to collateral - highest value
                              UNIQUE_IDENTIFICATION_TYPE_OF_SECURITY_OWNER = "", //tin/bvn
                              UNIQUE_IDENTIFIER_OF_SECURITY_OWNER = "", //tin/bvn
                              GUARANTEE = "", //if has collteral guarantee
                              GUARANTEE_TYPE = "", //if has collteral guarantee
                              GUARANTOR_UNIQUE_IDENTIFICATION_TYPE = "", //TIN/BVN
                              GUARANTOR_UNIQUE_IDENTIFICATION = "", //TIN/BVN
                              AMOUNT_GUARANTEED = "", //amount
                              //FIRSTPRINCIPALPAYMENTDATE = x.FIRSTPRINCIPALPAYMENTDATE
                              LOANID = x.CONTINGENTLOANID
                          }).ToList();

            var data = tLoan.Union(revolving).Union(contingent).ToList();

            return GenerateExportTemplate(data);
        }


    }
}

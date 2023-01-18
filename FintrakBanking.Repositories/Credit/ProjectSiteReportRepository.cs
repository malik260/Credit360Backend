using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.credit
{
    public class ProjectSiteReportRepository : IProjectSiteReportRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;
      

        public ProjectSiteReportRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }
        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            var dataTermLoan = (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join r in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID into ar
                                from ex in ar.DefaultIfEmpty()
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                where x.ISPROJECTRELATED == true && (x.APPLICATIONREFERENCENUMBER == searchString.Trim()
                                || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                                || c.LASTNAME.ToLower().Contains(searchString.Trim())
                                || c.MIDDLENAME.ToLower().Contains(searchString.Trim()))
                                let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p=>p.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                                orderby x.APPLICATIONREFERENCENUMBER
                                select new LoanApplicationViewModel
                                {
                                customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                customerCode = c.CUSTOMERCODE,
                                applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                customerId = c.CUSTOMERID,
                                loanSystemTypeId = (int)LoanSystemTypeEnum.TermDisbursedFacility,
                                loanId = ex.TERMLOANID,
                                loanPurpose = a.LOANPURPOSE,
                                legalContingentCode = "",
                                principalAmount = ex.PRINCIPALAMOUNT == null ? 0 : ex.PRINCIPALAMOUNT,
                                loanReferenceNumber = ex.LOANREFERENCENUMBER,
                                psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                applicationDate = x.APPLICATIONDATE,
                                applicationAmount = x.APPLICATIONAMOUNT,
                                approvedAmount = a.APPROVEDAMOUNT,
                                interestRate = x.INTERESTRATE,
                                productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                            }).ToList();

            var dataRevolving = (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join r in context.TBL_LOAN_REVOLVING on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID into ar
                                from ex in ar.DefaultIfEmpty()
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                where x.ISPROJECTRELATED == true && (x.APPLICATIONREFERENCENUMBER == searchString.Trim()
                                || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                                || c.LASTNAME.ToLower().Contains(searchString.Trim())
                                || c.MIDDLENAME.ToLower().Contains(searchString.Trim()))
                                let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p => p.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                                orderby x.APPLICATIONREFERENCENUMBER
                                 select new LoanApplicationViewModel
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    customerId = c.CUSTOMERID,
                                    loanPurpose = a.LOANPURPOSE,
                                    loanId = ex.REVOLVINGLOANID,
                                    legalContingentCode = "",
                                    loanSystemTypeId = (int)LoanSystemTypeEnum.OverdraftFacility,
                                    psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                    reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                    branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    principalAmount = ex.OVERDRAFTLIMIT == null ? 0 : ex.OVERDRAFTLIMIT,
                                    loanReferenceNumber = ex.LOANREFERENCENUMBER,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataContingent = (from x in context.TBL_LOAN_APPLICATION
                                 join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                 join r in context.TBL_LOAN_CONTINGENT on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID into ar
                                 from ex in ar.DefaultIfEmpty()
                                 join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                 where x.ISPROJECTRELATED == true && (x.APPLICATIONREFERENCENUMBER == searchString.Trim()
                                 || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                                 || c.LASTNAME.ToLower().Contains(searchString.Trim())
                                 || c.MIDDLENAME.ToLower().Contains(searchString.Trim()))
                                 let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p => p.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                                 orderby x.APPLICATIONREFERENCENUMBER 
                                 select new LoanApplicationViewModel
                                 {
                                     customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                     customerCode = c.CUSTOMERCODE,
                                     applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                     loanApplicationId = x.LOANAPPLICATIONID,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     customerId = c.CUSTOMERID,
                                     loanPurpose = a.LOANPURPOSE,
                                     legalContingentCode = ex.LEGALCONTINGENTCODE,
                                     loanSystemTypeId = (int)LoanSystemTypeEnum.ContingentLiability,
                                     psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                     reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                     branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                     applicationDate = x.APPLICATIONDATE,
                                     applicationAmount = x.APPLICATIONAMOUNT,
                                     loanId = ex.CONTINGENTLOANID,
                                     approvedAmount = a.APPROVEDAMOUNT,
                                     principalAmount = ex.CONTINGENTAMOUNT == null ? 0 : ex.CONTINGENTAMOUNT,
                                     loanReferenceNumber = ex.LOANREFERENCENUMBER,
                                     interestRate = x.INTERESTRATE,
                                     productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                     productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                     relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                     relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                     relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                     isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                 }).ToList();

            var data = dataTermLoan.Union(dataRevolving).Union(dataContingent);

            foreach (var item in data)
            {
                if(item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                {
                    var utilizations = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.OVERDRAFTLIMIT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.CONTINGENTAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;

                }
                if (item.productTypeId != (short)LoanProductTypeEnum.RevolvingLoan && item.productTypeId != (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.PRINCIPALAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
            }

            return data;
        }


        public IEnumerable<LoanApplicationViewModel> ProjectSiteReportLoans(int id)
        {
            var dataTerLoan =   (from x in context.TBL_LOAN_APPLICATION 
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join r in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                where x.ISPROJECTRELATED == true && x.LOANAPPLICATIONID == id
                                let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p => p.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                                select new LoanApplicationViewModel
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    loanPurpose = a.LOANPURPOSE,
                                    customerId = c.CUSTOMERID,
                                    psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                    reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                    branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = r.DATETIMECREATED,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    principalAmount = r.PRINCIPALAMOUNT,
                                    loanReferenceNumber = r.LOANREFERENCENUMBER,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                            }).ToList();

            var dataRevolving = (from x in context.TBL_LOAN_APPLICATION 
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join r in context.TBL_LOAN_REVOLVING on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                where x.ISPROJECTRELATED == true && x.LOANAPPLICATIONID == id
                                let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p => p.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                               select new LoanApplicationViewModel
                               {
                                   customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                   customerCode = c.CUSTOMERCODE,
                                   applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                   loanApplicationId = x.LOANAPPLICATIONID,
                                   loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                   loanPurpose = a.LOANPURPOSE,
                                   customerId = c.CUSTOMERID,
                                   psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                   reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                   branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                   applicationDate = r.DATETIMECREATED,
                                   applicationAmount = x.APPLICATIONAMOUNT,
                                   approvedAmount = a.APPROVEDAMOUNT,
                                   principalAmount = r.OVERDRAFTLIMIT,
                                   loanReferenceNumber = r.LOANREFERENCENUMBER,
                                   interestRate = x.INTERESTRATE,
                                   productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                   productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                   relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                   isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                               }).ToList();

            var dataContingent = (from x in context.TBL_LOAN_APPLICATION
                                 join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                 join r in context.TBL_LOAN_CONTINGENT on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                 join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                 where x.ISPROJECTRELATED == true && x.LOANAPPLICATIONID == id
                                 let projectSiteReport = context.TBL_PSR_PROJECT_SITE_REPORT.Where(p => p.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(p => p).FirstOrDefault()
                                 select new LoanApplicationViewModel
                                 {
                                     customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                     customerCode = c.CUSTOMERCODE,
                                     applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                     loanApplicationId = x.LOANAPPLICATIONID,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     loanPurpose = a.LOANPURPOSE,
                                     customerId = c.CUSTOMERID,
                                     psrReportTypeId = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.PSRREPORTTYPEID).FirstOrDefault(),
                                     reportTypeName = context.TBL_PSR_REPORT_TYPE.Where(b => b.PSRREPORTTYPEID == projectSiteReport.PSRREPORTTYPEID).Select(b => b.REPORTTYPENAME).FirstOrDefault(),
                                     branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                     applicationDate = r.DATETIMECREATED,
                                     applicationAmount = x.APPLICATIONAMOUNT,
                                     approvedAmount = a.APPROVEDAMOUNT,
                                     principalAmount = r.CONTINGENTAMOUNT,
                                     loanReferenceNumber = r.LOANREFERENCENUMBER,
                                     interestRate = x.INTERESTRATE,
                                     productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                     productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                     relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                     relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                     relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                     isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                 }).ToList();

            var data = dataTerLoan.Union(dataRevolving).Union(dataContingent);

            foreach (var item in data)
            {
                if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                {
                    var utilizations = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.OVERDRAFTLIMIT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.CONTINGENTAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;

                }
                if (item.productTypeId != (short)LoanProductTypeEnum.RevolvingLoan && item.productTypeId != (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.PRINCIPALAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
            }

            return data;
        }

        public IEnumerable<LoanApplicationViewModel> GetFacilities(int id)
        {
                 var dataTermLoan = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                    join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                    join r in context.TBL_LOAN on p.LOANID equals r.TERMLOANID 
                                    let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                    where p.PROJECTSITEREPORTID  == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility

                                     select new LoanApplicationViewModel
                                    {
                                    customerName = context.TBL_CUSTOMER.Where(o=>o.CUSTOMERID== a.CUSTOMERID).Select(o=>o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    customerId = x.CUSTOMERID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    loanPurpose = a.LOANPURPOSE,
                                    legalContingentCode = "",
                                    loanReferenceNumber = r.LOANREFERENCENUMBER,
                                    principalAmount = r.PRINCIPALAMOUNT == 0 ? 0 : r.PRINCIPALAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataRevolving = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                join r in context.TBL_LOAN_REVOLVING on p.LOANID equals r.REVOLVINGLOANID 
                                let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility

                                 select new LoanApplicationViewModel
                                {
                                    customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    customerId = x.CUSTOMERID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    loanReferenceNumber = r.LOANREFERENCENUMBER,
                                    principalAmount = r.OVERDRAFTLIMIT == 0 ? 0 : r.OVERDRAFTLIMIT,
                                    loanPurpose = a.LOANPURPOSE,
                                    interestRate = x.INTERESTRATE,
                                    legalContingentCode = "",
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataContingent =    (from p in context.TBL_PSR_PROJECT_FACILITIES
                                    join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                    join r in context.TBL_LOAN_CONTINGENT on p.LOANID equals r.CONTINGENTLOANID 
                                    let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                    where p.PROJECTSITEREPORTID == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability

                                     select new LoanApplicationViewModel
                                    {
                                        customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        customerId = x.CUSTOMERID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                        applicationDate = x.APPLICATIONDATE,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = a.APPROVEDAMOUNT,
                                        loanReferenceNumber = r.LOANREFERENCENUMBER,
                                        principalAmount = r.CONTINGENTAMOUNT == 0 ? 0 : r.CONTINGENTAMOUNT,
                                        loanPurpose = a.LOANPURPOSE,
                                        interestRate = x.INTERESTRATE,
                                        legalContingentCode = r.LEGALCONTINGENTCODE,
                                        productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                        relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                        relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                        operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                        isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                    }).ToList();

            var dataTermLoan2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LOANID == null && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility

                                select new LoanApplicationViewModel
                                {
                                    customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    customerId = x.CUSTOMERID,
                                    branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    loanPurpose = a.LOANPURPOSE,
                                    principalAmount = 0,
                                    loanReferenceNumber = "",
                                    legalContingentCode = "",
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataRevolving2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                 join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                 join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                 let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                 where p.PROJECTSITEREPORTID == id && p.LOANID == null && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility

                                  select new LoanApplicationViewModel
                                 {
                                     customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                     applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                     loanApplicationId = x.LOANAPPLICATIONID,
                                     customerId = x.CUSTOMERID,
                                     branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                     applicationDate = x.APPLICATIONDATE,
                                     applicationAmount = x.APPLICATIONAMOUNT,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     approvedAmount = a.APPROVEDAMOUNT,
                                     principalAmount = 0,
                                     loanReferenceNumber = "",
                                     legalContingentCode = "",
                                     loanPurpose = a.LOANPURPOSE,
                                     interestRate = x.INTERESTRATE,
                                     productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                     productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                     relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                     relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                     relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                     isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                 }).ToList();

            var dataContingent2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                  join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                  join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                  let loan_application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                  where p.PROJECTSITEREPORTID == id && p.LOANID == null && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability

                                   select new LoanApplicationViewModel
                                  {
                                      customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                      applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                      loanApplicationId = x.LOANAPPLICATIONID,
                                      loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                      customerId = x.CUSTOMERID,
                                      branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                      applicationDate = x.APPLICATIONDATE,
                                      applicationAmount = x.APPLICATIONAMOUNT,
                                      approvedAmount = a.APPROVEDAMOUNT,
                                      principalAmount = 0,
                                      loanReferenceNumber = "",
                                      legalContingentCode = "",
                                      loanPurpose = a.LOANPURPOSE,
                                      interestRate = x.INTERESTRATE,
                                      productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                      productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                      relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                      relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                      relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                      relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                      operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                      isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                  }).ToList();

            var data1 = dataTermLoan.Union(dataRevolving).Union(dataContingent);
            var data2 = dataTermLoan2.Union(dataRevolving2).Union(dataContingent2);

            var data = data1.Union(data2);
            foreach (var item in data)
            {
                if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                {
                    var utilizations = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.OVERDRAFTLIMIT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.CONTINGENTAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;

                }
                if (item.productTypeId != (short)LoanProductTypeEnum.RevolvingLoan && item.productTypeId != (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.PRINCIPALAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
            }
            return data;
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports()
        {
            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.ProjectSiteReportApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var psr1 = (from x in context.TBL_PSR_PROJECT_SITE_REPORT
                        where x.DELETED == false && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending)
                        select new ProjectSiteReportViewModel
                        {
                            projectSiteReportId = x.PROJECTSITEREPORTID,
                            psrReportTypeId = x.PSRREPORTTYPEID,
                            psrRepeortType = (from r in context.TBL_PSR_REPORT_TYPE where r.PSRREPORTTYPEID == x.PSRREPORTTYPEID select r.REPORTTYPENAME).FirstOrDefault(),
                            clientName = x.CLIENTNAME,
                            contractorName = x.CONTRACTORNAME,
                            consultantName = x.CONSULTANTNAME,
                            projectAmount = x.PROJECTAMOUNT,
                            projectDescription = x.PROJECTDESCRIPTION,
                            inspectionDate = x.INSPECTIONDATE,
                            commencementDate = x.COMMENCEMENTDATE,
                            completionDate = x.COMPLETIONDATE,
                            acceptance = x.ACCEPTANCE,
                            loanApplicationId = x.LOANAPPLICATIONID,
                            loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                            projectLocation = x.PROJECTLOCATION,
                            approvalStatusId = x.APPROVALSTATUSID,                          
                            currencyId = x.CURRENCYID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                            currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),                  
                        }).OrderByDescending(x => x.projectSiteReportId)
                .ToList();

                 var psr2 = (from x in context.TBL_PSR_PROJECT_SITE_REPORT
                        join trail in context.TBL_APPROVAL_TRAIL on x.PROJECTSITEREPORTID equals trail.TARGETID
                        where trail.OPERATIONID == (short)OperationsEnum.ProjectSiteReportApproval
                            && x.DELETED == false
                            && trail.TARGETID == x.PROJECTSITEREPORTID                    
                        orderby trail.APPROVALTRAILID descending
                        select new ProjectSiteReportViewModel
                        {
                            loopedStaffId = trail.LOOPEDSTAFFID,
                            projectSiteReportId = x.PROJECTSITEREPORTID,
                            psrReportTypeId = x.PSRREPORTTYPEID,
                            clientName = x.CLIENTNAME,
                            contractorName = x.CONTRACTORNAME,
                            consultantName = x.CONSULTANTNAME,
                            projectAmount = x.PROJECTAMOUNT,
                            projectDescription = x.PROJECTDESCRIPTION,
                            commencementDate = x.COMMENCEMENTDATE,
                            completionDate = x.COMPLETIONDATE,
                            acceptance = x.ACCEPTANCE,
                            loanApplicationId = x.LOANAPPLICATIONID,
                            loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                            projectLocation = x.PROJECTLOCATION,
                            approvalStatusId = trail.APPROVALSTATUSID,
                            approvalTrailId = trail.APPROVALTRAILID,
                            inspectionDate = x.INSPECTIONDATE,
                            currencyId = x.CURRENCYID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == trail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                            currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                        }).GroupBy(x => x.projectSiteReportId).Select(x => x.OrderByDescending(p => p.approvalTrailId).FirstOrDefault()).Where((trail => (trail.approvalStatusId == (short)ApprovalStatusEnum.Referred
                                    && trail.loopedStaffId == initiator) || trail.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)).ToList();
            var psr = psr1.Union(psr2).ToList();
            return psr;

        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports(int projectSiteReportId)
        {
            return context.TBL_PSR_PROJECT_SITE_REPORT.Where(x=>x.PROJECTSITEREPORTID== projectSiteReportId && x.DELETED == false).Select(x => new ProjectSiteReportViewModel
            {
                projectSiteReportId = x.PROJECTSITEREPORTID,
                psrReportTypeId = x.PSRREPORTTYPEID,
                clientName = x.CLIENTNAME,
                contractorName = x.CONTRACTORNAME,
                consultantName = x.CONSULTANTNAME,
                acceptance = x.ACCEPTANCE,
                projectAmount = x.PROJECTAMOUNT,
                projectDescription = x.PROJECTDESCRIPTION,
                commencementDate = x.COMMENCEMENTDATE,
                completionDate = x.COMPLETIONDATE,
                //nextVisitationDate = x.NEXTVISITATIONDATE,
                loanApplicationId = x.LOANAPPLICATIONID,
                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                projectLocation = x.PROJECTLOCATION,
                approvalStatusId = x.APPROVALSTATUSID,
                inspectionDate = x.INSPECTIONDATE,
                currencyId = x.CURRENCYID,
                psrRepeortType = (from r in context.TBL_PSR_REPORT_TYPE where r.PSRREPORTTYPEID == x.PSRREPORTTYPEID select r.REPORTTYPENAME).FirstOrDefault(),
                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),

            }).OrderByDescending(o => o.projectSiteReportId)
                .ToList();
        }

        public bool SubmitApproval(ProjectSiteReportViewModel model)
        {
            bool responce = false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.projectSiteReportId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.ProjectSiteReportApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var document = context.TBL_PSR_PROJECT_SITE_REPORT.Where(o => o.PROJECTSITEREPORTID == model.projectSiteReportId).FirstOrDefault();

                        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            if (document != null)
                            {
                                document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                        }
                        else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                           
                        }

                     }

                    responce = context.SaveChanges() > 0;
                    transaction.Commit();

                    return responce;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }

        public bool SubmitAcceptance(ProjectSiteReportViewModel model)
        {
            bool responce = false;
            var document = context.TBL_PSR_PROJECT_SITE_REPORT.Where(o => o.PROJECTSITEREPORTID == model.projectSiteReportId).FirstOrDefault();
            if(document != null)
            {
                document.ACCEPTANCE = true;
                responce = context.SaveChanges() > 0;
                
            }
           
            return responce;
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReportApprovals(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ProjectSiteReportApproval).ToList();

            var records =  (from x in context.TBL_PSR_PROJECT_SITE_REPORT
                   join atrail in context.TBL_APPROVAL_TRAIL on x.PROJECTSITEREPORTID equals atrail.TARGETID
                   where x.DELETED == false 
                    && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                    || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                    || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                    || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                    //&& x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && atrail.APPROVALSTATEID != (int)ApprovalState.Ended
                    && atrail.RESPONSESTAFFID == null
                    && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                    && atrail.LOOPEDSTAFFID == null
                    && atrail.OPERATIONID == (int)OperationsEnum.ProjectSiteReportApproval
                   select new ProjectSiteReportViewModel
                   {
                       projectSiteReportId = x.PROJECTSITEREPORTID,
                       psrReportTypeId = x.PSRREPORTTYPEID,
                       clientName = x.CLIENTNAME,
                       contractorName = x.CONTRACTORNAME,
                       consultantName = x.CONSULTANTNAME,
                       projectAmount = x.PROJECTAMOUNT,
                       projectDescription = x.PROJECTDESCRIPTION,
                       commencementDate = x.COMMENCEMENTDATE,
                       completionDate = x.COMPLETIONDATE,
                       nextVisitationDate = context.TBL_PSR_NEXT_INSPECTION_TASK.Where(p=>p.PROJECTSITEREPORTID == x.PROJECTSITEREPORTID && p.DELETED == false).Select(p=>p.NEXTINSPECTIONDATE).FirstOrDefault(),
                       loanApplicationId = x.LOANAPPLICATIONID,
                       projectLocation = x.PROJECTLOCATION,
                       approvalStatusId = x.APPROVALSTATUSID,
                       inspectionDate = x.INSPECTIONDATE,
                       operationId = atrail.OPERATIONID,
                       systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                       psrRepeortType = (from r in context.TBL_PSR_REPORT_TYPE where r.PSRREPORTTYPEID == x.PSRREPORTTYPEID select r.REPORTTYPENAME).FirstOrDefault(),
                       currency = context.TBL_CURRENCY.Where(o=>o.CURRENCYID==x.CURRENCYID).Select(o=>o.CURRENCYNAME).FirstOrDefault(),
                       approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),

                   }).OrderByDescending(o => o.projectSiteReportId)
                .ToList();
            return records;
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReportApproved(int staffId)
        {
            var records = (from x in context.TBL_PSR_PROJECT_SITE_REPORT
                           join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           where x.DELETED == false
                            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            && b.CREATEDBY == staffId
                           select new ProjectSiteReportViewModel
                           {
                               projectSiteReportId = x.PROJECTSITEREPORTID,
                               psrReportTypeId = x.PSRREPORTTYPEID,
                               clientName = x.CLIENTNAME,
                               acceptance = x.ACCEPTANCE,
                               contractorName = x.CONTRACTORNAME,
                               consultantName = x.CONSULTANTNAME,
                               projectAmount = x.PROJECTAMOUNT,
                               projectDescription = x.PROJECTDESCRIPTION,
                               commencementDate = x.COMMENCEMENTDATE,
                               completionDate = x.COMPLETIONDATE,
                               nextVisitationDate = context.TBL_PSR_NEXT_INSPECTION_TASK.Where(p => p.PROJECTSITEREPORTID == x.PROJECTSITEREPORTID && p.DELETED == false).Select(p => p.NEXTINSPECTIONDATE).FirstOrDefault(),
                               loanApplicationId = x.LOANAPPLICATIONID,
                               projectLocation = x.PROJECTLOCATION,
                               approvalStatusId = x.APPROVALSTATUSID,
                               inspectionDate = x.INSPECTIONDATE,
                               psrRepeortType = (from r in context.TBL_PSR_REPORT_TYPE where r.PSRREPORTTYPEID == x.PSRREPORTTYPEID select r.REPORTTYPENAME).FirstOrDefault(),
                               currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                               approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),

                           }).OrderByDescending(o => o.projectSiteReportId).ToList();
            return records;
        }


        public int AddProjectSiteReport(ProjectSiteReportViewModel model)
        {
            if (model.approvalStatusId == (short)ApprovalStatusEnum.Referred)
            {
                //int responce = 0;

                using (var transaction = context.Database.BeginTransaction())
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = model.projectSiteReportId;
                    workflow.Comment = "Update has been applied, Request for Project Site Report Approval";
                    workflow.OperationId = (int)OperationsEnum.ProjectSiteReportApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();
                    try
                    {

                        if(context.SaveChanges() > 0)
                        {
                            transaction.Commit();
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                        
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();


                        throw new SecureException(ex.Message);
                    }
                    
                }
            }
            else
            {
                var p = model.loanApplicationViewModel.FirstOrDefault();
                var entity = new TBL_PSR_PROJECT_SITE_REPORT
                {
                    PSRREPORTTYPEID = model.psrReportTypeId,
                    CLIENTNAME = model.clientName,
                    CONTRACTORNAME = model.contractorName,
                    CONSULTANTNAME = model.consultantName,
                    PROJECTAMOUNT = model.projectAmount,
                    PROJECTDESCRIPTION = model.projectDescription,
                    COMMENCEMENTDATE = model.commencementDate,
                    COMPLETIONDATE = model.completionDate,
                    //NEXTVISITATIONDATE = model.nextVisitationDate,
                    // COMPANYID = model.companyId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate(),
                    LOANAPPLICATIONID = p.loanApplicationId,
                    LOANAPPLICATIONDETAILID = p.loanApplicationDetailId,
                    PROJECTLOCATION = model.projectLocation,
                    CURRENCYID = model.currencyId,
                    INSPECTIONDATE = model.inspectionDate,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                };

                var id = context.TBL_PSR_PROJECT_SITE_REPORT.Add(entity);

                if (context.SaveChanges() > 0)
                {
                    foreach (var x in model.loanApplicationViewModel)
                    {
                        var proj = new TBL_PSR_PROJECT_FACILITIES
                        {
                            CREATEDBY = model.createdBy,
                            DATETIMECREATED = general.GetApplicationDate(),
                            LOANAPPLICATIONID = x.loanApplicationId,
                            LOANAPPLICATIONDETAILID = x.loanApplicationDetailId,
                            LOANID = x.loanId,
                            LANSYSTEMTYPEID = x.loanSystemTypeId,
                            DELETED = false,
                            PROJECTSITEREPORTID = entity.PROJECTSITEREPORTID
                        };

                        context.TBL_PSR_PROJECT_FACILITIES.Add(proj);

                    }

                    context.SaveChanges();
                }

               
                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' created by {auditStaff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                });
                // Audit Section end ------------------------

                context.SaveChanges();

                return entity.PROJECTSITEREPORTID;
            }
        }

        public bool ProjectSiteReportGoForApproval(ProjectSiteReportViewModel model)
        {
            var id = context.TBL_PSR_PROJECT_SITE_REPORT.Where(x=>x.PROJECTSITEREPORTID==model.projectSiteReportId && x.DELETED == false).Select(x=>x).FirstOrDefault();

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = id.PROJECTSITEREPORTID;
                workflow.Comment = "Request for Project Site Report approval";
                workflow.OperationId = (int)OperationsEnum.ProjectSiteReportApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

            id.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

            return context.SaveChanges() > 0;
        }

        public bool UpdateProjectSiteReport(ProjectSiteReportViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            entity.PSRREPORTTYPEID = model.psrReportTypeId;
            entity.CLIENTNAME = model.clientName;
            entity.CONTRACTORNAME = model.contractorName;
            entity.CONSULTANTNAME = model.consultantName;
            entity.PROJECTAMOUNT = model.projectAmount;
            entity.PROJECTDESCRIPTION = model.projectDescription;
            entity.COMMENCEMENTDATE = model.commencementDate;
            entity.COMPLETIONDATE = model.completionDate;
            //entity.NEXTVISITATIONDATE = model.nextVisitationDate;
            entity.PROJECTLOCATION = model.projectLocation;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.INSPECTIONDATE = model.inspectionDate;
            entity.DATETIMEUPDATED = DateTime.Now;
            entity.CURRENCYID = model.currencyId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PROJECTSITEREPORTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteProjectSiteReport(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PROJECTSITEREPORTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        public IEnumerable<PsrReportTypeViewModel> GetPsrReportTypes()
        {
            return context.TBL_PSR_REPORT_TYPE.Where(x => x.DELETED == false)
                .Select(x => new PsrReportTypeViewModel
                {
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    reportTypeName = x.REPORTTYPENAME,
                })
                .ToList();
        }

        public PsrReportTypeViewModel GetPsrReportTypesById(int id)
        {
            return context.TBL_PSR_REPORT_TYPE.Where(x => x.DELETED == false && x.PSRREPORTTYPEID == id)
                .Select(x => new PsrReportTypeViewModel
                {
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    reportTypeName = x.REPORTTYPENAME,
                })
                .FirstOrDefault();
        }

        #region
        public IEnumerable<PsrRecommendationViewModel> GetPsrRecommendations(int id)
        {
            return context.TBL_PSR_RECOMMENDATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrRecommendationViewModel
                {
                    psrRecommendationId = x.PSRRECOMMENDATIONID,
                    //customerRating = x.CUSTOMERRATING,
                    //projectRiskRating = x.PROJECTRISKRATING,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

        public bool UpdatePsrRecommendation(PsrRecommendationViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_RECOMMENDATION.Find(id);
            entity.COMMENTS = model.comment;
            //entity.CUSTOMERRATING = model.customerRating;
            //entity.PROJECTRISKRATING = model.projectRiskRating;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrRecommendationUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Recommendation '{entity.PSRRECOMMENDATIONID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRRECOMMENDATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        public bool AddPsrRecommendation(PsrRecommendationViewModel model)
        {
            var validate = context.TBL_PSR_RECOMMENDATION.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId).Select(p => p.PROJECTSITEREPORTID).ToList();
            if (validate.Any())
            {
                throw new SecureException("Recommendation has already been captured");
            }

            var entity = new TBL_PSR_RECOMMENDATION
            {
                COMMENTS = model.comment,
                //CUSTOMERRATING = model.customerRating,
                //PROJECTRISKRATING = model.projectRiskRating,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_RECOMMENDATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrRecommendationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Recommendation '{entity.PSRRECOMMENDATIONID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrRecommendation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_RECOMMENDATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrRecommendationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Recommendation '{entity.PSRRECOMMENDATIONID}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL =user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRRECOMMENDATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region

        public IEnumerable<PsrPerformanceEvaluationViewModel> GetPsrPerformanceEvaluations(int id)
        {
            return context.TBL_PSR_PERFORMANCE_EVALUATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceEvaluationViewModel
                {
                    psrPerformanceEvaluationId = x.PSRPERFORMANCEEVALUATIONID,
                    apgIssued = x.APGISSUED,
                    disbursedTodate = x.DISBURSEDTODATE,
                    initialProjectSum = x.INITIALPROJECTSUM,
                    paymentToDate = x.PROGRESSPAYMENT == null ? x.DISBURSEDTODATE : x.PROGRESSPAYMENT,
                    pmuAssessed = x.PMUASSESSED,
                    projectSum = x.PROJECTSUM,
                    progressPayment = x.PROGRESSPAYMENT == null ? x.DISBURSEDTODATE : x.PROGRESSPAYMENT,
                    vowdToDate = x.VOWDTODATE,
                    amortisedApg = x.AMORTISEDAPG,
                    costVariation = x.COSTVARIATION,
                    certifiedVowd = x.CERTIFIEDVOWD,
                    timeVariation = x.TIMEVARIATION,
                    consoltantVowd = x.CONSULTANTVOWD,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    amountReceived = x.AMOUNTRECEIVED,
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o=>o.PSRREPORTTYPEID== x.PSRREPORTTYPEID).Select(o=>o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderByDescending(x => x.psrPerformanceEvaluationId)
                .ToList();
        }
        public bool UpdatePsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model, int id)
        {
            var projectSite = context.TBL_PSR_PROJECT_SITE_REPORT.Find(model.projectSiteReportId);
            if (model.projectSum == null || model.projectSum < 1)
            {
                model.projectSum = projectSite.PROJECTAMOUNT;
            }

            if (model.psrReportTypeId == 2)
            {
                var projectAnalysis = context.TBL_PSR_ANALYSIS.Where(a => a.PROJECTSITEREPORTID == model.projectSiteReportId).FirstOrDefault();
                if (projectAnalysis != null)
                {
                    projectAnalysis.AMOUNTDISBURSED = model.disbursedTodate;
                    projectAnalysis.PMU = model.pmuAssessed;
                    projectAnalysis.IPC = model.certifiedVowd;
                    context.SaveChanges();
                }
            }

            var entity = this.context.TBL_PSR_PERFORMANCE_EVALUATION.Find(id);
            entity.APGISSUED = model.apgIssued;
            entity.DISBURSEDTODATE = model.disbursedTodate;
            entity.INITIALPROJECTSUM = model.initialProjectSum;
            entity.PAYMENTTODATE = model.paymentToDate;
            entity.PMUASSESSED = model.pmuAssessed;
            entity.PROJECTSUM = model.projectSum;
            entity.VOWDTODATE = model.vowdToDate;
            entity.COSTVARIATION = model.costVariation;
            entity.TIMEVARIATION = model.timeVariation;
            entity.CONSULTANTVOWD = model.consoltantVowd;
            entity.PROGRESSPAYMENT = model.progressPayment;
            entity.CREATEDBY = model.createdBy;
            entity.AMORTISEDAPG = model.amortisedApg;
            entity.AMOUNTRECEIVED = model.amountReceived;
            entity.CERTIFIEDVOWD = model.certifiedVowd;

            entity.LASTUPDATEDBY = model.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRPERFORMANCEEVALUATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddPsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model)
        {
            var validate = context.TBL_PSR_PERFORMANCE_EVALUATION.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId && p.DELETED == false).Select(p => p.PROJECTSITEREPORTID).ToList();
            if (validate.Any())
            {
                throw new SecureException("Performance Evaluation has already been captured");
            }

            
            var projectSite = context.TBL_PSR_PROJECT_SITE_REPORT.Find(model.projectSiteReportId);
            if (model.projectSum == null || model.projectSum < 1)
            {
                model.projectSum = projectSite.PROJECTAMOUNT;
            }

            if(model.psrReportTypeId == 2)
            {
                var projectAnalysis = context.TBL_PSR_ANALYSIS.Where(a=>a.PROJECTSITEREPORTID == model.projectSiteReportId).FirstOrDefault();
                if(projectAnalysis != null)
                {
                    projectAnalysis.AMOUNTDISBURSED = model.disbursedTodate;
                    projectAnalysis.PMU = model.pmuAssessed;
                    projectAnalysis.IPC = model.certifiedVowd;
                    context.SaveChanges();
                }
            }
            var entity = new TBL_PSR_PERFORMANCE_EVALUATION
            {
                APGISSUED = model.apgIssued,
                DISBURSEDTODATE = model.disbursedTodate,
                INITIALPROJECTSUM = projectSite.PROJECTAMOUNT,
                PAYMENTTODATE = model.paymentToDate,
                PMUASSESSED = model.pmuAssessed,
                PROJECTSUM = model.projectSum,
                VOWDTODATE = model.vowdToDate,
                COSTVARIATION = model.costVariation,
                TIMEVARIATION = model.timeVariation,
                CONSULTANTVOWD = model.consoltantVowd,
                PROGRESSPAYMENT = model.progressPayment,
                CREATEDBY = model.createdBy,
                AMORTISEDAPG = model.amortisedApg,
                AMOUNTRECEIVED = model.amountReceived,
                CERTIFIEDVOWD = model.certifiedVowd,
                DATETIMECREATED = general.GetApplicationDate(),
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                PSRREPORTTYPEID = model.psrReportTypeId
            };

             context.TBL_PSR_PERFORMANCE_EVALUATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------
            return context.SaveChanges() != 0;
        }

        public bool DeletePsrPerformanceEvaluation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PERFORMANCE_EVALUATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRPERFORMANCEEVALUATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrCommentImage(int id, UserInfo user)
        {
            var entity = context.TBL_PSR_COMMENT_IMAGES.Find(id);
            context.TBL_PSR_COMMENT_IMAGES.Remove(entity);
            return context.SaveChanges() != 0;
        }

        public bool DeletePsrImage(int id, UserInfo user)
        {
            var entity = context.TBL_PSR_IMAGES.Find(id);
            context.TBL_PSR_IMAGES.Remove(entity);
            return context.SaveChanges() != 0;
        }
        #endregion


        #region
        public IEnumerable<PsrObservationViewModel> GetPsrObservations(int id)
        {
            return context.TBL_PSR_OBSERVATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrObservationViewModel
                {
                    psrObservationId = x.PSROBSERVATIONID,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

        public bool UpdatePsrObservation(PsrObservationViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_OBSERVATION.Find(id);
            entity.COMMENTS = model.comment;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrObservationUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Observation '{entity.PSROBSERVATIONID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSROBSERVATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddPsrObservation(PsrObservationViewModel model)
        {
            var validate = context.TBL_PSR_OBSERVATION.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId && p.DELETED == false).Select(p => p.PROJECTSITEREPORTID).ToList();
            if (validate.Any())
            {
                throw new SecureException("Observation has already been captured");
            }

            var entity = new TBL_PSR_OBSERVATION
            {
                COMMENTS = model.comment,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_OBSERVATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrObservationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Observation '{entity.PSROBSERVATIONID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrObservation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_OBSERVATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrObservationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                //DETAIL = $"TBL_Psr Observation '{entity.DESCRIPTION}' was deleted by {auditStaff}",
                DETAIL = $"TBL_Psr Observation was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSROBSERVATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion


        #region

        public IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTasks(int id)
        {
            return context.TBL_PSR_NEXT_INSPECTION_TASK.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrNextInspectionTaskViewModel
                {
                    psrNextInspectionTaskId = x.PSRNEXTINSPECTIONTASKID,
                    comment = x.COMMENTS,
                    isDone = x.ISDONE,
                    nextInspectionDate = x.NEXTINSPECTIONDATE,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                })
                .ToList();
        }
        public bool UpdatePsrNextInspectionTask(PsrNextInspectionTaskViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_NEXT_INSPECTION_TASK.Find(id);
            entity.COMMENTS = model.comment;
            entity.ISDONE = model.isDone;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrNextInspectionTaskUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Next Inspection Task '{entity.PSRNEXTINSPECTIONTASKID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRNEXTINSPECTIONTASKID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        public bool AddPsrNextInspectionTask(PsrNextInspectionTaskViewModel model)
        {
            var validate = context.TBL_PSR_NEXT_INSPECTION_TASK.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId).Select(p => p.PROJECTSITEREPORTID).ToList();
            if (validate.Any())
            {
                throw new SecureException("Next Inspection Task has already been captured");
            }

            var entity = new TBL_PSR_NEXT_INSPECTION_TASK
            {
                COMMENTS = model.comment,
                ISDONE = model.isDone,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                CREATEDBY = model.createdBy,
                NEXTINSPECTIONDATE = model.nextInspectionDate,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_NEXT_INSPECTION_TASK.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrNextInspectionTaskAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Next Inspection Task '{entity.PSRNEXTINSPECTIONTASKID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrNextInspectionTask(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_NEXT_INSPECTION_TASK.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrNextInspectionTaskDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Next Inspection Task '{entity.PSRNEXTINSPECTIONTASKID}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL =user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRNEXTINSPECTIONTASKID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region
        public IEnumerable<PsrCommentViewModel> GetPsrComments(int id)
        {
            return context.TBL_PSR_COMMENT.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrCommentViewModel
                {
                    psrCommentId = x.PSRCOMMENTID,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

        public bool UpdatePsrComment(PsrCommentViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_COMMENT.Find(id);
            entity.COMMENTS = model.comment;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrCommentUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Comment '{entity.PSRCOMMENTID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRCOMMENTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        public bool AddPsrComment(PsrCommentViewModel model)
        {
            var validate = context.TBL_PSR_COMMENT.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId && p.DELETED == false).Select(p => p.PROJECTSITEREPORTID).ToList();
            if (validate.Any())
            {
                throw new SecureException("Comment has already been captured");
            }

            var entity = new TBL_PSR_COMMENT
            {
                COMMENTS = model.comment,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_COMMENT.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrCommentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Comment '{entity.PSRCOMMENTID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrComment(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_COMMENT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrCommentDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Comment '{entity.PSRCOMMENTID}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRCOMMENTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        #endregion


      public PsrReportViewModel GeneratePSRReport(int id)
        {
            var report = new PsrReportViewModel
            {
                apgExposure = "",
                comment = context.TBL_PSR_COMMENT.Where(o=>o.PROJECTSITEREPORTID == id && o.DELETED == false).Select(o=>o.COMMENTS).FirstOrDefault(),
                facilityDetail = "",
                observation = context.TBL_PSR_OBSERVATION.Where(o => o.PROJECTSITEREPORTID == id && o.DELETED == false).Select(o => o.COMMENTS).FirstOrDefault(),
                recomendation = context.TBL_PSR_RECOMMENDATION.Where(o=>o.PROJECTSITEREPORTID==id && o.DELETED == false).Select(o=>o.COMMENTS).FirstOrDefault(),
                taskForNextInspection = context.TBL_PSR_NEXT_INSPECTION_TASK.Where(o=>o.PROJECTSITEREPORTID==id && o.DELETED == false).Select(o=>o.COMMENTS).FirstOrDefault(),
            };

            return report;
        }

        #region
        public IEnumerable<PsrPerformanceAnalysisViewModel> GetPsrPerformanceAnalysis(int id)
        {
            return context.TBL_PSR_ANALYSIS.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceAnalysisViewModel
                {
                    psrAnalysisId = x.PSRANALYSISID,
                    valueOfCollateral = x.VALUEOFCOLLATERAL,
                    ipc = x.IPC,
                    pmu = x.PMU,
                    //less = x.LESS,
                    amountDisbursed = x.AMOUNTDISBURSED,
                    amountRequested = x.AMOUNTREQUESTED,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderByDescending(o => o.psrAnalysisId)
                .ToList();
        }
        public bool UpdatePsrPerformanceAnalysis(PsrPerformanceAnalysisViewModel model, int id)
        {
            var entity = this.context.TBL_PSR_ANALYSIS.Find(id);
            entity.VALUEOFCOLLATERAL = model.valueOfCollateral;
            entity.IPC = model.ipc;
            entity.PMU = model.pmu;
            entity.AMOUNTDISBURSED = model.amountDisbursed;
            entity.AMOUNTREQUESTED = model.amountRequested;
            entity.DELETED = false;
         
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceAnalysisUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"TBL_Psr Performance Analysis '{entity.PSRANALYSISID}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRANALYSISID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddPsrPerformanceAnalysis(PsrPerformanceAnalysisViewModel model)
        {
            var validate = context.TBL_PSR_ANALYSIS.Where(p => p.PROJECTSITEREPORTID == model.projectSiteReportId && p.DELETED == false).Select(p=>p.PROJECTSITEREPORTID).ToList();
            if(validate.Any())
            {
                throw new SecureException("Performance analysis has already been captured");
            }

            var entity = new TBL_PSR_ANALYSIS
            {
                VALUEOFCOLLATERAL = model.valueOfCollateral,
                IPC = model.ipc,
                PMU = model.pmu,
                AMOUNTDISBURSED = model.amountDisbursed,
                AMOUNTREQUESTED = model.amountRequested,
                PROJECTSITEREPORTID = model.projectSiteReportId,
            };

            context.TBL_PSR_ANALYSIS.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceAnalysisAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Performance Analysis '{entity.PSRANALYSISID}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------
            return context.SaveChanges() != 0;
        }

        public bool DeletePsrPerformanceAnalysis(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_ANALYSIS.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceAnalysisDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Performance Analysis '{entity.PSRANALYSISID}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRANALYSISID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region
        public IEnumerable<PsrImagesViewModel> GetPsrImages(int id)
        {
            return context.TBL_PSR_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrImagesViewModel
                {
                    psrImageId = x.PSRIMAGEID,
                    fileData = x.FILEDATA,
                    fileExtension = x.FILEEXTENSION,
                    fileName = x.FILENAME,
                    imageCaption = x.IMAGECAPTION,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderByDescending(o => o.psrImageId)
                .ToList();
        }

        public PsrImagesViewModel GetPsrImage(int id)
        {
            return context.TBL_PSR_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrImagesViewModel
                {
                    psrImageId = x.PSRIMAGEID,
                    fileData = x.FILEDATA,
                    fileExtension = x.FILEEXTENSION,
                    fileName = x.FILENAME,
                    imageCaption = x.IMAGECAPTION,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).FirstOrDefault();
        }

        public int AddPsrImage(PsrImagesViewModel model, byte[] buffer)
        {
            var existing = context.TBL_PSR_IMAGES.Where(x => x.FILENAME == model.fileName && x.PROJECTSITEREPORTID == model.projectSiteReportId)
            .Select(x => new PsrImagesViewModel
            {
                psrImageId = x.PSRIMAGEID,
                fileData = x.FILEDATA,
                fileExtension = x.FILEEXTENSION,
                fileName = x.FILENAME,
                imageCaption = x.IMAGECAPTION,
                projectSiteReportId = x.PROJECTSITEREPORTID,
            }).FirstOrDefault();

            if (existing != null && model.overwrite == false) return 3;

            var entity = new TBL_PSR_IMAGES
            {
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension.ToLower(),
                FILESIZE = model.fileSize,
                FILESIZEUNIT = model.fileSizeUnit,
                FILEDATA = buffer,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                IMAGECAPTION = model.imageCaption,
                PROJECTSITEREPORTID = model.projectSiteReportId,
            };

            context.TBL_PSR_IMAGES.Add(entity);
            context.SaveChanges();
            return 2;
        }

        public int UpdatePsrImage(PsrImagesViewModel model, byte[] buffer)
        {
            
            var recored = context.TBL_PSR_IMAGES.Find(model.psrImageId);
            if (recored != null) {
                recored.FILENAME = model.fileName;
                recored.FILEEXTENSION = model.fileExtension.ToLower();
                recored.FILESIZE = model.fileSize;
                recored.FILESIZEUNIT = model.fileSizeUnit;
                recored.FILEDATA = buffer;
                recored.CREATEDBY = model.createdBy;
                recored.DATETIMECREATED = DateTime.Now;
                recored.IMAGECAPTION = model.imageCaption;
                recored.PROJECTSITEREPORTID = model.projectSiteReportId;
                }
           
            context.SaveChanges();
         
            return 2;
        }

        public int AddPsrCommentImage(PsrCommentImagesViewModel model, byte[] buffer)
        {
            var existing = context.TBL_PSR_COMMENT_IMAGES.Where(x => x.FILENAME == model.fileName && x.PROJECTSITEREPORTID == model.projectSiteReportId)
            .Select(x => new PsrCommentImagesViewModel
            {
                psrCommentImageId = x.PSRCOMMENTIMAGEID,
                fileData = x.FILEDATA,
                fileExtension = x.FILEEXTENSION,
                fileName = x.FILENAME,
                imageCaption = x.IMAGECAPTION,
                projectSiteReportId = x.PROJECTSITEREPORTID,
            }).FirstOrDefault();

            if (existing != null && model.overwrite == false) return 3;

            var entity = new TBL_PSR_COMMENT_IMAGES
            {
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension.ToLower(),
                FILESIZE = model.fileSize,
                FILESIZEUNIT = model.fileSizeUnit,
                FILEDATA = buffer,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                IMAGECAPTION = model.imageCaption,
                PROJECTSITEREPORTID = model.projectSiteReportId,
            };

            context.TBL_PSR_COMMENT_IMAGES.Add(entity);
            context.SaveChanges();
            return 2;
        }

        public int UpdatePsrCommentImage(PsrCommentImagesViewModel model, byte[] buffer)
        {
            var existing = context.TBL_PSR_COMMENT_IMAGES.Find(model.psrCommentImageId);

            if (existing != null)
            {
                existing.FILENAME = model.fileName;
                existing.FILEEXTENSION = model.fileExtension.ToLower();
                existing.FILESIZE = model.fileSize;
                existing.FILESIZEUNIT = model.fileSizeUnit;
                existing.FILEDATA = buffer;
                existing.CREATEDBY = model.createdBy;
                existing.DATETIMECREATED = DateTime.Now;
                existing.IMAGECAPTION = model.imageCaption;
                existing.PROJECTSITEREPORTID = model.projectSiteReportId;

            }
            context.SaveChanges();
            return 2;
        }

        public IEnumerable<PsrCommentImagesViewModel> GetPsrCommentImages(int id)
        {
            return context.TBL_PSR_COMMENT_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrCommentImagesViewModel
                {
                    psrCommentImageId = x.PSRCOMMENTIMAGEID,
                    fileData = x.FILEDATA,
                    fileExtension = x.FILEEXTENSION,
                    fileName = x.FILENAME,
                    imageCaption = x.IMAGECAPTION,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderByDescending(o => o.psrCommentImageId)
                .ToList();
        }

        public PsrCommentImagesViewModel GetPsrCommentImage(int id)
        {
            return context.TBL_PSR_COMMENT_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrCommentImagesViewModel
                {
                    psrCommentImageId = x.PSRCOMMENTIMAGEID,
                    fileData = x.FILEDATA,
                    fileExtension = x.FILEEXTENSION,
                    fileName = x.FILENAME,
                    imageCaption = x.IMAGECAPTION,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).FirstOrDefault();
        }

        #endregion

    }
}


// kernel.Bind<IProjectSiteReportRepository>().To<ProjectSiteReportRepository>();
// ProjectSiteReportAdded = ???, ProjectSiteReportUpdated = ???, ProjectSiteReportDeleted = ???,

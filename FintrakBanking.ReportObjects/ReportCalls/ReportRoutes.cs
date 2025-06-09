using FintrakBanking.Common;
using FintrakBanking.Common.Crypto;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common.Extensions;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace FintrakBanking.ReportObjects.ReportCalls
{
    public class HashProperty
    {
        private FinTrakBankingContext context;
        public string hashedDateValue { get; set; }
    }
    public class ReportRoutes : IReportRoutes
    {
        HashHelper hash = new HashHelper();
        HashProperty hashProp = new HashProperty();
        string reportPath = CommonHelpers.ReportPath;
        Protection crypto = new Protection();
       string cryptoKey = "sqluser10$";
        string dateInfor = DateTime.Now.ToString("ddMMyyyyHHmmss");
       

        private HashProperty GetHashedDateValue(string dateInforString)
        {
            return new HashProperty
            {
                hashedDateValue = hash.HashString(dateInforString).Replace("-", "")
            };
        }

        private IQueryable<TBL_LOAN_APPLICATION> LoanApplication(int companyId, int staffId)
        {
            IQueryable<TBL_LOAN_APPLICATION> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var customerSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                data = context.TBL_LOAN_APPLICATION.Where(c => c.COMPANYID == companyId && c.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID <= customerSensitivityLevelId);
            }
            return data;
        }
        public IEnumerable<AuditViewModel> AuditType(string searchValue)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var auditType = from x in context.TBL_AUDIT_TYPE
                                where x.AUDITTYPENAME.ToUpper().Contains(searchValue.ToUpper())
                                select new AuditViewModel
                                {
                                    auditTypeId = x.AUDITTYPEID,
                                    auditType = x.AUDITTYPENAME,
                                  
                                };
                return auditType.ToList();

            }

        }
        public List<GLAccountSearchViewModel> GLAccount(string searchValue)
        {
            var gl = new List<GLAccountSearchViewModel>();

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                 gl = (from x in context.TBL_CHART_OF_ACCOUNT
                                where x.ACCOUNTNAME.ToLower().StartsWith(searchValue.ToLower()) || x.ACCOUNTCODE.ToLower().StartsWith(searchValue.ToLower())
                         select new GLAccountSearchViewModel
                         {
                             GLAccountCode=x.ACCOUNTCODE,
                             GLAccount   = x.ACCOUNTNAME,
                             GLAccountId = x.GLACCOUNTID
                         }).ToList();
            }
            return gl;

        }
        public string GetWorkflowSLA(int loanApplicationId, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            int operationId = (int)OperationsEnum.CreditAppraisal;
            path = reportPath + "ReportViews/ApprovalTrailWith_SLA.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId +  "&operationId=" + operationId + "&loanApplicationId=" + loanApplicationId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }
        public string GetWorkflowSLAMonitoring(int companyId, DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SLAReport.aspx?companyId=" + companyId.ToString() + "&approvalStatus=" + dateRange.approvalStatus + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&operationId=" + dateRange.operationId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }

        public string GetLoanScheduleReport(int tearmLoanId, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanRepaymentSchedule.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId.ToString() + "&tearmLoanId=" + tearmLoanId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }

        public string GetSectorLimitMonitoringReport(int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SectorialLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }

        public string GetBranchLoanAmountLimit(int branchId, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/BranchLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId.ToString() + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }
        public string GetWorkflowDefinition(int operationId, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/Workflow.aspx?companyId=" + companyId.ToString() + "&operationId=" + operationId.ToString() + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }
        public string GetDisburstLoans(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/DisbursedLoans.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&loanRefNo=" + dateRange.loanRefNo + "&branchId=" + dateRange.branchId + "&productClassId=" + dateRange.productClassId + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetRunningFacilities(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RunningFacilitiesReport.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&crmsCode="+dateRange.crmSCode;
            // path = reportPath + "ReportViews/RunningFacilitiesReport.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy")  + "&branchId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetLoanStatement(int companyId, int loanId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanStatement.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString() + "&staffId=" + staffId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetLoanAnniversery(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanAnniversery.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId  + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetLoanDocumentWaived(int companyId, DateRange dateRange, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentWaived.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&branchId="+ dateRange.branchId + "&searchParameter=" + dateRange.searchParameter + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetLoanDocumentDeferred(int companyId, DateRange dateRange, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferred.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&branchId=" + dateRange.branchId + "&searchParameter=" + dateRange.searchParameter + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetLoanDocumentDeferrals(int companyId, DateRange dateRange, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferral.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetLoanDocumentDeferralsMCC(int companyId, DateRange dateRange, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferalsForMCC.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&branchCode=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetCollateralEstimated(int companyId, string collateralCode, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
              path = reportPath + "ReportViews/CollateralEstimated.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&collateralCode=" + collateralCode.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
          //  path = reportPath + "ReportViews/CollateralEstimated.aspx?companyId=" + companyId.ToString()  + "&collateralCode=" + collateralCode.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;

            return path;
        }

        public string GetFCYScheuledLoan(int companyId, int loanId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/FCYScheuledLoan.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString() + "&staffId=" + staffId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        #region Offer Letter Generation

        //public string GetGeneratedOfferLetter(string applicationRefNumber)
        //{
        //    try
        //    {
        //        string path = string.Empty;
        //        path = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber;
        //        return path;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

        public string GetGeneratedOfferLetter(string applicationRefNumber)
        {
            try
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var productClassId = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x=>x.PRODUCTCLASSID).FirstOrDefault();
                    var productClassProcessId = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x=>x.PRODUCT_CLASS_PROCESSID).FirstOrDefault();
                    return GetProductSpecificTemplate(productClassProcessId, productClassId, applicationRefNumber);
                }
              
              
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetGeneratedCFLOfferLetter(string applicationRefNumber,  string ActionByName)
        {
            try
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var statusCode = "90"; // Approved 
                    var workflowStage = "14"; // Offer Letter
                    var reasonForRejection = String.Empty;
                    var loanApplication = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).FirstOrDefault();
                    //var productClassId = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
                    var productClassProcessId = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x => x.PRODUCT_CLASS_PROCESSID).FirstOrDefault();
                    return GetProductSpecificTemplateCFL(productClassProcessId, loanApplication.PRODUCTCLASSID, applicationRefNumber,
                       statusCode, loanApplication.APIREQUESTID, workflowStage, reasonForRejection, ActionByName);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int GetLoanApplicationIdByReferenceNumber(string applicationRefNumber)
        {
            int loanAppId;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var appl = context.TBL_LOAN_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).FirstOrDefault();
                loanAppId = appl?.LOANAPPLICATIONID ?? 0;
            }
            return loanAppId;
        }

        public int GetLoanApplicationIdByReferenceNumberLMS(string applicationRefNumber)
        {
            int loanAppId;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var appl = context.TBL_LMSR_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).FirstOrDefault();
                loanAppId = appl.LOANAPPLICATIONID;
            }
            return loanAppId;
        }

        public int GetLmsrApplicationIdByReferenceNumber(string applicationRefNumber)
        {
            int loanAppId;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var appl = context.TBL_LMSR_APPLICATION.Where(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).FirstOrDefault();
                loanAppId = appl.LOANAPPLICATIONID;
            }
            return loanAppId;
        }

        public string GetGeneratedFORM3800BLOS(string applicationRefNumber)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "Credit/OfferLetterGeneration/FORM3800B_LOS.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                          return path;

            }
            catch (Exception ex)
            { 
                throw ex;
            }
        }
        public string GetGeneratedFORM3800BLMS(string applicationRefNumber)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "Credit/OfferLetterGeneration/FORM3800B_LMS.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetGeneratedOfferLetterLMS(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var OpID = context.TBL_LMSR_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(a => a).FirstOrDefault();
            if(OpID.OPERATIONID == (int)OperationsEnum.WrittenOffLoanReviewApprovalAppraisal)
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                return reportPath + "Credit/OfferLetterGeneration/ClassifiedAssetManagement.aspx?loanId=" + OpID.LOANAPPLICATIONID + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            }
            else
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                return reportPath + "Credit/OfferLetterGeneration/OfferLetterLMSR.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;

            }
        }

        public string GetProductSpecificTemplate(short? productClassProcessId, short? productClassId, string applicationRefNumber)
        {

            HashProperty hashValue = GetHashedDateValue(dateInfor);

            var links = new
            {
                General = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                IDF = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                FirstEdu = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                FirstTrader = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                BondsAndGuarantees = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                ImportFinance = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                CashBackedOnly = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                InvoiceDiscountingFacility = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
                //mhss = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue,
            };

            if (productClassProcessId == (short)ProductClassProcessEnum.CAMBased)
            {
                return links.General;
            }
            else 
            {
                switch (productClassId)
                {
                    case (short)ProductClassEnum.BondAndGuarantees:
                        return links.BondsAndGuarantees;

                    case (short)ProductClassEnum.CashCollaterized:
                        return links.CashBackedOnly;

                    //case (short)ProductClassEnum.FirstEdu:
                    //    return links.FirstEdu;

                    //case (short)ProductClassEnum.FirstTrader:
                    //    return links.FirstTrader;

                    //case (short)ProductClassEnum.ImportFinance:
                    //    return links.ImportFinance;

                    case (short)ProductClassEnum.InvoiceDiscountingFacility:
                        return links.InvoiceDiscountingFacility;

                    case (short)ProductClassEnum.MHSS:
                        return links.General;

                    default:
                        return links.General;
                }

               
            }

            
        }

        public string GetProductSpecificTemplateCFL(short? productClassProcessId, short? productClassId, string applicationRefNumber,
                       string StatusCode, string RequestId, string WorkflowStage, string ReasonForRejection, string ActionByName)
            {

            HashProperty hashValue = GetHashedDateValue(dateInfor);

            var links = new
            {
                General = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage="+ WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                IDF = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                FirstEdu = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                FirstTrader = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                BondsAndGuarantees = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                ImportFinance = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                CashBackedOnly = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
                InvoiceDiscountingFacility = reportPath + "Credit/OfferLetterGeneration/CFLOfferLetter.aspx?applicationRefNumber=" + applicationRefNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&StatusCode=" + StatusCode + "&RequestId=" + RequestId + "&WorkflowStage=" + WorkflowStage + "&ReasonForRejection=" + ReasonForRejection + "&ActionByName=" + ActionByName,
            };

            if (productClassProcessId == (short)ProductClassProcessEnum.CAMBased)
            {
                return links.General;
            }
            else
            {
                switch (productClassId)
                {
                    case (short)ProductClassEnum.BondAndGuarantees:
                        return links.BondsAndGuarantees;

                    case (short)ProductClassEnum.CashCollaterized:
                        return links.CashBackedOnly;

                    case (short)ProductClassEnum.InvoiceDiscountingFacility:
                        return links.InvoiceDiscountingFacility;

                    case (short)ProductClassEnum.MHSS:
                        return links.General;

                    default:
                        return links.General;
                }


            }


        }

        #endregion Offer Letter Generation

        #region Loan Monitoring Reports

        public string GetCovenantsApproachingDueDateReport(int staffId, DateRange dateRange,int companyId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/CovenantsApproachingDueDate.aspx?staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + companyId.ToString()   + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCollateralPropertyRevaluationReport(int companyId,DateRange dateRange, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralPropertyRevaluation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetCollateralPropertyDueForVisitationReport(int companyId, DateRange dateRange, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralVisitation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetSelfLiquidatingLoansReport(DateRange dateRange,int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/ExpiredSelfLiquidatingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetNonPerformingLoansReport(DateRange dateRange,int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/NonPeformingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&classification=" + dateRange.classification + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetExpiredOverdraftLoansReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/Overdraft.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string GetBondAndGuaranteeReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/BondAndGuarantee.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&approvalStatus="+dateRange.approvalStatus + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetAllContingentsReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/AllContingentsReport.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&reportAllType=" + dateRange.reportAllType + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetBondsAndGuaranteeReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/BandGReport.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&approvalStatus=" + dateRange.approvalStatus + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string GetCollateralInsuranceReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralInsurance.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetTurnoverCovenantReport(DateRange dateRange, int companyId, int staffId)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/TurnOverConvenant.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion Loan Monitoring Reports

        public string GetLoanCommercialReport(DateRange dateRange, int companyId, int staffId)
        {
            throw new NotImplementedException();
        }

        public string GetTeamAndRevolving(DateRange dateRange, int companyId, int staffId)
        {
            throw new NotImplementedException();
        }

        public string GetEarnedUnearnedInterest(DateRange dateRange, int companyId, int staffId)
        {
            throw new NotImplementedException();
        }

        public string GetPostedTransactions(ReportSearchEntity searchEntity, int companyId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/GrantedFacilities.aspx?companyId=" + companyId.ToString() + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + 
                "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&glAccountId=" + searchEntity.glAccountId + "&PostedByStaffId=" + searchEntity.PostedByStaffId + "&branchId=" + searchEntity.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetCollateralRevaluation( int companyId, int value)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/PropertyRevaluation.aspx?companyId=" + companyId.ToString() + "&value=" + value;
            return path;
        }

        public string AccountWithLein(ReportSearchEntity searchEntity)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/Lein.aspx?companyId=" + searchEntity.companyId.ToString() + "&searchParamemter=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/GetStakeholdersOnExpirationOfFTP.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate="+ searchEntity.startDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetFacilityApprovedNotUtilized(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/FacilityApprovedNotUntilized.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetRuningLoansByLoanType(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RuningLoansByLoanType.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&productClassId=" + searchEntity.productClassId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetAuditTrail(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
             path = reportPath + "ReportViews/AuditTrailView.aspx?username=" + dateRange.username + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId + "&auditTypeId="+dateRange.auditTypeId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue ;
            return path;
        }
        public string GetLoanInterestReceivableAndPayable(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanInterestReceivableAndPayable.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&productClassId=" + searchEntity.productClassId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetBlacklist(ReportSearchEntity searchEntity)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/Blacklist.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&customerCode=" + searchEntity.customerCode + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetDailyAccrual(ReportSearchEntity searchEntity)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/DailyAccrualReport.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&categoryId=" + searchEntity.categoryId + "&searchParamemter=" + searchEntity.searchParamemter + "&companyId=" + searchEntity.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetRepayment(ReportSearchEntity searchEntity)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/Repayment.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&operationId=" + searchEntity.operationId + "&companyId=" + searchEntity.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetCustomeFacilityRepayment(ReportSearchEntity searchEntity)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CustomeFacilityRepayment.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&valueCode=" + searchEntity.valueCode + "&companyId=" + searchEntity.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue; 
            return path;
        }


        public string GetStalledPerfection (DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/StalledPerfection.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId  + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetCollateralPerfectionYetToCommence(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralPerfectionYetToCommence.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetAllCommercialLoanReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/AllCommercialLoanReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string InsiderRelatedLoansReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;

            path = reportPath + "ReportViews/InsiderRelatedLoansReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&loanRefNo=" + dateRange.loanRefNo + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;


            return path;
        }

        public string GetLoanStatusReport(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;

            path = reportPath + "ReportViews/LoanStatusReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId.ToString() + "&ReportType=" + dateRange.ReportType + "&companyId=" + companyId.ToString() + "&loanRefNo=" + dateRange.loanRefNo + "&branchId=" + dateRange.branchId + "&productClassId=" + dateRange.productClassId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;


            return path;
        }

        public string GetUnearnedLoanInterestReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/UnearnedLoanInterest.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetReceivableInterestReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ReceivableInterest.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetCashBackedReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CashBacked.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetCashBackedBondAndGuarantee(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CashBackedBondAndGuarantee.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&productClassId="+ dateRange.productClassId +"&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetweeklyRecoveryReportforFINCON(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/WeeklyRecoveryReportForFINCON.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
         public string GetLoggingStatus(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoggingStatus.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&loginStatus=" + dateRange.loginStatus + "&branchCode=" + dateRange.branchCode;
            return path; 
        }


        public string GetCashCollaterizedCredits(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CashCollaterizedCredits.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetStaffPrivilegeChangeReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/StaffPriviledgeChangeReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetUserGroupChangeReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/UserGroupChangeReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetProfileActivityReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ProfileActivityReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetStaffRoleProfileGroupReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/StaffRoleProfileGroupReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetJobRequestReport(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/JobRequestReport.aspx?username=" + dateRange.username + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId + "&auditTypeId=" + dateRange.auditTypeId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetStaffRoleProfileActivityReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/StaffRoleProfileActivityReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetInActiveContigentLiabilityReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ContigentLiabilityInformation.aspx?companyId=" + dateRange.companyId + "&loanStatusId=" + dateRange.loanStatusId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetMiddleOfficeReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/MiddleOfficeReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetAnalystReport(DateRange dateRange, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/AnalystReport.aspx?username=" + dateRange.username + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId + "&auditTypeId=" + dateRange.auditTypeId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetCollateralValuationReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralValuationReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetLoanClassificationReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanClassification.aspx?classification=" + dateRange.classification + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetAgeAnalysisReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/AgeAnalysisReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetCreditScheduleReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CreditScheduleReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetSanctionLimitReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SanctionLimitReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetImpairedWatchListReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ImpairedWatchListReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetInsuranceReport(int companyId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/InsuranceReport.aspx?companyId=" + companyId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetExpiredReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ExpiredReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetExcessReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ExcessReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string GetUnutilizedFacilityReport(int companyId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/UnutilizedFacilityReport.aspx?companyId=" + companyId.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetDisbursalCreditTurnover(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);
            string path = string.Empty;
            path = reportPath + "ReportViews/DisbursalCreditTurnover.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetRuniningLoanReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RunningLoanReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

            

        public string GetLoanBookingReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanBookingReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId+ "&searchInfo=" + dateRange.searchInfo + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string Form3800BApprovedFacility(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ApprovedForm3800BFacilities.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId  + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetOutPutDocument(int loanApplicationId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/OutPutTemplate.aspx?loanApplicationId=" + loanApplicationId;
            return path;
        }

       public string SubmissionOfOriginalDocument(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SubmissionOfOriginalDocument.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&referenceNumber=" + dateRange.referenceNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string SubmissionOfOriginalDocuments(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SubmissionOfOriginalDocuments.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&referenceNumber=" + dateRange.referenceNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string CorporateCustomerCreation(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CorporateCustomerCreation.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string InsuranceSpoolReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/InsuranceSpoolReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&documentTypeId=" + dateRange.documentTypeId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
        public string SecurityReleaseReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/SecurityReleaseReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&referenceNumber=" + dateRange.referenceNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string PSR(int psrReportTypeId, int projectSiteReportId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/PSR.aspx?psrReportTypeId=" + psrReportTypeId + "&projectSiteReportId="+ projectSiteReportId;
            return path;
        }

       public string RiskAssets(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
       {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetsReport.aspx?runDate="+runDate+"&level="+level+"&MisCode="+misCode+"&exposureType="+exposureType+"&divisionName="+divisionName+"&branchName="+branchName+"&regionName="+regionName+"&key1="+dateInfor+"&key2="+hashValue.hashedDateValue+"&groupName="+groupName;
            return path;
       }

        public string GetInterestIncome(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/InterestIncome.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetFixedDepositCollaterals(int companyId, string customerCode, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/FixedDepositCollateral.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&customerCode=" + customerCode.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetValidCollaterals(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ValidCollaterals.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string CbnTeam(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CbnNplTeamReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetByCbnNplClassification(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetByCbnClassification.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string ContigentLiabilityReportMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ContigentLiabilityReportMain.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }


        public string ContigentLiabilityReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ContigentLiabilityReportMain.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string ContigentLiabilityReportMain1(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ContigentLiabilityReportMain1.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string CopyOfRiskAssetMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CopyOfRiskAssetMain.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetCalcCombinedReportTeam(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetCalcCombinedReportTeam.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetsContigentReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetContigentReportMain.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetCalcCombinedReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetCalcCombinedReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string ContigentReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ContigentReport.aspx?runDate="+runDate+"&level="+level+"&MisCode="+misCode+"&exposureType="+exposureType+"&divisionName="+divisionName+"&branchName="+branchName+"&regionName="+regionName+"&key1="+dateInfor+"&key2="+hashValue.hashedDateValue+"&groupName="+groupName;
            return path;
        }

        public string Overline(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/Overline.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string ExpiredFacilityReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ExpiredFacilityReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string OverLineReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/OverLineReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }
        public string LargeExposureReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LargeExposureReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string ExtensionReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ExtensionReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string MaturityReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/MaturityReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string IfrsClassificationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/IfrsClassificationReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetByIFRSClassificationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetByIFRSClassificationReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }
        public string RiskAssetByVarianceReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetByVarianceReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetCombinedReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetCombinedReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetDistributionBySectorReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetDistributionBySectorReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetMainReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetMainReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string CopyOfRiskAssetByIfrsClassification(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CopyOfRiskAssetByIfrsClassification.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        


        public string RiskAssetMain1Report(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetMain1Report.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetTeamReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetTeamReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string UnpaidObligationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/UnpaidObligationReport.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string RiskAssetContigentReportMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RiskAssetContigentReportMain.aspx?runDate=" + runDate + "&level=" + level + "&MisCode=" + misCode + "&exposureType=" + exposureType + "&divisionName=" + divisionName + "&branchName=" + branchName + "&regionName=" + regionName + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&groupName=" + groupName;
            return path;
        }

        public string DrawdownReport(string referenceNumber)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);
            string path = string.Empty;
            path = reportPath + "ReportViews/DrawdownMemo.aspx?loanRefNo=" + referenceNumber + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string DeferralWaiverReport(int staffId, int operationId, int targetId, int loanApplicationDetailId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);
            string path = string.Empty;
            path = reportPath + "ReportViews/DeferralWaiver.aspx?staffId=" + staffId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue + "&operationId=" + operationId + "&targetId=" + targetId + "&loanApplicationDetailId=" + loanApplicationDetailId;
            return path;
        }

        public string GetTrialBalanceReport(int glAccountId, int currencyCode, int companyId, int staffId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/TrialBalance.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId.ToString() + "&glAccountId=" + glAccountId.ToString() + "&currencyCode=" + currencyCode.ToString() + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        // remedial assets report

        public string GetOutOfCourtSettlement(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/OutOfCourtSettlement.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetCollateralSales(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralSales.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetRecoveryAgentUpdate(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RecoveryAgentUpdate.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetRecoveryCommission(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RecoveryCommission.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetRecoveryAgentPerformance(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RecoveryAgentPerformance.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetLitigationRecoveries(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LitigationRecoveries.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetRevalidationOfFullAndFinalSettlement(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RevalidationOfFullAndFinalSettlement.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetIdleAssetsSales(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/IdleAssetsSales.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetFullAndFinalSettlementAndWaivers(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/FullAndFinalSettlementAndWaivers.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetCreditBureauReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/creditBureau.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&searchInfo=" + dateRange.searchInfo + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string CorporateLoansReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;

            path = reportPath + "ReportViews/CorporateLoansReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&loanRefNo=" + dateRange.loanRefNo + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;


            return path;
        }

        public string GetCollateralPerfection(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralPerfection.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&status=" + dateRange.approvalStatus + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetDisbursedFacilityCollateralReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/DisbursedFacilityCollateralReport.aspx?classification=" + dateRange.classification + "&productId=" + dateRange.productId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            //path = reportPath + "ReportViews/DisbursedFacilityCollateralReport.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&status=" + dateRange.approvalStatus + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }
       /* public string GetLoanClassificationReport(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/LoanClassification.aspx?classification=" + dateRange.classification + "&companyId=" + dateRange.companyId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }*/
        public string GetCollateralRegister(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralRegister.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }

        public string GetCollateralAdequacy(DateRange dateRange)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralAdequacy.aspx?startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&companyId=" + dateRange.companyId + "&branchId=" + dateRange.branchId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;
            return path;
        }


        public string GetRecoveryDelinquentAccountsReport(DateTime startDate, DateTime endDate, int dpd, decimal amount)
        {
            try
            {
                HashProperty hashValue = GetHashedDateValue(dateInfor);

                string path = string.Empty;
                path = reportPath + "ReportViews/DelinquentAccounts.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue + "&dpd=" + dpd + "&amount=" + amount;
                return path;
            }
            catch( Exception en)
            {
                throw en;
            }
        }

        public string GetPaydayLoanRecoveryCollectionReport(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/PaydayLoanCollection.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetComputationForExternalAgentsReport(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ComputationForExternalAgents.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetRecoveryCollectionReport(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/RecoveryCollectionReport.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }
        
        public string GetComputationForInternalAgentsReport(DateTime startDate, DateTime endDate)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/ComputationForInternalAgents.aspx?startDate=" + startDate + "&endDate=" + endDate + "&hashValue=" + hashValue;
            return path;
        }

        public string GetOutstandingDocumentDeferredList()
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/OutstandingDocumentDeferredList.aspx?hashValue=" + hashValue;
            return path;
        }

        public string GetAvailmentUtilizationTicketReport(int customerId)
        {
            HashProperty hashValue = GetHashedDateValue(dateInfor);

            string path = string.Empty;
            path = reportPath + "ReportViews/AvailmentUtilizationTicket.aspx?customerId=" + customerId + "&key1=" + dateInfor + "&key2=" + hashValue.hashedDateValue;

            return path;
        }


    }

}

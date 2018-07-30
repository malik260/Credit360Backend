using FintrakBanking.Common;
using FintrakBanking.Common.Crypto;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.ReportObjects.ReportCalls
{
    public class ReportRoutes : IReportRoutes
    {
        string reportPath = CommonHelpers.ReportPath;
        Protection crypto = new Protection();
       string cryptoKey = "sqluser10$";
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
        public List<TransactionViewModel> GLAccount(string searchValue)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var gl = from x in context.TBL_CHART_OF_ACCOUNT
                                where x.ACCOUNTNAME.ToUpper().Contains(searchValue.ToUpper()) //|| x.ACCOUNTCODE.ToUpper().Contains(searchValue.ToUpper())
                         select new TransactionViewModel
                         {
                             GLAccountCode=x.ACCOUNTCODE,
                             GLAccount   = x.ACCOUNTNAME,
                             GLAccountId = x.GLACCOUNTID
                         };
                return gl.ToList();
            }
        }
        public string GetWorkflowSLA(int loanApplicationId, int companyId, int staffId)
        {
            string path = string.Empty;
            int operationId = (int)OperationsEnum.CAM;
            path = reportPath + "ReportViews/ApprovalTrailWith_SLA.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId +  "&operationId=" + operationId + "&loanApplicationId=" + loanApplicationId.ToString();
            return path;
        }
        public string GetWorkflowSLAMonitoring(int companyId, DateRange dateRange)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/SLAReport.aspx?companyId=" + companyId.ToString() + "&approvalStatus=" + dateRange.approvalStatus + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&operationId=" + dateRange.operationId;
            return path;
        }

        public string GetLoanScheduleReport(int tearmLoanId, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanRepaymentSchedule.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId.ToString() + " & tearmLoanId=" + tearmLoanId.ToString();
            return path;
        }

        public string GetSectorLimitMonitoringReport(int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/SectorialLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId.ToString();
            return path;
        }

        public string GetBranchLoanAmountLimit(int branchId, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/BranchLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId.ToString() + "&staffId=" + staffId.ToString();
            return path;
        }
        public string GetWorkflowDefinition(int operationId, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/Workflow.aspx?companyId=" + companyId.ToString() + "&operationId=" + operationId.ToString() + "&staffId=" + staffId.ToString();
            return path;
        }
        public string GetDisburstLoans(DateRange dateRange, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/DisbursedLoans.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&loanRefNo="+ dateRange.loanRefNo + "&branchId="+ dateRange.branchId + "&productClassId="+ dateRange.productClassId + "&staffId=" + staffId.ToString();
            return path;
        }

        public string GetLoanStatement(int companyId, int loanId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanStatement.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString() + "&staffId=" + staffId.ToString();
            return path;
        }

        public string GetLoanAnniversery(DateRange dateRange, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanAnniversery.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId  + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
            return path;
        }
        public string GetLoanDocumentWaived(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentWaived.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&branchId="+ dateRange.branchId;
            return path;
        }
        public string GetLoanDocumentDeferrals(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferral.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&branchId=" + dateRange.branchId;
            return path;
        }
        public string GetLoanDocumentDeferralsMCC(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferalsForMCC.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&branchCode=" + dateRange.branchId;
            return path;
        }
        public string GetCollateralEstimated(int companyId, string collateralCode, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralEstimated.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&collateralCode=" + collateralCode.ToString();
            return path;
        }

        public string GetFCYScheuledLoan(int companyId, int loanId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/FCYScheuledLoan.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString() + "&staffId=" + staffId;
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

        public string GetGeneratedOfferLetterLMS(string applicationRefNumber)
        {
            return reportPath + "Credit/OfferLetterGeneration/OfferLetterLMSR.aspx?applicationRefNumber=" + applicationRefNumber;
        }

        public string GetProductSpecificTemplate(short? productClassProcessId, short? productClassId, string applicationRefNumber)
        {
          

            var links = new
            {
                General = reportPath + "Credit/OfferLetterGeneration/OfferLetterCAMbase.aspx?applicationRefNumber=" + applicationRefNumber,
                IDF = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                FirstEdu = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                FirstTrader = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                BondsAndGuarantees = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                ImportFinance = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                CashBackedOnly = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
                InvoiceDiscountingFacility = reportPath + "Credit/OfferLetterGeneration/OfferLetter.aspx?applicationRefNumber=" + applicationRefNumber,
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

                    case (short)ProductClassEnum.CashBackedOnly:
                        return links.CashBackedOnly;

                    case (short)ProductClassEnum.FirstEdu:
                        return links.FirstEdu;

                    case (short)ProductClassEnum.FirstTrader:
                        return links.FirstTrader;

                    case (short)ProductClassEnum.ImportFinance:
                        return links.ImportFinance;

                    case (short)ProductClassEnum.InvoiceDiscountingFacility:
                        return links.InvoiceDiscountingFacility;

                    default:
                        return links.General;
                }

               
            }

            //templateLink = links.General;

            //return templateLink;
        }

        #endregion Offer Letter Generation

        #region Loan Monitoring Reports

        public string GetCovenantsApproachingDueDateReport(int companyId, int staffId, DateRange dateRange)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "ReportViews/CovenantsApproachingDueDate.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralPropertyRevaluation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralVisitation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/ExpiredSelfLiquidatingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/NonPeformingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&classification=" + dateRange.classification + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/Overdraft.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/BondAndGuarantee.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&approvalStatus="+dateRange.approvalStatus;
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
                string path = string.Empty;
                path = reportPath + "ReportViews/CollateralInsurance.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
                string path = string.Empty;
                path = reportPath + "ReportViews/TurnOverConvenant.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy");
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
            string path = string.Empty;
            path = reportPath + "ReportViews/GrantedFacilities.aspx?companyId=" + companyId.ToString() + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + 
                "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&glAccountId=" + searchEntity.glAccountId + "&PostedByStaffId=" + searchEntity.PostedByStaffId + "&branchId=" + searchEntity.branchId;
            return path;
        }

        public string AccountWithLein(ReportSearchEntity searchEntity)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/Lein.aspx?companyId=" + searchEntity.companyId.ToString() + "&searchParamemter=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy");
            return path;
        }

        public string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/GetStakeholdersOnExpirationOfFTP.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate="+ searchEntity.startDate.ToString("dd-MM-yyyy");
            return path;
        }

        public string GetFacilityApprovedNotUtilized(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/FacilityApprovedNotUntilized.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy");
            return path;
        }
        public string GetRuningLoansByLoanType(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/RuningLoansByLoanType.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&productClassId=" + searchEntity.productClassId;
            return path;
        }

        public string GetAuditTrail(DateRange dateRange, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/AuditTrailView.aspx?username=" + dateRange.username + "&startDate=" + dateRange.startDate.ToString("dd-MM-yyyy") + "&endDate=" + dateRange.endDate.ToString("dd-MM-yyyy") + "&staffId=" + staffId + "&auditTypeId="+dateRange.auditTypeId;
            return path;
        }
        public string GetLoanInterestReceivableAndPayable(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanInterestReceivableAndPayable.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&productClassId=" + searchEntity.productClassId;
            return path;
        }

        public string GetBlacklist(ReportSearchEntity searchEntity)
        {

            string path = string.Empty;
            path = reportPath + "ReportViews/Blacklist.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&customerCode=" + searchEntity.customerCode;
            return path;
        }
        public string GetDailyAccrual(ReportSearchEntity searchEntity)
        {

            string path = string.Empty;
            path = reportPath + "ReportViews/DailyAccrualReport.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&categoryId=" + searchEntity.categoryId + "&transactionTypeId=" + searchEntity.transactionTypeId + "&companyId=" + searchEntity.companyId;
            return path;
        }
        public string GetRepayment(ReportSearchEntity searchEntity)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/Repayment.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&operationId=" + searchEntity.operationId + "&companyId=" + searchEntity.companyId;
            return path;
        }
        public string GetCustomeFacilityRepayment(ReportSearchEntity searchEntity)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/CustomeFacilityRepayment.aspx?startDate=" + searchEntity.startDate.ToString("dd-MM-yyyy") + "&endDate=" + searchEntity.endDate.ToString("dd-MM-yyyy") + "&valueCode=" + searchEntity.valueCode + "&companyId=" + searchEntity.companyId; 
            return path;
        }
    }

}

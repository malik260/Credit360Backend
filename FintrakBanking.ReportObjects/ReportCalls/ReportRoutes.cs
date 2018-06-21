using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Linq;
namespace FintrakBanking.ReportObjects.ReportCalls
{
    public class ReportRoutes : IReportRoutes
    {
        string reportPath = CommonHelpers.ReportPath;
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
            path = reportPath + "ReportViews/SLAReport.aspx?companyId=" + companyId.ToString() + "&approvalStatus=" + dateRange.approvalStatus + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate + "&operationId=" + dateRange.operationId;
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
            path = reportPath + "ReportViews/DisbursedLoans.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&loanRefNo="+ dateRange.loanRefNo + "&branchId="+ dateRange.branchId + "&productClassId="+ dateRange.productClassId + "&staffId=" + staffId.ToString();
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
            path = reportPath + "ReportViews/LoanAnniversery.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId  + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString();
            return path;
        }
        public string GetLoanDocumentWaived(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentWaived.aspx?companyId=" + "&staffId=" + staffId  + companyId+ "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&branchId="+ dateRange.branchId;
            return path;
        }
        public string GetLoanDocumentDeferrals(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferral.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&branchId=" + dateRange.branchId;
            return path;
        }
        public string GetLoanDocumentDeferralsMCC(int companyId, DateRange dateRange, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferalsForMCC.aspx?companyId=" + companyId + "&staffId=" + staffId + "&startDate=" + dateRange.startDate.ToShortDateString() + "&branchCode=" + dateRange.branchCode;
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
                    var productClassId = context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).PRODUCTCLASSID;
                    var productClassProcessId = context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.APPLICATIONREFERENCENUMBER == applicationRefNumber).PRODUCT_CLASS_PROCESSID;
                    return GetProductSpecificTemplate(productClassProcessId, productClassId, applicationRefNumber);
                }
              
              
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetGeneratedOfferLetterLMS(string refNumber)
        {
            try
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var loanApplId = 0;
                    var facility = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == refNumber);
                    if (facility == null)
                    {
                        var facility1 = context.TBL_LOAN_REVOLVING.Where(x => x.LOANREFERENCENUMBER == refNumber);
                        loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility1.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;
                    }

                    loanApplId = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == facility.FirstOrDefault().LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;
                    var targetAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == loanApplId);

                    var productClassId = context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).PRODUCTCLASSID;
                    var productClassProcessId = context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER).PRODUCT_CLASS_PROCESSID;
                    return GetProductSpecificTemplate(productClassProcessId, productClassId, targetAppl.APPLICATIONREFERENCENUMBER);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
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
                path = reportPath + "ReportViews/CovenantsApproachingDueDate.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/CollateralPropertyRevaluation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/CollateralVisitation.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/ExpiredSelfLiquidatingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/NonPeformingLoans.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&classification=" + dateRange.classification + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/Overdraft.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/BondAndGuarantee.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/CollateralInsurance.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
                path = reportPath + "ReportViews/TurnOverConvenant.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate;
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
            path = reportPath + "ReportViews/GrantedFacilities.aspx?companyId=" + companyId.ToString() + "&startDate=" + searchEntity.startDate.ToShortDateString() + 
                "&endDate=" + searchEntity.endDate.ToShortDateString() + "&staffId=" + searchEntity.staffId + "&excludeSystem=" + searchEntity.excludeSystem + "&branchId=" + searchEntity.branchId;
            return path;
        }

        public string GetAccountWithLein(int staffId, short? branchId, string customerName, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanCASAaccountWithLein.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId + "&customerName=" + customerName
                + "&staffId=" + staffId;
            return path;
        }

        public string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/GetStakeholdersOnExpirationOfFTP.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate="+ searchEntity.startDate;
            return path;
        }

        public string GetFacilityApprovedNotUtilized(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/FacilityApprovedNotUntilized.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate=" + searchEntity.startDate + "&endDate=" + searchEntity.endDate;
            return path;
        }
        public string GetRuningLoansByLoanType(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/RuningLoansByLoanType.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate + "&endDate=" + searchEntity.endDate + "&productClassId=" + searchEntity.productClassId;
            return path;
        }

        public string GetAuditTrail(DateRange dateRange, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/AuditTrailView.aspx?username=" + dateRange.username + "&startDate=" + dateRange.startDate + "&endDate=" + dateRange.endDate + "&staffId=" + staffId;
            return path;
        }
        public string GetLoanInterestReceivableAndPayable(ReportSearchEntity searchEntity, int companyId, int staffId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanInterestReceivableAndPayable.aspx?companyId=" + companyId.ToString() + "&staffId=" + staffId + "&branchId=" + searchEntity.branchId + "&loanRefNo=" + searchEntity.searchParamemter + "&startDate=" + searchEntity.startDate + "&endDate=" + searchEntity.endDate + "&productClassId=" + searchEntity.productClassId;
            return path;
        }

        public string GetBlacklist(ReportSearchEntity searchEntity)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/Blacklist.aspx?startDate=" + searchEntity.startDate + "&endDate=" + searchEntity.endDate + "&customerCode=" + searchEntity.customerCode;
            return path;
        }
    }

}

using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.ReportObjects.Credit
{
    public class LoanMonitoring
    {
        public static IEnumerable<CollateralViewModel> CollateralPropertyRevaluation()
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            GeneralSetupRepository genSetup = new GeneralSetupRepository(context);
            var data = (from a in context.tbl_Collateral_Customer
                        join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                        join c in context.tbl_Staff on a.CreatedBy equals c.StaffId
                        join d in context.tbl_Collateral_Type on a.CollateralTypeId equals d.CollateralTypeId
                        join e in context.tbl_Collateral_Type_Sub on a.CollateralSubTypeId equals e.CollateralSubTypeId
                        join f in context.tbl_Collateral_Immovable_Property on a.CollateralCustomerId equals f
                            .CollateralCustomerId
                        where (DbFunctions.DiffDays(DbFunctions.AddDays(f.LastValuationDate, a.ValuationCycle), genSetup.GetApplicationDate()) <= 30)
                        select new CollateralViewModel
                        {
                            collateralTypeId = a.CollateralTypeId,
                            collateralType = d.CollateralTypeName,
                            collateralCode = a.CollateralCode,
                            collateralSubType = e.CollateralSubTypeName,
                            customerName = b.FirstName + " " + b.LastName,
                            propertyName = f.PropertyName,
                            lastValuationDate = f.LastValuationDate,
                            relationshipManagerId = a.CreatedBy,
                            relationshipManager = c.FirstName + " " + c.LastName,
                            relationshipManagerEmail = c.Email
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<CollateralViewModel>();
        }

        public static IEnumerable<LoanCovenantDetailViewModel> CovenantsApproachingDueDate()
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            GeneralSetupRepository genSetup = new GeneralSetupRepository(context);
            var data = (from a in context.tbl_Loan_Covenant_Detail
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Staff on b.RelationshipManagerId equals c.StaffId
                        join d in context.tbl_Staff on b.RelationshipOfficerId equals d.StaffId
                        join e in context.tbl_Loan_Application_Detail on b.LoanApplicationDetailId equals e.LoanApplicationDetailId
                        join f in context.tbl_Frequency_Type on a.FrequencyTypeId equals f.FrequencyTypeId
                        join g in context.tbl_Loan_Covenant_Type on a.CovenantTypeId equals g.CovenantTypeId
                        where DbFunctions.DiffDays(genSetup.GetApplicationDate(), a.NextCovenantDate) <= 10
                        select new LoanCovenantDetailViewModel
                        {
                            companyId = a.CompanyId,
                            covenantAmount = a.CovenantAmount,
                            covenantDate = a.CovenantDate,
                            dueDate = a.NextCovenantDate,
                            covenantDetail = a.CovenantDetail,
                            covenantTypeId = a.CovenantTypeId,
                            covenantTypeName = g.CovenantTypeName,
                            frequencyTypeId = a.FrequencyTypeId,
                            frequencyTypeName = f.Mode,
                            loanId = a.LoanId,
                            loanRefNumber = e.tbl_Loan_Application.ApplicationReferenceNumber,
                            relationshipManager = c.FirstName + " " + c.LastName,
                            managerEmail = c.Email,
                            relationshipOfficer = d.FirstName + " " + d.LastName,
                            officerEmail = d.Email,
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanCovenantDetailViewModel>();
        }

        public static IEnumerable<LoanViewModel> NplLoanMonitoring()
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var data = (from a in context.tbl_Loan_Application
                        join d in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals d.LoanApplicationId
                        join b in context.tbl_Loan on d.LoanApplicationDetailId equals b.LoanApplicationDetailId
                        join c in context.tbl_Loan_Revolving on d.LoanApplicationDetailId equals c.LoanApplicationDetailId
                        where b.InternalPrudentialGuidelineStatusId != (int)LoanPrudentialStatusEnum.Performing
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            loanReferenceNumber = b.LoanReferenceNumber,
                            bookingDate = b.BookingDate,
                            disburseDate = b.DisburseDate,
                            maturityDate = b.MaturityDate,
                            productName = b.tbl_Product.ProductName,
                            nplDate = b.NPLDate.Value,
                            outstandingInterest = b.OutstandingInterest,
                            outstandingPrincipal = b.OutstandingPrincipal,
                            productTypeName = b.tbl_Product.tbl_Product_Type.ProductTypeName,
                            loanTypeName = b.tbl_Loan_Type.LoanTypeName,
                            relationshipManagerId = b.RelationshipOfficerId,
                            relationshipManagerName = b.tbl_Staff.FirstName + " " + b.tbl_Staff.LastName,
                            relationshipManagerEmail = b.tbl_Staff.Email,
                            relationshipOfficerId = b.RelationshipOfficerId,
                            relationshipOfficerName = b.tbl_Staff1.FirstName + " " + b.tbl_Staff1.LastName,
                            relationshipOfficerEmail = b.tbl_Staff1.Email
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanViewModel>();
        }

        public IEnumerable<LoanViewModel> SelfLiquidatingLoanExpiry()
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            GeneralSetupRepository genSetup = new GeneralSetupRepository(context);

            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Product on a.ProductId equals b.ProductId
                        join c in context.tbl_Product_Type on b.ProductTypeId equals c.ProductTypeId
                        join d in context.tbl_Loan_Application_Detail on a.LoanApplicationDetailId equals d.LoanApplicationDetailId
                        where a.tbl_Product.ProductTypeId == (int)LoanProductTypeEnum.SelfLiquidating &&
                        DbFunctions.DiffDays(a.MaturityDate, genSetup.GetApplicationDate()) <= 30
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.tbl_Loan_Application.ApplicationReferenceNumber,
                            loanReferenceNumber = a.LoanReferenceNumber,
                            bookingDate = a.BookingDate,
                            disburseDate = a.DisburseDate,
                            maturityDate = a.MaturityDate,
                            productName = b.ProductName,
                            nplDate = a.NPLDate,
                            outstandingInterest = a.OutstandingInterest,
                            outstandingPrincipal = a.OutstandingPrincipal,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            productTypeName = b.tbl_Product_Type.ProductTypeName,
                            relationshipManagerId = a.RelationshipOfficerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                            relationshipManagerEmail = a.tbl_Staff.Email,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff1.FirstName + " " + a.tbl_Staff1.LastName,
                            relationshipOfficerEmail = a.tbl_Staff1.Email
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanViewModel>();
        }
    }
}
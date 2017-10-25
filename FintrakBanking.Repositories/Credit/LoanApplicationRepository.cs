using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {

        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        //private IWorkFlowRepository workFlow;
        private ICasaRepository casa;

        private ICustomerCollateralRepository collateral;
        //private IFinanceTransactionRepository finance;

        public LoanApplicationRepository(IAuditTrailRepository _auditTrail,
            ICasaRepository _casa,
            ICustomerCollateralRepository _collateral,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context)
        {
            this.collateral = _collateral;
            //this.finance = _finance;
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.casa = _casa;
            this.collateral = _collateral;

        }

        // public 

        public IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId)
        {
            var data = context.tbl_Loan_Application.Where(c => c.CustomerId == customerId && c.CompanyId == companyId)
                .Select(c => new ExistingLoanApplicationViewModel()
                {
                    applicationDate = c.ApplicationDate,
                    applicationReferenceNumber = c.ApplicationReferenceNumber,
                    interestRate = c.InterestRate,
                    loanTypeName = c.tbl_Loan_Type.LoanTypeName,
                    branch = c.tbl_Branch.BranchName,
                    principalAmount = c.ApplicationAmount,
                    tenor = c.ApplicationTenor
                }).ToList();
            return data;
        }

        private IQueryable<LoanApplicationViewModel> GetLoanApplications(int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        where a.CompanyId == companyId && a.Deleted == false
                        select new LoanApplicationViewModel
                        {
                            approvalStatusId = a.ApprovalStatusId,
                            loanApplicationId = a.LoanApplicationId,
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            customerId = a.CustomerId ?? 0,
                            customerName = a.CustomerId.HasValue ? a.tbl_Customer.FirstName + " " + a.tbl_Customer.MiddleName + " " + a.tbl_Customer.LastName : "",
                            loanInformation = a.LoanInformation,
                            companyId = a.CompanyId,
                            branchId = (short)a.BranchId,
                            branchName = a.tbl_Branch.BranchName,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = a.RelationshipManagerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            misCode = a.MISCode,
                            teamMisCode = a.TeamMISCode,
                            interestRate = a.InterestRate,
                            isRelatedParty = a.IsRelatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            submittedForAppraisal = a.SubmittedForAppraisal,
                            customerGroupId = a.CustomerGroupId ?? 0,
                            customerGroupName = a.CustomerGroupId.HasValue ? a.tbl_Customer_Group.GroupName : "",
                            loanTypeId = a.LoanTypeId,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            createdBy = a.CreatedBy,
                            applicationDate = a.ApplicationDate,
                            dateTimeCreated = a.DateTimeCreated,
                            LoanApplicationCollateral = context.tbl_Loan_Application_Collateral.Where(d => d.LoanApplicationId == a.LoanApplicationId)
                             .Select(d => new LoanApplicationCollateralViewModel()
                             {
                                 //     collateralValue   = d.CollateralReferenceNumber,
                                 cityId = d.CityId,
                                 collateralTypeId = d.CollateralTypeId,
                                 customerCollateralId = d.CustomerCollateralId,
                                 documentTitle = d.DocumentTitle,
                                 latitude = d.Latitude,
                                 loanApplicationId = d.LoanApplicationId,
                                 locationAddress = d.LocationAddress,
                                 longitude = d.Longitude,
                                 nearestBusStop = d.NearestBusStop,
                                 nearestLandmark = d.NearestLandmark,
                                 otherInformations = d.OtherInformations,
                                 city = d.tbl_City.CityName,
                                 collateralType = d.tbl_Collateral_Type.CollateralTypeName,
                                 companyName = d.tbl_Loan_Application.tbl_Company.Name,
                                 // applicationReferanceNumber = int.Parse(d.tbl_Loan_Application.ApplicationReferenceNumber),

                             }).ToList(),
                            LoanApplicationDetail = context.tbl_Loan_Application_Detail.Where(c => c.LoanApplicationId == a.LoanApplicationId)
                             .Select(c => new LoanApplicationDetailViewModel()
                             {
                                 approvedAmount = c.ApprovedAmount,
                                 approvedInterestRate = c.ApprovedInterestRate,
                                 approvedProductId = c.ApprovedProductId,
                                 approvedTenor = c.ApprovedTenor,
                                 currencyId = c.CurrencyId,
                                 currencyName = c.tbl_Currency.CurrencyName,
                                 customerId = c.CustomerId,
                                 exchangeRate = c.ExchangeRate,
                                 loanApplicationDetailId = c.LoanApplicationDetailId,
                                 subSectorId = c.SubSectorId,
                                 loanApplicationId = c.LoanApplicationId,
                                 proposedAmount = c.ProposedAmount,
                                 proposedInterestRate = c.ProposedInterestRate,
                                 proposedProductId = c.ProposedProductId,
                                 proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.ProposedTenor) * Convert.ToDecimal(12 / 365))),
                                 statusId = c.StatusId
                             }).ToList()
                        });
            return data;
        }

        public IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId)
        {
            return GetLoanApplications(companyId).ToList();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationJobs(int companyId, int levelId, int scope)
        {
            var applications = GetLoanApplications(companyId)
                .Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending); // scope 3 entire process

            if (scope == (int)ProcessViewScopeEnum.Group)
            {
                int levelGroupId;
                var level = context.tbl_Approval_Level.Find(levelId);

                if (level != null)
                {
                    levelGroupId = (int)level.GroupId;

                    var groupApprovalLevelIds = context.tbl_Approval_Level
                        .Where(x => x.GroupId == levelGroupId)
                        .Select(x => x.ApprovalLevelId);

                    applications = applications.Where(x => groupApprovalLevelIds.Contains(x.approvalLevelId));
                }
            }

            if (scope == (int)ProcessViewScopeEnum.Level)
            {
                applications = applications.Where(x => x.approvalLevelId == levelId);
            }

            return applications
                .OrderByDescending(x => x.applicationDate)
                .ThenByDescending(x => x.loanApplicationId)
                .ToList();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId)
        {
            return GetLoanApplications(companyId).Where(c => c.loanApplicationId == loanApplicationId).ToList();
        }

        public IEnumerable<ProductClassViewModel> GetProductClass()
        {
            return (from data in context.tbl_Product_Class
                    select new ProductClassViewModel()
                    {
                        productClassId = data.ProductClassId,
                        productClassName = data.ProductClassName,
                        productClassTypeId = data.ProductClassTypeId
                    });
        }

        public IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        where a.CompanyId == companyId && a.Deleted == false
                        //&& (a.ApplicationReferenceNumber == referenceNumberOrName || $"{a.tbl_Customer.FirstName} {a.tbl_Customer.MiddleName} {a.tbl_Customer.LastName} {a.tbl_Customer.CustomerCode} ".Contains(referenceNumberOrName))
                        select new LoanApplicationViewModel
                        {
                            loanApplicationId = a.LoanApplicationId,
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            customerId = a.CustomerId.Value,
                            loanInformation = a.LoanInformation,
                            companyId = a.CompanyId,
                            branchId = (short)a.BranchId,
                            //tenor = a.ApplicationTenor,
                            // tenorModeId = a.TenorModeId,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipManagerId = a.RelationshipManagerId,

                            misCode = a.MISCode,
                            //productId = (short)a.ProductId,
                            teamMisCode = a.TeamMISCode,

                            interestRate = a.InterestRate,
                            isRelatedParty = a.IsRelatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            //principalAmount = a.ApplicationAmount,
                            customerGroupId = a.CustomerGroupId.Value,
                            loanTypeId = a.LoanTypeId,
                            //loanStatusId = a.LoanStatusId,
                            createdBy = a.CreatedBy,
                            applicationDate = a.ApplicationDate,
                            dateTimeCreated = a.DateTimeCreated
                        }).ToList();
            return data;
        }

        private string GetLoanStatus(short loanStatusId)
        {
            return this.context.tbl_Loan_Status.Where(x => x.LoanStatusId == loanStatusId).SingleOrDefault()
                .AccountStatus;
        }

        public async Task<bool> UpdateApprovalStatus(ApprovalViewModel entity)
        {
            var data = this.context.tbl_Loan_Application.Find(entity.targetId);
            {
                //data.LoanStatusId = (short)entity.approvalStatusId;
                //data.ActedOnaBy = entity.staffId;
                //data.DateActedOn = genSetup.GetApplicaionDate();
                data.LoanApplicationId = (short)entity.approvalStatusId;
            }

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalStatusUpdated,
                StaffId = entity.staffId,
                BranchId = (short)entity.BranchId,
                Detail =
                    $"Change Loan Application Status with reference number '{data.ApplicationReferenceNumber}' to {GetLoanStatus((short)entity.approvalStatusId)}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = entity.targetId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        public bool AddLoanApplication(LoanApplicationViewModel loan)
        {
            try
            {
                bool isGroupLoan = false;
                int response = 0;
                if (loan.loanTypeId > (int)LoanTypeEnum.CustomerGroup)
                {
                    isGroupLoan = true;
                }
                int casaAccountId = -1;
                //     string refNumber = GenerateLoanReference(loan.customerId.Value);
                if (loan.customerAccount != "N/A")
                {
                    casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);
                }


                //var loanStatusId = (short)LoanStatusEnum.Inactive;

                var data = new tbl_Loan_Application
                {
                    ApplicationReferenceNumber = loan.applicationReferenceNumber,
                    LoanTypeId = loan.loanTypeId,
                    CompanyId = loan.companyId,
                    BranchId = (short)loan.branchId,
                    RelationshipOfficerId = loan.relationshipOfficerId,
                    RelationshipManagerId = loan.relationshipManagerId,
                    MISCode = loan.misCode,
                    TeamMISCode = loan.teamMisCode,
                    InterestRate = loan.interestRate,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    LoanInformation = loan.loanInformation,
                    IsRelatedParty = loan.isRelatedParty,
                    IsPoliticallyExposed = loan.isPoliticallyExposed,
                    CreatedBy = (int)loan.createdBy,
                    DateTimeCreated = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now,
                    CustomerGroupId = loan.customerGroupId,
                    ApplicationStatusId = (short)LoanApplicationStatusEnum.ApplicationCompleted,
                    ApplicationAmount = loan.proposedAmount,
                    ApplicationTenor = Convert.ToInt32(Math.Round(((decimal)(loan.proposedTenor / 12) * (decimal)365))),
                    IsInvestmentGrade = loan.isInvestmentGrade,
                    LoanPreliminaryEvaluationId = loan.loanPreliminaryEvaluationId,
                    CustomerId = loan.customerId,
                    SubmittedForAppraisal = loan.submittedForAppraisal,
                    OperationId = (int)OperationsEnum.CAM
                };

                if (loan.LoanApplicationCollateral.Count > 0)
                {
                    LoanApplicationCollateral(loan.LoanApplicationCollateral);
                }
                if (loan.LoanApplicationDetail.Count > 0)
                {
                    LoanApplicationDetail(loan.LoanApplicationDetail);
                }

                if (isGroupLoan)
                {
                    data.CustomerGroupId = loan.customerGroupId;
                    data.CustomerId = null;
                }
                else
                {
                    data.CustomerId = loan.customerId;
                    data.CustomerGroupId = null;
                }

                context.tbl_Loan_Application.Add(data);

                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LoanApplication,
                    StaffId = loan.createdBy,
                    BranchId = (short)loan.userBranchId,
                    Detail = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                    IPAddress = loan.userIPAddress,
                    Url = loan.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now,
                    TargetId = loan.loanApplicationId
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                response = context.SaveChanges();

                return response > 0;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void LoanApplicationDetail(List<LoanApplicationDetailViewModel> entity)
        {
            var ApplicationDetail = entity.Select(a => new tbl_Loan_Application_Detail()
            {
                ApprovedAmount = a.proposedAmount,
                ApprovedInterestRate = a.proposedInterestRate,
                ApprovedProductId = a.proposedProductId,
                ApprovedTenor = Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                ExchangeRate = a.exchangeRate,
                CurrencyId = a.currencyId,
                CustomerId = a.customerId,
                LoanApplicationId = a.loanApplicationId,
                StatusId = (short)LoanApplicationDetailsStatusEnum.Pending,


                ProposedAmount = a.proposedAmount,
                ProposedInterestRate = a.proposedInterestRate,
                ProposedProductId = a.proposedProductId,
                ProposedTenor = Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                SubSectorId = a.subSectorId,
                CreatedBy = a.createdBy,
                DateTimeCreated = DateTime.Now,

            });
            context.tbl_Loan_Application_Detail.AddRange(ApplicationDetail);

        }

        private void LoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity)
        {
            foreach (var item in entity)
            {
                var loanCollateral = new tbl_Loan_Application_Collateral()
                {
                    CollateralReferenceNumber = item.collateralReferenceNumber,
                    CityId = item.cityId,
                    CollateralValue = item.collateralValue,
                    IsBankAccount = item.isBankAccount,
                    CollateralTypeId = item.collateralTypeId,
                    CreatedBy = item.createdBy,
                    DateTimeCreated = genSetup.GetApplicationDate(),
                    OtherInformations = item.otherInformations,
                    Latitude = item.latitude,
                    Longitude = item.longitude,

                    LocationAddress = item.locationAddress,
                    NearestBusStop = item.nearestBusStop,
                    NearestLandmark = item.nearestLandmark,
                    DocumentTitle = item.documentTitle,
                    //LoanApplicationId = item.loanApplicationId,
                    SystemDateTime = DateTime.Now,
                    CasaAccountId = item.casaAccountId

                };
                context.tbl_Loan_Application_Collateral.Add(loanCollateral);
            }
        }

        //private void ApplicationCollateralRef(List<LoanApplicationCollateralRefNoViewModel> entity)
        //{
        //   var item = entity.Select(c => new tbl_Loan_Application_Collateral()
        //    {
        //        DocumentNumber = c.documentNumber,
        //        IsBankAccount = c.isBankAccount,
        //        Worth = c.worth,
        //        CustomerCollateralId = c.customerCollateralId
        //    });
        //    context.tbl_Loan_Application_Collateral.AddRange(item);
        //}

        private string GenerateLoanReference(int customerId)
        {
            string code = "";
            int data = 0;
            if (customerId > 2)
            {
                var grp = this.context.tbl_Customer_Group.Where(x => x.CustomerGroupId == customerId);
                if (grp.Any())
                {
                    code = grp.First().GroupCode;
                }
                data = ((this.context.tbl_Loan_Application.Count(x => x.CustomerId == customerId)) + 1);
            }
            else
            {
                var cust = context.tbl_Customer.Where(x => x.CustomerId == customerId);
                if (cust.Any())
                {
                    code = cust.First().CustomerCode;
                }
                data = ((context.tbl_Loan_Application.Count(x => x.CustomerId == customerId)) + 1);
            }

            return $"{code}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }

        public bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId)
        {
            bool isExisting = false;
            var collate = collateral.GetCustomerCollateral(companyId).Where(c => c.collateralCode == certificateOfOwnership);
            if (collate.Any())
            {
                return isExisting = true;
            }
            return isExisting;
        }

        #region PENDING APPLICATIONS

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int companyId, int branchId, int staffId)
        {
            int operationId = (int)OperationsEnum.CAM;
            int scope = this.GetStaffWorkflowViewScope(operationId, staffId);

            if (scope == (int)ProcessViewScopeEnum.Process) // 3
            {
                return context.tbl_Loan_Application.Where(x => x.CompanyId == companyId && x.Deleted == false //&& x.BranchId == branchId
                )
                    .GroupJoin(
                        context.tbl_Approval_Trail.Where(x => x.OperationId == operationId),
                        a => a.LoanApplicationId,
                        b => b.TargetId,
                        (x, y) => new { a = x, bs = y })
                    .SelectMany(
                        xy => xy.bs.DefaultIfEmpty(),
                        (x, y) => new LoanApplicationViewModel
                        {
                            loanApplicationId = x.a.LoanApplicationId,
                            applicationReferenceNumber = x.a.ApplicationReferenceNumber,
                            customerId = x.a.CustomerId,
                            branchId = x.a.BranchId,
                            //productClassId = x.a.tbl_Product.ProductClassId,
                            //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                            customerGroupId = x.a.CustomerGroupId,
                            loanTypeId = x.a.LoanTypeId,
                            relationshipOfficerId = x.a.RelationshipOfficerId,
                            relationshipManagerId = x.a.RelationshipManagerId,
                            applicationDate = x.a.ApplicationDate,
                            applicationAmount = x.a.ApplicationAmount,
                            approvedAmount = x.a.ApprovedAmount,
                            interestRate = x.a.InterestRate,
                            applicationTenor = x.a.ApplicationTenor,
                            lastComment = y.Comment,
                            currentApprovalStateId = y.ApprovalStateId,
                            currentApprovalLevelId = y.ToApprovalLevelId,
                            currentApprovalLevel = y.tbl_Approval_Level1.LevelName, // pls note! tbl_Approval_Level1<---1
                            approvalTrailId = y == null ? 0 : y.ApprovalTrailId, // for inner sequence ordering
                            loanInformation = x.a.LoanInformation,
                            submittedForAppraisal = x.a.SubmittedForAppraisal,
                            isRelatedParty = x.a.IsRelatedParty,
                            isPoliticallyExposed = x.a.IsPoliticallyExposed,
                            approvalStatusId = x.a.ApprovalStatusId,
                            applicationStatusId = x.a.ApplicationStatusId,
                            branchName = x.a.tbl_Branch.BranchName,
                            relationshipOfficerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                            relationshipManagerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                            misCode = x.a.MISCode,
                            customerGroupName = x.a.CustomerGroupId.HasValue ? x.a.tbl_Customer_Group.GroupName : "",
                            loanTypeName = x.a.tbl_Loan_Type.LoanTypeName,
                            createdBy = x.a.CreatedBy,
                            loanPreliminaryEvaluationId = x.a.LoanPreliminaryEvaluationId,
                            //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                            operationId = x.a.OperationId,
                        })
                        .GroupBy(d => d.loanApplicationId)
                        .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                        .OrderByDescending(x => x.applicationDate)
                        .ThenByDescending(x => x.loanApplicationId)
                        ;
            }

            var staffApprovalLevelIds = context.tbl_Approval_Level_Staff
                .Where(x => x.Deleted == false && x.StaffId == staffId).Select(x => x.ApprovalLevelId);

            var pendingApplications = context.tbl_Loan_Application
                .Join(context.tbl_Approval_Trail,
                    a => a.LoanApplicationId, b => b.TargetId, (a, b) => new { a, b })
                .Where(x => x.b.OperationId == operationId //&& x.a.BranchId == branchId
                );

            int count = pendingApplications.Count(); // for testing

            if (scope == (int)ProcessViewScopeEnum.Group) // 2
            {
                var groupApprovalLevelIds = context.tbl_Approval_Level_Staff
                    .Where(x => x.Deleted == false && x.StaffId == staffId)
                    .Select(x => x.tbl_Approval_Level)
                    .Select(x => x.tbl_Approval_Group)
                    .SelectMany(x => x.tbl_Approval_Group_Mapping)
                    .Where(x => x.Deleted == false && x.OperationId == operationId)
                    .Select(x => x.tbl_Approval_Group)
                    .SelectMany(x => x.tbl_Approval_Level)
                    .Select(x => x.ApprovalLevelId);

                pendingApplications = pendingApplications.Where(x => groupApprovalLevelIds.Contains((int)x.b.ToApprovalLevelId) && x.b.ResponseStaffId == null);
            }

            if (scope == (int)ProcessViewScopeEnum.Level) // 1
            {
                pendingApplications = pendingApplications.Where(x => staffApprovalLevelIds.Contains((int)x.b.ToApprovalLevelId) && x.b.ResponseStaffId == null);
            }

            return pendingApplications.Select(x => new LoanApplicationViewModel
            {
                loanApplicationId = x.a.LoanApplicationId,
                applicationReferenceNumber = x.a.ApplicationReferenceNumber,
                customerId = x.a.CustomerId,
                branchId = x.a.BranchId,
                //productClassId = x.a.tbl_Product.ProductClassId,
                //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                customerGroupId = x.a.CustomerGroupId,
                loanTypeId = x.a.LoanTypeId,
                relationshipOfficerId = x.a.RelationshipOfficerId,
                relationshipManagerId = x.a.RelationshipManagerId,
                applicationDate = x.a.ApplicationDate,
                applicationAmount = x.a.ApplicationAmount,
                approvedAmount = x.a.ApprovedAmount,
                interestRate = x.a.InterestRate,
                applicationTenor = x.a.ApplicationTenor,
                lastComment = x.b.Comment,
                currentApprovalStateId = x.b.ApprovalStateId,
                currentApprovalLevelId = x.b.ToApprovalLevelId,
                currentApprovalLevel = x.b.tbl_Approval_Level1.LevelName, // pls note! tbl_Approval_Level1<---1
                loanInformation = x.a.LoanInformation,
                submittedForAppraisal = x.a.SubmittedForAppraisal,
                isRelatedParty = x.a.IsRelatedParty,
                isPoliticallyExposed = x.a.IsPoliticallyExposed,
                approvalStatusId = x.a.ApprovalStatusId,
                applicationStatusId = x.a.ApplicationStatusId,
                branchName = x.a.tbl_Branch.BranchName,
                relationshipOfficerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                relationshipManagerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                misCode = x.a.MISCode,
                customerGroupName = x.a.CustomerGroupId.HasValue ? x.a.tbl_Customer_Group.GroupName : "",
                loanTypeName = x.a.tbl_Loan_Type.LoanTypeName,
                createdBy = x.a.CreatedBy,
                loanPreliminaryEvaluationId = x.a.LoanPreliminaryEvaluationId,
                //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                operationId = x.a.OperationId,
            })
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId)
            .Distinct();
        }

        public int GetStaffWorkflowViewScope(int operationId, int staffId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default @Level
            var staffWorkflow = context.tbl_Approval_Group_Mapping.Where(x => x.OperationId == operationId)
                .Select(g => g.tbl_Approval_Group)
                .SelectMany(g => g.tbl_Approval_Level)
                .SelectMany(l => l.tbl_Approval_Level_Staff).Where(x => x.StaffId == staffId);

            if (staffWorkflow.Count() > 0)
            {
                scope = staffWorkflow.Max(x => x.ProcessViewScopeId);
            }

            return scope;
        }

        #endregion PENDING APPLICATIONS

        #region OfferLetter & Availment Process

        private IQueryable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        join b in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals b.LoanApplicationId
                        join c in context.tbl_Credit_Appraisal_Memorandum on a.LoanApplicationId equals c.LoanApplicationId
                        join d in context.tbl_Credit_Appraisal_Memorandum_Document on c.AppraisalMemorandumId equals d.AppraisalMemorandumId
                        join cust in context.tbl_Customer on a.CustomerId equals cust.CustomerId into cc
                        from cust in
cc.DefaultIfEmpty()
                        join cGrp in context.tbl_Customer_Group on a.CustomerGroupId equals cGrp.CustomerGroupId into grp
                        from cGrp in grp.DefaultIfEmpty()
                        join ss in context.tbl_Sub_Sector on b.SubSectorId equals ss.SubSectorId into sec
                        from ss in sec.DefaultIfEmpty()

                        where a.CompanyId == companyId && a.Deleted == false
                              && b.StatusId == (int)ApprovalStatusEnum.Approved
                        group a by new
                        {
                            a.LoanApplicationId,
                            a.ApplicationReferenceNumber,
                            b.ApprovedAmount,
                            a.LoanTypeId,
                            a.ApplicationStatusId,
                            c.CAMRef,
                            d.CAMDocumentation,
                            a.ApplicationDate,
                            a.RelationshipManagerId,
                            a.RelationshipOfficerId,
                            cust.FirstName,
                            cust.LastName,
                            cust.MiddleName,
                            cust.CustomerCode,
                            cust.CustomerId,
                            cGrp.CustomerGroupId,
                            cGrp.GroupCode,
                            cGrp.GroupName,
                            ss.SubSectorId
                        } into g
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = g.Key.LoanApplicationId,
                            applicationReferenceNumber = g.Key.ApplicationReferenceNumber,
                            customerCode = g.Key.CustomerCode,
                            customerName = g.Key.CustomerId.Equals(0) ? g.Key.GroupName : g.Key.FirstName + " " + g.Key.MiddleName + " " + g.Key.LastName,
                            //customerGroupId = g.Key.CustomerGroupId,
                            customerGroupName = g.Key.GroupName,
                            customerGroupCode = g.Key.GroupCode,
                            relationshipOfficerId = g.Key.RelationshipOfficerId,
                            //relationshipOfficerName =
                            //    a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = g.Key.RelationshipManagerId,
                            //relationshipManagerName =
                            //    a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,

                            //currencyId = a.CurrencyId,
                            //currencyCode = a.tbl_Currency.CurrencyCode,
                            loanTypeId = g.Key.LoanTypeId,
                            loanTypeName = context.tbl_Loan_Type.FirstOrDefault(x => x.LoanTypeId == g.Key.LoanTypeId).LoanTypeName,
                            camReference = g.Key.CAMRef,
                            camDocumentation = g.Key.CAMDocumentation,
                            approvedAmount = g.Sum(x => x.ApprovedAmount),
                            applicationDate = g.Key.ApplicationDate,
                            applicationStatusId = g.Key.ApplicationStatusId,
                            subSectorId = g.Key.SubSectorId
                        });

            return data;
        }

        public bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId)
        {
            var target = context.tbl_Loan_Application.FirstOrDefault(x => x.ApplicationReferenceNumber ==
                applicationRefNumber.ToString());

            if (target != null)
            {
                switch (applicationStatusId)
                {
                    case (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress:
                        if (target.ApplicationStatusId != (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                        {
                            target.ApplicationStatusId =
                                (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;

                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted:
                        if (target.ApplicationStatusId != (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted)
                        {
                            target.ApplicationStatusId = (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress:
                        if (target.ApplicationStatusId !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress)
                        {
                            target.ApplicationStatusId =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted:
                        if (target.ApplicationStatusId !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted)
                        {
                            target.ApplicationStatusId =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.AvailmentInProgress:
                        if (target.ApplicationStatusId != (short)LoanApplicationStatusEnum.AvailmentInProgress)
                        {
                            target.ApplicationStatusId = (short)LoanApplicationStatusEnum.AvailmentInProgress;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.AvailmentCompleted:
                        if (target.ApplicationStatusId != (short)LoanApplicationStatusEnum.AvailmentCompleted)
                        {
                            target.ApplicationStatusId = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    default:
                        return false;
                }
            }

            return false;

            //public IEnumerable
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());
            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());
            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.CAMCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());
            return data;
        }

        #endregion OfferLetter & Availment Process

        #region "Loan Applications Awaiting Checklist"
        public IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsAwaitingCheckList(int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        join b in context.tbl_Loan_Application_Detail
                        on a.LoanApplicationId equals b.LoanApplicationId
                        where a.ApplicationStatusId == (short)LoanApplicationStatusEnum.ApplicationCompleted
                        && b.StatusId == (short)LoanApplicationDetailsStatusEnum.Pending
                        && b.HasDoneChecklist == false
                        && a.CompanyId == companyId && a.Deleted == false
                        select new LoanApplicationDetailViewModel()
                        {
                            loanApplicationId = b.LoanApplicationId,
                            applicationRefNo = a.ApplicationReferenceNumber,
                            customerId = b.CustomerId,
                            customerName = b.tbl_Customer.FirstName + " " + b.tbl_Customer.MiddleName + " " + b.tbl_Customer.LastName,
                            loanApplicationDetailId = b.LoanApplicationDetailId,
                            proposedProductId = b.ProposedProductId,
                            proposedProductName = b.tbl_Product.ProductName,
                            proposedTenor = b.ProposedTenor,
                            proposedAmount = b.ProposedAmount,
                            proposedInterestRate = b.ProposedInterestRate
                        });
            //var data = (from a in context.tbl_Loan_Application
            //            join b in context.tbl_Loan_Application_Detail
            //            on a.LoanApplicationId equals b.LoanApplicationId
            //            where a.ApplicationStatusId == (short)LoanApplicationStatusEnum.ApplicationCompleted
            //            && a.CompanyId == companyId && a.Deleted == false group b by new
            //            {
            //                b.ProposedProductId,
            //                a.ApplicationReferenceNumber,
            //                b.CustomerId,
            //                b.LoanApplicationDetailId,
            //                b.tbl_Customer, 
            //                b.LoanApplicationId,
            //                b.ProposedTenor,
            //                b.ProposedInterestRate,
            //                b.ProposedAmount, 
            //                b.tbl_Product
            //            }                        
            //            into g
            //            select new LoanApplicationDetailViewModel() {
            //                loanApplicationId = g.Key.LoanApplicationId,
            //                applicationRefNo = g.Key.ApplicationReferenceNumber,
            //                customerId = g.Key.CustomerId,
            //                customerName = g.Key.tbl_Customer.FirstName + " " + g.Key.tbl_Customer.MiddleName + " " + g.Key.tbl_Customer.LastName,
            //                loanApplicationDetailId = g.Key.LoanApplicationDetailId,
            //                proposedProductId = g.Key.ProposedProductId,
            //                proposedProductName = g.Key.tbl_Product.ProductName,
            //                proposedTenor = g.Key.ProposedTenor,
            //                proposedAmount = g.Key.ProposedAmount,
            //                proposedInterestRate = g.Key.ProposedInterestRate
            //            });


            return data;
        }
        #endregion
    }
}
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using System.ComponentModel.Composition;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkFlowRepository workFlow;
        private ICasaRepository casa;
        private ICustomerCollateralRepository collateral;
        private IFinanceTransactionRepository finance;

        public LoanApplicationRepository(IAuditTrailRepository _auditTrail, ICasaRepository _casa, ICustomerCollateralRepository _collateral,
                                    IGeneralSetupRepository _genSetup, IWorkFlowRepository _workFlow, IFinanceTransactionRepository _finance,
        FinTrakBankingContext _context)
        {
            this.collateral = _collateral;
            this.finance = _finance;
            this.context = _context;
            this.casa = _casa;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            workFlow = _workFlow;
        }

        public IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId)
        {
            var data = context.tbl_Loan_Application.Where(c => c.CustomerId == customerId && c.CompanyId == companyId).Select(c => new ExistingLoanApplicationViewModel()
            {
                applicationDate = c.ApplicationDate,
                applicationReferenceNumber = c.ApplicationReferenceNumber,
                interestRate = c.InterestRate,
                loanTypeName = c.tbl_Loan_Type.LoanTypeName,
                branch = c.tbl_Branch.BranchName,
                principalAmount = c.PrincipalAmount,
                tenor = c.Tenor
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
                            tenor = a.Tenor,
                            //tenorModeId = a.TenorModeId,
                            //tenorModeName = a.tbl_Tenor_Mode.TenorModeName,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = a.RelationshipManagerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,

                            misCode = a.MISCode,

                            productId = (short)a.ProductId,
                            productName = a.tbl_Product.ProductName,
                            teamMisCode = a.TeamMISCode,

                            interestRate = a.InterestRate,
                            isRealatedParty = a.IsRealatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            submittedForAppraisal = a.SubmittedForAppraisal,
                            principalAmount = a.PrincipalAmount,
                            customerGroupId = a.CustomerGroupId ?? 0,
                            customerGroupName = a.CustomerGroupId.HasValue ? a.tbl_Customer_Group.GroupName : "",
                            loanTypeId = a.LoanTypeId,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            createdBy = a.CreatedBy,
                            applicationDate = a.ApplicationDate,
                            dateTimeCreated = a.DateTimeCreated
                        });
            return data;

        }

        public IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId)
        {
            return GetLoanApplications(companyId).ToList();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationJobs(int companyId, int levelId, int scope)
        {
            var applications = GetLoanApplications(companyId).Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending); // scope 3 entire process

            if (scope == (int)ProcessViewScopeEnum.Group)
            {
                int levelGroupId;
                var level = context.tbl_Approval_Level.Find(levelId);

                if (level != null)
                {
                    levelGroupId = level.GroupOperationMappingId;

                    var groupApprovalLevelIds = context.tbl_Approval_Level
                        .Where(x => x.GroupOperationMappingId == levelGroupId)
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
                            tenor = a.Tenor,
                            // tenorModeId = a.TenorModeId,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipManagerId = a.RelationshipManagerId,

                            misCode = a.MISCode,
                            productId = (short)a.ProductId,
                            teamMisCode = a.TeamMISCode,

                            interestRate = a.InterestRate,
                            isRealatedParty = a.IsRealatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            principalAmount = a.PrincipalAmount,
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
            return this.context.tbl_Loan_Status.Where(x => x.LoanStatusId == loanStatusId).SingleOrDefault().AccountStatus;
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
                Detail = $"Change Loan Application Status with reference number '{data.ApplicationReferenceNumber}' to {GetLoanStatus((short)entity.approvalStatusId)}",
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

        public async Task<bool> AddLoanApplication(LoanApplicationViewModel loan)
        {
            bool isGroupLoan = false;
            int response = 0;
            if (loan.loanTypeId > (int)LoanTypeEnum.CustomerGroup)
            {
                isGroupLoan = true;
            }

            //     string refNumber = GenerateLoanReference(loan.customerId.Value);

            var casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);

            var loanStatusId = (short)LoanStatusEnum.Inactive;

            var data = new tbl_Loan_Application
            {
                ApplicationReferenceNumber = loan.applicationReferenceNumber,
                LoanTypeId = loan.loanTypeId,
                //LoanStatusId = loanStatusId,
                CompanyId = loan.companyId,
                BranchId = (short)loan.branchId,
                Tenor = loan.tenor,
                CasaAccountId = casaAccountId,
                RelationshipOfficerId = loan.relationshipOfficerId,
                RelationshipManagerId = loan.relationshipManagerId,
                MISCode = loan.misCode,
                CurrencyId = loan.currencyId,
                TeamMISCode = loan.teamMisCode,
                InterestRate = loan.interestRate,
                ProductId = loan.productId,
                PrincipalAmount = loan.principalAmount,
                ApplicationDate = genSetup.GetApplicationDate(),
                LoanInformation = loan.loanInformation,
                IsRealatedParty = loan.isRealatedParty,
                IsPoliticallyExposed = loan.isPoliticallyExposed,
                CreatedBy = (int)loan.createdBy,
                DateTimeCreated = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                //SubSectorId = (short)loan.subSectorId,

                ExchangeRate = loan.exchangeRate,
                LoanPreliminaryEvaluationId = loan.loanPreliminaryEvaluationId,
                CustomerId = loan.customerId,
                SubmittedForAppraisal = loan.submittedForAppraisal
            };

            if (loan.LoanApplicationCollateral.Count > 0)
            {
                LoanApplicationCollateral(loan.LoanApplicationCollateral);
            }

            if (isGroupLoan)
            {
                data.CustomerGroupId = loan.customerId;
                data.CustomerId = null;
            }
            else
            {
                data.CustomerId = loan.customerId;
                data.CustomerGroupId = null;
            }

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.LoanApplication, loan.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
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


                        response = await context.SaveChangesAsync();

                        // send for approval

                        if (response > 0)
                        {
                            var wf = new ApprovalViewModel
                            {
                                companyId = data.CompanyId,
                                operationId = (int)OperationsEnum.LoanApplication,
                                staffId = data.CreatedBy,
                                targetId = data.LoanApplicationId,
                                approvalStatusId = (short)ApprovalStatusEnum.Pending,
                            };
                            var result = await workFlow.LogForApproval(wf);
                            if (result.Item1)
                            {
                                return response > 0;
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }

            }
            else
            {
                throw new MyException("Approval path has not been defined for this request");
            }
            return response > 0;
        }
        private void LoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity)
        {
            foreach (var item in entity)
            {
                var loanCollateral = new tbl_Loan_Application_Collateral()
                {
                    CertificateOfOwnership = item.certificateOfOwnership,
                    CityId = item.cityId,
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
                    LoanApplicationId = item.loanApplicationId,
                    SystemDateTime = DateTime.Now

                };
                context.tbl_Loan_Application_Collateral.Add(loanCollateral);
            }
        }



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

       

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int companyId, int branchId, int staffId)
        {
            int operationId = (int)OperationsEnum.CAM;
            int scope = this.GetStaffWorkflowViewScope(operationId, staffId);

            if (scope == (int)ProcessViewScopeEnum.Process) // 3
            {
                return context.tbl_Loan_Application.Where(x => x.CompanyId == companyId && x.Deleted == false //&& x.BranchId == branchId
                )
                    .Select(a => new LoanApplicationViewModel
                    {
                        approvalStatusId = a.ApprovalStatusId,
                        loanApplicationId = a.LoanApplicationId,
                        applicationReferenceNumber = a.ApplicationReferenceNumber,
                        customerId = a.CustomerId ?? 0,
                        customerName = a.CustomerId.HasValue ? a.tbl_Customer.FirstName + " " + a.tbl_Customer.MiddleName + " " + a.tbl_Customer.LastName : "",
                        loanInformation = a.LoanInformation,
                        companyId = a.CompanyId,
                        branchId = a.BranchId,
                        branchName = a.tbl_Branch.BranchName,
                        tenor = a.Tenor,
                        relationshipOfficerId = a.RelationshipOfficerId,
                        relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                        relationshipManagerId = a.RelationshipManagerId,
                        relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                        misCode = a.MISCode,
                        productClassId = a.tbl_Product.ProductClassId,
                        productClassName = a.tbl_Product.tbl_Product_Class.ProductClassName,
                        interestRate = a.InterestRate,
                        isRealatedParty = a.IsRealatedParty,
                        isPoliticallyExposed = a.IsPoliticallyExposed,
                        submittedForAppraisal = a.SubmittedForAppraisal,
                        principalAmount = a.PrincipalAmount,
                        customerGroupId = a.CustomerGroupId ?? 0,
                        customerGroupName = a.CustomerGroupId.HasValue ? a.tbl_Customer_Group.GroupName : "",
                        loanTypeId = a.LoanTypeId,
                        loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                        createdBy = a.CreatedBy,
                        applicationDate = a.ApplicationDate,
                        dateTimeCreated = a.DateTimeCreated,
                    });
            }

            var staffApprovalLevelIds = context.tbl_Approval_Level_Staff.Where(x => x.Deleted == false && x.StaffId == staffId).Select(x => x.ApprovalLevelId);

            var pendingApplications = context.tbl_Loan_Application
                    .Join(context.tbl_Approval_Trail,
                        a => a.LoanApplicationId, b => b.TargetId, (a, b) => new { a, b })
                    .Where(x => x.b.OperationId == operationId //&& x.a.BranchId == branchId
                    );

            if (scope == (int)ProcessViewScopeEnum.Group) // 2
            {
                var groupApprovalLevelIds = context.tbl_Approval_Level_Staff
                    .Where(x => x.Deleted == false && x.StaffId == staffId)
                    .Select(x => x.tbl_Approval_Level)
                    .Select(x => x.tbl_Approval_Group_Mapping)
                    .Where(x => x.Deleted == false
                            && x.OperationId == operationId
                            //&& x.ProductClassId == productClassId
                            //&& x.ProductId == productId
                            )
                    .SelectMany(x => x.tbl_Approval_Level)
                    .Select(x => x.ApprovalLevelId);

                pendingApplications = pendingApplications.Where(x => groupApprovalLevelIds.Contains(x.b.ToApprovalLevelId));
            }

            if (scope == (int)ProcessViewScopeEnum.Level) // 1
            {
                pendingApplications = pendingApplications.Where(x => staffApprovalLevelIds.Contains(x.b.ToApprovalLevelId));
            }

            return pendingApplications.Select(x => new LoanApplicationViewModel
            {
                loanApplicationId = x.a.LoanApplicationId,
                applicationReferenceNumber = x.a.ApplicationReferenceNumber,
                customerId = x.a.CustomerId,
                branchId = x.a.BranchId,
                productClassId = x.a.tbl_Product.ProductClassId,
                productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                customerGroupId = x.a.CustomerGroupId,
                loanTypeId = x.a.LoanTypeId,
                currencyId = x.a.CurrencyId,
                //loanStatusId = x.a.LoanStatusId,
                relationshipOfficerId = x.a.RelationshipOfficerId,
                relationshipManagerId = x.a.RelationshipManagerId,
                applicationDate = x.a.ApplicationDate,
                principalAmount = x.a.PrincipalAmount,
                interestRate = x.a.InterestRate,
                tenor = x.a.Tenor,
                loanInformation = x.a.LoanInformation,
                submittedForAppraisal = x.a.SubmittedForAppraisal,
                isRealatedParty = x.a.IsRealatedParty,
                isPoliticallyExposed = x.a.IsPoliticallyExposed,
                approvalStatusId = x.a.ApprovalStatusId,
                branchName = x.a.tbl_Branch.BranchName,
                relationshipOfficerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                relationshipManagerName = x.a.tbl_Staff.FirstName + " " + x.a.tbl_Staff.MiddleName + " " + x.a.tbl_Staff.LastName,
                misCode = x.a.MISCode,
                customerGroupName = x.a.CustomerGroupId.HasValue ? x.a.tbl_Customer_Group.GroupName : "",
                loanTypeName = x.a.tbl_Loan_Type.LoanTypeName,
                createdBy = x.a.CreatedBy,
                loanPreliminaryEvaluationId = x.a.LoanPreliminaryEvaluationId,
                customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
            })
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId);
        }

        public int GetStaffWorkflowViewScope(int operationId, int staffId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default @Level
            var staffWorkflow = context.tbl_Approval_Group_Mapping.Where(x => x.OperationId == operationId)
                .SelectMany(g => g.tbl_Approval_Level)
                .SelectMany(l => l.tbl_Approval_Level_Staff).Where(x => x.StaffId == staffId);

            if (staffWorkflow.Count() > 0)
            {
                scope = staffWorkflow.Max(x => x.ProcessViewScopeId);
            }

            return scope;
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
    }
}

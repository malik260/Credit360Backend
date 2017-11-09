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
            var data = context.TBL_LOAN_APPLICATION.Where(c => c.CUSTOMERID == customerId && c.COMPANYID == companyId)
                .Select(c => new ExistingLoanApplicationViewModel()
                {
                    applicationDate = c.APPLICATIONDATE,
                    applicationReferenceNumber = c.APPLICATIONREFERENCENUMBER,
                    interestRate = c.INTERESTRATE,
                    loanTypeName = c.TBL_LOAN_TYPE.LOANTYPENAME,
                    branch = c.TBL_BRANCH.BRANCHNAME,
                    principalAmount = c.APPLICATIONAMOUNT,
                    tenor = c.APPLICATIONTENOR
                }).ToList();
            return data;
        }

        private IQueryable<LoanApplicationViewModel> GetLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationViewModel
                        {
                            approvalStatusId = a.APPROVALSTATUSID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID ?? 0,
                            customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                            misCode = a.MISCODE,
                            teamMisCode = a.TEAMMISCODE,
                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                            customerGroupId = a.CUSTOMERGROUPID ?? 0,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeId = a.LOANTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            createdBy = a.CREATEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            dateTimeCreated = a.DATETIMECREATED,
                            LoanApplicationCollateral = context.TBL_LOAN_APPLICATION_COLLATERAL.Where(d => d.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                             .Select(d => new LoanApplicationCollateralViewModel()
                             {
                                 //     collateralValue   = d.CollateralReferenceNumber,
                                 //cityId = d.CITYID,
                                 //collateralTypeId = d.COLLATERALTYPEID,
                                 //customerCollateralId = d.CUSTOMERCOLLATERALID,
                                 //documentTitle = d.DOCUMENTTITLE,
                                 //latitude = d.LATITUDE,
                                 loanApplicationId = d.LOANAPPLICATIONID,
                                 //locationAddress = d.LOCATIONADDRESS,
                                 //longitude = d.LONGITUDE,
                                 //nearestBusStop = d.NEARESTBUSSTOP,
                                 //nearestLandmark = d.NEARESTLANDMARK,
                                 //otherInformations = d.OTHERINFORMATIONS,
                                 //city = d.TBL_CITY.CITYNAME,
                                 //collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                 //companyName = d.TBL_LOAN_APPLICATION.TBL_COMPANY.NAME,
                                 // applicationReferanceNumber = int.Parse(d.tbl_Loan_Application.ApplicationReferenceNumber),

                             }).ToList(),
                            LoanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                             .Select(c => new LoanApplicationDetailViewModel()
                             {
                                 approvedAmount = c.APPROVEDAMOUNT,
                                 approvedInterestRate = c.APPROVEDINTERESTRATE,
                                 approvedProductId = c.APPROVEDPRODUCTID,
                                 approvedTenor = c.APPROVEDTENOR,
                                 currencyId = c.CURRENCYID,
                                 currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                 customerId = c.CUSTOMERID,
                                 exchangeRate = c.EXCHANGERATE,
                                 loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                 subSectorId = c.SUBSECTORID,
                                 loanApplicationId = c.LOANAPPLICATIONID,
                                 proposedAmount = c.PROPOSEDAMOUNT,
                                 proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                 proposedProductId = c.PROPOSEDPRODUCTID,
                                 proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                                 statusId = c.STATUSID
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
                var level = context.TBL_APPROVAL_LEVEL.Find(levelId);

                if (level != null)
                {
                    levelGroupId = (int)level.GROUPID;

                    var groupApprovalLevelIds = context.TBL_APPROVAL_LEVEL
                        .Where(x => x.GROUPID == levelGroupId)
                        .Select(x => x.APPROVALLEVELID);

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
            return (from data in context.TBL_PRODUCT_CLASS
                    select new ProductClassViewModel()
                    {
                        productClassId = data.PRODUCTCLASSID,
                        productClassName = data.PRODUCTCLASSNAME,
                        productClassTypeId = data.PRODUCTCLASSTYPEID
                    });
        }

        public IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        //&& (a.ApplicationReferenceNumber == referenceNumberOrName || $"{a.tbl_Customer.FirstName} {a.tbl_Customer.MiddleName} {a.tbl_Customer.LastName} {a.tbl_Customer.CustomerCode} ".Contains(referenceNumberOrName))
                        select new LoanApplicationViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID.Value,
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            //tenor = a.ApplicationTenor,
                            // tenorModeId = a.TenorModeId,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,

                            misCode = a.MISCODE,
                            //productId = (short)a.ProductId,
                            teamMisCode = a.TEAMMISCODE,

                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            //principalAmount = a.ApplicationAmount,
                            customerGroupId = a.CUSTOMERGROUPID.Value,
                            loanTypeId = a.LOANTYPEID,
                            //loanStatusId = a.LoanStatusId,
                            createdBy = a.CREATEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            dateTimeCreated = a.DATETIMECREATED
                        }).ToList();
            return data;
        }

        private string GetLoanStatus(short loanStatusId)
        {
            return this.context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == loanStatusId).SingleOrDefault()
                .ACCOUNTSTATUS;
        }

        public async Task<bool> UpdateApprovalStatus(ApprovalViewModel entity)
        {
            var data = this.context.TBL_LOAN_APPLICATION.Find(entity.targetId);
            {
                //data.LoanStatusId = (short)entity.approvalStatusId;
                //data.ActedOnaBy = entity.staffId;
                //data.DateActedOn = genSetup.GetApplicaionDate();
                data.LOANAPPLICATIONID = (short)entity.approvalStatusId;
            }

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalStatusUpdated,
                STAFFID = entity.staffId,
                BRANCHID = (short)entity.BranchId,
                DETAIL =
                    $"Change Loan Application Status with reference number '{data.APPLICATIONREFERENCENUMBER}' to {GetLoanStatus((short)entity.approvalStatusId)}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.targetId
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
                if (loan.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
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

                var data = new TBL_LOAN_APPLICATION
                {
                    APPLICATIONREFERENCENUMBER = loan.applicationReferenceNumber,
                    LOANTYPEID = loan.loanTypeId,
                    COMPANYID = loan.companyId,
                    BRANCHID = (short)loan.branchId,
                    RELATIONSHIPOFFICERID = loan.relationshipOfficerId,
                    RELATIONSHIPMANAGERID = loan.relationshipManagerId,
                    MISCODE = loan.misCode,
                    TEAMMISCODE = loan.teamMisCode,
                    INTERESTRATE = loan.interestRate,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    LOANINFORMATION = loan.loanInformation,
                    ISRELATEDPARTY = loan.isRelatedParty,
                    ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed,
                    CREATEDBY = (int)loan.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    CUSTOMERGROUPID = loan.customerGroupId,
                    APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationCompleted,
                    APPLICATIONAMOUNT = loan.proposedAmount,
                    APPLICATIONTENOR = Convert.ToInt32(Math.Round(((decimal)(loan.proposedTenor / 12) * (decimal)365))),
                    ISINVESTMENTGRADE = loan.isInvestmentGrade,
                    LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                    CUSTOMERID = loan.customerId,
                    SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                    OPERATIONID = (int)OperationsEnum.CAM
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
                    data.CUSTOMERGROUPID = loan.customerGroupId;
                    data.CUSTOMERID = null;
                }
                else
                {
                    data.CUSTOMERID = loan.customerId;
                    data.CUSTOMERGROUPID = null;
                }

                context.TBL_LOAN_APPLICATION.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                    STAFFID = loan.createdBy,
                    BRANCHID = (short)loan.userBranchId,
                    DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                    IPADDRESS = loan.userIPAddress,
                    URL = loan.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = loan.loanApplicationId
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                response = context.SaveChanges();

                return response > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void LoanApplicationDetail(List<LoanApplicationDetailViewModel> entity)
        {
            var ApplicationDetail = entity.Select(a => new TBL_LOAN_APPLICATION_DETAIL()
            {
                APPROVEDAMOUNT = a.proposedAmount,
                APPROVEDINTERESTRATE = a.proposedInterestRate,
                APPROVEDPRODUCTID = a.proposedProductId,
                APPROVEDTENOR = Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                EXCHANGERATE = a.exchangeRate,
                CURRENCYID = a.currencyId,
                CUSTOMERID = a.customerId,
                LOANAPPLICATIONID = a.loanApplicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,


                PROPOSEDAMOUNT = a.proposedAmount,
                PROPOSEDINTERESTRATE = a.proposedInterestRate,
                PROPOSEDPRODUCTID = a.proposedProductId,
                PROPOSEDTENOR = Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                SUBSECTORID = a.subSectorId,
                CREATEDBY = a.createdBy,
                DATETIMECREATED = DateTime.Now,

            });
            context.TBL_LOAN_APPLICATION_DETAIL.AddRange(ApplicationDetail);

        }

        private void LoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity)
        {
            foreach (var item in entity)
            {
                var loanCollateral = new TBL_LOAN_APPLICATION_COLLATERAL()
                {
                    //COLLATERALREFERENCENUMBER = item.collateralReferenceNumber,
                    //CITYID = item.cityId,
                    //COLLATERALVALUE = item.collateralValue,
                    //ISBANKACCOUNT = item.isBankAccount,
                    //COLLATERALTYPEID = item.collateralTypeId,
                    CREATEDBY = item.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    //OTHERINFORMATIONS = item.otherInformations,
                    //LATITUDE = item.latitude,
                    //LONGITUDE = item.longitude,

                    //LOCATIONADDRESS = item.locationAddress,
                    //NEARESTBUSSTOP = item.nearestBusStop,
                    //NEARESTLANDMARK = item.nearestLandmark,
                    //DOCUMENTTITLE = item.documentTitle,
                    //LoanApplicationId = item.loanApplicationId,
                    SYSTEMDATETIME = DateTime.Now,
                    //CASAACCOUNTID = item.casaAccountId

                };
                context.TBL_LOAN_APPLICATION_COLLATERAL.Add(loanCollateral);
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
                var grp = this.context.TBL_CUSTOMER_GROUP.Where(x => x.CUSTOMERGROUPID == customerId);
                if (grp.Any())
                {
                    code = grp.First().GROUPCODE;
                }
                data = ((this.context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
            }
            else
            {
                var cust = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId);
                if (cust.Any())
                {
                    code = cust.First().CUSTOMERCODE;
                }
                data = ((context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
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

        #region CAM Pending Applications

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int companyId, int branchId, int staffId)
        {
            int operationId = (int)OperationsEnum.CAM;
            int scope = this.GetStaffWorkflowViewScope(operationId, staffId);

            int[] camStages = new int[] {
                (int)LoanApplicationStatusEnum.CAMInProgress,
                (int)LoanApplicationStatusEnum.CAMCompleted,
                (int)LoanApplicationStatusEnum.ChecklistCompleted
            };

            if (scope == (int)ProcessViewScopeEnum.Process) // 3
            {
                return context.TBL_LOAN_APPLICATION.Where(x => 
                    x.COMPANYID == companyId 
                    && x.DELETED == false 
                    && camStages.Contains(x.APPLICATIONSTATUSID)
                    //&& x.BranchId == branchId
                )
                    .GroupJoin(
                        context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                        a => a.LOANAPPLICATIONID,
                        b => b.TARGETID,
                        (x, y) => new { a = x, bs = y })
                    .SelectMany(
                        xy => xy.bs.DefaultIfEmpty(),
                        (x, y) => new LoanApplicationViewModel
                        {
                            loanApplicationId = x.a.LOANAPPLICATIONID,
                            applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                            customerId = x.a.CUSTOMERID,
                            branchId = x.a.BRANCHID,
                            //productClassId = x.a.tbl_Product.ProductClassId,
                            //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                            customerGroupId = x.a.CUSTOMERGROUPID,
                            loanTypeId = x.a.LOANTYPEID,
                            relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                            applicationDate = x.a.APPLICATIONDATE,
                            applicationAmount = x.a.APPLICATIONAMOUNT,
                            approvedAmount = x.a.APPROVEDAMOUNT,
                            interestRate = x.a.INTERESTRATE,
                            applicationTenor = x.a.APPLICATIONTENOR,
                            lastComment = y.COMMENT,
                            currentApprovalStateId = y.APPROVALSTATEID,
                            currentApprovalLevelId = y.TOAPPROVALLEVELID,
                            currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                            approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                            loanInformation = x.a.LOANINFORMATION,
                            submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                            customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                            notInNegativeCrms = x.a.NOTINNEGATIVECRMS,
                            notInBlackbook = x.a.NOTINBLACKBOOK,
                            notInCamsol = x.a.NOTINCAMSOL,
                            isRelatedParty = x.a.ISRELATEDPARTY,
                            isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                            approvalStatusId = x.a.APPROVALSTATUSID,
                            applicationStatusId = x.a.APPLICATIONSTATUSID,
                            branchName = x.a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                            relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                            misCode = x.a.MISCODE,
                            customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeName = x.a.TBL_LOAN_TYPE.LOANTYPENAME,
                            createdBy = x.a.CREATEDBY,
                            loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                            //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                            operationId = x.a.OPERATIONID,
                        })
                        .GroupBy(d => d.loanApplicationId)
                        .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                        .OrderByDescending(x => x.applicationDate)
                        .ThenByDescending(x => x.loanApplicationId)
                        ;
            }

            var staffApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF
                .Where(x => x.DELETED == false && x.STAFFID == staffId).Select(x => x.APPROVALLEVELID);

            var pendingApplications = context.TBL_LOAN_APPLICATION.Where(x => camStages.Contains(x.APPLICATIONSTATUSID))
                .Join(context.TBL_APPROVAL_TRAIL,
                    a => a.LOANAPPLICATIONID, b => b.TARGETID, (a, b) => new { a, b })
                .Where(x => x.b.OPERATIONID == operationId //&& x.a.BranchId == branchId
                );

            int count = pendingApplications.Count(); // for testing

            if (scope == (int)ProcessViewScopeEnum.Group) // 2
            {
                var groupApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF
                    .Where(x => x.DELETED == false && x.STAFFID == staffId)
                    .Select(x => x.TBL_APPROVAL_LEVEL)
                    .Select(x => x.TBL_APPROVAL_GROUP)
                    .SelectMany(x => x.TBL_APPROVAL_GROUP_MAPPING)
                    .Where(x => x.DELETED == false && x.OPERATIONID == operationId)
                    .Select(x => x.TBL_APPROVAL_GROUP)
                    .SelectMany(x => x.TBL_APPROVAL_LEVEL)
                    .Select(x => x.APPROVALLEVELID);

                pendingApplications = pendingApplications.Where(x => groupApprovalLevelIds.Contains((int)x.b.TOAPPROVALLEVELID) && x.b.RESPONSESTAFFID == null);
            }

            if (scope == (int)ProcessViewScopeEnum.Level) // 1
            {
                pendingApplications = pendingApplications.Where(x => staffApprovalLevelIds.Contains((int)x.b.TOAPPROVALLEVELID) && x.b.RESPONSESTAFFID == null);
            }

            return pendingApplications.Select(x => new LoanApplicationViewModel
            {
                loanApplicationId = x.a.LOANAPPLICATIONID,
                applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                customerId = x.a.CUSTOMERID,
                branchId = x.a.BRANCHID,
                //productClassId = x.a.tbl_Product.ProductClassId,
                //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                customerGroupId = x.a.CUSTOMERGROUPID,
                loanTypeId = x.a.LOANTYPEID,
                relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                applicationDate = x.a.APPLICATIONDATE,
                applicationAmount = x.a.APPLICATIONAMOUNT,
                approvedAmount = x.a.APPROVEDAMOUNT,
                interestRate = x.a.INTERESTRATE,
                applicationTenor = x.a.APPLICATIONTENOR,
                lastComment = x.b.COMMENT,
                currentApprovalStateId = x.b.APPROVALSTATEID,
                currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                loanInformation = x.a.LOANINFORMATION,
                submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                notInNegativeCrms = x.a.NOTINNEGATIVECRMS,
                notInBlackbook = x.a.NOTINBLACKBOOK,
                notInCamsol = x.a.NOTINCAMSOL,
                isRelatedParty = x.a.ISRELATEDPARTY,
                isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                approvalStatusId = x.a.APPROVALSTATUSID,
                applicationStatusId = x.a.APPLICATIONSTATUSID,
                branchName = x.a.TBL_BRANCH.BRANCHNAME,
                relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                misCode = x.a.MISCODE,
                customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeName = x.a.TBL_LOAN_TYPE.LOANTYPENAME,
                createdBy = x.a.CREATEDBY,
                loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                operationId = x.a.OPERATIONID,
            })
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId)
            .Distinct();
        }

        public int GetStaffWorkflowViewScope(int operationId, int staffId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default @Level
            var staffWorkflow = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(g => g.TBL_APPROVAL_LEVEL)
                .SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

            if (staffWorkflow.Count() > 0)
            {
                scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);
            }

            return scope;
        }

        #endregion CAM Pending Applications

        #region OfferLetter & Availment Process

        private IQueryable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        join d in context.TBL_CREDIT_APPRAISAL_MEMORANDUM_DOCUMENT on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID
                        join cust in context.TBL_CUSTOMER on a.CUSTOMERID equals cust.CUSTOMERID into cc
                        from cust in cc.DefaultIfEmpty()

                        join cGrp in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals cGrp.CUSTOMERGROUPID into grp
                        from cGrp in grp.DefaultIfEmpty()
                        join ss in context.TBL_SUB_SECTOR on b.SUBSECTORID equals ss.SUBSECTORID into sec
                        from ss in sec.DefaultIfEmpty()

                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        group a by new
                        {
                            a.LOANAPPLICATIONID,
                            a.APPLICATIONREFERENCENUMBER,
                            b.APPROVEDAMOUNT,
                            a.LOANTYPEID,
                            a.APPLICATIONSTATUSID,
                            c.CAMREF,
                            d.CAMDOCUMENTATION,
                            a.APPLICATIONDATE,
                            a.RELATIONSHIPMANAGERID,
                            a.RELATIONSHIPOFFICERID,
                            cust.FIRSTNAME,
                            cust.LASTNAME,
                            cust.MIDDLENAME,
                            cust.CUSTOMERCODE,
                            cust.CUSTOMERID,
                            cGrp.CUSTOMERGROUPID,
                            cGrp.GROUPCODE,
                            cGrp.GROUPNAME,
                            ss.SUBSECTORID
                        } into g
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = g.Key.LOANAPPLICATIONID,
                            applicationReferenceNumber = g.Key.APPLICATIONREFERENCENUMBER,
                            customerCode = g.Key.CUSTOMERCODE,
                            customerName = g.Key.CUSTOMERID.Equals(0) ? g.Key.GROUPNAME : g.Key.FIRSTNAME + " " + g.Key.MIDDLENAME + " " + g.Key.LASTNAME,
                            //customerGroupId = g.Key.CustomerGroupId,
                            customerGroupName = g.Key.GROUPNAME,
                            customerGroupCode = g.Key.GROUPCODE,
                            relationshipOfficerId = g.Key.RELATIONSHIPOFFICERID,
                            //relationshipOfficerName =
                            //    a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = g.Key.RELATIONSHIPMANAGERID,
                            //relationshipManagerName =
                            //    a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,

                            //currencyId = a.CurrencyId,
                            //currencyCode = a.tbl_Currency.CurrencyCode,
                            loanTypeId = g.Key.LOANTYPEID,
                            loanTypeName = context.TBL_LOAN_TYPE.FirstOrDefault(x => x.LOANTYPEID == g.Key.LOANTYPEID).LOANTYPENAME,
                            camReference = g.Key.CAMREF,
                            camDocumentation = g.Key.CAMDOCUMENTATION,
                            approvedAmount = g.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = g.Key.APPLICATIONDATE,
                            applicationStatusId = g.Key.APPLICATIONSTATUSID,
                            subSectorId = g.Key.SUBSECTORID,
                        });

            return data;
        }

        public bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId)
        {
            var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER ==
                applicationRefNumber.ToString());

            if (target != null)
            {
                switch (applicationStatusId)
                {
                    case (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress:
                        if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                        {
                            target.APPLICATIONSTATUSID =
                                (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;

                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted:
                        if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted)
                        {
                            target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.OfferLetterGenerationCompleted;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress:
                        if (target.APPLICATIONSTATUSID !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress)
                        {
                            target.APPLICATIONSTATUSID =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewInProgress;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted:
                        if (target.APPLICATIONSTATUSID !=
                            (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted)
                        {
                            target.APPLICATIONSTATUSID =
                                (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.AvailmentInProgress:
                        if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.AvailmentInProgress)
                        {
                            target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentInProgress;
                            return context.SaveChanges() > 0;
                        }
                        return true;

                    case (short)LoanApplicationStatusEnum.AvailmentCompleted:
                        if (target.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.AvailmentCompleted)
                        {
                            target.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
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
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.ApplicationCompleted
                        && b.STATUSID == (short)LoanApplicationDetailsStatusEnum.Pending
                        && b.HASDONECHECKLIST == false
                        && a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationRefNo = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE
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

        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            var applications = context.TBL_LOAN_APPLICATION
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL,
                        a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER,
                        g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
                    .Select(x => new LoanApplicationViewModel
                    {
                        loanApplicationId = x.g.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.g.a.APPLICATIONREFERENCENUMBER,
                        customerId = x.g.a.CUSTOMERID,
                        branchId = x.g.a.BRANCHID,
                        customerGroupId = x.g.a.CUSTOMERGROUPID,
                        loanTypeId = x.g.a.LOANTYPEID,
                        relationshipOfficerId = x.g.a.RELATIONSHIPOFFICERID,
                        relationshipManagerId = x.g.a.RELATIONSHIPMANAGERID,
                        applicationDate = x.g.a.APPLICATIONDATE,
                        applicationAmount = x.g.a.APPLICATIONAMOUNT,
                        approvedAmount = x.g.a.APPROVEDAMOUNT,
                        interestRate = x.g.a.INTERESTRATE,
                        applicationTenor = x.g.a.APPLICATIONTENOR,
                        loanInformation = x.g.a.LOANINFORMATION,
                        submittedForAppraisal = x.g.a.SUBMITTEDFORAPPRAISAL,
                        customerInfoValidated = x.g.a.CUSTOMERINFOVALIDATED,
                        notInNegativeCrms = x.g.a.NOTINNEGATIVECRMS,
                        notInBlackbook = x.g.a.NOTINBLACKBOOK,
                        notInCamsol = x.g.a.NOTINCAMSOL,
                        isRelatedParty = x.g.a.ISRELATEDPARTY,
                        isPoliticallyExposed = x.g.a.ISPOLITICALLYEXPOSED,
                        approvalStatusId = x.g.a.APPROVALSTATUSID,
                        applicationStatusId = x.g.a.APPLICATIONSTATUSID,
                        branchName = x.g.a.TBL_BRANCH.BRANCHNAME,
                        relationshipOfficerName = x.g.a.TBL_STAFF.FIRSTNAME + " " + x.g.a.TBL_STAFF.MIDDLENAME + " " + x.g.a.TBL_STAFF.LASTNAME,
                        relationshipManagerName = x.g.a.TBL_STAFF1.FIRSTNAME + " " + x.g.a.TBL_STAFF1.MIDDLENAME + " " + x.g.a.TBL_STAFF1.LASTNAME,
                        misCode = x.g.a.MISCODE,
                        customerGroupName = x.g.a.CUSTOMERGROUPID.HasValue ? x.g.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        loanTypeName = x.g.a.TBL_LOAN_TYPE.LOANTYPENAME,
                        createdBy = x.g.a.CREATEDBY,
                        loanPreliminaryEvaluationId = x.g.a.LOANPRELIMINARYEVALUATIONID,
                        operationId = x.g.a.OPERATIONID,
                    })
                    //.Where(x => x.applicationReferenceNumber == searchString)
                    ;

            return applications;
        }
    }
}
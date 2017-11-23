using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;
        private ICasaRepository casa;

        private ICustomerCollateralRepository collateral;

        //private IFinanceTransactionRepository finance;
        private IApprovalLevelStaffRepository approvalLevel;

        public LoanApplicationRepository(IAuditTrailRepository _auditTrail,
            ICasaRepository _casa,
            ICustomerCollateralRepository _collateral,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workFlow)
        {
            this.collateral = _collateral;
            //this.finance = _finance;
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.casa = _casa;
            this.collateral = _collateral;
            approvalLevel = _approvallevel;
            workFlow = _workFlow;
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

        public IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId)
        {
            var data = from a in context.TBL_LOAN_APPLICATION
                       where a.APPROVALSTATUSID == (int)LoanApplicationStatusEnum.ApplicationInProgress && a.COMPANYID == companyId && a.DELETED == false
                       && a.CREATEDBY == relationshipOfficerId || a.RELATIONSHIPOFFICERID == relationshipOfficerId
                       select new
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
                           applicationTenor = Math.Round((double)a.APPLICATIONTENOR) * (12.0 / 365.0),
                           applicationAmount = a.APPROVEDAMOUNT
                       };
            return data.ToList();
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

        public bool UpdateApprovalStatusForApplication(int applocationId)//, object entity)
        {
            var data = this.context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.LOANAPPLICATIONID == applocationId);
            {
                //data.LoanStatusId = (short)entity.approvalStatusId;
                //data.ActedOnaBy = entity.staffId;
                //data.DateActedOn = genSetup.GetApplicaionDate();

                data.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ChecklistCompleted;
            }

            //Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.ApprovalStatusUpdated,
            //    STAFFID = entity.staffId,
            //    BRANCHID = (short)entity.BranchId,
            //    DETAIL =
            //        $"Change Loan Application Status with reference number '{data.APPLICATIONREFERENCENUMBER}' to {GetLoanStatus((short)entity.approvalStatusId)}",
            //    IPADDRESS = entity.userIPAddress,
            //    URL = entity.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.targetId
            //};

            //  this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public int AddLoanApplication(LoanApplicationViewModel loan)
        {
            try
            {
                bool isGroupLoan = false;
                int response = 0; int loanId = 0;
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
                    CASAACCOUNTID = loan.casaAccountId,
                    APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
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

                // response = context.SaveChanges();
                try
                {
                    response = context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                    throw new DbEntityValidationException(errorMessages);
                }
                TBL_LOAN_APPLICATION result;
                if (response > 0) result = data;

                return data.LOANAPPLICATIONID;
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

        #region OfferLetter & Availment Process

        private IQueryable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID
                        where a.COMPANYID == companyId && a.DELETED == false  && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = c.CAMREF,
                            camDocumentation = d.CAMDOCUMENTATION,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID
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

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId)
        {
            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanAvailment);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(companyId).Where(x => x.operationId == (int)OperationsEnum.LoanAvailment).ToList();

            IQueryable<CamProcessedLoanViewModel> data;

            // Check if the current staff is the first level
            if (staffApprovalLevelId == approvalLvlStaff[0].approvalLevelId)
            {
                data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID
                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = c.CAMREF,
                            camDocumentation = d.CAMDOCUMENTATION,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            approvalLevelId = staffApprovalLevelId
                        });
            }
            else
            {
                data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CREDIT_APPRAISAL_MEMORANDUM on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        join d in context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM on c.APPRAISALMEMORANDUMID equals d.APPRAISALMEMORANDUMID
                        join e in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals e.TARGETID into apprTrail
                        from e in apprTrail.DefaultIfEmpty()
                        where a.COMPANYID == companyId && a.DELETED == false
                              && b.STATUSID == (int)ApprovalStatusEnum.Approved &&
                              e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                && e.RESPONSESTAFFID == null
                          && e.OPERATIONID == (int)OperationsEnum.LoanAvailment && e.TOAPPROVALLEVELID == staffApprovalLevelId
                        select new CamProcessedLoanViewModel
                        {
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = a.CUSTOMERID == 3 ? a.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerGroupName = a.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            loanTypeId = a.LOANTYPEID,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = c.CAMREF,
                            camDocumentation = d.CAMDOCUMENTATION,
                            approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.Sum(x => x.APPROVEDAMOUNT),
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatusId = a.APPLICATIONSTATUSID,
                            subSectorId = b.TBL_SUB_SECTOR.SUBSECTORID,
                            approvalLevelId = staffApprovalLevelId,
                            operationId = e.OPERATIONID,
                            currentApprovalStateId = e.APPROVALSTATEID
                        });
            }

            var loanAvailmentData = data.Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.RelationshipManagerOfferLetterReviewCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());

            return loanAvailmentData;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int companyId)
        {
            var data = GetCamProcessedLoanApplications(companyId).Where(x =>
                x.applicationStatusId == (short)LoanApplicationStatusEnum.CAMCompleted || x.applicationStatusId == (short)LoanApplicationStatusEnum.OfferLetterGenerationInProgress)
                .GroupBy(c => c.loanApplicationId).Select(y => y.FirstOrDefault());
            return data;
        }

        public OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var obligorDetails = (from a in context.TBL_LOAN_APPLICATION
                                  join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                  join b in context.TBL_CUSTOMER on d.CUSTOMERID equals b.CUSTOMERID into cc
                                  from b in cc.DefaultIfEmpty()
                                  join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID into cg
                                  from c in cg.DefaultIfEmpty()
                                  where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                  a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  select new OfferLetterViewModel
                                  {
                                      companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                      //customerId = b.CustomerId,
                                      customerName = a.LOANTYPEID != 3 ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerGroupName = c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerAddress = a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                      customerEmailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
                                      customerPhoneNumber = a.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault().PHONENUMBER,
                                      applicationDate = a.APPLICATIONDATE
                                  }).FirstOrDefault();

            var applicant = obligorDetails.customerName;
            var applicantDetails = $"{obligorDetails.customerEmailAddress}. {obligorDetails.customerEmailAddress}. {obligorDetails.customerPhoneNumber}";
            var applicantBeneficiaryDetails = string.Empty;

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower() &&
                                     b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new OfferLetterDetailViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                   currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   loanAmount = b.APPROVEDAMOUNT,
                                   exchangeRate = b.EXCHANGERATE,
                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                               }).ToList();

            var facilityType = loanDetails.FirstOrDefault().loanTypeName;

            var totalLoanAmount = $"{loanDetails.Sum(x => x.baseCurrencyLoanAmount):f}";

            var currency = loanDetails.FirstOrDefault().currencyName;

            var interestRate = loanDetails.Sum(x => x.interestRate);

            var facilityPurpose = "To obtain the loan for the purpose of business expansion";

            var facilityTenor = $"{loanDetails.Sum(x => x.tenor)} days";

            var loanDetailsTable = string.Empty;

            loanDetailsTable = "<table><tr><th>Product</th><th>Loan Amount</th><th>Tenor</th><th>Interest Rate</th></tr>";

            foreach (var item in loanDetails)
            {
                loanDetailsTable = loanDetailsTable +
                    $"<tr><td>{item.productName}</td><td>{item.loanAmount:f}</td><td>{item.tenor:f} days</td><td>{item.interestRate:f}</td></tr>";
            }

            var finalLoanDetails = loanDetailsTable + "</table>";

            var conditionPrecedent = (from a in context.TBL_LOAN_APPLICATION
                                      join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISEXTERNAL == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.LOANAPPLICATIONID
                                      }).ToList();

            var conditions = string.Empty;

            conditions = "<table><tr>Conditions</tr>";

            foreach (var item in conditionPrecedent)
            {
                conditions = conditions +
                    $"<tr><td>{item.conditionPrecident}</td></tr>";
            }

            var finalConditions = conditions + "</table>";

            var preparedTemplate = PopulateOfferLetterPlaceholders(applicant, applicantDetails,
                applicantBeneficiaryDetails, facilityType, totalLoanAmount, facilityPurpose, facilityTenor);

            if (preparedTemplate != null)
            {
                return new OfferLetterTemplateViewModel { documentTemplate = preparedTemplate };
            }

            return new OfferLetterTemplateViewModel { };
        }

        private static string PopulateOfferLetterPlaceholders(string obligorName, string obligorDetails, string obligorBeneficiaryDetails,
            string facilityType, string facilityAmount, string facilityPurpose, string facilityTenor)
        {
            string body;

            string templateLink = "~/EmailTemplates/OfferLetter.html";

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{@LoanApplicant}", obligorName);
            body = body.Replace("{@ApplicantDetails}", obligorDetails);
            body = body.Replace("{@ApplicantBeneficiaryDetails}", obligorBeneficiaryDetails);
            body = body.Replace("{@FacilityType}", facilityType);
            body = body.Replace("{@FacilityAmount}", facilityAmount);
            body = body.Replace("{@FacilityPurpose}", facilityPurpose);
            body = body.Replace("{@FacilityTenor}", facilityTenor);

            return body;
        }

        public bool SaveDraftOfferLetter(OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var exisitingDocument = context.TBL_TEMP_OFFERLETTER.Where(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber).FirstOrDefault();

                    if (exisitingDocument != null)
                    {
                        exisitingDocument.HTML_DOCUMENT = model.documentTemplate;
                        exisitingDocument.APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber;
                        exisitingDocument.COMMENTS = model.comments;
                        exisitingDocument.PRODUCTID = model.productId;
                        exisitingDocument.ISACCEPTED = model.isAccepted;
                    }
                    else
                    {
                        var document = new TBL_TEMP_OFFERLETTER
                        {
                            HTML_DOCUMENT = model.documentTemplate,
                            APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                            COMMENTS = model.comments,
                            PRODUCTID = model.productId,
                            ISACCEPTED = model.isAccepted
                        };

                        context.TBL_TEMP_OFFERLETTER.Add(document);
                    }

                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public bool UpdateDraftOfferLetter(int documentId, OfferLetterTemplateViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var exisitingDocument = context.TBL_TEMP_OFFERLETTER.Find(documentId);

                    exisitingDocument.HTML_DOCUMENT = model.documentTemplate;
                    exisitingDocument.APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber;
                    exisitingDocument.COMMENTS = model.comments;
                    exisitingDocument.PRODUCTID = model.productId;
                    exisitingDocument.ISACCEPTED = model.isAccepted;

                    if (model.isAccepted == true)
                    {
                        return SaveFinalOfferLetter(model);
                    }

                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public IEnumerable<OfferLetterTemplateViewModel> GetAllDraftOfferLetters()
        {
            var data = (from a in context.TBL_TEMP_OFFERLETTER
                        select new OfferLetterTemplateViewModel
                        {
                            documentId = a.DOCUMENTID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            documentTemplate = a.HTML_DOCUMENT,
                            comments = a.COMMENTS,
                            productId = a.PRODUCTID,
                            isAccepted = a.ISACCEPTED
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<OfferLetterTemplateViewModel> { };
        }

        public OfferLetterTemplateViewModel GetDraftOfferLetterByApplRefNumber(string applicationRefNumber)
        {
            var data = GetAllDraftOfferLetters().Where(x => x.applicationReferenceNumber == applicationRefNumber).FirstOrDefault();

            if (data != null)
            {
                return data;
            }

            return new OfferLetterTemplateViewModel { };
        }

        public IEnumerable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters()
        {
            var data = (from a in context.TBL_OFFERLETTER
                        select new OfferLetterTemplateViewModel
                        {
                            documentId = a.DOCUMENTID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            documentTemplate = a.HTML_DOCUMENT,
                            comments = a.COMMENTS,
                            productId = a.PRODUCTID,
                            isAccepted = a.ISACCEPTED
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<OfferLetterTemplateViewModel> { };
        }

        public OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(string applicationRefNumber)
        {
            var data = GetAllFinalOfferLetters().Where(x => x.applicationReferenceNumber == applicationRefNumber).FirstOrDefault();

            if (data != null)
            {
                return data;
            }

            return new OfferLetterTemplateViewModel { };
        }

        public bool SaveFinalOfferLetter(OfferLetterTemplateViewModel model)
        {
            try
            {
                var exisitingDocument = context.TBL_OFFERLETTER.Any(x => x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);

                if (exisitingDocument)
                {
                    return true;
                }

                var document = new TBL_OFFERLETTER
                {
                    HTML_DOCUMENT = model.documentTemplate,
                    APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                    COMMENTS = model.comments,
                    PRODUCTID = model.productId,
                    ISACCEPTED = model.isAccepted
                };

                context.TBL_OFFERLETTER.Add(document);

                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.LoanAvailment;

            entity.externalInitialization = false;

            var levelResult = approvalLevel.GetAllApprovalLevelStaffByStaffId(entity.staffId, entity.companyId, (int)OperationsEnum.LoanAvailment);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var approvalLvlStaff = approvalLevel.GetAllAssignedApprovalLevelStaff(entity.companyId).Where(x => x.operationId == (int)OperationsEnum.LoanAvailment).ToList();

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var targetLoanAppl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x =>
                        x.APPLICATIONREFERENCENUMBER == entity.applicationReferenceNumber);

                    ForwardViewModel forward;

                    //if (staffApprovalLevelId == approvalLvlStaff[0].approvalLevelId)
                    //{
                    //    return LogApplicationForApproval(entity);
                    //}

                    if (entity.amount >= (long)LoanAvailmentApprovalFlowEnum.LevelTwo && entity.amount <= (long)LoanAvailmentApprovalFlowEnum.LevelThree)
                    {

                        if (staffApprovalLevelId != approvalLvlStaff[3].approvalLevelId) // forward only if the approval level Id is not the third level
                        {
                            forward = new ForwardViewModel
                            {
                                createdBy = entity.createdBy,
                                amount = entity.amount,
                                companyId = entity.companyId,
                                receiverLevelId = approvalLvlStaff[3].approvalLevelId,
                                receiverStaffId = approvalLvlStaff[3].staffId,
                                operationId = entity.operationId,
                                applicationId = targetLoanAppl.LOANAPPLICATIONID,
                                forwardAction = entity.approvalStatusId
                            };

                            return ForwardApplicationToNextLevel(forward);
                        }

                        // indicate an end to the process before logging on the trail
                        workFlow.KeepPending = false;
                        workFlow.ForcefullyEndProcess = true;

                        workFlow.LogForApproval(entity);
                    }
                    else if (entity.amount >= (long)LoanAvailmentApprovalFlowEnum.LevelThree)
                    {

                        if (staffApprovalLevelId != approvalLvlStaff[4].approvalLevelId)
                        {
                            forward = new ForwardViewModel
                            {
                                createdBy = entity.createdBy,
                                amount = entity.amount,
                                companyId = entity.companyId,
                                receiverLevelId = approvalLvlStaff[4].approvalLevelId,
                                receiverStaffId = approvalLvlStaff[4].staffId,
                                operationId = entity.operationId,
                                applicationId = targetLoanAppl.LOANAPPLICATIONID,
                                forwardAction = entity.approvalStatusId
                            };

                            return ForwardApplicationToNextLevel(forward);

                        }

                        // indicate an end to the process before logging on the trail
                        workFlow.KeepPending = false;
                        workFlow.ForcefullyEndProcess = true;

                        workFlow.LogForApproval(entity);

                    }
                    else
                    {
                        // indicate an end to the process before logging on the trail
                        workFlow.KeepPending = false;
                        workFlow.ForcefullyEndProcess = true;

                        workFlow.LogForApproval(entity);
                    }

                    var b = workFlow.NextLevelId ?? 0;

                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = UpdateLoanApplicationStatus(entity.applicationReferenceNumber, entity.applicationStatusId);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool ForwardApplicationToNextLevel(ForwardViewModel model)
        {
            workFlow.StaffId = model.createdBy;
            workFlow.OperationId = model.operationId;
            workFlow.TargetId = model.applicationId;
            workFlow.CompanyId = model.companyId;
            workFlow.Vote = model.vote;
            workFlow.ProductClassId = model.productClassId;
            workFlow.ProductId = model.productId;
            workFlow.NextLevelId = model.receiverLevelId;
            workFlow.ToStaffId = model.receiverStaffId;
            workFlow.StatusId = model.forwardAction;
            workFlow.Comment = model.comment;

            workFlow.Amount = model.amount;
            workFlow.InvestmentGrade = model.investmentGrade;
            workFlow.Tenor = model.applicationTenor;
            workFlow.PoliticallyExposed = model.politicallyExposed;

            return workFlow.LogActivity();
        }

        public bool LogApplicationForApproval(LoanAvailmentApprovalViewModel model)
        {
            try
            {
                var target = context.TBL_LOAN_APPLICATION.FirstOrDefault(x =>
                    x.APPLICATIONREFERENCENUMBER == model.applicationReferenceNumber);

                var entity = new ApprovalViewModel
                {
                    staffId = model.createdBy,
                    companyId = model.companyId,
                    approvalStatusId = (int)ApprovalStatusEnum.Pending,
                    targetId = target.LOANAPPLICATIONID,
                    operationId = (int)OperationsEnum.LoanAvailment,
                    comment = model.comment,
                    amount = model.amount,
                    BranchId = model.BranchId,
                    externalInitialization = false
                };

                return workFlow.LogForApproval(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

            return data;
        }

        public IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.LOANAPPLICATIONID == loanApplicationId && a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.ApplicationInProgress
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

            return data.ToList();
        }

        #endregion "Loan Applications Awaiting Checklist"

        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            var applications = context.TBL_LOAN_APPLICATION
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL,
                        a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER,
                        g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
                    .Join(context.TBL_CASA,
                        o => o.c.CUSTOMERID, s => s.CUSTOMERID, (o, s) => new { o, s })
                    .Select(x => new LoanApplicationViewModel
                    {
                        firstName = x.o.c.FIRSTNAME,
                        middleName = x.o.c.MIDDLENAME,
                        lastName = x.o.c.LASTNAME,
                        customerCode = x.o.c.CUSTOMERCODE,
                        loanApplicationId = x.o.g.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.o.g.a.APPLICATIONREFERENCENUMBER,
                        customerId = x.o.g.a.CUSTOMERID,
                        branchId = x.o.g.a.BRANCHID,
                        customerGroupId = x.o.g.a.CUSTOMERGROUPID,
                        loanTypeId = x.o.g.a.LOANTYPEID,
                        relationshipOfficerId = x.o.g.a.RELATIONSHIPOFFICERID,
                        relationshipManagerId = x.o.g.a.RELATIONSHIPMANAGERID,
                        applicationDate = x.o.g.a.APPLICATIONDATE,
                        applicationAmount = x.o.g.a.APPLICATIONAMOUNT,
                        approvedAmount = x.o.g.a.APPROVEDAMOUNT,
                        interestRate = x.o.g.a.INTERESTRATE,
                        applicationTenor = x.o.g.a.APPLICATIONTENOR,
                        loanInformation = x.o.g.a.LOANINFORMATION,
                        submittedForAppraisal = x.o.g.a.SUBMITTEDFORAPPRAISAL,
                        customerInfoValidated = x.o.g.a.CUSTOMERINFOVALIDATED,
                        notInNegativeCrms = x.o.g.a.NOTINNEGATIVECRMS,
                        notInBlackbook = x.o.g.a.NOTINBLACKBOOK,
                        notInCamsol = x.o.g.a.NOTINCAMSOL,
                        isRelatedParty = x.o.g.a.ISRELATEDPARTY,
                        isPoliticallyExposed = x.o.g.a.ISPOLITICALLYEXPOSED,
                        approvalStatusId = x.o.g.a.APPROVALSTATUSID,
                        applicationStatusId = x.o.g.a.APPLICATIONSTATUSID,
                        branchName = x.o.g.a.TBL_BRANCH.BRANCHNAME,
                        relationshipOfficerName = x.o.g.a.TBL_STAFF.FIRSTNAME + " " + x.o.g.a.TBL_STAFF.MIDDLENAME + " " + x.o.g.a.TBL_STAFF.LASTNAME,
                        relationshipManagerName = x.o.g.a.TBL_STAFF1.FIRSTNAME + " " + x.o.g.a.TBL_STAFF1.MIDDLENAME + " " + x.o.g.a.TBL_STAFF1.LASTNAME,
                        misCode = x.o.g.a.MISCODE,
                        customerGroupName = x.o.g.a.CUSTOMERGROUPID.HasValue ? x.o.g.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        loanTypeName = x.o.g.a.TBL_LOAN_TYPE.LOANTYPENAME,
                        createdBy = x.o.g.a.CREATEDBY,
                        loanPreliminaryEvaluationId = x.o.g.a.LOANPRELIMINARYEVALUATIONID,
                        operationId = x.o.g.a.OPERATIONID,
                        accountNumber = x.s.PRODUCTACCOUNTNUMBER,
                    })
                    .Where(x => x.applicationReferenceNumber == searchString
                        || x.accountNumber.ToLower().Contains(searchString.ToLower())
                        || x.firstName.ToLower().Contains(searchString.ToLower())
                        || x.lastName.ToLower().Contains(searchString.ToLower())
                        || x.middleName.ToLower().Contains(searchString.ToLower())
                        || x.customerCode == searchString)
                    ;

            return applications.ToList();
        }
    }
}
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
using System.Linq;
using System.Threading.Tasks;

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
                                 //proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
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

        public IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailById(int loanApplicationDetailId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        where a.TBL_LOAN_APPLICATION.COMPANYID == companyId && a.DELETED == false
                        && a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                        select new jobLoanApplicationDetailViewModel
                        {
                            approvedAmount = a.APPROVEDAMOUNT,
                            approvedInterestRate = a.APPROVEDINTERESTRATE,
                            approvedProductId = a.APPROVEDPRODUCTID,
                            approvedTenor = a.APPROVEDTENOR,
                            currencyId = a.CURRENCYID,
                            currencyName = a.TBL_CURRENCY.CURRENCYNAME,
                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerId = a.CUSTOMERID,
                            exchangeRate = a.EXCHANGERATE,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            subSectorId = a.SUBSECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "/" +  a.TBL_SUB_SECTOR.NAME ,
                            branchName = a.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHNAME,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            proposedAmount = a.PROPOSEDAMOUNT,
                            proposedInterestRate = a.PROPOSEDINTERESTRATE,
                            proposedProductId = a.PROPOSEDPRODUCTID,
                            proposedTenor = a.PROPOSEDTENOR, //Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                            statusId = a.STATUSID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = a.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = a.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            relationshipOfficerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPMANAGERID,
                            invoiceDiscountDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_INV.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new LoanApplicationDetailInvoiceViewModel
                                                     {
                                                         approvalComment = i.APPROVAL_COMMENT,
                                                         invoiceAmount = i.INVOICE_AMOUNT,
                                                         invoiceNo = i.INVOICENO,
                                                         approvaStatusId = i.APPROVALSTATUSID,
                                                         approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                         contractEndDate = i.CONTRACT_ENDDATE,
                                                         contractStartDate = i.CONTRACT_STARTDATE,
                                                         invoiceDate = i.INVOICE_DATE,
                                                         invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                         principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                         principalAccount = i.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                         principalRegNo = i.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                         principalId = i.PRINCIPALID,
                                                     }).ToList(),
                            firstEducationtDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_EDU.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new EducationLoanViewModel
                                                     {
                                                        educationId = i.EDUCATIONID,
                                                        loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                        numberOfStudent = i.NUMBER_OF_STUDENTS,
                                                        averageSchoolFees = i.AVERAGE_SCHOOL_FEES,
                                                        totalPreviousTermSchoolFees = i.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                                        productClassId = context.TBL_PRODUCT_CLASS.Where(x=>x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                                        productClassName = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                                     }).ToList(),
                            firstTradderDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_TRA.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new TraderLoanViewModel
                                                  {
                                                      tradderId = i.TRADDERID,
                                                      marketId = i.MARKETID,
                                                      marketName = i.TBL_LOAN_MARKET.MARKETNAME,
                                                      averageMonthlyTurnover = i.AVERAGE_MONTHLY_TURNOVER,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      //productClassId = i.
                                                }).ToList(),
                            loanCollateral = (from i in context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                              select new CollateralViewModel
                                              {
                                                  allowSharing = i.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                                                  collateralCode = i.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                                                  collateralValue = i.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                  collateralTypeName = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                                  collateralTypeId = i.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                                                  //collateralSubTypeId = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.
                                                 currencyCode = i.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                 valuationCycle = i.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                 haircut = i.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                 customerName = i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.FIRSTNAME + " "+ i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.MIDDLENAME
                                                 +" "+ i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.LASTNAME,
                                              }).ToList(),

                        }).ToList();
            foreach(var i in data)
            {
                var relationshipOfficer = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipOfficerId).FirstOrDefault();
                var relationshipManager = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipManagerId).FirstOrDefault();
                i.relationshipOfficerName = relationshipOfficer.FIRSTNAME + " " + relationshipOfficer.MIDDLENAME + " " + relationshipOfficer.LASTNAME;
                i.relationshipManagerName = relationshipManager.FIRSTNAME + " " + relationshipManager.MIDDLENAME + " " + relationshipManager.LASTNAME;
            }
            return data;
        }

        public IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId)
        {
            var data = from a in context.TBL_LOAN_APPLICATION
                       where a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationInProgress && a.COMPANYID == companyId && a.DELETED == false
                       orderby a.APPLICATIONDATE descending
                       // && a.CREATEDBY == relationshipOfficerId || a.RELATIONSHIPOFFICERID == relationshipOfficerId
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
            ////var loanData = (from l in context.TBL_LOAN_APPLICATION_DETAIL where l.LOANAPPLICATIONID == applocationId select l).ToList();
            ////if (loanData != null)
            ////{
            ////    var custNo = loanData.Count();
            ////    var checkedNo = 0;
            ////    foreach (var item in loanData)
            ////    {
            ////        if (item.HASDONECHECKLIST == true)
            ////        {
            ////            ++checkedNo;
            ////        }
            ////    }
            ////    if (custNo == checkedNo)
            ////    {
            ////        var loanApplication = context.TBL_LOAN_APPLICATION.Find(applocationId);
            ////        if (loanApplication != null)
            ////        {
            ////            loanApplication.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ChecklistCompleted;
            ////        }
            ////    }
            ////}
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
                     
                    PRODUCTCLASSID = loan.productClassId,
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
                    APPLICATIONTENOR = (loan.proposedTenor * 365) / 12,
                    ISINVESTMENTGRADE = loan.isInvestmentGrade,
                    LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                    CUSTOMERID = loan.customerId,
                    SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                    OPERATIONID = (int)OperationsEnum.CAM,
                     
                };

                //if (loan.LoanApplicationCollateral.Count > 0)
                //{
                //    LoanApplicationCollateral(loan.LoanApplicationCollateral);
                //}
                if (loan.LoanApplicationDetail.Count > 0)
                {
                   
                    LoanApplicationDetail(loan.LoanApplicationDetail, loan.createdBy);
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

        private void TradderLoan(TraderLoanViewModel entity, int loanApplicationId, int createdBy)
        {
            var data = new TBL_LOAN_APPLICATION_DETL_TRA()
            {
                AVERAGE_MONTHLY_TURNOVER = entity.averageMonthlyTurnover,
                MARKETID = entity.marketId,
                CREATEDBY = createdBy,
                LOANAPPLICATIONDETAILID = loanApplicationId,
                DATETIMECREATED = DateTime.Now

            };
            context.TBL_LOAN_APPLICATION_DETL_TRA.Add(data);
        }

        private void EducationLoan(EducationLoanViewModel entity, int loanApplicationDetailsId, int createdBy)
        {
            var data =  new TBL_LOAN_APPLICATION_DETL_EDU()
            {
                AVERAGE_SCHOOL_FEES = entity.averageSchoolFees,
                LOANAPPLICATIONDETAILID = loanApplicationDetailsId,
                NUMBER_OF_STUDENTS = entity.numberOfStudent,
                 TOTAL_PREVIOUS_TERM_SCHOL_FEES = entity.schoolFeesCollected,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now
            };
            context.TBL_LOAN_APPLICATION_DETL_EDU.Add(data);

        }

        private void InvoiceDetails(List<InvoiceDetailViewModel> entity, int createdBy)
        {
         var data = entity.Select(c => new TBL_LOAN_APPLICATION_DETL_INV()
            {
                CONTRACT_ENDDATE = c.contractEndDate,
                CONTRACT_STARTDATE = c.contractStartDate,
                INVOICENO = c.invoiceNo,
                INVOICE_AMOUNT = c.invoiceAmount,
                INVOICE_CURRENCYID = c.invoiceCurrencyId,
                INVOICE_DATE = c.invoiceDate,
                LOANAPPLICATIONDETAILID = c.loanApplicationDetailId,
                PRINCIPALID = c.principalId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy
            });
            context.TBL_LOAN_APPLICATION_DETL_INV.AddRange(data);
        }

        private void LoanApplicationDetail(List<LoanApplicationDetailViewModel> entity, int createdBy)
        {
            foreach(var a in entity)
            {

                var data = new TBL_LOAN_APPLICATION_DETAIL
                {
                    APPROVEDAMOUNT = a.proposedAmount,
                    APPROVEDINTERESTRATE = a.proposedInterestRate,
                    APPROVEDPRODUCTID = a.proposedProductId,
                    APPROVEDTENOR = (a.proposedTenor * 365) / 12, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                    EXCHANGERATE = a.exchangeRate,
                    CURRENCYID = a.currencyId,
                    CUSTOMERID = a.customerId,
                    LOANAPPLICATIONID = a.loanApplicationId,
                    STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,

                    PROPOSEDAMOUNT = a.proposedAmount,
                    PROPOSEDINTERESTRATE = a.proposedInterestRate,
                    PROPOSEDPRODUCTID = a.proposedProductId,
                    PROPOSEDTENOR = (a.proposedTenor * 365) / 12, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                    SUBSECTORID = a.subSectorId,
                    CREATEDBY = createdBy,
                    DATETIMECREATED = DateTime.Now,
                    LOANPURPOSE = a.loanPurpose
                };

                context.TBL_LOAN_APPLICATION_DETAIL.Add(data);

                if (a.invoiceDetails.Any() && a.productClassId == 6)
                {
                    
                    InvoiceDetails(a.invoiceDetails,   createdBy);
                }
                if (a.educationLoan != null && a.productClassId == 7)
                {
                    EducationLoan(a.educationLoan, a.loanApplicationDetailId,createdBy);
                }

                if (a.traderLoan != null && a.productClassId == 8)
                {
                    TradderLoan(a.traderLoan, a.loanApplicationDetailId,  createdBy);
                }


            }


        }

        public IEnumerable<LoanApplicationCollateralViewModel> GetLoanApplicationCollateral(int loanApplicatioinCollateralId)
        {
            var data = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == loanApplicatioinCollateralId).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID
            }).OrderByDescending(x => x.loanAppCollateralId);
            return data.ToList();
        }

        public bool AddLoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity)
        {
            var unmapped = new List<TBL_LOAN_APPLICATION_COLLATERL>();
            foreach (var ent in entity)
            {
                var dat = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c =>
                c.COLLATERALCUSTOMERID == ent.collateralCustomerId && c.LOANAPPLICATIONID == ent.loanApplicationId)
                .FirstOrDefault();
                if (dat == null)
                {
                    unmapped.Add(new TBL_LOAN_APPLICATION_COLLATERL
                    {
                        COLLATERALCUSTOMERID = ent.collateralCustomerId,
                        CREATEDBY = ent.createdBy,
                        LOANAPPLICATIONDETAILID = ent.loanApplicationDetailId,
                        LOANAPPLICATIONID = ent.loanApplicationId
                    });
                }
            }


            var data = unmapped.Select(item => new TBL_LOAN_APPLICATION_COLLATERL
            {
                LOANAPPLICATIONDETAILID = item.LOANAPPLICATIONDETAILID,
                COLLATERALCUSTOMERID = item.COLLATERALCUSTOMERID,
                LOANAPPLICATIONID = item.LOANAPPLICATIONID,
                CREATEDBY = item.CREATEDBY,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            });
            context.TBL_LOAN_APPLICATION_COLLATERL.AddRange(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = entity.FirstOrDefault().createdBy,
                BRANCHID = (short)entity.FirstOrDefault().userBranchId,
                DETAIL = $"Added collateral loan application with reference Number: {entity.FirstOrDefault().applicationReferenceNumber}",
                IPADDRESS = entity.FirstOrDefault().userIPAddress,
                URL = entity.FirstOrDefault().applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.FirstOrDefault().loanAppCollateralId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
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

            return applications.Distinct().ToList();
        }

        public dynamic GetLoanApplicationDetailsProductProgram(int loanApplicationDetailId)
        {
            var details = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_APPLICATION
                           on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select b.PRODUCTCLASSID).FirstOrDefault();

            if (details == (short)ProductClassEnum.InvoiceDiscountingFacility)
            {
                var inv = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
                           where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select new InvoiceDetailViewModel()
                           {
                               invoiceId = a.INVOICEID,
                               loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                               principalId = a.PRINCIPALID,
                               principalName = a.TBL_LOAN_PRINCIPAL.NAME,
                               invoiceNo = a.INVOICENO,
                               invoiceDate = a.INVOICE_DATE,
                               invoiceAmount = a.INVOICE_AMOUNT,
                               invoiceCurrencyId = a.INVOICE_CURRENCYID,
                               invoiceCurrencyName = a.TBL_CURRENCY.CURRENCYNAME,
                               contractStartDate = a.CONTRACT_STARTDATE,
                               contractEndDate = a.CONTRACT_ENDDATE,
                               approvalStatusId = a.APPROVALSTATUSID,
                               productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                           }).ToList();
                return inv;
            }
            else if (details == (short)ProductClassEnum.FirstTrader)
            {
                var trader = (from tra in context.TBL_LOAN_APPLICATION_DETL_TRA
                              where tra.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                              select new TraderLoanViewModel()
                              {
                                  traderId = tra.TRADDERID,
                                  loanApplicationDetailId = tra.LOANAPPLICATIONDETAILID,
                                  marketId = tra.MARKETID,
                                  marketName = tra.TBL_LOAN_MARKET.MARKETNAME,
                                  averageMonthlyTurnover = tra.AVERAGE_MONTHLY_TURNOVER,
                                  productClassId = (int)ProductClassEnum.FirstTrader
                              }).ToList();
                return trader;
            }
            else if (details == (short)ProductClassEnum.FirstEdu)
            {
                var edu = (from e in context.TBL_LOAN_APPLICATION_DETL_EDU
                           where e.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select new EducationLoanViewModel()
                           {
                               educationId = e.EDUCATIONID,
                               loanApplicationDetailId = e.LOANAPPLICATIONDETAILID,
                               numberOfStudent = e.NUMBER_OF_STUDENTS,
                               averageSchoolFees = e.AVERAGE_SCHOOL_FEES,
                               totalPreviousTermSchoolFees = e.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                               productClassId = (int)ProductClassEnum.FirstEdu
                           }).ToList();
                return edu;
           }
            return null;
        }


        public ValidateDataViewModel ValidateDocumentDate(ValidateDataViewModel data)
        {

            var dat = context.TBL_PRODUCT.Where(c => c.PRODUCTID == data.productId).FirstOrDefault();
            int days = DateTime.Now.Subtract(data.date).Days;
            return new ValidateDataViewModel
            {
                dayInterval = dat.EXPIRYPERIOD,
                InvoiceStatus = (days >= 0 && dat.EXPIRYPERIOD >= days) ? true : false,
            };

        }

      

    }
}
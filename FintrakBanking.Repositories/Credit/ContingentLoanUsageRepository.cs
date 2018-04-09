using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using System.Data.Entity;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Repositories.Credit
{
    public class ContingentLoanUsageRepository : IContingentLoanUsageRepository
    {
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IWorkflow workflow;
        private IAuditTrailRepository auditTrail;
        public ContingentLoanUsageRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup, IAuditTrailRepository auditTrail, IWorkflow workflow)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
        }

        public IEnumerable<ContingentLoansViewModel> GetAllContingentLoans(int staffId, int companyId)
        {
            try
            {
                List<ContingentLoansViewModel> contingentData = new List<ContingentLoansViewModel>();
                DateTime currentDate = genSetup.GetApplicationDate();
                var data = context.TBL_LOAN_CONTINGENT
                    .Where(c => c.MATURITYDATE <= currentDate)
                    .Select(c => new ContingentLoansViewModel()
                    {
                        principalName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                        bookingDate = c.BOOKINGDATE,
                        casaAccountNumber = c.TBL_CASA.PRODUCTACCOUNTNUMBER,
                        facilityAmount = c.CONTINGENTAMOUNT,
                        contingentLoanId = c.CONTINGENTLOANID,
                        currencyCode = c.TBL_CURRENCY.CURRENCYCODE,
                        currencyId = c.CURRENCYID,
                        customerId = c.CUSTOMERID,
                        firstName = c.TBL_CUSTOMER.FIRSTNAME,
                        lastName = c.TBL_CUSTOMER.LASTNAME,
                        middleName = c.TBL_CUSTOMER.MIDDLENAME,
                        productId = c.PRODUCTID,
                        effectiveDate = c.EFFECTIVEDATE,
                        exchangeRate = c.EXCHANGERATE,
                        //  requestedAmount = ,
                        loanApplicationReferenceNumber = c.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                        loanReferenceNumber = c.LOANREFERENCENUMBER,
                        maturityDate = c.MATURITYDATE,
                        productName = c.TBL_PRODUCT.PRODUCTNAME,
                        loanStatus = c.TBL_LOAN_STATUS.ACCOUNTSTATUS
                    });

                foreach (var item in data)
                {
                    var usedData = context.TBL_LOAN_CONTINGENT_USAGE.Where(d => d.CONTINGENTLOANID == item.contingentLoanId);
                    if (usedData.Any())
                    {
                        item.usedAmount = usedData.Sum(c => c.AMOUNTREQUESTED);
                    }
                    contingentData.Add(item);

                }


                return contingentData.Where(c => c.percentageUsed < 100).OrderByDescending(d => d.bookingDate).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool SaveContigentLoans(ContingentLoanUsageViewModel entity, int companyId)
        {
            try
            {
                var data = new TBL_LOAN_CONTINGENT_USAGE
                {
                    AMOUNTREQUESTED = entity.amountRequuested,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    CONTINGENTLOANID = entity.contingentLoanId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    DELETED = false,
                };
                context.TBL_LOAN_CONTINGENT_USAGE.Add(data);


                // ----------------Drop into CAM-------------------
                workflow.StaffId = entity.staffId;
                workflow.OperationId = (int)OperationsEnum.ContingentLiabilityUsage;
                workflow.TargetId = data.CONTINGENTLOANID;
                workflow.CompanyId = companyId;
             //   workflow.ProductClassId = context.TBL_PRODUCT.Where(c => c.PRODUCTID == entity.productId).FirstOrDefault().PRODUCTCLASSID;
                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                workflow.Comment = "New APS";
                workflow.ExternalInitialization = true;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                // --

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ContingentLoanUsageAdd,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Applied for APS for {entity.productName} with reference number: {entity.loanReferenceNumber}",
                    IPADDRESS = entity.userIPAddress,
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = data.CONTINGENTLOANUSAGEID
                };

                this.auditTrail.AddAuditTrail(audit);

                //--------------------------------------------------
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<ContingentLoansViewModel> GetPendingRequest(int staffId,int branchId)
        {
            try
            {
               return GetRequestWaitingApprovalByOperation((int)OperationsEnum.ContingentLiabilityUsage,  branchId, staffId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public IQueryable<ContingentLoansViewModel> GetRequestWaitingApprovalByOperation(int operationId,  int branchId, int staffId)
        {
            bool isHeadOffice = (branchId == 1) ? true : false;

            var staffApprovalLevelIds =
                context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId  )
                .Select(x => x.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL).Where(l => l.ISACTIVE == true)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF.Where(s => s.STAFFID == staffId))
                .Select(x => x.APPROVALLEVELID)
                .ToList();

            var applications = context.TBL_LOAN_CONTINGENT_USAGE
                .Where(x =>
                (isHeadOffice || x.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.BRANCHID == branchId)
                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved // <-------------------------------------hard codes!!!
                  
            //  && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BondAndGuaranteesInProgress // <--------hard codes!!!
            )
            .Join(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                && staffApprovalLevelIds.Contains((int)x.TOAPPROVALLEVELID) && x.RESPONSESTAFFID == null
                ),
                a => a.CONTINGENTLOANUSAGEID,
                b => b.TARGETID,
                (a, b) => new { a, b })
                .Select(x => new ContingentLoansViewModel()
                {
                    principalName = x.a.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                    bookingDate = x.a.TBL_LOAN_CONTINGENT.BOOKINGDATE,
                    casaAccountNumber = x.a.TBL_LOAN_CONTINGENT.TBL_CASA.PRODUCTACCOUNTNUMBER,
                    facilityAmount = x.a.TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT,
                    contingentLoanId = x.a.TBL_LOAN_CONTINGENT.CONTINGENTLOANID,
                    currencyCode = x.a.TBL_LOAN_CONTINGENT.TBL_CURRENCY.CURRENCYCODE,
                    currencyId = x.a.TBL_LOAN_CONTINGENT.CURRENCYID,
                    customerId = x.a.TBL_LOAN_CONTINGENT.CUSTOMERID,
                    firstName = x.a.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.FIRSTNAME,
                    lastName = x.a.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.LASTNAME,
                    middleName = x.a.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.MIDDLENAME,
                    productId = x.a.TBL_LOAN_CONTINGENT.PRODUCTID,
                    effectiveDate = x.a.TBL_LOAN_CONTINGENT.EFFECTIVEDATE,
                    exchangeRate = x.a.TBL_LOAN_CONTINGENT.EXCHANGERATE,
                    //  requestedAmount = ,
                    loanApplicationReferenceNumber = x.a.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    loanReferenceNumber = x.a.TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                    maturityDate = x.a.TBL_LOAN_CONTINGENT.MATURITYDATE,
                    productName = x.a.TBL_LOAN_CONTINGENT.TBL_PRODUCT.PRODUCTNAME,
                    loanStatus = x.a.TBL_LOAN_CONTINGENT.TBL_LOAN_STATUS.ACCOUNTSTATUS
                });
            return applications;
        }



    }
}

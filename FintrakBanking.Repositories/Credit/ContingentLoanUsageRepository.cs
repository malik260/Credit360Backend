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
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Interfaces.CASA;

namespace FintrakBanking.Repositories.Credit
{
    public class ContingentLoanUsageRepository : IContingentLoanUsageRepository
    {
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IWorkflow workflow;
        private IAuditTrailRepository auditTrail;
        private ICasaLienRepository casaLien;
        public ContingentLoanUsageRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup, IAuditTrailRepository auditTrail, IWorkflow workflow, ICasaLienRepository casaLien)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.casaLien = casaLien;
        }

        public IEnumerable<ContingentLoansViewModel> GetAllContingentLoans(int staffId, int companyId)
        {
            try
            {
                List<ContingentLoansViewModel> contingentData = new List<ContingentLoansViewModel>();
                DateTime currentDate = genSetup.GetApplicationDate();
                var data = context.TBL_LOAN_CONTINGENT
                    .Where(c => c.MATURITYDATE <= currentDate
                    && context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == c.PRODUCTID)
                    .FirstOrDefault().ALLOWFUNDUSAGE == true)
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


                return contingentData.Where(c => c.percentageUsed < 100).OrderByDescending(d => d.contingentLoanId).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private bool LogForApproval(ApproveAPSRequestViewModel entity)
        {
            bool response = false;
            workflow.StaffId = entity.staffId;
            workflow.OperationId = entity.operationId;
            workflow.TargetId = entity.targetId;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = entity.approvalStatusId;
            workflow.Comment = entity.comment;
            workflow.ExternalInitialization = entity.externalInitialization;
            workflow.DeferredExecution = entity.deferredExecution;
            return response = workflow.LogActivity();
        }

        public bool SaveContigentLoans(ContingentLoanUsageViewModel entity, int companyId)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{

            //    try
            //    {
            var data = new TBL_LOAN_CONTINGENT_USAGE
            {
                AMOUNTREQUESTED = entity.amountRequuested,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                CONTINGENTLOANID = entity.contingentLoanId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                REMARK = entity.remark
            };
            context.TBL_LOAN_CONTINGENT_USAGE.Add(data);




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
            bool response = false;
            //--------------------------------------------------
            response = context.SaveChanges() > 0;
            if (response)
            {

                // ----------------Drop into CAM-------------------
                if (data.CONTINGENTLOANUSAGEID > 0)
                {
                    var log = new ApproveAPSRequestViewModel
                    {
                        staffId = entity.staffId,
                        operationId = (int)OperationsEnum.ContingentLiabilityUsage,
                        targetId = data.CONTINGENTLOANUSAGEID,
                        companyId = entity.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        comment = "New APS Request",
                        deferredExecution = false,
                    };

                    response = LogForApproval(log);

                }
            }
            return response;
        //}
        //catch (Exception ex)
        //{
        //    trans.Rollback();
        //    throw new Exception(ex.Message);
        //}
    } 
            
      

        public IEnumerable<ContingentLoansViewModel> GetPendingRequest(int staffId)
        {
            try
            {
                return GetRequestWaitingApprovalByOperation(staffId).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IQueryable<ContingentLoansViewModel> GetRequestWaitingApprovalByOperation( int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ContingentLiabilityUsage).ToList();


            var applications = from lcu in context.TBL_LOAN_CONTINGENT_USAGE
                               join atrail in context.TBL_APPROVAL_TRAIL on lcu.CONTINGENTLOANUSAGEID equals atrail.TARGETID
                               where atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                                     && atrail.OPERATIONID == (int)OperationsEnum.ContingentLiabilityUsage
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
                               orderby lcu.CONTINGENTLOANUSAGEID descending

                               select new ContingentLoansViewModel()
                               {
                                   principalName = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                                   bookingDate = lcu.TBL_LOAN_CONTINGENT.BOOKINGDATE,
                                   casaAccountNumber = lcu.TBL_LOAN_CONTINGENT.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   facilityAmount = lcu.TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT,
                                   contingentLoanId = lcu.TBL_LOAN_CONTINGENT.CONTINGENTLOANID,
                                   currencyCode = lcu.TBL_LOAN_CONTINGENT.TBL_CURRENCY.CURRENCYCODE,
                                   currencyId = lcu.TBL_LOAN_CONTINGENT.CURRENCYID,
                                   customerId = lcu.TBL_LOAN_CONTINGENT.CUSTOMERID,
                                   firstName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.FIRSTNAME,
                                   lastName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.LASTNAME,
                                   middleName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.MIDDLENAME,
                                   productId = lcu.TBL_LOAN_CONTINGENT.PRODUCTID,
                                   effectiveDate = lcu.TBL_LOAN_CONTINGENT.EFFECTIVEDATE,
                                   exchangeRate = lcu.TBL_LOAN_CONTINGENT.EXCHANGERATE,
                                   loanApplicationReferenceNumber = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   loanReferenceNumber = lcu.TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                                   maturityDate = lcu.TBL_LOAN_CONTINGENT.MATURITYDATE,
                                   productName = lcu.TBL_LOAN_CONTINGENT.TBL_PRODUCT.PRODUCTNAME,
                                   loanStatus = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_STATUS.ACCOUNTSTATUS
                               };
            return applications;
        }

        private bool ApproveAPSRequest(ApproveAPSRequestViewModel entity)
        {
           
            var contingentLoanRecord = context.TBL_LOAN_CONTINGENT_USAGE.Where(d => d.CONTINGENTLOANUSAGEID == entity.contingenliabilityUsageId);

            var log = new ApproveAPSRequestViewModel
            {
                staffId = entity.staffId,
                operationId = (int)OperationsEnum.ContingentLiabilityUsage,
                targetId = entity.contingenliabilityUsageId,
                companyId = entity.companyId,                 
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                comment =entity.comment,               
                deferredExecution = true
            };

            LogForApproval(log);
            


            if (workflow.NewState == (int)ApprovalState.Ended)
            {                
                decimal newLienAmount = 0;

                string lienReferenceNumber = string.Empty;

                if (contingentLoanRecord.Count() == 0)
                {
                      lienReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER;
                }
                else
                {
                    //lienReferenceNumber =  contingentLoanRecord.OrderByDescending(c=> c.CONTINGENTLOANUSAGEID).FirstOrDefault().LIENREFERENCENUMBER;
                }

                decimal oldLien = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT;

                var casaAccountId =   contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.CASAACCOUNTID;
       
                var lienModel = new CasaLienViewModel
                {
                    productAccountNumber = context.TBL_CASA.FirstOrDefault(c=> c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                    sourceReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                    userBranchId = (short)entity.BranchId,
                    branchId = (short)entity.BranchId,
                    companyId = entity.companyId,
                    lienAmount = oldLien,
                    description = "Release Lien for APS Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                };

                casaLien.ReleaseLien(lienModel);


                lienReferenceNumber = string.Concat( lienReferenceNumber, contingentLoanRecord.Count());
                newLienAmount = oldLien - contingentLoanRecord.FirstOrDefault().AMOUNTREQUESTED;
                var lienModel2 = new CasaLienViewModel
                {
                    productAccountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                    sourceReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                    userBranchId = (short)entity.BranchId,
                    branchId = (short)entity.BranchId,
                    companyId = entity.companyId,
                    lienAmount = newLienAmount,
                    description = "Place Lien on APG Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                };

                casaLien.PlaceLien(lienModel);
            }
                   

            return this.context.SaveChanges() > 0;
        }


    }
}

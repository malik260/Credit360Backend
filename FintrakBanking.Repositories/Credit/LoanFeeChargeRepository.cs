using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanFeeChargeRepository : ILoanFeeChargeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private IAdminRepository admin;
        private IFinanceTransactionRepository financeTransaction;
        private IFinanceTransactionRepository transRepo;
        bool USE_THIRD_PARTY_INTEGRATION = false;
        private ITwoFactorAuthIntegrationService twoFactoeAuth;
        private CreditCommonRepository creditCommon;

        private List<int> camOperationIds = new List<int> { 46, 71, 79 }; // RMU(71), CAM(79)

        private readonly int classifiedAssetManagementRoleId = 46;

        public LoanFeeChargeRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository audit,
            IWorkflow workflow,
            IAdminRepository admin,
            IFinanceTransactionRepository _financeTransaction,
            IFinanceTransactionRepository _transRepo,
             ITwoFactorAuthIntegrationService _twoFactoeAuth,
        CreditCommonRepository creditCommon
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.admin = admin;
            this.creditCommon = creditCommon;
            this.financeTransaction = _financeTransaction;
            this.transRepo = _transRepo;
            this.twoFactoeAuth = _twoFactoeAuth;
            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();

            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;

        }

        public string SubmitTakeFee(LoanFeeChargesViewModel model)
        {
            int staffId = model.createdBy;
            var applicationDate = general.GetApplicationDate();

 
            List<int> customerIds = new List<int>();
            LoanViewModel loan = new LoanViewModel();
            TBL_LOAN_FEE feeCharge = new TBL_LOAN_FEE();

            foreach (var detail in model.feeDetails)
            {
                int tenor = detail.loanSystemTypeId == 4 ? loan.tenorUsed : loan.tenor;

                feeCharge = context.TBL_LOAN_FEE.Add(new TBL_LOAN_FEE
                {
                    LOANID = detail.loanId,
                    LOANSYSTEMTYPEID = detail.loanSystemTypeId,/*Term/Disbursed Facility..Overdraft Facility..Contingent Liability*/
                    CHARGEFEEID = detail.chargeFeeId, // refactor to operationId from ui!
                    ISPOSTED = false,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,// REMOVE DUPLICATE [STATUSID]
                    FEERATEVALUE = detail.feeRate,
                    FEEDEPENDENTAMOUNT = 0,
                    FEEAMOUNT = detail.feeAmount,
                    EARNEDFEEAMOUNT = 0,
                    TAXAMOUNT = 0,
                    EARNEDTAXAMOUNT = 0,
                    ISINTEGRALFEE = false,
                    ISRECURRING = false,
                    RECURRINGPAYMENTDAY = 0,
                    DESCRIPTION = detail.description,
                    ISMANUAL = true,
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now,
                    CASAACCOUNTID = detail.casaAccount,
                    
                });
                if (context.SaveChanges() == 0) throw new SecureException("An error occured while saving the data!"); // this save is necessary to grab targetid
                audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanChargeFee,
                    STAFFID = model.createdBy,
                    BRANCHID = model.userBranchId,
                    DETAIL = $"Added to tbl_Loan_Fee '{ feeCharge.LOANCHARGEFEEID}' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                workflow.TargetId = feeCharge.LOANCHARGEFEEID; // model.loanReviewOperationsId;
                workflow.Comment = "Take Fee";
                workflow.OperationId = (int)OperationsEnum.ManualFeeChargeCollectionApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                var response = workflow.LogActivity();

               

            }
            var result = context.SaveChanges() > 0;
            if (result)
            {
                return "Successful!! Fee(s) Has Been Sent For Approval ";
            }
            else
            {
                throw new SecureException("An error occured while saving the data!");
            }

        }
        public IEnumerable<LoanReviewOperationApprovalViewModel> GetTakeFeeAwaitingApproval(int staffId, int companyId)
        {
            var activities = admin.GetUserActivitiesByUser(staffId);
            var defaultCurrencyId = context.TBL_COMPANY.Where(x => x.CURRENCYID == companyId).Select(x => x).FirstOrDefault().CURRENCYID;
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ManualFeeChargeCollectionApproval).ToList();
            var dataLoan = (from op in context.TBL_LOAN_FEE
                            join ln in context.TBL_LOAN on op.LOANID equals ln.TERMLOANID
                            join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                            join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                            join atrail in context.TBL_APPROVAL_TRAIL on op.LOANCHARGEFEEID equals atrail.TARGETID
                            join cu in context.TBL_CUSTOMER on ld.CUSTOMERID equals cu.CUSTOMERID
                            join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                            join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                            join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                            join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                            join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID into x
                            from ch in x.DefaultIfEmpty()
                            where 
                            (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing 
                            || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                            && atrail.OPERATIONID == (int)OperationsEnum.ManualFeeChargeCollectionApproval
                            && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                            && op.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                            && atrail.RESPONSESTAFFID == null 
                            && op.ISPOSTED == false
                            orderby op.DATETIMECREATED descending

                            select new LoanReviewOperationApprovalViewModel
                            {
                                divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == c.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                                divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cu.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                chargeFeeName = context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == op.CHARGEFEEID).Select(a => a.CHARGEFEENAME).FirstOrDefault(),
                                description = op.DESCRIPTION,
                                chargeFeeId = op.CHARGEFEEID,
                                feeAmount = op.FEEAMOUNT,
                                loanChargeFeeId=op.LOANCHARGEFEEID,
                                takeFeeCasaAccountId = op.CASAACCOUNTID,
                                takeFeeCasaAccountName = op.CASAACCOUNTID < 0 ? "n/a" : context.TBL_CASA.Where(x => x.CASAACCOUNTID == op.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER + "("+ x.PRODUCTACCOUNTNAME + "-" + x.TBL_CURRENCY.CURRENCYNAME+")").FirstOrDefault(),
                                currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                loanId = ln.TERMLOANID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                casaAccountId = ln.CASAACCOUNTID,
                                casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                misCode = ln.MISCODE,
                                teamMiscode = ln.TEAMMISCODE,
                                interestRate = ln.INTERESTRATE,
                                effectiveDate = ln.EFFECTIVEDATE,
                                maturityDate = ln.MATURITYDATE,
                                bookingDate = ln.BOOKINGDATE,
                                principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                                principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                approvalStatusId = op.APPROVALSTATUSID,
                                approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                approvedBy = (int)ln.APPROVEDBY,
                                approverComment = ln.APPROVERCOMMENT,
                                dateApproved = ln.DATEAPPROVED,
                                loanStatusId = ln.LOANSTATUSID,
                                scheduleTypeId = ln.SCHEDULETYPEID,
                                isDisbursed = ln.ISDISBURSED,
                                disbursedBy = (int)ln.DISBURSEDBY,
                                disburserComment = ln.DISBURSERCOMMENT,
                                disburseDate = ln.DISBURSEDATE,
                                customerGroupId = lp.CUSTOMERGROUPID,
                                operationId = ln.OPERATIONID,
                                loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                equityContribution = ln.EQUITYCONTRIBUTION,
                                subSectorId = ln.SUBSECTORID,
                                subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                fixedPrincipal = ln.FIXEDPRINCIPAL,
                                profileLoan = ln.PROFILELOAN,
                                dischargeLetter = ln.DISCHARGELETTER,
                                suspendInterest = ln.SUSPENDINTEREST,
                                scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                customerCode = cu.CUSTOMERCODE,
                                productAccountNumber = ch.ACCOUNTCODE,
                                productAccountName = ch.ACCOUNTNAME,
                                loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                currencyId = ln.CURRENCYID,
                                branchName = "",
                                relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                productName = pr.PRODUCTNAME,
                                comment = "",
                                approvedAmount = ld.APPROVEDAMOUNT,
                                creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                //lmsLoanReferenceNumber = context.TBL_LMSR_APPLICATION.Where(x => x.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(a => a.LOANID).FirstOrDefault() == ln.TERMLOANID).Select(x => x.APPLICATIONREFERENCENUMBER).FirstOrDefault(),
                                // lmsLoanReferenceNumber = mp.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER,
                                dateTimeCreated = op.DATETIMECREATED
                            }).ToList();

            var dataRevolvingLoan = (from op in context.TBL_LOAN_FEE 
                                     join ln in context.TBL_LOAN_REVOLVING on op.LOANID equals ln.REVOLVINGLOANID
                                     join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                     join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                     join atrail in context.TBL_APPROVAL_TRAIL on op.LOANCHARGEFEEID equals atrail.TARGETID
                                     join cu in context.TBL_CUSTOMER on ld.CUSTOMERID equals cu.CUSTOMERID
                                     join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID into x
                                     from ch in x.DefaultIfEmpty()
                                     where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                                        && atrail.OPERATIONID == (int)OperationsEnum.ManualFeeChargeCollectionApproval
                                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                        && atrail.RESPONSESTAFFID == null && op.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                        && op.ISPOSTED == false

                                     orderby op.DATETIMECREATED descending
                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         chargeFeeName = context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == op.CHARGEFEEID).Select(a => a.CHARGEFEENAME).FirstOrDefault(),
                                         description = op.DESCRIPTION,
                                         chargeFeeId = op.CHARGEFEEID,
                                         feeAmount = op.FEEAMOUNT,
                                         loanChargeFeeId = op.LOANCHARGEFEEID,
                                         takeFeeCasaAccountId = op.CASAACCOUNTID,
                                         takeFeeCasaAccountName = op.CASAACCOUNTID < 0 ? "n/a" : context.TBL_CASA.Where(x => x.CASAACCOUNTID == op.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER + "(" + x.PRODUCTACCOUNTNAME + "-" + x.TBL_CURRENCY.CURRENCYNAME + ")").FirstOrDefault(),
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.REVOLVINGLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,
                                         teamMiscode = ln.TEAMMISCODE,
                                         interestRate = ln.INTERESTRATE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvalStatusId = op.APPROVALSTATUSID,
                                         approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         customerGroupId = lp.CUSTOMERGROUPID,
                                         operationId = ln.OPERATIONID,
                                         loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         suspendInterest = ln.SUSPENDINTEREST,
                                         customerCode = cu.CUSTOMERCODE,
                                         loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = "",
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                         dateTimeCreated = op.DATETIMECREATED,
                                         currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                         productAccountNumber = ch.ACCOUNTCODE,
                                         productAccountName = ch.ACCOUNTNAME,
                                         approvedAmount = ld.APPROVEDAMOUNT,
                                         creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                     }).ToList();

            var dataContingentLoan = (from op in context.TBL_LOAN_FEE
                                      join ln in context.TBL_LOAN_CONTINGENT on op.LOANID equals ln.CONTINGENTLOANID
                                      join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                      join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                      join atrail in context.TBL_APPROVAL_TRAIL on op.LOANCHARGEFEEID equals atrail.TARGETID
                                      join cu in context.TBL_CUSTOMER on ld.CUSTOMERID equals cu.CUSTOMERID
                                      join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                      join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                      join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                      join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                      join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID into x
                                      from ch in x.DefaultIfEmpty()

                                      where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                                         && atrail.OPERATIONID == (int)OperationsEnum.ManualFeeChargeCollectionApproval
                                         && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                         && atrail.RESPONSESTAFFID == null && op.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                                         && op.ISPOSTED == false

                                      orderby op.DATETIMECREATED descending
                                      select new LoanReviewOperationApprovalViewModel
                                      {
                                          chargeFeeName = context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == op.CHARGEFEEID).Select(a => a.CHARGEFEENAME).FirstOrDefault(),
                                          description = op.DESCRIPTION,
                                          chargeFeeId = op.CHARGEFEEID,
                                          feeAmount = op.FEEAMOUNT,
                                          loanChargeFeeId = op.LOANCHARGEFEEID,
                                          takeFeeCasaAccountId = op.CASAACCOUNTID,
                                          takeFeeCasaAccountName = op.CASAACCOUNTID < 0 ? "n/a" : context.TBL_CASA.Where(x => x.CASAACCOUNTID == op.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER + "(" + x.PRODUCTACCOUNTNAME + "-" + x.TBL_CURRENCY.CURRENCYNAME + ")").FirstOrDefault(),


                                          loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                          loanId = ln.CONTINGENTLOANID,
                                          customerId = ln.CUSTOMERID,
                                          productId = ln.PRODUCTID,
                                          productTypeId = pr.PRODUCTTYPEID,
                                          productTypeName = context.TBL_PRODUCT_TYPE.Where(x => x.PRODUCTTYPEID == pr.PRODUCTTYPEID).Select(m => m.PRODUCTTYPENAME).FirstOrDefault(),
                                          casaAccountId = ln.CASAACCOUNTID,
                                          casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                          branchId = ln.BRANCHID,
                                          loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                          applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                          relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                          relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                          misCode = ln.MISCODE,
                                          teamMiscode = ln.TEAMMISCODE,
                                          principalAmount = ln.CONTINGENTAMOUNT,
                                          //interestRate = ln.INTERESTRATE,
                                          effectiveDate = ln.EFFECTIVEDATE,
                                          maturityDate = ln.MATURITYDATE,
                                          bookingDate = ln.BOOKINGDATE,
                                          approvalStatusId = op.APPROVALSTATUSID,
                                          approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                          approvedBy = (int)ln.APPROVEDBY,
                                          approverComment = ln.APPROVERCOMMENT,
                                          dateApproved = ln.DATEAPPROVED,
                                          loanStatusId = ln.LOANSTATUSID,
                                          isDisbursed = ln.ISDISBURSED,
                                          disburserComment = ln.DISBURSERCOMMENT,
                                          disburseDate = ln.DISBURSEDATE,
                                          customerGroupId = lp.CUSTOMERGROUPID,
                                          operationId = ln.OPERATIONID,
                                          loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                          subSectorId = ln.SUBSECTORID,
                                          subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                          sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                          dischargeLetter = ln.DISCHARGELETTER,
                                          //suspendInterest = ln.SUSPENDINTEREST,
                                          customerCode = cu.CUSTOMERCODE,
                                          loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                          customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                          currencyId = ln.CURRENCYID,
                                          currency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == ln.CURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                                          branchName = "",
                                          relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                          relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                          productName = pr.PRODUCTNAME,
                                          comment = "",
                                          
                                          //lmsLoanReferenceNumber = context.TBL_LMSR_APPLICATION.Where(x => x.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(a => a.LOANID).FirstOrDefault() == ln.CONTINGENTLOANID).Select(x => x.APPLICATIONREFERENCENUMBER).FirstOrDefault(),
                                          dateTimeCreated = op.DATETIMECREATED,


                                          currentApprovalLevelId = (int)atrail.TOAPPROVALLEVELID,
                                          productAccountNumber = ch.ACCOUNTCODE,
                                          productAccountName = ch.ACCOUNTNAME,
                                          approvedAmount = ld.APPROVEDAMOUNT,
                                          creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),

                                      }).ToList();


            var termLoanData = dataLoan.OrderByDescending(x => x.dateTimeCreated).ToList();
            var revolvingLoanData = dataRevolvingLoan.OrderByDescending(x => x.dateTimeCreated).ToList();
            var contingentLoanData = dataContingentLoan.OrderByDescending(x => x.dateTimeCreated).ToList();
            var unionAll = termLoanData.Union(revolvingLoanData);

            var data = unionAll.Union(contingentLoanData);
            List<LoanReviewOperationApprovalViewModel> lcyLoans = new List<LoanReviewOperationApprovalViewModel>();
            List<LoanReviewOperationApprovalViewModel> fcyLoans = new List<LoanReviewOperationApprovalViewModel>();

            var isLCYUser = activities.Contains("lcy-user");
            var isFCYUser = activities.Contains("fcy-user");

            if (isLCYUser == true)
            {
                lcyLoans = data.Where(x => x.currencyId == defaultCurrencyId && x.productTypeId != (short)LoanProductTypeEnum.CommercialLoan).Select(x => x).ToList();
                //data = data.Where(x => x.currencyId == company.CURRENCYID).Select(x => x);
            }

            if (isFCYUser == true)
            {
                fcyLoans = data.Where(x => x.currencyId != defaultCurrencyId || x.productTypeId == (short)LoanProductTypeEnum.CommercialLoan).Select(x => x).ToList();

            }

            data = lcyLoans.Union(fcyLoans).ToList();
            return data;
        }

        public List<FinanceTransactionViewModel> BuildLoanManualChargeFeesPosting(int loanChargeFeeId)
        {
            var loanFee = context.TBL_LOAN_FEE.Find(loanChargeFeeId);
            
            LoanViewModel loanDetails = new LoanViewModel();
            if (loanFee.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                loanDetails = (from a in context.TBL_LOAN
                               where a.TERMLOANID == loanFee.LOANID
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   companyId = a.COMPANYID,
                                   currencyId=a.CURRENCYID,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   branchId =a.BRANCHID,
                                   createdBy = loanFee.CREATEDBY
                               }).FirstOrDefault();//context.TBL_LOAN.Find(loanFee.LOANID);

            }
            if (loanFee.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                 loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                where a.CONTINGENTLOANID == loanFee.LOANID
                                select new LoanViewModel
                                {
                                    loanId = a.CONTINGENTLOANID,
                                    companyId = a.COMPANYID,
                                    currencyId = a.CURRENCYID,
                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                    branchId = a.BRANCHID,
                                    createdBy = loanFee.CREATEDBY

                                }).FirstOrDefault();//context.TBL_LOAN_CONTINGENT.Find(loanFee.LOANID);

            }
            if (loanFee.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                 loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                where a.REVOLVINGLOANID == loanFee.LOANID
                                select new LoanViewModel
                                {
                                    loanId = a.REVOLVINGLOANID,
                                    companyId = a.COMPANYID,
                                    currencyId = a.CURRENCYID,
                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                    branchId = a.BRANCHID,
                                    createdBy = loanFee.CREATEDBY
                                }).FirstOrDefault();//context.TBL_LOAN_REVOLVING.Find(loanFee.LOANID);

            }
            if (loanFee.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.LineFacility)
            {
                 loanDetails = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                                join b in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                where a.LOANAPPLICATIONDETAILID == loanFee.LOANID
                                select new LoanViewModel
                                {
                                    loanId = a.LOANAPPLICATIONDETAILID,
                                    companyId = b.COMPANYID,
                                    currencyId = a.CURRENCYID,
                                    loanReferenceNumber = b.APPLICATIONREFERENCENUMBER + "-" + a.LOANAPPLICATIONDETAILID,
                                    branchId = b.BRANCHID,
                                    createdBy = loanFee.CREATEDBY
                                }).FirstOrDefault();//context.TBL_LOAN_APPLICATION_DETAIL.Find(loanFee.LOANID);

            }

            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            TBL_LOAN loanTable = new TBL_LOAN();
            //var bookingRequestDetails = context.TBL_LOAN_BOOKING_REQUEST.FirstOrDefault(x => x.LOAN_BOOKING_REQUESTID == loanDetails.loanBookingRequestId);

            var company = context.TBL_COMPANY.Find(loanDetails.companyId);
          //  foreach (var item in loanDetails.loanChargeFee)
           // {
                if (loanFee.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility)
                {
                    loanTable = context.TBL_LOAN.Find(loanFee.LOANID);
                }

                if (loanFee.ISPOSTED == false && loanFee.FEEAMOUNT > 0)
                {

                    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanFee.CASAACCOUNTID);

                    var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == loanFee.CHARGEFEEID select details.POSTINGGROUP).Distinct().ToList();

                    foreach (var post in postingGroups)
                    {
                        var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == loanFee.CHARGEFEEID && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                        {
                            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                            decimal debitAmount = 0;
                            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                debitAmount = (decimal)loanFee.FEEAMOUNT * (decimal)(debits.VALUE / 100.0);
                            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                debitAmount = (decimal)debits.VALUE;

                            debit.operationId = (int)OperationsEnum.ManualFeeCharge;
                            debit.description = $"Fee charge on {debits.DESCRIPTION}";
                            debit.valueDate = general.GetApplicationDate();
                            debit.transactionDate = debit.valueDate;
                            debit.currencyId = casa.CURRENCYID;
                            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, loanDetails.companyId).sellingRate;
                            debit.isApproved = true;
                            debit.postedBy = loanDetails.createdBy;
                            debit.approvedBy = loanDetails.createdBy;
                            debit.approvedDate = debit.transactionDate;
                            debit.approvedDateTime = DateTime.Now;
                            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            debit.companyId = loanDetails.companyId;


                            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                            debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                            debit.batchCode = batchCode;
                            debit.casaAccountId = casa.CASAACCOUNTID;
                            debit.debitAmount = debitAmount;
                            debit.creditAmount = 0;
                            debit.sourceBranchId = loanDetails.branchId;
                            debit.destinationBranchId = casa.BRANCHID;

                            debit.rateCode = "TTB"; //loanDetails.nostroRateCode;
                            debit.rateUnit = string.Empty;
                            debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(debit);
                        }

                        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                        {
                            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                            decimal creditAmount = 0;
                            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                creditAmount = (decimal)loanFee.FEEAMOUNT * (decimal)(credits.VALUE / 100.0);
                            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                creditAmount = (decimal)credits.VALUE;


                            credit.operationId = (int)OperationsEnum.ManualFeeCharge;
                            credit.description = $"Fee charge on {credits.DESCRIPTION}";
                            credit.valueDate = general.GetApplicationDate();
                            credit.transactionDate = credit.valueDate;
                            credit.currencyId =  context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == loanDetails.companyId).CURRENCYID; //(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, loanDetails.companyId); //casa.CURRENCYID;
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, loanDetails.companyId).sellingRate;
                            credit.isApproved = true;
                            credit.postedBy = loanDetails.createdBy;
                            credit.approvedBy = loanDetails.createdBy;
                            credit.approvedDate = credit.transactionDate;
                            credit.approvedDateTime = DateTime.Now;
                            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            credit.companyId = loanDetails.companyId;
                            credit.glAccountId = (int)credits.GLACCOUNTID1;
                            credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                            credit.batchCode = batchCode;
                            credit.casaAccountId = null;
                            credit.debitAmount = 0;
                            credit.creditAmount = creditAmount;
                            credit.sourceBranchId = loanDetails.branchId;
                            credit.destinationBranchId = loanDetails.branchId;
                            credit.rateCode = "TTB"; //loanDetails.nostroRateCode;
                            credit.rateUnit = string.Empty;
                            credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(credit);
                        }
                    }
                }

            //}
            return inputTransactions;
        }

        public ApprovalStatusEnum ApproveTakeFee(ApprovalViewModel userModel)
        {
            //if(userModel.approvalStatusId<0 || string.IsNullOrEmpty(userModel.comment))
            //{
            //    throw new ConditionNotMetException("Kindly Enter Comment And Select an Approval Status Before Proceeding...");
            //}

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                passcode = userModel.passCode,
                username = userModel.userName
            }; 
            var feeCharge = context.TBL_LOAN_FEE.Find(userModel.targetId);
            bool output = false;
            int data = 0;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    

                    workflow.StaffId = userModel.staffId;
                    workflow.CompanyId = userModel.companyId;
                    workflow.StatusId = ((int)userModel.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)userModel.approvalStatusId;
                    workflow.TargetId = userModel.targetId;
                    workflow.Comment = userModel.comment;
                    workflow.OperationId = (int)OperationsEnum.ManualFeeCharge;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;

                    workflow.LogActivity();


                    if (userModel.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        if (twoFADetails != null && admin.TwoFactorAuthenticationEnabled())
                        {
                            var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                            if (authenticated.authenticated == false)
                                throw new TwoFactorAuthenticationException(authenticated.message);
                        }
                        //twoFADetails.skipAuthentication = true;

                        feeCharge.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                        context.SaveChanges();
                        trans.Commit();
                        return ApprovalStatusEnum.Disapproved;
                    }
                    if (workflow.NewState != (int)ApprovalState.Ended)
                    {
                        feeCharge.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                        output = context.SaveChanges() > 0;
                        trans.Commit();
                        return ApprovalStatusEnum.Processing;
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (twoFADetails != null && admin.TwoFactorAuthenticationEnabled())
                        {
                            var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                            if (authenticated.authenticated == false)
                                throw new TwoFactorAuthenticationException(authenticated.message);
                        }
                        twoFADetails.skipAuthentication = true;



                        var feePostings = BuildLoanManualChargeFeesPosting(userModel.targetId);

                        if (feePostings.Count() > 0)
                        {
                            financeTransaction.PostTransaction(feePostings, false, twoFADetails);

                            twoFADetails.skipAuthentication = true;
                            //financeTransaction.PostTransaction(disbursementTransactions, false, twoFADetails);
                        }
                        //else
                        //{
                            //financeTransaction.PostTransaction(disbursementTransactions, false, twoFADetails);
                        //}
                        //response = BuildLoanManualChargeFeesPosting(userModel.targetId);
                            try
                            {
                            feeCharge.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                            context.SaveChanges();
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                var EXE = ex;
                            }
                        
                        return ApprovalStatusEnum.Approved;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }

        }

    }
}

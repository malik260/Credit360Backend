using FintrakBanking.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.ComponentModel.Composition;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.CASA;
using System.Configuration;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Finance;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ILoanCovenantRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanCovenantRepository : ILoanCovenantRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        
        private CustomerDetails customer;
        //bool USE_TWO_FACTOR_AUTHENTICATION = false;
        bool USE_THIRD_PARTY_INTEGRATION = false;

        public LoanCovenantRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository _genSetup,
                                    CustomerDetails customer,
                                    FinTrakBankingContext _context)
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.customer = customer;
        }

        #region LoanCovenantDetail
        public async Task<int> AddMultipleLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel)
        {
            if (covenantModel.Count <= 0)
                return -1;

            foreach (LoanCovenantDetailViewModel entity in covenantModel)
            {
                AddLoanCovenantDetail(entity);
            }

            return 1;

        }

        public bool AddLoanCovenantDetail(LoanCovenantDetailViewModel entity)
        {
            var convenant = new TBL_LOAN_COVENANT_DETAIL
            {
                COMPANYID = entity.companyId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDATE = entity.covenantDate,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTTYPEID = entity.covenantTypeId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = this.genSetup.GetApplicationDate(),
                FREQUENCYTYPEID = entity.frequencyTypeId,
                LOANID = entity.loanId
            };
            context.TBL_LOAN_COVENANT_DETAIL.Add(convenant);

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == entity.loanId).LOANREFERENCENUMBER;

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
            //    STAFFID = entity.createdBy,
            //    BRANCHID = (short)entity.userBranchId,
            //    DETAIL = $"Added loan convent to loan ref: { loanRef } ",
            //    IPADDRESS = entity.userIPAddress,
            //    URL = entity.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteLoanCovenantDetail(int loanCovenantDetailId, UserInfo user)
        {
            var convenant = context.TBL_LOAN_COVENANT_DETAIL.Find(loanCovenantDetailId);
            convenant.DELETED = true;
            convenant.DELETEDBY = user.staffId;
            convenant.DATETIMEDELETED = this.genSetup.GetApplicationDate().Date;

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == loanCovenantDetailId).LOANREFERENCENUMBER;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan convent to loan ref: { loanRef } ",
                IPADDRESS = user.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<LoanCovenantDetailViewModel> LoanCovenantDetail(int companyId)
        {
            return context.TBL_LOAN_COVENANT_DETAIL.Where(c => c.COMPANYID == companyId).Select(c => new LoanCovenantDetailViewModel
            {
                covenantAmount = c.COVENANTAMOUNT,
                companyId = c.COMPANYID,
                covenantDate = c.COVENANTDATE,
                covenantDetail = c.COVENANTDETAIL,
                covenantTypeId = c.COVENANTTYPEID,
                covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                frequencyTypeId = c.FREQUENCYTYPEID,
                frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                loanId = c.LOANID,
                //loanRef = c.tbl_Loan.LoanReferenceNumber,
                // productName = c.tbl_Loan.tbl_Product.ProductName
            });
        }

        //public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetail(int companyId)
        //{
        //    return LoanCovenantDetail(companyId);
        //}

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByCovenantType(int covenantTypeId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.covenantTypeId == covenantTypeId);
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByloanId(int loanId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.loanId == loanId);
        }

        public async Task<bool> UpdateLoanCovenantDetail(int id, LoanCovenantDetailViewModel entity)
        {
            var convenant = context.TBL_LOAN_COVENANT_DETAIL.Find(id);

            convenant.COMPANYID = entity.companyId;
            convenant.COVENANTAMOUNT = entity.covenantAmount;
            convenant.COVENANTDATE = entity.covenantDate;
            convenant.COVENANTDETAIL = entity.covenantDetail;
            convenant.COVENANTTYPEID = entity.covenantTypeId;
            convenant.CREATEDBY = entity.createdBy;
            convenant.DATETIMEUPDATED = this.genSetup.GetApplicationDate().Date;
            convenant.FREQUENCYTYPEID = entity.frequencyTypeId;
            convenant.LOANID = entity.loanId;

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == entity.loanId).LOANREFERENCENUMBER;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailUpdate,
                STAFFID = entity.lastUpdatedBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated loan convent to loan ref: { loanRef } ",
                IPADDRESS = entity.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);

            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantDetailById(int covenantDetailId, int companyId)
        {
            return LoanCovenantType(companyId).Where(c => c.covenantTypeId == covenantDetailId);
        }

        #endregion LoanCovenantDetail

        #region Loan Covenant Type
        public async Task<bool> AddLoanCovenantType(LoanCovenantTypeViewModel entity)
        {
            var convenant = new TBL_LOAN_COVENANT_TYPE
            {
                ISFINANCIAL = entity.isFinancial,
                COMPANYID = entity.companyId,
                COVENANTTYPENAME = entity.covenantTypeName,
                REQUIREAMOUNT = entity.requireAmount,
                REQUIREFREQUENCY = entity.requireFrequency
            };
            context.TBL_LOAN_COVENANT_TYPE.Add(convenant);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantTypeAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Defined loan convent type: { entity.covenantTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateLoanCovenantType(short id, LoanCovenantTypeViewModel entity)
        {
            var convenant = context.TBL_LOAN_COVENANT_TYPE.Find(id);
            convenant.ISFINANCIAL = entity.isFinancial;
            convenant.COMPANYID = entity.companyId;
            convenant.COVENANTTYPENAME = entity.covenantTypeName;
            convenant.REQUIREAMOUNT = entity.requireAmount;
            convenant.REQUIREFREQUENCY = entity.requireFrequency;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantTypeAdd,
                STAFFID = entity.lastUpdatedBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated loan convent type: { entity.covenantTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            //this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        IEnumerable<LoanCovenantTypeViewModel> LoanCovenantType(int companyId)
        {
            return context.TBL_LOAN_COVENANT_TYPE.Where(c => c.COMPANYID == companyId).Select(c => new LoanCovenantTypeViewModel
            {
                companyId = c.COMPANYID,
                isFinancial = c.ISFINANCIAL,
                covenantTypeId = c.COVENANTTYPEID,
                covenantTypeName = c.COVENANTTYPENAME,
                requireAmount = c.REQUIREAMOUNT,
                requireFrequency = c.REQUIREFREQUENCY,
                requireCasaAccount = c.REQUIRECASAACCOUNT
            });
        }

        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantType(int companyId)
        {
            return LoanCovenantType(companyId);
        }

        // application 

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenant(int applicationId)
        {
            var ids = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => x.LOANAPPLICATIONDETAILID);

            var records = context.TBL_LOAN_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && ids.Contains(x.LOANAPPLICATIONDETAILID)
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    isFinancial = c.TBL_LOAN_COVENANT_TYPE.ISFINANCIAL,
                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
                    
                });

            return records;
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenant(int applicationDetailId)
        {
            var records = context.TBL_LOAN_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && x.LOANAPPLICATIONDETAILID == applicationDetailId
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    isFinancial = c.TBL_LOAN_COVENANT_TYPE.ISFINANCIAL,
                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME

                });

            return records;
        }


        public bool UpdateLoanApplicationCovenant(DateTime date, int companyId, int staffId, out string transactionReferenceNo)
        {
            var covenants = context.TBL_LOAN_APPLICATION_COVENANT.Where(o => o.NEXTCOVENANTDATE == date && o.COMPANYID == companyId).ToList();


            var eod_Operation_Log = context.TBL_EOD_OPERATION_LOG.Where(c => c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant && c.COMPANYID == companyId).FirstOrDefault();


            List<TBL_EOD_OPERATION_LOG_DETAIL> eod_operation_Detail_List = new List<TBL_EOD_OPERATION_LOG_DETAIL>();

            if (covenants.Count() > 0)
            {
                var eodOperations = context.TBL_EOD_OPERATION.OrderBy(x => x.POSITION).ToList();

                foreach (TBL_LOAN_APPLICATION_COVENANT loan in covenants)
                {

                    TBL_EOD_OPERATION_LOG_DETAIL eod_operation_Detail = new TBL_EOD_OPERATION_LOG_DETAIL();

                    //var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == loan.COVENANTDETAIL && c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant).FirstOrDefault();
                    var refNumber = loan.LOANCOVENANTDETAILID.ToString() + '-' + loan.LOANAPPLICATIONDETAILID.ToString();
                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == refNumber && c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant).FirstOrDefault();

                    if (checkExistence == null)
                    {
                        eod_operation_Detail.EODOPERATIONLOGID = eod_Operation_Log.EODOPERATIONLOGID;
                        eod_operation_Detail.EODSTATUSID = (int)EodOperationStatusEnum.Processing;
                        eod_operation_Detail.REFERENCENUMBER = loan.LOANCOVENANTDETAILID.ToString() + '-' + loan.LOANAPPLICATIONDETAILID.ToString();
                        eod_operation_Detail.EODOPERATIONID = (int)EodOperationEnum.UpdateLoanApplicationCovenant;
                        eod_operation_Detail.EODDATE = date;
                        eod_operation_Detail.EODUSERID = staffId;
                        eod_operation_Detail_List.Add(eod_operation_Detail);

                    }

                }

                context.TBL_EOD_OPERATION_LOG_DETAIL.AddRange(eod_operation_Detail_List);

                context.SaveChanges();

            }


                transactionReferenceNo = "";
                foreach (var covenant in covenants)
                {
                    var refNumber = covenant.LOANCOVENANTDETAILID.ToString() + '-' + covenant.LOANAPPLICATIONDETAILID.ToString();
                    var checkExistence = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == refNumber && c.EODDATE == date && c.EODSTATUSID != (int)EodOperationStatusEnum.Completed && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant).FirstOrDefault();

                    if (checkExistence != null)
                    {

                        var eod_Operation_Log_Detail_Set_Value = context.TBL_EOD_OPERATION_LOG_DETAIL.Where(c => c.REFERENCENUMBER == refNumber && c.EODDATE == date && c.EODOPERATIONID == (int)EodOperationEnum.UpdateLoanApplicationCovenant).FirstOrDefault();
                        eod_Operation_Log_Detail_Set_Value.STARTDATETIME = DateTime.Now;
                        eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;

                        context.SaveChanges();

                        try
                        {
                            //transactionReferenceNo = covenant.COVENANTDETAIL.ToString();
                            transactionReferenceNo = covenant.LOANCOVENANTDETAILID.ToString() + '-' + covenant.LOANAPPLICATIONDETAILID.ToString();
                            covenant.PREVIOUSCOVENANTDATE = (DateTime)covenant.NEXTCOVENANTDATE;
                            covenant.NEXTCOVENANTDATE = GetFrequencyDate((int)covenant.FREQUENCYTYPEID, (DateTime)covenant.NEXTCOVENANTDATE);

                            eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                            eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Completed;
                            eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                            eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = "No Error";
                            context.SaveChanges();

                            if (covenants.Count() > 0)
                            {
                                AlertsViewModel alerts = new AlertsViewModel();
                                string emailList = "";
                                var casaAccount = context.TBL_CASA.Find(covenant.CASAACCOUNTID);
                                var data = GetCustomerAccountBalance(casaAccount.PRODUCTACCOUNTNUMBER);

                                if ((DateTime)covenant.PREVIOUSCOVENANTDATE.Value.Date == DateTime.Now.Date && covenant.COVENANTTYPEID == (short)LoanCovenantTypeEnum.Cleanup)
                                {
                                    if (data != null)
                                    {
                                        var availableBalance = data.availableBalance;
                                        if (covenant.COVENANTAMOUNT > availableBalance)
                                        {
                                            var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(covenant.LOANAPPLICATIONDETAILID);
                                            var appDetails = context.TBL_LOAN_APPLICATION.Find(loanDetails.LOANAPPLICATIONID);
                                            var staffMisCode = context.TBL_STAFF.Find(loanDetails.CREATEDBY).MISCODE;
                                            var customerDetail = context.TBL_CUSTOMER.Find(loanDetails.CUSTOMERID);
                                            emailList = GetBusinessTeamsEmails(staffMisCode);
                                            alerts.receiverEmailList.Add(emailList);
                                            var subject = "OD CLEAN-UP VIOLATION NOTIFICATION";
                                            var message = "This is to inform you that an OD clean-up with reference number: " + appDetails.APPLICATIONREFERENCENUMBER + " with customer detail: ( " + customerDetail.CUSTOMERCODE + "," + customerDetail.FIRSTNAME + " " + customerDetail.MIDDLENAME + " " + customerDetail.LASTNAME + ") condition has been violated by the customer.";
                                            LogEmailAlert(message, subject, alerts.receiverEmailList, "100456", 100456, "OdViolationNotification");
                                            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

                                            var casaLienViewModel = new CasaLienViewModel
                                            {
                                                productAccountNumber = casaAccount.PRODUCTACCOUNTNUMBER,
                                                sourceReferenceNumber = appDetails.APPLICATIONREFERENCENUMBER,
                                                companyId = appDetails.COMPANYID,
                                                branchId = appDetails.BRANCHID,
                                                lienAmount = (loanDetails.APPROVEDAMOUNT - (decimal)covenant.COVENANTAMOUNT),
                                                description = "Place lien on the account " + casaAccount.PRODUCTACCOUNTNUMBER + "with amount " + (loanDetails.APPROVEDAMOUNT - (decimal)covenant.COVENANTAMOUNT),
                                                lienTypeId = (short)LienTypeEnum.OverdraftCleanUp,
                                                dateTimeCreated = DateTime.Now,
                                                createdBy = loanDetails.CREATEDBY,
                                                lienReferenceNumber = referenceNumber,
                                            };

                                            PlaceLienSub(casaLienViewModel);
                                        }
                                    }
                                }
                                else
                                if ((DateTime)covenant.PREVIOUSCOVENANTDATE.Value.Date == DateTime.Now.Date && covenant.COVENANTTYPEID != (short)LoanCovenantTypeEnum.Cleanup)
                                {
                                    if (data != null)
                                    {
                                        var availableBalance = data.availableBalance;
                                        if (covenant.COVENANTAMOUNT > availableBalance)
                                        {
                                            var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(covenant.LOANAPPLICATIONDETAILID);
                                            var appDetails = context.TBL_LOAN_APPLICATION.Find(loanDetails.LOANAPPLICATIONID);
                                            var staffMisCode = context.TBL_STAFF.Find(loanDetails.CREATEDBY).MISCODE;
                                            var customerDetail = context.TBL_CUSTOMER.Find(loanDetails.CUSTOMERID);
                                            emailList = GetBusinessTeamsEmails(staffMisCode);
                                            alerts.receiverEmailList.Add(emailList);
                                            var subject = "COVENANT VIOLATION NOTIFICATION";
                                            var message = "This is to inform you that a loan with reference number: " + appDetails.APPLICATIONREFERENCENUMBER + " with customer detail: ( " + customerDetail.CUSTOMERCODE + "," + customerDetail.FIRSTNAME + " " + customerDetail.MIDDLENAME + " " + customerDetail.LASTNAME + ") condition has been violated by the customer.";
                                            LogEmailAlert(message, subject, alerts.receiverEmailList, "100455", 100455, "CovenantViolationNotification");
                                        }
                                    }
                                }
                            }
                                
                         
                        }
                        catch (Exception ex)
                        {

                            eod_Operation_Log_Detail_Set_Value.ENDDATETIME = DateTime.Now;
                            eod_Operation_Log_Detail_Set_Value.EODSTATUSID = (int)EodOperationStatusEnum.Error;
                            eod_Operation_Log_Detail_Set_Value.EODUSERID = staffId;
                            //eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {covenant.COVENANTDETAIL} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                            eod_Operation_Log_Detail_Set_Value.ERRORINFORMATION = $"Ref No - {covenant.LOANCOVENANTDETAILID.ToString() + '-' + covenant.LOANAPPLICATIONDETAILID.ToString()} Exception - {ex.Message}  - inner exception -  {ex.InnerException}";
                            context.SaveChanges();
                        }



                    }

                }
                    var interestRateChanges = context.TBL_LOAN_REVIEW_OPERATION.Where(r => r.OPERATIONTYPEID == (int)OperationsEnum.OverdraftInterestRate || r.OPERATIONTYPEID == (int)OperationsEnum.ContractualInterestRateChange).ToList();
                    if (interestRateChanges.Count() > 0)
                    {
                        AlertsViewModel alerts = new AlertsViewModel();
                        string emailList = "";

                        foreach (var interestRateChange in interestRateChanges)
                        {
                            if (interestRateChange.EFFECTIVEDATE.Date == DateTime.Now.Date)
                            {
                                var loanDetails = context.TBL_LMSR_APPLICATION_DETAIL.Find(interestRateChange.LOANREVIEWAPPLICATIONID);
                                var appDetails = context.TBL_LMSR_APPLICATION.Find(loanDetails.LOANAPPLICATIONID);
                                var staffMisCode = context.TBL_STAFF.Find(interestRateChange.CREATEDBY).MISCODE;
                                var customerDetail = context.TBL_CUSTOMER.Find(loanDetails.CUSTOMERID);
                                emailList = GetBusinessTeamsEmails(staffMisCode);
                                alerts.receiverEmailList.Add(emailList);
                                var subject = "POSTDATED PERIOD OF RATE CHANGE NOTIFICATION";
                                var message = "This is to inform you that Postdated Period of Rate Change of effective date " + interestRateChange.EFFECTIVEDATE + "  on a loan with reference number: " + appDetails.APPLICATIONREFERENCENUMBER + " with customer detail: ( " + customerDetail.CUSTOMERCODE + "," + customerDetail.FIRSTNAME + " " + customerDetail.MIDDLENAME + " " + customerDetail.LASTNAME + ") is due today.";
                                LogEmailAlert(message, subject, alerts.receiverEmailList, "100433", 100433, "PostdatedRateChangeNotification");
                            }
                        }
                    }
            return context.SaveChanges() != 0;
        }

        //public bool UpdateLoanApplicationCovenant(DateTime date)
        //{
        //    var covenants = context.TBL_LOAN_APPLICATION_COVENANT.Where(o => o.NEXTCOVENANTDATE == date).ToList();

        //    foreach (var covenant in covenants)
        //    {
        //        covenant.PREVIOUSCOVENANTDATE = (DateTime)covenant.NEXTCOVENANTDATE;
        //        covenant.NEXTCOVENANTDATE = GetFrequencyDate((int)covenant.FREQUENCYTYPEID, (DateTime)covenant.NEXTCOVENANTDATE);
        //    }

        //    return context.SaveChanges() != 0;
        //}


        public bool AddLoanApplicationCovenant(LoanCovenantDetailViewModel entity)
        {
            if (entity.loanCovenantDetailId > 0)
            {
                var convenant = context.TBL_LOAN_APPLICATION_COVENANT.Find(entity.loanCovenantDetailId);
                convenant.COVENANTAMOUNT = entity.covenantAmount;
                convenant.COVENANTDATE = entity.covenantDate;
                convenant.COVENANTDETAIL = entity.covenantDetail;
                convenant.COVENANTTYPEID = entity.covenantTypeId;
                convenant.FREQUENCYTYPEID = entity.frequencyTypeId;
                convenant.LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId;
                convenant.ISPERCENTAGE = entity.isPercentage;
                convenant.NEXTCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate);
                convenant.PREVIOUSCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate);
                convenant.CASAACCOUNTID = entity.casaAccountId;
                convenant.DATETIMEUPDATED = DateTime.Now;
                context.SaveChanges();
            }
            else
            {
                var convenant = new TBL_LOAN_APPLICATION_COVENANT
                {
                    LOANCOVENANTDETAILID = entity.loanCovenantDetailId,
                    COVENANTAMOUNT = entity.covenantAmount,
                    COVENANTDATE = entity.covenantDate,
                    COVENANTDETAIL = entity.covenantDetail,
                    COVENANTTYPEID = entity.covenantTypeId,
                    FREQUENCYTYPEID = entity.frequencyTypeId,
                    LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                    ISPERCENTAGE = entity.isPercentage,
                    NEXTCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate), //entity.nextCovenantDate,
                    PREVIOUSCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate), //entity.nextCovenantDate,
                    CASAACCOUNTID = entity.casaAccountId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = this.genSetup.GetApplicationDate().Date,
                    COMPANYID = entity.companyId,
                };
                context.TBL_LOAN_APPLICATION_COVENANT.Add(convenant);
                context.SaveChanges();
            }
            var appl = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added loan application covenant on application: { appl.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER } ",
                IPADDRESS = entity.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool DeleteLoanApplicationCovenant(int covenantId, UserInfo user)
        {
            var covenant = context.TBL_LOAN_APPLICATION_COVENANT.Find(covenantId);
            covenant.DELETED = true;
            covenant.DELETEDBY = user.staffId;
            covenant.DATETIMEDELETED = DateTime.Now;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan application covenant: { covenant.COVENANTDETAIL } ",
                IPADDRESS = user.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        // end application

        #endregion Loan Covenant Type


        #region Begin FrequencyType

        public DateTime GetFrequencyDate(int frequencyTypeId, DateTime date)
        {
            DateTime nextDate = new DateTime();

            if (frequencyTypeId == (int)FrequencyTypeEnum.Yearly)
            {
                nextDate = date.AddMonths(12);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.TwiceYearly)
            {
                nextDate = date.AddMonths(6);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Quarterly)
            {
                nextDate = date.AddMonths(3);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.SixTimesYearly)
            {
                nextDate = date.AddMonths(2);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Monthly)
            {
                nextDate = date.AddMonths(1);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.ThriceYearly)
            {
                nextDate = date.AddMonths(4);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Daily)
            {
                nextDate = date.AddDays(1);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Weekly)
            {
                nextDate = date.AddDays(7);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.TwiceMonthly)
            {
                nextDate = date.AddDays(14);
            }
            return nextDate;
        }

        #endregion End FrequencyType

        #region LMS APPROVAL


        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenantLms(int applicationId)
        {
            var ids = context.TBL_LMSR_APPLICATION_DETAIL
                 .Where(x => x.LOANAPPLICATIONID == applicationId)
                 .Select(x => x.LOANREVIEWAPPLICATIONID);

            return context.TBL_LMSR_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && ids.Contains(x.LOANREVIEWAPPLICATIONID)
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANREVIEWAPPLICATIONID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    isFinancial = c.TBL_LOAN_COVENANT_TYPE.ISFINANCIAL,
                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LMSR_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME

                });
        }

        public bool AddLoanApplicationCovenantLms(LoanCovenantDetailViewModel entity)
        {
            var convenant = new TBL_LMSR_APPLICATION_COVENANT
            {
                LOANCOVENANTDETAILID = entity.loanCovenantDetailId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDATE = entity.covenantDate,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTTYPEID = entity.covenantTypeId,
                FREQUENCYTYPEID = entity.frequencyTypeId,
                LOANREVIEWAPPLICATIONID = entity.loanApplicationDetailId,
                ISPERCENTAGE = entity.isPercentage,
                NEXTCOVENANTDATE = entity.nextCovenantDate,
                CASAACCOUNTID = entity.casaAccountId,

                CREATEDBY = entity.createdBy,
                DATETIMECREATED = this.genSetup.GetApplicationDate().Date,
                COMPANYID = entity.companyId,
            };
            context.TBL_LMSR_APPLICATION_COVENANT.Add(convenant);

            var appl = context.TBL_LMSR_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added loan REVIEW application covenant on application: { appl.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER } ",
                IPADDRESS = entity.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool DeleteLoanApplicationCovenantLms(int covenantId, UserInfo user)
        {
            var covenant = context.TBL_LMSR_APPLICATION_COVENANT.Find(covenantId);
            covenant.DELETED = true;
            covenant.DELETEDBY = user.staffId;
            covenant.DATETIMEDELETED = DateTime.Now;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan REVIEW application covenant: { covenant.COVENANTDETAIL } ",
                IPADDRESS = user.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        #endregion LMS APPROVAL

        private string GetBusinessTeamsEmails(string accountOfficerMIsCode)
        {
            string emailList = "";

            var accountOfficer = context.TBL_STAFF.Where(x => x.MISCODE.ToLower() == accountOfficerMIsCode.ToLower()).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }

        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = title;
                string messageContent = messageBody;
                //string templateUrl = context.TBL_ALERT_GENERAL_TEMPLATE.Find(1).TEMPLATEBODY; //"~/EmailTemp/Monitoring.html";
                //string mailBody = templateUrl.Replace("{Description}", messageContent);  //EmailHelpers.PopulateBody(messageContent, templateUrl); 
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = messageContent,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = referenceCode,
                    targetId = targetId,
                    operationMethod = operationMehtod,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                new SecureException(ex.ToString());
            }
        }

        private void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId,
                OPERATIONMETHOD = model.operationMethod
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();

        }

        //public string PlaceLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        //{
        //    var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
        //    model.lienReferenceNumber = referenceNumber;

        //    //call     
        //    if (USE_TWO_FACTOR_AUTHENTICATION)
        //    {
        //        if (twoFADetails == null)
        //            throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

        //        if (twoFADetails.skipAuthentication == false)
        //        {
        //            var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

        //            if (authenticated.authenticated == false)
        //                throw new TwoFactorAuthenticationException(authenticated.message);
        //        }
        //    }

        //    if (USE_THIRD_PARTY_INTEGRATION)
        //    {

        //        ResponseMessage result = null;

        //        Task.Run(async () => { result = await tran.APIProcessLien(model, "PLACE"); }).GetAwaiter().GetResult();

        //        if (result.APIResponse != null)
        //        {
        //            if (result.APIResponse.responseCode == "0")
        //            {
        //                PlaceLienSub(model);
        //            }
        //            else
        //            {
        //                throw new ConditionNotMetException("Core Banking API Error - " + result.APIResponse.webRequestStatus);
        //            }
        //        }
        //        else
        //        {
        //            throw new APIErrorException("Core Banking API Error - " + result.Message.ReasonPhrase);
        //        }

        //    }

        //    else
        //    {
        //        PlaceLienSub(model);
        //    }

        //    return referenceNumber;
        //}

        private void PlaceLienSub(CasaLienViewModel model)
        {
            var validate = context.TBL_CASA_LIEN.Where(x => x.SOURCEREFERENCENUMBER == model.sourceReferenceNumber).FirstOrDefault();
            if (validate == null)
            {
                var data = new TBL_CASA_LIEN
                {
                    PRODUCTACCOUNTNUMBER = model.productAccountNumber,
                    LIENREFERENCENUMBER = model.lienReferenceNumber,
                    SOURCEREFERENCENUMBER = model.sourceReferenceNumber,
                    BRANCHID = model.branchId,
                    COMPANYID = model.companyId,
                    LIENAMOUNT = model.lienAmount,
                    DESCRIPTION = model.description,
                    LIENTYPEID = model.lienTypeId,
                    CREATEDBY = model.createdBy,
                    LIENSTATUS = (int)LienStatusEnum.Active,
                    DATETIMECREATED = DateTime.Now,
                    ISLIENREMOVED = false
                };

                context.TBL_CASA_LIEN.Add(data);

                // Audit Section ---------------------------            

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LienPlaced,
                    STAFFID = model.createdBy,
                    BRANCHID = model.branchId,
                    DETAIL = $"Applied lien with reference number: {model.lienReferenceNumber}",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()


                };
                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                context.SaveChanges();
            }

        }

        public CasaBalanceViewModel GetCustomerAccountBalance(string customerAccount)
        {
            //if (!USE_THIRD_PARTY_INTEGRATION) return null;
            CasaBalanceViewModel accountOutput = null;
            Task.Run(async () => accountOutput = await customer.GetCustomerAccountBalance(customerAccount)).GetAwaiter()
                .GetResult();
            return accountOutput;

        }

    }
}

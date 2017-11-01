using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IFeeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FeeRepository : IFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkFlowRepository workFlow;


        public FeeRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IWorkFlowRepository _workFlow)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            workFlow = _workFlow;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        #region tbl_Product Fee

        public int AddFee(FeeViewModel fee)
        {
            var data = new TBL_FEE()
            {
                FEENAME = fee.feeName,
                ACCOUNTCATEGORYID = fee.accountCategoryId,
                FEETYPEID = fee.feeTypeId,
                FEEINTERVALID = fee.feeIntervalId,
                PRODUCTTYPEID = fee.productTypeId,
                FEETARGETID = fee.feeTargetId,
                ISINTEGRALFEE = fee.isIntegralFee,
                GLACCOUNTID = fee.glAccountId,
                FEEAMORTISATIONTYPEID = fee.feeAmortisationTypeId,
                INCLUDECUTOFFDAY = fee.includeCutOffDay,
                CUTOFFDAY = fee.cutOffDay,
                COMPANYID = fee.companyId,
                FEEDATE = DateTime.Now,

                CREATEDBY = fee.createdBy,
                DATETIMECREATED = DateTime.Now,
            };           

            this.context.TBL_FEE.Add(data);

            var status = this.SaveAll();

            if (status)
            {
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.FeeAdded,
                    STAFFID = fee.createdBy,
                    BRANCHID = (short)fee.userBranchId,
                    DETAIL = $"Added fee: { fee.feeName } of type {fee.feeTypeName} ",
                    IPADDRESS = fee.userIPAddress,
                    URL = fee.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                return data.FEEID;
            }
            
            else
                return -1;
        }

        public bool AddTempFee(FeeViewModel feeModel)
        {
            bool output = false;

            var tempFee = new TBL_TEMP_FEE()
            {
                FEENAME = feeModel.feeName,
                ACCOUNTCATEGORYID = feeModel.accountCategoryId,
                FEETYPEID = feeModel.feeTypeId,
                FEEINTERVALID = feeModel.feeIntervalId,
                PRODUCTTYPEID = feeModel.productTypeId,
                FEETARGETID = feeModel.feeTargetId,
                ISINTEGRALFEE = feeModel.isIntegralFee,
                GLACCOUNTID = feeModel.glAccountId,
                FEEAMORTISATIONTYPEID = feeModel.feeAmortisationTypeId,
                INCLUDECUTOFFDAY = feeModel.includeCutOffDay,
                CUTOFFDAY = feeModel.cutOffDay,
                COMPANYID = feeModel.companyId,
                FEEDATE = DateTime.Now,

                CREATEDBY = feeModel.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.FeeAdded,
                STAFFID = feeModel.createdBy,
                BRANCHID = (short)feeModel.userBranchId,
                DETAIL = $"Added fee: { feeModel.feeName } of type {feeModel.feeTypeName} ",
                IPADDRESS = feeModel.userIPAddress,
                URL = feeModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------


            if (workFlow.CheckRouteForOperation((int)OperationsEnum.FeeCreation, feeModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        this.auditTrail.AddAuditTrail(audit);
                        this.context.TBL_TEMP_FEE.Add(tempFee);

                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = feeModel.createdBy,
                            companyId = feeModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = feeModel.feeId,
                            operationId = (int)OperationsEnum.FeeCreation,
                            BranchId = feeModel.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
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
                throw new Exception("Approval route have not been defined for this operation");
            }

            return output;

        }

        public IEnumerable<FeeViewModel> GetAllFee()
        {
            return (from data in context.TBL_FEE
                        //where account.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new FeeViewModel()
                    {
                        feeId = data.FEEID,
                        feeName = data.FEENAME,
                        accountCategoryId = data.ACCOUNTCATEGORYID,
                        accountCategoryName = data.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                        feeTypeId = data.FEETYPEID,
                        feeTypeName = string.Empty,//data.tbl_Fee_Type.FeeTypeName,
                        feeTypeByAmountRequired = false,//data.tbl_Fee_Type.ByAmountRequired,
                        byAmountRequired = false,//data.tbl_Fee_Type.ByAmountRequired,
                        feeIntervalId = data.FEEINTERVALID,
                        isIntegralFee = data.ISINTEGRALFEE,
                        feeIntervalName = data.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                        productTypeId = data.PRODUCTTYPEID,
                        productTypeName = data.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        feeTargetId = data.FEETARGETID,
                        feeTargetName = data.TBL_FEE_TARGET.FEETARGETNAME,
                        glAccountId = data.GLACCOUNTID,
                        glAccountCode = data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                        glAccountName = data.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                        includeCutOffDay = data.INCLUDECUTOFFDAY,
                        feeAmortisationTypeId = data.FEEAMORTISATIONTYPEID,

                        cutOffDay = data.CUTOFFDAY,
                        companyId = data.COMPANYID,
                        feeDate = data.FEEDATE,
                        createdBy = data.CREATEDBY,
                        
                        dateTimeCreated = data.DATETIMECREATED,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED
                    });
        }

        public FeeViewModel GetFeeViewModel(int feeId)
        {
            return (from data in context.TBL_FEE
                    where data.FEEID == feeId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new FeeViewModel()
                    {
                        feeId = data.FEEID,
                        feeName = data.FEENAME,
                        accountCategoryId = data.ACCOUNTCATEGORYID,
                        feeTypeId = data.FEETYPEID,
                        feeIntervalId = data.FEEINTERVALID,
                        productTypeId = data.PRODUCTTYPEID,
                        feeTargetId = data.FEETARGETID,
                        glAccountId = data.GLACCOUNTID,
                        feeAmortisationTypeId = data.FEEAMORTISATIONTYPEID,
                        isIntegralFee = data.ISINTEGRALFEE,
                        includeCutOffDay = data.INCLUDECUTOFFDAY,
                        cutOffDay = data.CUTOFFDAY,
                        companyId = data.COMPANYID,
                        feeDate = data.FEEDATE,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED,
                    }).FirstOrDefault();
        }

        public bool UpdateFee(int feeId, FeeViewModel fee)
        {
            var feeModel = this.context.TBL_FEE.Find(feeId);

            if (feeModel == null)
                return false;

            feeModel.FEENAME = fee.feeName;
            feeModel.ACCOUNTCATEGORYID = fee.accountCategoryId;
            feeModel.FEETYPEID = fee.feeTypeId;
            feeModel.FEEINTERVALID = fee.feeIntervalId;
            feeModel.PRODUCTTYPEID = fee.productTypeId;
            feeModel.FEETARGETID = fee.feeTargetId;
            feeModel.ISINTEGRALFEE = fee.isIntegralFee;
            feeModel.GLACCOUNTID = fee.glAccountId;
            feeModel.FEEAMORTISATIONTYPEID = fee.feeAmortisationTypeId;
            feeModel.INCLUDECUTOFFDAY = fee.includeCutOffDay;
            feeModel.CUTOFFDAY = fee.cutOffDay;
            feeModel.COMPANYID = fee.companyId;
            feeModel.FEEDATE = fee.feeDate;            

            feeModel.LASTUPDATEDBY = fee.lastUpdatedBy;
            feeModel.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralCategoryAdded,
                STAFFID = fee.createdBy,
                BRANCHID = (short)fee.userBranchId,
                DETAIL = $"Udated fee: { fee.feeName } of type {fee.feeTypeName} ",
                IPADDRESS = fee.userIPAddress,
                URL = fee.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool UpdateFeeForApproval(int feeId, FeeViewModel feeModel)
        {
            if (feeModel == null)
                return false;

            var existStingTempFee = context.TBL_TEMP_FEE.Where(x => x.ACCOUNTCATEGORYID ==
            feeModel.accountCategoryId && x.ISCURRENT == true &&
            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            if (existStingTempFee.Any())
            {
                foreach (var item in existStingTempFee)
                {
                    item.ISCURRENT = false;
                    item.DATETIMEUPDATED = DateTime.Now;
                }
            }

            var targetFee = this.context.TBL_FEE.Find(feeId);

            var unApprovedFeeEdit = context.TBL_TEMP_FEE.Where(x => x.ISCURRENT == true
            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending);

            TBL_TEMP_FEE tempFee;

            if (unApprovedFeeEdit.Any())
            {
                throw new Exception("Fee is already undergoing approval");
            }
            else
            {
                tempFee = new TBL_TEMP_FEE()
                {
                    FEENAME = feeModel.feeName,
                    ACCOUNTCATEGORYID = targetFee.ACCOUNTCATEGORYID,
                    FEETYPEID = feeModel.feeTypeId,
                    FEEINTERVALID = feeModel.feeIntervalId,
                    PRODUCTTYPEID = feeModel.productTypeId,
                    FEETARGETID = feeModel.feeTargetId,
                    ISINTEGRALFEE = feeModel.isIntegralFee,
                    GLACCOUNTID = feeModel.glAccountId,
                    FEEAMORTISATIONTYPEID = feeModel.feeAmortisationTypeId,
                    INCLUDECUTOFFDAY = feeModel.includeCutOffDay,
                    CUTOFFDAY = feeModel.cutOffDay,
                    COMPANYID = feeModel.companyId,
                    FEEDATE = DateTime.Now,

                    CREATEDBY = feeModel.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                context.TBL_TEMP_FEE.Add(tempFee);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.FeeUpdated,
                STAFFID = feeModel.createdBy,
                BRANCHID = (short)feeModel.userBranchId,
                DETAIL = $"Updated Fee: { feeModel.feeName } with fee account category '{feeModel.accountCategoryName}'",
                IPADDRESS = feeModel.userIPAddress,
                URL = feeModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = feeId
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 

            var output = this.SaveAll();

            var approvalEntity = new ApprovalViewModel
            {
                staffId = feeModel.createdBy,
                companyId = feeModel.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempFee.FEEID,
                operationId = (int)OperationsEnum.FeeCreation,
                BranchId = feeModel.userBranchId
            };
            var response = workFlow.LogForApproval(approvalEntity);

            return output;
        }

        public async Task<bool> GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.FeeCreation;

            var response = await workFlow.GoForApproval(entity);

            if (response.Item1)
            {
                return ApproveFee(entity.targetId, response.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveFee(int feeId, short approvalStatusId, UserInfo user)
        {
            var feeModel = context.TBL_TEMP_FEE.Find(feeId);
            var feeToUpdate = context.TBL_FEE.Where(x => x.ACCOUNTCATEGORYID == feeModel.ACCOUNTCATEGORYID);
            var existingFee = feeToUpdate.FirstOrDefault();

            //Update existing fee with tempFee record
            if (feeToUpdate.Any())
            {
                existingFee.FEENAME = feeModel.FEENAME;
                existingFee.ACCOUNTCATEGORYID = feeModel.ACCOUNTCATEGORYID;
                existingFee.FEETYPEID = feeModel.FEETYPEID;
                existingFee.FEEINTERVALID = feeModel.FEEINTERVALID;
                existingFee.PRODUCTTYPEID = feeModel.PRODUCTTYPEID;
                existingFee.FEETARGETID = feeModel.FEETARGETID;
                existingFee.ISINTEGRALFEE = feeModel.ISINTEGRALFEE;
                existingFee.GLACCOUNTID = feeModel.GLACCOUNTID;
                existingFee.FEEAMORTISATIONTYPEID = feeModel.FEEAMORTISATIONTYPEID;
                existingFee.INCLUDECUTOFFDAY = feeModel.INCLUDECUTOFFDAY;
                existingFee.CUTOFFDAY = feeModel.CUTOFFDAY;
                existingFee.COMPANYID = feeModel.COMPANYID;
                existingFee.FEEDATE = feeModel.FEEDATE;

                existingFee.CREATEDBY = feeModel.CREATEDBY;
                existingFee.DATETIMEUPDATED = DateTime.Now;
            }
            else //Insert a newfee record into the real fee table
            {
                var fee = new TBL_FEE()
                {
                    FEENAME = feeModel.FEENAME,
                    ACCOUNTCATEGORYID = feeModel.ACCOUNTCATEGORYID,
                    FEETYPEID = feeModel.FEETYPEID,
                    FEEINTERVALID = feeModel.FEEINTERVALID,
                    PRODUCTTYPEID = feeModel.PRODUCTTYPEID,
                    FEETARGETID = feeModel.FEETARGETID,
                    ISINTEGRALFEE = feeModel.ISINTEGRALFEE,
                    GLACCOUNTID = feeModel.GLACCOUNTID,
                    FEEAMORTISATIONTYPEID = feeModel.FEEAMORTISATIONTYPEID,
                    INCLUDECUTOFFDAY = feeModel.INCLUDECUTOFFDAY,
                    CUTOFFDAY = feeModel.CUTOFFDAY,
                    COMPANYID = feeModel.COMPANYID,
                    FEEDATE = DateTime.Now,
                    DATETIMECREATED = DateTime.Now
                };
                context.TBL_FEE.Add(fee);

            }

            feeModel.ISCURRENT = false;
            feeModel.APPROVALSTATUSID = approvalStatusId;
            feeModel.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.FeeApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Fee '{feeModel.FEENAME}' with fee account category '{feeModel.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }
            #endregion tbl_Product Fee

            #region Fee Related Lookups

            public IEnumerable<LookupViewModel> GetFeeAccountCategory()
        {
            return (from data in context.TBL_ACCOUNT_CATEGORY
                    where data.ACCOUNTCATEGORYID == (short)AccountCategoryEnum.Income || data.ACCOUNTCATEGORYID == (short)AccountCategoryEnum.Expense
                    //orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.ACCOUNTCATEGORYID,
                        lookupName = data.ACCOUNTCATEGORYNAME
                    });
        }

        public IEnumerable<LookupViewModel> GetFeeType()
        {
            //return (from data in context.tbl_Fee_Type
            //            //orderby data.FinType, data.Position
            //        select new LookupViewModel()
            //        {
            //            lookupId = data.FeeTypeId,
            //            lookupName = data.FeeTypeName
            //        });
            return null;
        }

        public IEnumerable<LookupViewModel> GetFeeInterval()
        {
            return (from data in context.TBL_FEE_INTERVAL
                        //orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.FEEINTERVALID,
                        lookupName = data.FEEINTERVALNAME
                    });
        }

        public IEnumerable<LookupViewModel> GetFeeTarget()
        {
            return (from data in context.TBL_FEE_TARGET
                    select new LookupViewModel()
                    {
                        lookupId = data.FEETARGETID,
                        lookupName = data.FEETARGETNAME
                    });
        }

        #endregion Fee Related Lookups
    }
}
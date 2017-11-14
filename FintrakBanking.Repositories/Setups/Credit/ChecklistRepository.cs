using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FintrakBanking.Interfaces.Setups;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(IChecklistRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChecklistRepository : IChecklistRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public ChecklistRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        #region Loan Checklist Definition

        public IEnumerable<ChecklistDefinitionViewModel> GetAllChecklistDefinition()
        {
            var data = (from a in context.TBL_CHECKLIST_DEFINITION
                        where a.DELETED == false
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            isActive = a.ISACTIVE,
                            isRequired = a.ISREQUIRED,
                            productId = a.PRODUCTID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            itemDescription = a.ITEMDESCRIPTION,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;

        }

        public IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByProductId(int productId)
        {
            var data = (from a in context.TBL_CHECKLIST_DEFINITION
                        where a.DELETED == false && a.PRODUCTID == productId
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            isActive = a.ISACTIVE,
                            isRequired = a.ISREQUIRED,
                            productId = a.PRODUCTID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            itemDescription = a.ITEMDESCRIPTION,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;

        }

        public IEnumerable<ChecklistItemViewModel> GetAllUnmappedChecklistItemsToApprovalLevelAndProduct(int approvalLevelId, int productId)
        {
            var dataList = (from data in context.TBL_CHECKLIST_DEFINITION
                            where data.PRODUCTID == productId && data.APPROVALLEVELID == approvalLevelId && data.DELETED == false //orderby account.AccountCode ascending, account.AccountName ascending
                            select data.CHECKLISTITEMID).ToList();

            var unmappedChecklistItems = (from data in context.TBL_CHECKLIST_ITEM
                                          where data.DELETED == false
                                          select new ChecklistItemViewModel
                                          {
                                              checkListItemId = data.CHECKLISTITEMID,
                                              checkListItemName = data.CHECKLISTITEMNAME,
                                              dateTimeCreated = data.DATETIMECREATED,
                                              createdBy = (int)data.CREATEDBY
                                          });

            if (dataList.Any())
            {
                unmappedChecklistItems = unmappedChecklistItems.Where(x => !dataList.Contains(x.checkListItemId));
            }

            return unmappedChecklistItems;
        }

        public List<ChecklistDefinitionViewModel> GetAllChecklistDefinitionById(int CheckListDefinitionId)
        {
            var data = (from a in context.TBL_CHECKLIST_DEFINITION
                        where a.DELETED == false && CheckListDefinitionId == a.CHECKLISTDEFINITIONID
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            approvalLevelId = a.APPROVALLEVELID,
                            isActive = a.ISACTIVE,
                            isRequired = a.ISREQUIRED,
                            productId = a.PRODUCTID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            itemDescription = a.ITEMDESCRIPTION,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByApprovalLevelAndProduct(int approvalLevelId, int productId)
        {
            var data = GetAllChecklistDefinition().Where(x => x.approvalLevelId == approvalLevelId && x.productId == productId).ToList();

            return data;
        }

        public IEnumerable<ChecklistDefinitionViewModel> GetUnmappedChecklistDefintionToApprovalLevel(int approvalLevelId)
        {
            var dataList = (from data in context.TBL_CHECKLIST_DEFINITION
                            where data.APPROVALLEVELID == approvalLevelId && data.DELETED == false //orderby account.AccountCode ascending, account.AccountName ascending
                            select data.CHECKLISTDEFINITIONID).ToList();

            var unmappedChecklistItems = (from data in context.TBL_CHECKLIST_DEFINITION
                                          where data.DELETED == false
                                          select new ChecklistDefinitionViewModel
                                          {
                                              checkListDefinitionId = data.CHECKLISTDEFINITIONID,
                                              approvalLevelId = data.APPROVALLEVELID,
                                              isActive = data.ISACTIVE,
                                              isRequired = data.ISREQUIRED,
                                              productId = data.PRODUCTID,
                                              productName = data.TBL_PRODUCT.PRODUCTNAME,
                                              checkListItemId = data.CHECKLISTITEMID,
                                              checkListItemName = data.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                                              itemDescription = data.ITEMDESCRIPTION,
                                              companyId = data.COMPANYID,
                                              companyName = data.TBL_COMPANY.NAME,
                                              dateTimeCreated = data.DATETIMECREATED,
                                              createdBy = data.CREATEDBY
                                          });

            if (dataList.Any())
            {
                unmappedChecklistItems = unmappedChecklistItems.Where(x => !dataList.Contains(x.checkListDefinitionId));
            }

            return unmappedChecklistItems;
        }

        public bool AddChecklistDefinition(ChecklistDefinitionViewModel model)
        {
            var data = new TBL_CHECKLIST_DEFINITION
            {
                APPROVALLEVELID = (int)model.approvalLevelId,
                CHECKLISTITEMID = model.checkListItemId,
                ITEMDESCRIPTION = model.itemDescription,
                ISREQUIRED = model.isRequired,
                COMPANYID = model.companyId,
                ISACTIVE = model.isActive,
                PRODUCTID = (short)model.productId,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            //Audit Section ---------------------------

            var audit_product = (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == data.PRODUCTID))?.PRODUCTNAME;
            var audit_checklist = (context.TBL_CHECKLIST_ITEM.FirstOrDefault(x => x.CHECKLISTITEMID == data.CHECKLISTITEMID))?.CHECKLISTITEMNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_CHECKLIST_DEFINITION.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }


        public bool AddMultipleChecklistDefinition(List<ChecklistDefinitionViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (ChecklistDefinitionViewModel model in models)
            {
                AddChecklistDefinition(model);
            }
            return true;
        }

        public bool AddMultipleChecklistDefinitionWithMultipleItems(ChecklistDefinitionViewModel model)
        {
            if (model.checklistItems.Count <= 0)
                return false;

            foreach (var item in model.checklistItems)
            {
                var data = new TBL_CHECKLIST_DEFINITION
                {
                    APPROVALLEVELID = (int)model.approvalLevelId,
                    PRODUCTID = (short)model.productId,
                    COMPANYID = model.companyId,
                    CHECKLISTITEMID = item.checkListItemId,
                    ITEMDESCRIPTION = item.itemDescription,
                    ISREQUIRED = item.isRequired,
                    ISACTIVE = item.isActive,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    CREATEDBY = (int)model.createdBy
                };

                //Audit Section ---------------------------

                var audit_product = (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == data.PRODUCTID)).PRODUCTNAME;
                var audit_checklist = (context.TBL_CHECKLIST_ITEM.FirstOrDefault(x => x.CHECKLISTITEMID == data.CHECKLISTITEMID)).CHECKLISTITEMNAME;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                context.TBL_CHECKLIST_DEFINITION.Add(data);
                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------

            }

            return context.SaveChanges() != 0;
        }

        public bool UpdateChecklistDefinition(int CheckListDefinitionId, ChecklistDefinitionViewModel model)
        {
            var data = this.context.TBL_CHECKLIST_DEFINITION.Find(CheckListDefinitionId);
            if (data == null) return false;
            data.APPROVALLEVELID = (int)model.approvalLevelId;
            data.ITEMDESCRIPTION = model.itemDescription;
            data.COMPANYID = model.companyId;
            data.CHECKLISTITEMID = model.checkListItemId;
            data.ISACTIVE = model.isActive;
            data.ISREQUIRED = model.isRequired;
            data.PRODUCTID = (short)model.productId;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit_product = (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == data.PRODUCTID)).PRODUCTNAME;
            var audit_checklist = (context.TBL_CHECKLIST_ITEM.FirstOrDefault(x => x.CHECKLISTITEMID == data.CHECKLISTITEMID)).CHECKLISTITEMNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        public bool DeleteChecklistDefinition(int CheckListDefinitionId, UserInfo user)
        {
            var data = this.context.TBL_CHECKLIST_DEFINITION.Find(CheckListDefinitionId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.staffId;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------

            var audit_product = (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == data.PRODUCTID)).PRODUCTNAME;
            var audit_checklist = (context.TBL_CHECKLIST_ITEM.FirstOrDefault(x => x.CHECKLISTITEMID == data.CHECKLISTITEMID)).CHECKLISTITEMNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region Checklist Details
        public IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail()
        {
            var data = (from a in context.TBL_CHECKLIST_DETAIL
                        where a.DELETED == false
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.CHECKLISTID,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            checkListDefinitionItemName = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TBL_CHECKLIST_TARGETTYPE.TARGETTYPENAME,
                            targetId = a.TARGETID,
                            checkListStatusId = a.CHECKLISTSTATUSID,
                            checkListStatusName = a.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                            checkedBy = a.CHECKEDBY,
                            deferedDate = a.DEFEREDDATE,
                            remark = a.REMARK,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailById(int ChecklistId)
        {
            var data = (from a in context.TBL_CHECKLIST_DETAIL
                        where a.DELETED == false && ChecklistId == a.CHECKLISTID
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.CHECKLISTID,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            checkListDefinitionItemName = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TBL_CHECKLIST_TARGETTYPE.TARGETTYPENAME,
                            targetId = a.TARGETID,
                            checkListStatusId = a.CHECKLISTSTATUSID,
                            checkedBy = a.CHECKEDBY,
                            deferedDate = a.DEFEREDDATE,
                            remark = a.REMARK,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailByProductAndTargetId(int targetTypeId, int productId)
        {
            var data = (from a in context.TBL_CHECKLIST_DETAIL
                        where a.DELETED == false && a.TARGETTYPEID == targetTypeId && a.TARGETID == productId
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.CHECKLISTID,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            checkListDefinitionItemName = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TBL_CHECKLIST_TARGETTYPE.TARGETTYPENAME,
                            targetId = a.TARGETID,
                            checkListStatusId = a.CHECKLISTSTATUSID,
                            checkedBy = a.CHECKEDBY,
                            deferedDate = a.DEFEREDDATE,
                            remark = a.REMARK,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailByProductId(int productId)
        {
            var data = (from a in context.TBL_CHECKLIST_DETAIL
                        where a.DELETED == false && a.TARGETID == productId
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.CHECKLISTID,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            checkListDefinitionItemName = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TBL_CHECKLIST_TARGETTYPE.TARGETTYPENAME,
                            targetId = a.TARGETID,
                            checkListStatusId = a.CHECKLISTSTATUSID,
                            checkedBy = a.CHECKEDBY,
                            deferedDate = a.DEFEREDDATE,
                            remark = a.REMARK,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailByChecklistDefinitionId(int checklistDefinitionId)
        {
            var data = (from a in context.TBL_CHECKLIST_DETAIL
                        where a.DELETED == false && a.CHECKLISTDEFINITIONID == checklistDefinitionId
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.CHECKLISTID,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            checkListDefinitionItemName = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TBL_CHECKLIST_TARGETTYPE.TARGETTYPENAME,
                            targetId = a.TARGETID,
                            checkListStatusId = a.CHECKLISTSTATUSID,
                            checkedBy = a.CHECKEDBY,
                            deferedDate = a.DEFEREDDATE,
                            remark = a.REMARK,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }
        public IEnumerable<ChecklistDetailViewModel> GetChecklistByTargetId(int targetId)
        {
            var checkList = (from cl in context.TBL_CHECKLIST_DETAIL
                             where cl.TARGETID == targetId && cl.TARGETTYPEID == (int)CheckListTargetTypeEnum.Loan 
                             && cl.DELETED == false
                             select new ChecklistDetailViewModel()
                             {
                                 checklistId = cl.CHECKLISTID,
                                 checkListDefinitionId = cl.CHECKLISTDEFINITIONID,
                                 remark = cl.REMARK,
                                 checkedBy = cl.CHECKEDBY,
                                 targetTypeId = cl.TARGETTYPEID,
                                 targetId = cl.TARGETID,
                                 checkListStatusId = cl.CHECKLISTSTATUSID,
                                 deferedDate = cl.DEFEREDDATE

                             }).ToList();
            return checkList;
        }
        public bool AddMultipleChecklistDetails(List<ChecklistDetailViewModel> models, int staffId, short BranchId)
        {
            if (models.Count <= 0)
                return false;

            int loanApplicationDetailId = models.FirstOrDefault().targetId;
            int loanApplicationId = (int)models.FirstOrDefault().checklistId;
            foreach (ChecklistDetailViewModel model in models)
            {
                model.createdBy = staffId;
                model.checkedBy = staffId;
                model.targetTypeId = (int)CheckListTargetTypeEnum.Loan;
                model.userBranchId = BranchId;
                model.remark = "Remark";
                AddChecklistDetail(model);

            }
            var loanDetailsData = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            if (loanDetailsData != null)
            {
                loanDetailsData.HASDONECHECKLIST = true;
            }
            var loanData = (from l in context.TBL_LOAN_APPLICATION_DETAIL where l.LOANAPPLICATIONID == loanApplicationId select l).ToList();
            if (loanData != null)
            {
                var custNo = loanData.Count();
                var checkedNo = 0;
                foreach (var item in loanData)
                {
                    if (item.HASDONECHECKLIST == true)
                    {
                        ++checkedNo;
                    }
                }
                if (custNo == checkedNo)
                {
                    var loanApplication = context.TBL_LOAN_APPLICATION.Find(loanApplicationId);
                    if (loanApplication != null)
                    {
                        loanApplication.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ChecklistCompleted;
                    }
                }

            }
            return context.SaveChanges() != 0; ;
        }
        public bool AddChecklistDetail(ChecklistDetailViewModel model)
        {
            var data = new TBL_CHECKLIST_DETAIL
            {
                CHECKLISTDEFINITIONID = model.checkListDefinitionId,
                TARGETTYPEID = (int)CheckListTargetTypeEnum.Loan,
                TARGETID = model.targetId,
                CHECKLISTSTATUSID = model.checkListStatusId,
                CHECKEDBY = (int)model.createdBy,
                DEFEREDDATE = model.deferedDate,
                REMARK = model.remark,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit_checklist = (context.TBL_CHECKLIST_DEFINITION.FirstOrDefault(x => x.CHECKLISTDEFINITIONID == data.CHECKLISTDEFINITIONID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Checklist {audit_checklist.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME}", // on Loan '{data.LoanId}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_CHECKLIST_DETAIL.Add(data);
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------



            var loanDetailsData = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.targetId);
            if (loanDetailsData != null)
            {
                var productCheckListCount = (from dd in context.TBL_CHECKLIST_DEFINITION
                                             where dd.PRODUCTID == loanDetailsData.PROPOSEDPRODUCTID
                                             select dd).Count();

                var loanCheckListCount = (from dd in context.TBL_CHECKLIST_DETAIL
                                          where dd.TARGETID == loanDetailsData.LOANAPPLICATIONDETAILID
                                          && dd.TARGETTYPEID == (int)CheckListTargetTypeEnum.Loan
                                          select dd).Count();

                if (productCheckListCount == (loanCheckListCount + 1))
                {
                    loanDetailsData.HASDONECHECKLIST = true;
                }
            }
            var loanData = (from l in context.TBL_LOAN_APPLICATION_DETAIL where l.LOANAPPLICATIONID == model.checklistId select l).ToList();
            if (loanData != null)
            {
                var custNo = loanData.Count();
                var checkedNo = 0;
                foreach (var item in loanData)
                {
                    if (item.HASDONECHECKLIST == true)
                    {
                        ++checkedNo;
                    }
                }
                if (custNo == checkedNo)
                {
                    var loanApplication = context.TBL_LOAN_APPLICATION.Find(model.checklistId);
                    if (loanApplication != null)
                    {
                        loanApplication.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ChecklistCompleted;
                    }
                }

            }

            return context.SaveChanges() != 0;
        }

        public bool UpdateChecklistDetail(int ChecklistId, ChecklistDetailViewModel model)
        {
            var data = this.context.TBL_CHECKLIST_DETAIL.Find(ChecklistId);
            if (data == null) return false;

            data.CHECKLISTDEFINITIONID = model.checkListDefinitionId;
            data.TARGETTYPEID = model.targetTypeId;
            data.TARGETID = model.targetId;
            data.CHECKLISTSTATUSID = model.checkListStatusId;
            data.CHECKEDBY = (int)model.createdBy;
            data.DEFEREDDATE = model.deferedDate;
            data.REMARK = model.remark;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            // Audit Section ---------------------------
            var audit_checklist = (context.TBL_CHECKLIST_DEFINITION.FirstOrDefault(x => x.CHECKLISTDEFINITIONID == data.CHECKLISTDEFINITIONID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Checklist {audit_checklist.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteChecklistDetail(int ChecklistId, UserInfo user)
        {
            var data = context.TBL_CHECKLIST_DETAIL.Find(ChecklistId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.staffId;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_checklist = (context.TBL_CHECKLIST_DEFINITION.FirstOrDefault(x => x.
             CHECKLISTDEFINITIONID == context.TBL_CHECKLIST_DETAIL.Find(ChecklistId).CHECKLISTDEFINITIONID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Added Loan Checklist {audit_checklist.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region Checklist Item
        public IEnumerable<ChecklistItemViewModel> GetAllChecklistItem()
        {
            var data = (from a in context.TBL_CHECKLIST_ITEM
                        where a.DELETED == false
                        select new ChecklistItemViewModel
                        {
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.CHECKLISTITEMNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<ChecklistItemViewModel> GetAllChecklistItemById(int CheckListItemId)
        {
            var data = (from a in context.TBL_CHECKLIST_ITEM
                        where a.DELETED == false && CheckListItemId == a.CHECKLISTITEMID
                        select new ChecklistItemViewModel
                        {
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.CHECKLISTITEMNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public bool AddChecklistItem(ChecklistItemViewModel model)
        {
            var data = new TBL_CHECKLIST_ITEM
            {
                CHECKLISTITEMNAME = model.checkListItemName,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChecklistItemAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Checklist Item Item '{model.checkListItemName}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_CHECKLIST_ITEM.Add(data);
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddMultipleChecklistItem(List<ChecklistItemViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (ChecklistItemViewModel model in models)
            {
                AddChecklistItem(model);
            }

            return true;
        }

        public bool UpdateChecklistItem(int CheckListItemId, ChecklistItemViewModel model)
        {
            var data = this.context.TBL_CHECKLIST_ITEM.Find(CheckListItemId);
            if (data == null) return false;

            data.CHECKLISTITEMNAME = model.checkListItemName;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChecklistItemUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Checklist Item '{model.checkListItemName}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteChecklistItem(int CheckListItemId, UserInfo user)
        {
            var data = context.TBL_CHECKLIST_ITEM.Find(CheckListItemId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.staffId;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChecklistItemDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Checklist Item '{data.CHECKLISTITEMNAME}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region CheckList Select List
        public IEnumerable<CheckListStatusViewModel> GetAllChecklistStatus()
        {
            var data = (from a in context.TBL_CHECKLIST_STATUS
                        where a.DELETED == false
                        select new CheckListStatusViewModel
                        {
                            checklistStatusId = a.CHECKLISTSTATUSID,
                            checklistStatusName = a.CHECKLISTSTATUSNAME,
                        }).ToList();
            return data;
        }

        public IEnumerable<CheckListTargetTypeViewModel> GetAllChecklistTargetType()
        {
            var data = (from a in context.TBL_CHECKLIST_TARGETTYPE
                        where a.DELETED == false
                        select new CheckListTargetTypeViewModel
                        {
                            targetTypeId = a.TARGETTYPEID,
                            targetTypeName = a.TARGETTYPENAME
                        }).ToList();
            return data;
        }
        #endregion

    }
}

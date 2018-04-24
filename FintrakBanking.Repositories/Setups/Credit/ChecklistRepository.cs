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
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Interfaces.Setups.Approval;

namespace FintrakBanking.Repositories.Credit
{
    public class ChecklistRepository : IChecklistRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;

        public ChecklistRepository(FinTrakBankingContext _context, IApprovalLevelStaffRepository _level,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail, IWorkflow _workFlow)
        {
            this.context = _context;
            this._genSetup = genSetup;
            this.auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            this.level = _level;
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
        public IEnumerable<ChecklistDefinitionAndDetailViewModel> GetChecklistDefinitionByApprovalLevelCheckListType(int staffId, int? productId, int loanTargetId, int operationId, int checkListTypeId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            
            List <CheckListStatusViewModel> responseTypes = new List<CheckListStatusViewModel>();
            var detailItem = (from s in context.TBL_CHECKLIST_DETAIL
                              join k in context.TBL_CHECKLIST_DEFINITION
                              on s.CHECKLISTDEFINITIONID equals k.CHECKLISTDEFINITIONID
                              where s.TARGETID == loanTargetId && k.CHECKLIST_TYPEID == checkListTypeId
                              select new ChecklistDefinitionAndDetailViewModel
                              {
                                  checkListDetailId = s.CHECKLISTID,
                                  checkListDefinitionId = s.CHECKLISTDEFINITIONID,
                                  responseTypeId = k.TBL_CHECKLIST_ITEM.RESPONSE_TYPEID,
                                  requireUpload = k.TBL_CHECKLIST_ITEM.REQUIREUPLOAD,
                                  checkListTypeId = k.CHECKLIST_TYPEID,
                                  checkListTypeName = k.TBL_CHECKLIST_TYPE.CHECKLIST_TYPE_NAME,
                                  checkListItemId = k.CHECKLISTITEMID,
                                  checkListItemName = k.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                                  itemDescription = k.ITEMDESCRIPTION,
                                  checklistStatusId = s.CHECKLISTSTATUSID,
                                  responseTypes = context.TBL_CHECKLIST_STATUS.Where(x => x.RESPONSE_TYPEID == k.TBL_CHECKLIST_ITEM.RESPONSE_TYPEID).
                            Select(x => new CheckListStatusViewModel()
                            {
                                checklistStatusId = x.CHECKLISTSTATUSID,
                                checklistStatusName = x.CHECKLISTSTATUSNAME,
                            }).ToList()
                              });
            var proposedProductId = (from id in context.TBL_LOAN_APPLICATION_DETAIL where id.LOANAPPLICATIONID == loanTargetId select (short?)id.PROPOSEDPRODUCTID).ToList();
            var isproductBased = context.TBL_CHECKLIST_TYPE.FirstOrDefault(x => x.CHECKLIST_TYPEID == checkListTypeId).ISPRODUCT_BASED;

            var data = (from a in context.TBL_CHECKLIST_DEFINITION
                        join d in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals d.CHECKLISTITEMID
                        where ids.Contains((int)a.APPROVALLEVELID) && a.CHECKLIST_TYPEID == checkListTypeId 
                        && a.OPERATIONID == operationId && a.DELETED == false
                        select new ChecklistDefinitionAndDetailViewModel
                        {
                           checkListDetailId = 0,
                            checkListDefinitionId = a.CHECKLISTDEFINITIONID,
                            responseTypeId = d.RESPONSE_TYPEID,
                            requireUpload = d.REQUIREUPLOAD,
                            checkListTypeId = a.CHECKLIST_TYPEID,
                            checkListTypeName = a.TBL_CHECKLIST_TYPE.CHECKLIST_TYPE_NAME,
                            checkListItemId = a.CHECKLISTITEMID,
                            checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                            itemDescription = a.ITEMDESCRIPTION,
                            productId = a.PRODUCTID,
                            responseTypes = context.TBL_CHECKLIST_STATUS.Where(x => x.RESPONSE_TYPEID == d.RESPONSE_TYPEID).
                            Select(x => new CheckListStatusViewModel()
                            {
                                checklistStatusId = x.CHECKLISTSTATUSID,
                                checklistStatusName = x.CHECKLISTSTATUSNAME,
                            }).ToList()

                        });
            if (isproductBased)
            {
                if (productId > 0)
                {
                    data = data.Where(x => x.productId == productId);
                }
                else if (proposedProductId.Any())
                {
                    data = data.Where(x => proposedProductId.Contains(x.productId));
                }
            }
            var definitionList = data.ToList();
            var detailList = detailItem.ToList();
            var detailId = detailItem.Select(a => a.checkListDefinitionId).ToList();
            if (detailItem.Any())
            {
                var checklist = detailList.Concat(definitionList.Where(x => !detailId.Contains(x.checkListDefinitionId)));
                return checklist.ToList();
            }
            return data.ToList();

        }
        //    public IEnumerable<ChecklistDefinitionViewModel> GetChecklistDefinitionByApprovalLevelCheckListType(int staffId, int? productId, int loanTargetId, int operationId, int checkListTypeId)
        //{
        //    var detailItem = (from s in context.TBL_CHECKLIST_DETAIL
        //                      join k in context.TBL_CHECKLIST_DEFINITION
        //                      on s.CHECKLISTDEFINITIONID equals k.CHECKLISTDEFINITIONID
        //                      where s.TARGETID == loanTargetId && k.CHECKLIST_TYPEID == checkListTypeId
        //                      select s.CHECKLISTDEFINITIONID).ToList();
        //    var proposedProductId = (from id in context.TBL_LOAN_APPLICATION_DETAIL where id.LOANAPPLICATIONID == loanTargetId select (short?)id.PROPOSEDPRODUCTID).ToList();
        //    var isproductBased = context.TBL_CHECKLIST_TYPE.FirstOrDefault(x => x.CHECKLIST_TYPEID == checkListTypeId).ISPRODUCT_BASED;
        //    if (isproductBased)
        //    {
        //        var data = (from a in context.TBL_CHECKLIST_DEFINITION
        //                    join d in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals d.CHECKLISTITEMID
        //                    join b in context.TBL_APPROVAL_LEVEL_STAFF on
        //                    a.APPROVALLEVELID equals b.APPROVALLEVELID
        //                    where b.STAFFID == staffId && a.CHECKLIST_TYPEID == checkListTypeId
        //                    where a.CHECKLIST_TYPEID == checkListTypeId
        //                    && a.OPERATIONID == operationId && a.DELETED == false // && (productId == a.PRODUCTID || productId == null)
        //                    select new ChecklistDefinitionViewModel
        //                    {
        //                        checkListDefinitionId = a.CHECKLISTDEFINITIONID,
        //                        approvalLevelId = a.APPROVALLEVELID,
        //                        approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
        //                        isActive = a.ISACTIVE,
        //                        isRequired = a.ISREQUIRED,
        //                        productId = a.PRODUCTID,
        //                        responseTypeId = d.RESPONSE_TYPEID,
        //                        requireUpload = d.REQUIREUPLOAD,
        //                        checkListTypeId = a.CHECKLIST_TYPEID,
        //                        checkListTypeName = a.TBL_CHECKLIST_TYPE.CHECKLIST_TYPE_NAME,
        //                        productName = a.TBL_PRODUCT.PRODUCTNAME,
        //                        checkListItemId = a.CHECKLISTITEMID,
        //                        checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
        //                        itemDescription = a.ITEMDESCRIPTION,
        //                        companyId = a.COMPANYID,
        //                        companyName = a.TBL_COMPANY.NAME,
        //                        dateTimeCreated = a.DATETIMECREATED,
        //                        createdBy = a.CREATEDBY
        //                    });
        //        if (detailItem.Any())
        //        {
        //            data = data.Where(x => !detailItem.Contains(x.checkListDefinitionId));
        //        }
        //        if (productId > 0)
        //        {
        //            data = data.Where(x => x.productId == productId);
        //        }
        //        else if (proposedProductId.Any())
        //        {
        //            data = data.Where(x => proposedProductId.Contains(x.productId));
        //        }

        //        return data;
        //    }
        //    else
        //    {
        //        var data = (from a in context.TBL_CHECKLIST_DEFINITION
        //                    join d in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals d.CHECKLISTITEMID
        //                    join b in context.TBL_APPROVAL_LEVEL_STAFF on
        //                    a.APPROVALLEVELID equals b.APPROVALLEVELID
        //                    where b.STAFFID == staffId && a.CHECKLIST_TYPEID == checkListTypeId
        //                    where a.CHECKLIST_TYPEID == checkListTypeId
        //                && a.OPERATIONID == operationId && a.DELETED == false
        //                    select new ChecklistDefinitionViewModel
        //                    {
        //                        checkListDefinitionId = a.CHECKLISTDEFINITIONID,
        //                        approvalLevelId = a.APPROVALLEVELID,
        //                        approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
        //                        isActive = a.ISACTIVE,
        //                        isRequired = a.ISREQUIRED,
        //                        productId = a.PRODUCTID,
        //                        checkListTypeId = a.CHECKLIST_TYPEID,
        //                        checkListTypeName = a.TBL_CHECKLIST_TYPE.CHECKLIST_TYPE_NAME,
        //                        productName = a.TBL_PRODUCT.PRODUCTNAME,
        //                        checkListItemId = a.CHECKLISTITEMID,
        //                        requireUpload = d.REQUIREUPLOAD,
        //                        responseTypeId = d.RESPONSE_TYPEID,
        //                        checkListItemName = a.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
        //                        itemDescription = a.ITEMDESCRIPTION,
        //                        companyId = a.COMPANYID,
        //                        companyName = a.TBL_COMPANY.NAME,
        //                        dateTimeCreated = a.DATETIMECREATED,
        //                        createdBy = a.CREATEDBY
        //                    });
        //        if (detailItem.Any())
        //        {
        //            data = data.Where(x => !detailItem.Contains(x.checkListDefinitionId));
        //        }
        //        return data;
        //    }
        //}

        public IEnumerable<CheckListTargetTypeViewModel> GetAllChecklistType()
        {

            var checkListTypeList = (from a in context.TBL_CHECKLIST_TYPE
                                     select new CheckListTargetTypeViewModel
                                     {
                                         targetTypeId = a.CHECKLIST_TYPEID,
                                         targetTypeName = a.CHECKLIST_TYPE_NAME,
                                     }).ToList();
            return checkListTypeList;
        }
        public IEnumerable<CheckListTargetTypeViewModel> GetChecklistTypeByApprovalLevel(int staffId, int companyId, int operationId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();
           
            var checkType = (from a in context.TBL_CHECKLIST_TYPE
                             join b in context.TBL_CHECKLIST_TYPE_APROV_LEVL on a.CHECKLIST_TYPEID equals b.CHECKLIST_TYPEID
                             where ids.Contains((int)b.APPROVALLEVELID)
                             select new CheckListTargetTypeViewModel
                             {
                                 targetTypeId = a.CHECKLIST_TYPEID,
                                 targetTypeName = a.CHECKLIST_TYPE_NAME,
                                 isproductbased = a.ISPRODUCT_BASED,
                                 canValidateChecklist = b.CANVALIDATE
                             });
            var debug = checkType.ToList();
            return checkType.GroupBy(x => x.targetTypeId).Select(y => y.FirstOrDefault()).ToList();
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
                            operationId = a.OPERATIONID,
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
                            operationId = a.OPERATIONID,
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
                                              operationId = data.OPERATIONID,
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
                OPERATIONID = model.operationId,
                APPROVALLEVELID = (int)model.approvalLevelId,
                CHECKLISTITEMID = model.checkListItemId,
                CHECKLIST_TYPEID = (short)model.checkListTypeId,
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
                    OPERATIONID = model.operationId,
                    APPROVALLEVELID = (int)model.approvalLevelId,
                    PRODUCTID = (short)model.productId,
                    COMPANYID = model.companyId,
                    CHECKLIST_TYPEID = (short)model.checkListTypeId,
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
            data.CHECKLIST_TYPEID = (short)model.checkListTypeId;
            data.ISACTIVE = model.isActive;
            data.ISREQUIRED = model.isRequired;
            data.PRODUCTID = (short)model.productId;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;
            data.OPERATIONID = model.operationId;
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
                             where cl.TARGETID == targetId && cl.TARGETTYPEID == (int)CheckListTargetTypeEnum.LoanApplicationProductChecklist
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
        public IEnumerable<ChecklistDetailViewModel> GetChecklistByCheckListTypeAndTargetId(int targetId, int checkListtypeId, bool isCamChecklist)
        {
            var isproductBased = context.TBL_CHECKLIST_TYPE.Where(x => x.CHECKLIST_TYPEID == checkListtypeId).Select(k => k.ISPRODUCT_BASED).FirstOrDefault();
            if (isproductBased)
            {
                var detailId = (from id in context.TBL_LOAN_APPLICATION_DETAIL where id.LOANAPPLICATIONID == targetId select id.LOANAPPLICATIONDETAILID).ToList();
                var checkList = (from cl in context.TBL_CHECKLIST_DETAIL
                                 join def in context.TBL_CHECKLIST_DEFINITION
                                  on cl.CHECKLISTDEFINITIONID equals def.CHECKLISTDEFINITIONID
                                 where def.CHECKLIST_TYPEID == checkListtypeId
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
                                     deferedDate = cl.DEFEREDDATE,
                                     checkListValidationStatus1 = cl.CHECKLISTSTATUSID2,
                                     checkListValidationStatus2 = cl.CHECKLISTSTATUSID3,
                                     checkListStatusName = cl.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                                     checkListDefinitionItemName = cl.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME
                                 }).ToList();
                if (isCamChecklist)
                {
                    checkList = checkList.Where(x => x.targetId == targetId).ToList();
                }
                else
                {
                    checkList = checkList.Where(x => detailId.Contains(x.targetId)).ToList();
                }
                return checkList;
            }
            else
            {
                var checkList = (from cl in context.TBL_CHECKLIST_DETAIL
                                 join def in context.TBL_CHECKLIST_DEFINITION
                                  on cl.CHECKLISTDEFINITIONID equals def.CHECKLISTDEFINITIONID
                                 where cl.TARGETID == targetId && def.CHECKLIST_TYPEID == checkListtypeId
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
                                     deferedDate = cl.DEFEREDDATE,
                                     checkListValidationStatus1 = cl.CHECKLISTSTATUSID2,
                                     checkListValidationStatus2 = cl.CHECKLISTSTATUSID3,
                                     checkListStatusName = cl.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                                     checkListDefinitionItemName = cl.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME

                                 }).ToList();
                return checkList;
            }
            return null;
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
                model.targetTypeId = (int)CheckListTargetTypeEnum.LoanApplicationProductChecklist;
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
            return context.SaveChanges() > 0;
        }
        public bool AddChecklistDetail(ChecklistDetailViewModel model)
        {
            if (model != null)
            {
                try
                {
                    var isProductBased = (from a in context.TBL_CHECKLIST_DEFINITION
                                          join b in context.TBL_CHECKLIST_TYPE on
                                          a.CHECKLIST_TYPEID equals b.CHECKLIST_TYPEID
                                          where a.CHECKLISTDEFINITIONID == model.checkListDefinitionId
                                          select b.ISPRODUCT_BASED).FirstOrDefault();
                    if (isProductBased)
                    {
                        model.targetTypeId = (int)CheckListTargetTypeEnum.LoanApplicationProductChecklist;
                    }
                    else
                    {
                        model.targetTypeId = (int)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist;
                    }
                    TBL_CHECKLIST_DETAIL data;
                    if (model.checklistId != 0 || model.checklistId > 0)
                    {
                        data = context.TBL_CHECKLIST_DETAIL.Find(model.checklistId);
                        if (data != null)
                        {
                            data.CHECKLISTSTATUSID = model.checkListStatusId;
                            data.DATETIMEUPDATED = DateTime.Now;
                        }
                    }
                    else
                    {
                        data = new TBL_CHECKLIST_DETAIL
                        {
                            CHECKLISTDEFINITIONID = model.checkListDefinitionId,
                            TARGETTYPEID = model.targetTypeId,
                            TARGETID = model.targetId,
                            CHECKLISTSTATUSID = model.checkListStatusId,
                            CHECKEDBY = (int)model.createdBy,
                            DEFEREDDATE = model.deferedDate,
                            REMARK = model.remark,
                            DATETIMECREATED = _genSetup.GetApplicationDate(),
                            CREATEDBY = (int)model.createdBy
                        };
                        context.TBL_CHECKLIST_DETAIL.Add(data);
                    }
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
                    this.auditTrail.AddAuditTrail(audit);

                    //end of Audit section -------------------------------
                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return false;
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
            if (data != null)
            {
                this.context.TBL_CHECKLIST_DETAIL.Remove(data);
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Added Loan Checklist with Id: {ChecklistId}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool ValidateChecklistDetail(ValidateChecklistDetailViewModel entity)
        {
            if (entity == null) return false;

            var data = this.context.TBL_CHECKLIST_DETAIL.Find(entity.checklistId);
            if (data == null) return false;
            if (entity.isCAMchecklist == true)
            {
                data.CHECKLISTSTATUSID2 = entity.checkListStatusId2;
            }
            if (entity.isAvailmentChecklist == true)
            {
                data.CHECKLISTSTATUSID3 = entity.checkListStatusId3;
            }
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
                            responseTypeName = a.TBL_CHECKLIST_RESPONSE_TYPE.RESPONSE_TYPE_NAME,
                            requireUpload = a.REQUIREUPLOAD,
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
                            requireUpload = a.REQUIREUPLOAD,
                            responseTypeName = a.TBL_CHECKLIST_RESPONSE_TYPE.RESPONSE_TYPE_NAME,
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
                RESPONSE_TYPEID = model.responseTypeId,
                REQUIREUPLOAD = model.requireUpload,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy,
                DELETED = false
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

            data.REQUIREUPLOAD = model.requireUpload;
            data.RESPONSE_TYPEID = model.responseTypeId;
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
                            responseTypeId = a.RESPONSE_TYPEID
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
        public IEnumerable<CheckListResponseTypeViewModel> GetAllChecklistResponseType()
        {
            var data = (from a in context.TBL_CHECKLIST_RESPONSE_TYPE
                        select new CheckListResponseTypeViewModel
                        {
                            responseId = a.RESPONSE_TYPEID,
                            responseName = a.RESPONSE_TYPE_NAME
                        }).ToList();
            return data;
        }
        #endregion


        #region Checklist entry validation
        public bool ValidateChecklistDetailEntry(int checklistDefinitionId, int targetId)
        {
            var data = (from c in context.TBL_CHECKLIST_DETAIL
                        where c.CHECKLISTDEFINITIONID == checklistDefinitionId
                        && c.TARGETID == targetId
                        select c).ToList();
            if (data.Any())
            {
                return true;
            }
            return false;
        }
        public bool ValidateChecklistForDefferalOrWaival(int conditionId)
        {
            var data = context.TBL_LOAN_CONDITION_PRECEDENT.Find(conditionId);
            if (data.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved &&
                (data.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred || data.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Waived))
            {
                return true;
            }
            return false;
        }
        #endregion


        #region Condition Precedence Checklist
        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklist(int loanApplicationId, bool isAvailment)
        {
            if (isAvailment)
            {
                var condition = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                                 where c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId && c.ISSUBSEQUENT == false
                                 select new ConditionPrecedentViewModel()
                                 {
                                     condition = c.CONDITION,
                                     conditionId = c.LOANCONDITIONID,
                                     loanApplicationId = c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                     loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                     isExternal = c.ISEXTERNAL,
                                     responseTypeId = c.RESPONSE_TYPEID,
                                     checkListStatusId = c.CHECKLISTSTATUSID,
                                     validationStatus = c.CHECKLISTVALIDATED,
                                     approvalStatusId = c.APPROVALSTATUSID,

                                 }).ToList();
                return condition;
            }
            else
            {
                var condition = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                                 where c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId && c.ISEXTERNAL == true && c.ISSUBSEQUENT == false &&
                                  c.CHECKLISTSTATUSID == null
                                 select new ConditionPrecedentViewModel()
                                 {
                                     condition = c.CONDITION,
                                     conditionId = c.LOANCONDITIONID,
                                     loanApplicationId = c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                     loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                     isExternal = c.ISEXTERNAL,
                                     responseTypeId = c.RESPONSE_TYPEID,
                                     checkListStatusId = c.CHECKLISTSTATUSID,
                                     checkListValidated = c.CHECKLISTVALIDATED,
                                     approvalStatusId = c.APPROVALSTATUSID,

                                 }).ToList();
                return condition;
            }
        }
        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklistStatus(int loanApplicationId, bool isAvailment)
        {
            if (isAvailment)
            {
                var status = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                              where c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId &&
                             c.ISSUBSEQUENT == false
                              orderby c.ISEXTERNAL descending
                              select new ConditionPrecedentViewModel()
                              {
                                  condition = c.CONDITION,
                                  conditionId = c.LOANCONDITIONID,
                                  status = c.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                                  approvalStatus = c.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  validationStatus = c.CHECKLISTVALIDATED,
                                  isExternal = c.ISEXTERNAL

                              }).ToList();
                return status;
            }
            else
            {
                var status = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                              where c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId &&
                               c.CHECKLISTSTATUSID != null
                              orderby c.ISEXTERNAL descending
                              select new ConditionPrecedentViewModel()
                              {
                                  condition = c.CONDITION,
                                  conditionId = c.LOANCONDITIONID,
                                  status = c.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                                  approvalStatus = c.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  validationStatus = c.CHECKLISTVALIDATED,
                                  isExternal = c.ISEXTERNAL
                              }).ToList();
                return status;
            }
        }
        public bool UpdateLoanConditionPrecedenceStatus(ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(model.conditionId);
            if (data == null) return false;

            data.CHECKLISTSTATUSID = model.checkListStatusId;
            data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            if (model.checkListStatusId == (int)CheckListStatusEnum.Deferred || model.checkListStatusId == (int)CheckListStatusEnum.Waived)
            {
                data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                data.DEFEREDDATE = model.deferedDate;
            }

            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;
            if (model.checkListStatusId == (int)CheckListStatusEnum.Deferred || model.checkListStatusId == (int)CheckListStatusEnum.Deferred)
            {
                var deferral = new TBL_LOAN_CONDITION_DEFERRAL();
                deferral.LOANCONDITIONID = data.LOANCONDITIONID;
                deferral.DEFERRALREASON = model.reason;
                deferral.DEFERREDDATE = model.deferedDate == null ? DateTime.Now : (DateTime)model.deferedDate;
                deferral.DATETIMECREATED = DateTime.Now;
                deferral.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                deferral.CREATEDBY = model.createdBy;
                context.TBL_LOAN_CONDITION_DEFERRAL.Add(deferral);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Condition Precedence Checklist with Condition ID: {model.condition}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {

                    var output = context.SaveChanges() != 0;

                    if (model.checkListStatusId == (int)CheckListStatusEnum.Deferred || model.checkListStatusId == (int)CheckListStatusEnum.Waived)
                    {
                        var approvalModel = new ApprovalViewModel
                        {
                            staffId = model.createdBy,
                            companyId = model.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = data.LOANCONDITIONID,
                            operationId = (int)OperationsEnum.ChecklistApproval,
                            BranchId = model.userBranchId,
                            comment = "Initiation",
                            externalInitialization = true
                        };
                        var response = workFlow.LogForApproval(approvalModel);
                    }
                    trans.Commit();
                    return output;
                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    return false;
                    throw new Exception(ex.Message);
                }
            }
        }
        public IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId);
            int staffApprovalLevelId = 0;
            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                        join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        && atrail.OPERATIONID == (int)OperationsEnum.ChecklistApproval
                        && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                        && atrail.RESPONSESTAFFID == null
                        orderby a.DATETIMECREATED descending
                        select new ChecklistApprovalViewModel()
                        {
                            customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            proposedAmount = a.APPROVEDAMOUNT,
                            approvalStatus = b.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            deferredDate = b.DEFEREDDATE,
                            deferralDuration = 1,
                            cummulativeDays = 1,
                            condition = b.CONDITION,
                            conditionId = b.LOANCONDITIONID,
                            loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            checklistStatus = b.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                            dateCreated = b.DATETIMECREATED,
                            //Loan Information
                            relationshipOfficerName = a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME,
                            relationshipManagerName = a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME,
                            applicationAmount = a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                            applicationTenor = a.PROPOSEDTENOR,
                            applicationDate = a.TBL_LOAN_APPLICATION.APPLICATIONDATE,
                            isInvestmentGrade = a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                            isPoliticallyExposed = a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                            isRelatedParty = a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                            approvalStatusId = a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                            applicationStatusId = a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                            submittedForAppraisal = a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                            loanInformation = a.LOANPURPOSE
                        }).ToList();
            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);

                    if (workFlow.Saved)
                    {
                        var response = ApproveChecklistDeferral(entity.targetId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Rollback();
                    }

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
            return false;
        }
        private bool ApproveChecklistDeferral(int targetId, ApprovalViewModel user)
        {
            bool output = false;
            var checklistRecord = (from s in context.TBL_LOAN_CONDITION_PRECEDENT
                                   where s.LOANCONDITIONID == targetId
                                  && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                   select s).FirstOrDefault();
            var deferredRecord = (from s in context.TBL_LOAN_CONDITION_DEFERRAL
                                  where s.LOANCONDITIONID == targetId
                                 && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                  select s).FirstOrDefault();
            if (workFlow.NewState != (int)ApprovalState.Ended)
            {
                if (checklistRecord.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing)
                {
                    checklistRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    deferredRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }
            }
            else if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                checklistRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                deferredRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approve Loan Condition Precedence deferral with Condition ID: {targetId}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            output = context.SaveChanges() > 0;

            return output;
        }
        private IQueryable<DeferredChecklistViewModel> GetDeferralChecklist()
        {
            var data = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                        join b in context.TBL_LOAN_CONDITION_DEFERRAL
                        on a.LOANCONDITIONID equals b.LOANCONDITIONID
                        join c in context.TBL_LOAN_APPLICATION
                        on a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        where (a.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred || a.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Waived)
                        select new DeferredChecklistViewModel()
                        {
                            checklistDeferralId = b.CHECKLISTDEFERRALID,
                            deferredDate = b.DEFERREDDATE,
                            conditionId = b.LOANCONDITIONID,
                            checklistStatus = a.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                            condition = a.CONDITION,
                            approvalStatusId = b.APPROVALSTATUSID,
                            approvalStatusName = b.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            deferralReason = b.DEFERRALREASON,
                            createdBy = c.TBL_STAFF.FIRSTNAME + " " + c.TBL_STAFF.LASTNAME,
                            dateCreated = b.DATETIMECREATED,
                            customerName = c.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? c.TBL_CUSTOMER_GROUP.GROUPNAME : c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_CUSTOMER.LASTNAME,
                            applicationRefNo = c.APPLICATIONREFERENCENUMBER,
                            loanApplicationId = c.LOANAPPLICATIONID
                        });
            return data;
        }
        public IEnumerable<DeferredChecklistViewModel> GetAllDeferralChecklist()
        {
            var groupedData = GetDeferralChecklist().GroupBy(c => c.conditionId).Select(y => y.FirstOrDefault());
            return groupedData.ToList();
        }
        public IEnumerable<DeferredChecklistViewModel> GetDeferralChecklistByConditionId(int conditionId)
        {
            var condition = GetDeferralChecklist().Where(x => x.conditionId == conditionId).Select(y => y);
            return condition.ToList();
        }

        public bool ExtendChecklistDeferralDate(ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(model.conditionId);
            if (data == null) return false;
            data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
            data.DEFEREDDATE = model.deferedDate;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            var deferral = new TBL_LOAN_CONDITION_DEFERRAL();
            deferral.LOANCONDITIONID = data.LOANCONDITIONID;
            deferral.DEFERRALREASON = model.reason;
            deferral.DEFERREDDATE = (DateTime)model.deferedDate;
            deferral.DATETIMECREATED = DateTime.Now;
            deferral.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
            deferral.CREATEDBY = model.createdBy;
            context.TBL_LOAN_CONDITION_DEFERRAL.Add(deferral);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Condition Precedence Checklist with Condition ID: {model.condition}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;

        }
        public bool ValidateConditionPrecedentDetail(ConditionPrecedentViewModel entity)
        {
            if (entity == null) return false;
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(entity.conditionId);
            if (data == null) return false;
            data.CHECKLISTVALIDATED = entity.validationStatus;
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Validated Loan Condition Precedence Checklist with Condition ID: {entity.conditionId}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;
        }
        public bool UpdateProvidedChecklist(ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(model.conditionId);
            if (data == null) return false;
            data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            if (data.CHECKLISTSTATUSID != null)
            {
                data.CHECKLISTSTATUSID = (int)CheckListStatusEnum.Provided;
            }

            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            var deferral = (from a in this.context.TBL_LOAN_CONDITION_DEFERRAL where a.LOANCONDITIONID == model.conditionId select a).FirstOrDefault();
            if (deferral == null)
                deferral.APPROVALSTATUSID = (int)CheckListStatusEnum.Provided;



            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Condition Precedence Checklist with Condition ID: {model.condition}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;

        }

        public bool ValidateDeferralDateExpiration(int conditionId)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(conditionId);
            if (data != null)
            {
                if (DateTime.Now < data.DEFEREDDATE)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Checklist Type Mapping
        public IEnumerable<CheckListTypeMappingViewModel> GetAllChecklistTypeMapping()
        {
            var data = (from a in context.TBL_CHECKLIST_TYPE_APROV_LEVL
                        orderby a.APPROVALLEVELID descending
                        select new CheckListTypeMappingViewModel()
                        {
                            checklistTypeMappingId = a.CHECKLISTTYPE_APPROVALLEVEL,
                            approvalLevelId = a.APPROVALLEVELID,
                            checklistTypeId = a.CHECKLIST_TYPEID,
                            checkListTypeName = a.TBL_CHECKLIST_TYPE.CHECKLIST_TYPE_NAME,
                            approvalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            validateChecklist = a.CANVALIDATE
                        }).ToList();
            return data;
        }

        public bool AddChecklistTypeMapping(CheckListTypeMappingViewModel model)
        {
            if (model == null) return false;
            TBL_CHECKLIST_TYPE_APROV_LEVL typeLevel;
            if (model.checklistTypeMappingId > 0)
            {
                typeLevel = context.TBL_CHECKLIST_TYPE_APROV_LEVL.Find(model.checklistTypeMappingId);
                if (typeLevel != null)
                {
                    typeLevel.CHECKLIST_TYPEID = model.checklistTypeId;
                    typeLevel.APPROVALLEVELID = model.approvalLevelId;
                    typeLevel.CANVALIDATE = model.validateChecklist;
                }

            }
            else
            {
                typeLevel = new TBL_CHECKLIST_TYPE_APROV_LEVL();
                typeLevel.CHECKLIST_TYPEID = model.checklistTypeId;
                typeLevel.APPROVALLEVELID = model.approvalLevelId;
                typeLevel.CANVALIDATE = model.validateChecklist;
                context.TBL_CHECKLIST_TYPE_APROV_LEVL.Add(typeLevel);
            }
            //Audit Section --------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = "Added Checklist Type Mapping",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;
        }
        public bool ValidateChecklistTypeMapping(short checklistTypeId, int approvallevelId)
        {
            var data = (from a in context.TBL_CHECKLIST_TYPE_APROV_LEVL
                        where a.APPROVALLEVELID == approvallevelId && a.CHECKLIST_TYPEID == checklistTypeId
                        select a).ToList();
            return data.Any();
        }
        #endregion
    }
}

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
            var data = (from a in context.tbl_Checklist_Definition
                        where a.Deleted == false
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CheckListDefinitionId,
                            approvalLevelId = a.ApprovalLevelId,
                            approvalLevelName = a.tbl_Approval_Level.LevelName,
                            isActive = a.IsActive,
                            isRequired = a.IsRequired,
                            productClassId = a.ProductId,
                            productClassName = a.tbl_Product.ProductName,
                            checkListItemId = a.CheckListItemId,
                            checkListItemName = a.tbl_CheckList_Item.CheckListItemName,
                            itemDescription = a.ItemDescription,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;

        }

        public IEnumerable<ChecklistDefinitionViewModel> GetAllChecklistDefinitionByProductId(int productId)
        {
            var data = (from a in context.tbl_Checklist_Definition
                        where a.Deleted == false && a.ProductId == productId
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CheckListDefinitionId,
                            approvalLevelId = a.ApprovalLevelId,
                            approvalLevelName = a.tbl_Approval_Level.LevelName,
                            isActive = a.IsActive,
                            isRequired = a.IsRequired,
                            productClassId = a.ProductId,
                            productClassName = a.tbl_Product.ProductName,
                            checkListItemId = a.CheckListItemId,
                            checkListItemName = a.tbl_CheckList_Item.CheckListItemName,
                            itemDescription = a.ItemDescription,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;

        }
        public List<ChecklistDefinitionViewModel> GetAllChecklistDefinitionById(int CheckListDefinitionId)
        {
            var data = (from a in context.tbl_Checklist_Definition
                        where a.Deleted == false && CheckListDefinitionId == a.CheckListDefinitionId
                        select new ChecklistDefinitionViewModel
                        {
                            checkListDefinitionId = a.CheckListDefinitionId,
                            approvalLevelId = a.ApprovalLevelId,
                            isActive = a.IsActive,
                            isRequired = a.IsRequired,
                            productClassId = a.ProductId,
                            productClassName = a.tbl_Product.ProductName,
                            checkListItemId = a.CheckListItemId,
                            checkListItemName = a.tbl_CheckList_Item.CheckListItemName,
                            itemDescription = a.ItemDescription,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public IEnumerable<ChecklistDefinitionViewModel> GetChecklistDefinitionByApprovalLevel(int approvalLevelId)
        {
            var data = GetAllChecklistDefinition().Where(x => x.approvalLevelId == approvalLevelId).ToList();

            return data;
        }

        public bool AddChecklistDefinition(ChecklistDefinitionViewModel model)
        {
            var data = new tbl_Checklist_Definition
            {
                ApprovalLevelId = (int)model.approvalLevelId,
                CheckListItemId = model.checkListItemId,
                ItemDescription = model.itemDescription,
                IsRequired = model.isRequired,
                CompanyId = model.companyId,
                IsActive = model.isActive,
                ProductId = (short)model.productClassId,
                DateTimeCreated = _genSetup.GetApplicationDate(),
                CreatedBy = (int)model.createdBy
            };

            //Audit Section ---------------------------

            var audit_product = (context.tbl_Product.FirstOrDefault(x => x.ProductClassId == data.ProductId)).ProductName;
            var audit_checklist = (context.tbl_CheckList_Item.FirstOrDefault(x => x.CheckListItemId == data.CheckListItemId)).CheckListItemName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            context.tbl_Checklist_Definition.Add(data);
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
                var data = new tbl_Checklist_Definition
                {
                    ApprovalLevelId = (int)model.approvalLevelId,
                    ProductId = (short)model.productClassId,
                    CompanyId = model.companyId,
                    CheckListItemId = item.checkListItemId,
                    ItemDescription = item.itemDescription,
                    IsRequired = item.isRequired,
                    IsActive = item.isActive,
                    DateTimeCreated = _genSetup.GetApplicationDate(),
                    CreatedBy = (int)model.createdBy
                };

                //Audit Section ---------------------------

                var audit_product = (context.tbl_Product.FirstOrDefault(x => x.ProductClassId == data.ProductId)).ProductName;
                var audit_checklist = (context.tbl_CheckList_Item.FirstOrDefault(x => x.CheckListItemId == data.CheckListItemId)).CheckListItemName;

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LoanChecklistAdded,
                    StaffId = model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Added Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                context.tbl_Checklist_Definition.Add(data);
                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------

            }

            return context.SaveChanges() != 0;
        }

        public bool UpdateChecklistDefinition(int CheckListDefinitionId, ChecklistDefinitionViewModel model)
        {
            var data = this.context.tbl_Checklist_Definition.Find(CheckListDefinitionId);
            if (data == null) return false;
            data.ApprovalLevelId = (int)model.approvalLevelId;
            data.ItemDescription = model.itemDescription;
            data.CompanyId = model.companyId;
            data.CheckListItemId = model.checkListItemId;
            data.IsActive = model.isActive;
            data.IsRequired = model.isRequired;
            data.ProductId = (short)model.productClassId;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();
            data.LastUpdatedBy = (int)model.createdBy;

            //Audit Section ---------------------------
            var audit_product = (context.tbl_Product.FirstOrDefault(x => x.ProductClassId == data.ProductId)).ProductName;
            var audit_checklist = (context.tbl_CheckList_Item.FirstOrDefault(x => x.CheckListItemId == data.CheckListItemId)).CheckListItemName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        public bool DeleteChecklistDefinition(int CheckListDefinitionId, UserInfo user)
        {
            var data = this.context.tbl_Checklist_Definition.Find(CheckListDefinitionId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------

            var audit_product = (context.tbl_Product.FirstOrDefault(x => x.ProductClassId == data.ProductId)).ProductName;
            var audit_checklist = (context.tbl_CheckList_Item.FirstOrDefault(x => x.CheckListItemId == data.CheckListItemId)).CheckListItemName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Checklist Definition {audit_checklist} to tbl_Product '{audit_product}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region Checklist Details
        public IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail()
        {
            var data = (from a in context.tbl_Checklist_Detail
                        where a.Deleted == false
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.ChecklistId,
                            checkListDefinitionId = a.CheckListDefinitionId,
                            checkListDefinitionItemName = a.tbl_Checklist_Definition.tbl_CheckList_Item.CheckListItemName,
                            targetTypeId = a.TargetTypeId,
                            targetTypeName = a.tbl_Checklist_TargetType.TargetTypeName,
                            targetId = a.TargetId,
                            checkListStatusId = a.CheckListStatusId,
                            checkListStatusName = a.tbl_Checklist_Status.ChecklistStatusName,
                            checkedBy = a.CheckedBy,
                            deferedDate = a.DeferedDate,
                            remark = a.Remark,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailById(int ChecklistId)
        {
            var data = (from a in context.tbl_Checklist_Detail
                        where a.Deleted == false && ChecklistId == a.ChecklistId
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.ChecklistId,
                            checkListDefinitionId = a.CheckListDefinitionId,
                            checkListDefinitionItemName = a.tbl_Checklist_Definition.tbl_CheckList_Item.CheckListItemName,
                            targetTypeId = a.TargetTypeId,
                            targetTypeName = a.tbl_Checklist_TargetType.TargetTypeName,
                            targetId = a.TargetId,
                            checkListStatusId = a.CheckListStatusId,
                            checkedBy = a.CheckedBy,
                            deferedDate = a.DeferedDate,
                            remark = a.Remark,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<ChecklistDetailViewModel> GetAllChecklistDetailByProductId(int targetTypeId, int targetId)
        {
            var data = (from a in context.tbl_Checklist_Detail
                        where a.Deleted == false && a.TargetTypeId == targetTypeId && a.TargetId == targetId
                        select new ChecklistDetailViewModel
                        {
                            checklistId = a.ChecklistId,
                            checkListDefinitionId = a.CheckListDefinitionId,
                            checkListDefinitionItemName = a.tbl_Checklist_Definition.tbl_CheckList_Item.CheckListItemName,
                            targetTypeId = a.TargetTypeId,
                            targetTypeName = a.tbl_Checklist_TargetType.TargetTypeName,
                            targetId = a.TargetId,
                            checkListStatusId = a.CheckListStatusId,
                            checkedBy = a.CheckedBy,
                            deferedDate = a.DeferedDate,
                            remark = a.Remark,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public bool AddMultipleChecklistDetails(List<ChecklistDetailViewModel> models, int staffId, short BranchId)
        {
            if (models.Count <= 0)
                return false;
            int loanApplicationId = models.FirstOrDefault().targetId;
            foreach (ChecklistDetailViewModel model in models)
            {
                model.createdBy = staffId;
                model.checkedBy = staffId;
                model.targetTypeId = (int)CheckListTargetTypeEnum.Loan;
                model.userBranchId = BranchId;
                model.remark = "Remark";
                AddChecklistDetail(model);

            }
            var loanData = context.tbl_Loan_Application.Find(loanApplicationId);
            if (loanData != null)
            {
                loanData.ApplicationStatusId = (int)LoanApplicationStatusEnum.ChecklistCompleted;
            }
            return context.SaveChanges() != 0;
        }
        public bool AddChecklistDetail(ChecklistDetailViewModel model)
        {
            var data = new tbl_Checklist_Detail
            {
                CheckListDefinitionId = model.checkListDefinitionId,
                TargetTypeId = model.targetTypeId,
                TargetId = model.targetId,
                CheckListStatusId = model.checkListStatusId,
                CheckedBy = (int)model.createdBy,
                DeferedDate = model.deferedDate,
                Remark = model.remark,
                DateTimeCreated = _genSetup.GetApplicationDate(),
                CreatedBy = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit_checklist = (context.tbl_Checklist_Definition.FirstOrDefault(x => x.CheckListDefinitionId == data.CheckListDefinitionId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Loan Checklist {audit_checklist.tbl_CheckList_Item.CheckListItemName}", // on Loan '{data.LoanId}' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            context.tbl_Checklist_Detail.Add(data);
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateChecklistDetail(int ChecklistId, ChecklistDetailViewModel model)
        {
            var data = this.context.tbl_Checklist_Detail.Find(ChecklistId);
            if (data == null) return false;

            data.CheckListDefinitionId = model.checkListDefinitionId;
            data.TargetTypeId = model.targetTypeId;
            data.TargetId = model.targetId;
            data.CheckListStatusId = model.checkListStatusId;
            data.CheckedBy = (int)model.createdBy;
            data.DeferedDate = model.deferedDate;
            data.Remark = model.remark;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();
            data.LastUpdatedBy = (int)model.createdBy;

            // Audit Section ---------------------------
            var audit_checklist = (context.tbl_Checklist_Definition.FirstOrDefault(x => x.CheckListDefinitionId == data.CheckListDefinitionId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Loan Checklist {audit_checklist.tbl_CheckList_Item.CheckListItemName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteChecklistDetail(int ChecklistId, UserInfo user)
        {
            var data = context.tbl_Checklist_Detail.Find(ChecklistId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_checklist = (context.tbl_Checklist_Definition.FirstOrDefault(x => x.
             CheckListDefinitionId == context.tbl_Checklist_Detail.Find(ChecklistId).CheckListDefinitionId));

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanChecklistAdded,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Added Loan Checklist {audit_checklist.tbl_CheckList_Item.CheckListItemName}",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region Checklist Item
        public IEnumerable<ChecklistItemViewModel> GetAllChecklistItem()
        {
            var data = (from a in context.tbl_CheckList_Item
                        where a.Deleted == false
                        select new ChecklistItemViewModel
                        {
                            checkListItemId = a.CheckListItemId,
                            checkListItemName = a.CheckListItemName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<ChecklistItemViewModel> GetAllChecklistItemById(int CheckListItemId)
        {
            var data = (from a in context.tbl_CheckList_Item
                        where a.Deleted == false && CheckListItemId == a.CheckListItemId
                        select new ChecklistItemViewModel
                        {
                            checkListItemId = a.CheckListItemId,
                            checkListItemName = a.CheckListItemName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }

        public bool AddChecklistItem(ChecklistItemViewModel model)
        {
            var data = new tbl_CheckList_Item
            {
                CheckListItemName = model.checkListItemName,
                DateTimeCreated = _genSetup.GetApplicationDate(),
                CreatedBy = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChecklistItemAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Checklist Item Item '{model.checkListItemName}'",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            context.tbl_CheckList_Item.Add(data);
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
            var data = this.context.tbl_CheckList_Item.Find(CheckListItemId);
            if (data == null) return false;

            data.CheckListItemName = model.checkListItemName;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();
            data.LastUpdatedBy = (int)model.createdBy;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChecklistItemUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Checklist Item '{model.checkListItemName}'",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteChecklistItem(int CheckListItemId, UserInfo user)
        {
            var data = context.tbl_CheckList_Item.Find(CheckListItemId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChecklistItemDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Checklist Item '{data.CheckListItemName}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region CheckList Select List
        public IEnumerable<CheckListStatusViewModel> GetAllChecklistStatus()
        {
            var data = (from a in context.tbl_Checklist_Status
                        where a.Deleted == false
                        select new CheckListStatusViewModel
                        {
                            checklistStatusId = a.ChecklistStatusId,
                            checklistStatusName = a.ChecklistStatusName,
                        }).ToList();
            return data;
        }

        public IEnumerable<CheckListTargetTypeViewModel> GetAllChecklistTargetType()
        {
            var data = (from a in context.tbl_Checklist_TargetType
                        where a.Deleted == false
                        select new CheckListTargetTypeViewModel
                        {
                            targetTypeId = a.TargetTypeId,
                            targetTypeName = a.TargetTypeName
                        }).ToList();
            return data;
        }
        #endregion

    }
}

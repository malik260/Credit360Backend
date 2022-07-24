using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class BusinessRuleRepository : IBusinessRuleRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;
        private IAdminRepository admin;

        public BusinessRuleRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _genSetup,
                IAuditTrailRepository _auditTrail,
                IWorkflow _workflow,
                IAdminRepository _admin
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workflow = _workflow;
            this.admin = _admin;
        }

        public bool AddBusinessRule(BusinessRuleViewModel model)
        {
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                var data = new TBL_APPROVAL_BUSINESS_RULE
                {
                    DESCRIPTION = model.description,
                    MINIMUMAMOUNT = model.minimumAmount,
                    MAXIMUMAMOUNT = model.maximumAmount,
                    PEPAMOUNT = model.pepAmount,
                    PEP = model.pep,
                    PROJECTRELATED = model.projectRelated,
                    INSIDERRELATED = model.insiderRelated,
                    ONLENDING = model.onLending,
                    INTERVENTIONFUNDS = model.interventionFunds,
                    ORRBASEDAPPROVAL = model.orrBasedApproval,
                    WITHINSTRUCTION = model.withoutInstruction,
                    DOMICILIATIONNOTINPLACE = model.domiciliationNotInPlace,
                    ESRM = model.esrm,
                    ISFORCONTINGENTFACILITY = model.isForContingentFacility,
                    ISFORREVOLVINGFACILITY = model.isForRevolvingFacility,
                    ISFORRENEWAL = model.isForRenewal,
                    EXEMPTCONTINGENTFACILITY = model.exemptContingentFacility,
                    EXEMPTREVOLVINGFACILITY = model.exemptRevolvingFacility,
                    EXEMPTRENEWAL = model.exemptRenewal,
                    TENOR = model.tenor,
                    EXCLUDELEVEL = model.excludeLevel,
                    ISAGRICRELATED = model.isAgricRelated,
                    ISSYNDICATED = model.isSyndicated,
                    COMPANYID = model.companyId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                };

                context.TBL_APPROVAL_BUSINESS_RULE.Add(data);

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"New business rule '{ model.description }' created by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                throw new NotImplementedException();
            }

            return context.SaveChanges() != 0;
        }

        public bool DeleteBusinessRule(int id, UserInfo user)
        {
            var model = this.context.TBL_APPROVAL_BUSINESS_RULE.Find(id);
            if (admin.IsSuperAdmin(user.createdBy) == true)
            {
                model.DELETED = true;
                model.DELETEDBY = user.createdBy;
                model.DATETIMEDELETED = genSetup.GetApplicationDate();

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Workflow business rule '{model.DESCRIPTION}' was deleted by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.APPROVALBUSINESSRULEID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALBUSINESSRULEID == id).Any()) throw new SecureException("Can not delete this business rule because it is being used. You can de activate it.");

            return context.SaveChanges() != 0;
        }

        public bool DeleteDynamicBusinessRule(int id, UserInfo user)
        {
            var model = this.context.TBL_WORKFLOW_ITEM_EXPRESSION.Find(id);
            var ruleInUse = context.TBL_WORKFLOW_ITEM_EXPRESSION.Where(x => x.APPROVALBUSINESSRULEID == model.APPROVALBUSINESSRULEID).FirstOrDefault()?.APPROVALBUSINESSRULEID;
            if (admin.IsSuperAdmin(user.createdBy) == true)
            {
                model.DELETED = true;
                model.DELETEDBY = user.createdBy;
                model.DATETIMEDELETED = genSetup.GetApplicationDate();

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Dynamic Workflow business rule '{model.WORKFLOWEXPRESSION}' was deleted by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.EXPRESSIONID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALBUSINESSRULEID == ruleInUse).Any()) throw new SecureException("Can not delete this business rule because it is being used. You can deactivate it.");

            return context.SaveChanges() != 0;
        }


        public IEnumerable<BusinessRuleViewModel> GetBusinessRule(int companyId)
        {
            var businessRile = context.TBL_APPROVAL_BUSINESS_RULE
                .Where(x => x.COMPANYID == companyId && x.DELETED == false)
                .Select(x => new BusinessRuleViewModel
                {
                    levelBusinessRuleId = x.APPROVALBUSINESSRULEID,
                    description = x.DESCRIPTION,
                    minimumAmount = x.MINIMUMAMOUNT,
                    maximumAmount = x.MAXIMUMAMOUNT,
                    pepAmount = x.PEPAMOUNT,
                    pep = x.PEP,
                    projectRelated = x.PROJECTRELATED,
                    insiderRelated = x.INSIDERRELATED,
                    onLending = x.ONLENDING,
                    interventionFunds = x.INTERVENTIONFUNDS,
                    orrBasedApproval = x.ORRBASEDAPPROVAL,
                    esrm = x.ESRM,
                    isForContingentFacility = x.ISFORCONTINGENTFACILITY,
                    isForRevolvingFacility = x.ISFORREVOLVINGFACILITY,
                    isForRenewal = x.ISFORRENEWAL,
                    exemptContingentFacility = x.EXEMPTCONTINGENTFACILITY,
                    exemptRevolvingFacility = x.EXEMPTREVOLVINGFACILITY,
                    exemptRenewal = x.EXEMPTRENEWAL,
                    tenor = x.TENOR,
                    withoutInstruction = x.WITHINSTRUCTION,
                    domiciliationNotInPlace = x.DOMICILIATIONNOTINPLACE,
                    excludeLevel = x.EXCLUDELEVEL,
                    isAgricRelated = x.ISAGRICRELATED,
                    isSyndicated = x.ISSYNDICATED
                }).OrderBy(b => b.description)
                .ToList();

            return businessRile;
        }

        public BusinessRuleViewModel GetBusinessRuleById(int businessRuleId)
        {
            var rule = context.TBL_APPROVAL_BUSINESS_RULE.FirstOrDefault(x => x.APPROVALBUSINESSRULEID == businessRuleId && x.DELETED == false);

            return new BusinessRuleViewModel
            {
                levelBusinessRuleId = rule.APPROVALBUSINESSRULEID,
                description = rule.DESCRIPTION,
                minimumAmount = rule.MINIMUMAMOUNT,
                maximumAmount = rule.MAXIMUMAMOUNT,
                pepAmount = rule.PEPAMOUNT,
                pep = rule.PEP,
                projectRelated = rule.PROJECTRELATED,
                insiderRelated = rule.INSIDERRELATED,
                onLending = rule.ONLENDING,
                interventionFunds = rule.INTERVENTIONFUNDS,
                orrBasedApproval = rule.ORRBASEDAPPROVAL,
                withoutInstruction = rule.WITHINSTRUCTION,
                domiciliationNotInPlace = rule.DOMICILIATIONNOTINPLACE,
                esrm = rule.ESRM,
                isForContingentFacility = rule.ISFORCONTINGENTFACILITY,
                isForRevolvingFacility = rule.ISFORREVOLVINGFACILITY,
                isForRenewal = rule.ISFORRENEWAL,
                exemptContingentFacility = rule.EXEMPTCONTINGENTFACILITY,
                exemptRevolvingFacility = rule.EXEMPTREVOLVINGFACILITY,
                exemptRenewal = rule.EXEMPTRENEWAL,
                tenor = rule.TENOR,
                excludeLevel = rule.EXCLUDELEVEL,
                isAgricRelated = rule.ISAGRICRELATED,
                isSyndicated = rule.ISSYNDICATED
            };
        }

        public bool UpdateBusinessRule(BusinessRuleViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_APPROVAL_BUSINESS_RULE.Find(id);
            if (admin.IsSuperAdmin(user.createdBy) == true)
            {
                entity.DESCRIPTION = model.description;
                entity.MINIMUMAMOUNT = model.minimumAmount;
                entity.MAXIMUMAMOUNT = model.maximumAmount;
                entity.PEPAMOUNT = model.pepAmount;
                entity.PEP = model.pep;
                entity.PROJECTRELATED = model.projectRelated;
                entity.INSIDERRELATED = model.insiderRelated;
                entity.ONLENDING = model.onLending;
                entity.INTERVENTIONFUNDS = model.interventionFunds;
                entity.ORRBASEDAPPROVAL = model.orrBasedApproval;
                entity.ESRM = model.esrm;
                entity.ISFORCONTINGENTFACILITY = model.isForContingentFacility;
                entity.ISFORREVOLVINGFACILITY = model.isForRevolvingFacility;
                entity.ISFORRENEWAL = model.isForRenewal;
                entity.EXEMPTCONTINGENTFACILITY = model.exemptContingentFacility;
                entity.EXEMPTREVOLVINGFACILITY = model.exemptRevolvingFacility;
                entity.EXEMPTRENEWAL = model.exemptRenewal;
                entity.TENOR = model.tenor;
                entity.WITHINSTRUCTION = model.withoutInstruction;
                entity.DOMICILIATIONNOTINPLACE = model.domiciliationNotInPlace;
                entity.EXCLUDELEVEL = model.excludeLevel;
                entity.ISAGRICRELATED = model.isAgricRelated;
                entity.ISSYNDICATED = model.isSyndicated;
                entity.LASTUPDATEDBY = user.createdBy;
                entity.DATETIMEUPDATED = DateTime.Now;

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleUpdated,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Workflow business rule '{entity.DESCRIPTION}' was updated by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = entity.APPROVALBUSINESSRULEID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                throw new NotImplementedException();
            }

            return context.SaveChanges() != 0;
        }
    }
}

using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{
    public class AccreditedConsultantsRepository : IAccreditedConsultantsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;

        private int AccreditedConsultantId;
        public AccreditedConsultantsRepository(FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, IWorkflow _workflow
)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workFlow = _workflow;

        }

        #region Solicitors
        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId,int accreditedConsultantId)
        {
            return (from m in context.TBL_ACCREDITEDCONSULTANT
                    where m.COMPANYID == companyId && m.ACCREDITEDCONSULTANTTYPEID == accreditedConsultantId
                    select new AccreditedConsultantsViewModel
                    {
                        accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                        registrationNumber = m.REGISTRATIONNUMBER,
                        name = m.NAME,
                        firmName = m.FIRMNAME,
                        accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                        cityId = (short)m.CITYID,
                        accountNumber = m.ACCOUNTNUMBER,
                        solicitorBVN = m.SOLICITORBVN,
                        countryId = m.COUNTRYID,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,
                        dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                        category = m.CATEGORY,
                        staffCode = m.STAFFCODE,
                        agentCategory = m.AGENTCATEGORY,
                        coreCompetence = m.CORECOMPETENCE,
                        accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(x => x.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).FirstOrDefault().NAME,
                        accreditedConsultantStates = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID).Select(k =>
                           new AccreditedConsultantStateViewModel()
                           {
                               accreditedConsultantStateCoveredID = k.CONSULT_STATE_COVREDID,
                               stateId = k.STATEID,
                               stateName = context.TBL_STATE.FirstOrDefault(x => x.STATEID == k.STATEID).STATENAME,
                               accreditedConsultantId = k.ACCREDITEDCONSULTANTID
                           }).ToList()
                    }).ToList();
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultants(int companyId)
        {
            var data = (from m in context.TBL_ACCREDITEDCONSULTANT
                        join c in context.TBL_ACCREDITEDCONSULTANT_STATE on m.ACCREDITEDCONSULTANTID equals c.ACCREDITEDCONSULTANTID
                        where m.COMPANYID == companyId
                        && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.RecoveryAgent
                        select new AccreditedConsultantsViewModel
                        {
                            accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                            registrationNumber = m.REGISTRATIONNUMBER,
                            name = m.NAME,
                            firmName = m.FIRMNAME,
                            accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                            cityId = (short)m.CITYID,
                            accountNumber = m.ACCOUNTNUMBER,
                            solicitorBVN = m.SOLICITORBVN,
                            countryId = m.COUNTRYID,
                            emailAddress = m.EMAILADDRESS,
                            phoneNumber = m.PHONENUMBER,
                            address = m.ADDRESS,
                            dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                            category = m.CATEGORY,
                            staffCode = m.STAFFCODE,
                            agentCategory = m.AGENTCATEGORY,
                            consultantType = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(t => t.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).Select(t => t.NAME).FirstOrDefault(),
                            coreCompetence = m.CORECOMPETENCE,
                            stateName = context.TBL_STATE.FirstOrDefault(x => x.STATEID == c.STATEID).STATENAME,
                        }).Distinct();

            return data;
        }


        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId)
        {
            var data = (from m in context.TBL_ACCREDITEDCONSULTANT
                        where m.COMPANYID == companyId
                        && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.RecoveryAgent
                        select new AccreditedConsultantsViewModel
                        {
                            accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                            registrationNumber = m.REGISTRATIONNUMBER,
                            name = m.NAME,
                            firmName = m.FIRMNAME,
                            accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                            cityId = (short)m.CITYID,
                            accountNumber = m.ACCOUNTNUMBER,
                            solicitorBVN = m.SOLICITORBVN,
                            countryId = m.COUNTRYID,
                            emailAddress = m.EMAILADDRESS,
                            phoneNumber = m.PHONENUMBER,
                            address = m.ADDRESS,
                            dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                            category = m.CATEGORY,
                            staffCode = m.STAFFCODE,
                            agentCategory = m.AGENTCATEGORY,
                            consultantType = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(t => t.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).Select(t => t.NAME).FirstOrDefault(),
                            coreCompetence = m.CORECOMPETENCE,
                            stateName = (from s in context.TBL_STATE join c in context.TBL_ACCREDITEDCONSULTANT_STATE on s.STATEID equals c.STATEID where c.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID select s.STATENAME).FirstOrDefault(),
                        }).ToList();

            return data;
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetSearchedAgent(string search)
        {

            var agencies = from m in context.TBL_ACCREDITEDCONSULTANT
                           where m.DELETED == false
                           && (m.NAME.Contains(search.ToUpper())
                           || m.FIRMNAME.Contains(search.ToUpper())
                           || m.PHONENUMBER.Contains(search)
                           || m.ADDRESS.Contains(search.ToUpper()))
                           && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.RecoveryAgent

                           select new AccreditedConsultantsViewModel
                           {
                               accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                               registrationNumber = m.REGISTRATIONNUMBER,
                               name = m.NAME,
                               firmName = m.FIRMNAME,
                               accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                               cityId = (short)m.CITYID,
                               accountNumber = m.ACCOUNTNUMBER,
                               solicitorBVN = m.SOLICITORBVN,
                               countryId = m.COUNTRYID,
                               emailAddress = m.EMAILADDRESS,
                               phoneNumber = m.PHONENUMBER,
                               address = m.ADDRESS,
                               dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                               category = m.CATEGORY,
                               staffCode = m.STAFFCODE,
                               agentCategory = m.AGENTCATEGORY,
                               stateName = (from s in context.TBL_STATE join c in context.TBL_ACCREDITEDCONSULTANT_STATE on s.STATEID equals c.STATEID where c.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID select s.STATENAME).FirstOrDefault(),
                           };

            return agencies.ToList();
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultantsByStateId(int companyId, int stateId)
        {
            var data = (from m in context.TBL_ACCREDITEDCONSULTANT
                        join c in context.TBL_ACCREDITEDCONSULTANT_STATE on m.ACCREDITEDCONSULTANTID equals c.ACCREDITEDCONSULTANTID
                        where m.COMPANYID == companyId && c.STATEID == stateId
                        select new AccreditedConsultantsViewModel
                        {
                            accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                            registrationNumber = m.REGISTRATIONNUMBER,
                            name = m.NAME,
                            firmName = m.FIRMNAME,
                            accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                            cityId = (short)m.CITYID,
                            accountNumber = m.ACCOUNTNUMBER,
                            solicitorBVN = m.SOLICITORBVN,
                            countryId = m.COUNTRYID,
                            emailAddress = m.EMAILADDRESS,
                            phoneNumber = m.PHONENUMBER,
                            address = m.ADDRESS,
                            coreCompetence = m.CORECOMPETENCE,
                            dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                            category = m.CATEGORY,
                            staffCode = m.STAFFCODE,
                            agentCategory = m.AGENTCATEGORY,
                            accreditedConsultantStates = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID).Select(k =>
                               new AccreditedConsultantStateViewModel()
                               {
                                   accreditedConsultantStateCoveredID = k.CONSULT_STATE_COVREDID,
                                   stateId = k.STATEID,
                                   stateName = context.TBL_STATE.FirstOrDefault(x => x.STATEID == k.STATEID).STATENAME,
                                   accreditedConsultantId = k.ACCREDITEDCONSULTANTID
                               }).ToList()
                        });
            var v = data.ToList();

            return data;
        }

        public bool AddConsultantType(AccreditedConsultantTypeViewModel entity)
        {
            bool output = false;

            TBL_ACCREDITEDCONSULTANT_TYPE consultanttype = new TBL_ACCREDITEDCONSULTANT_TYPE();

            consultanttype.NAME = entity.name;
                
                try
                {
                context.TBL_ACCREDITEDCONSULTANT_TYPE.Add(consultanttype);
                output = context.SaveChanges() > 0;

                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }

           
            return output;
        }

        public async Task<AccreditedConsultantsViewModel> AddAccreditedConsultants(AccreditedConsultantsViewModel entity)
        {
            bool output = false;
            List<TBL_TEMP_ACCREDITEDCONSULTANT_STATE> accreditedConsultantStates = new List<TBL_TEMP_ACCREDITEDCONSULTANT_STATE>();
            if (entity.accreditedConsultantStates.Count > 0)
            {
                foreach (var ent in entity.accreditedConsultantStates)
                {
                    if (ent.accreditedConsultantStateCoveredID == 0)
                    {
                        var state = new TBL_TEMP_ACCREDITEDCONSULTANT_STATE();
                        //state.AccreditedConsultantId = AccreditedConsultantId;
                        state.STATEID = ent.stateId;
                        context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Add(state);
                    }
                }
            }
            var consultant = new TBL_TEMP_ACCREDITEDCONSULTANT
            {
                CITYID = entity.cityId,
                NAME = entity.name,
                FIRMNAME = entity.firmName,
                REGISTRATIONNUMBER = entity.registrationNumber,
                ACCREDITEDCONSULTANTTYPEID = entity.accreditedConsultantTypeId,
                SOLICITORBVN = entity.solicitorBVN,
                ACCOUNTNUMBER = entity.accountNumber,
                COUNTRYID = entity.countryId,
                EMAILADDRESS = entity.emailAddress,
                PHONENUMBER = entity.phoneNumber,
                ADDRESS = entity.address,
                CORECOMPETENCE = entity.coreCompetence,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                ISCURRENT = true,
                OPERATION = "insert",
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                TBL_TEMP_ACCREDITEDCONSULTANT_STATE = accreditedConsultantStates,
                DATEOFENGAGEMENT = entity.dateOfEngagement,
                CATEGORY = entity.category,
                STAFFCODE = entity.staffCode,
                AGENTCATEGORY = entity.agentCategory
            };
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralValuerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_AccreditedConsultant with Id: {entity.accreditedConsultantId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_TEMP_ACCREDITEDCONSULTANT.Add(consultant);
                    auditTrail.AddAuditTrail(audit);
                    output = await context.SaveChangesAsync() > 0;

                    var appTrail = new ApprovalViewModel
                    {
                        staffId = entity.createdBy,
                        companyId = entity.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        comment = "Please approve this Accredited Consultant",
                        targetId = consultant.TEMPACCREDITEDCONSULTANTID,
                        operationId = (int)OperationsEnum.AccreditedConsultantCreated,
                        BranchId = entity.userBranchId,
                        externalInitialization = true
                    };

                    var response = workFlow.LogForApproval(appTrail);

                    if (response)
                    {
                        trans.Commit();

                        if (output)
                        {
                            return new AccreditedConsultantsViewModel { accreditedConsultantId = consultant.ACCREDITEDCONSULTANTID };
                        }
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }

            }
            return new AccreditedConsultantsViewModel();
        }
        public async Task<bool> AddAccreditedConsultants2(AccreditedConsultantsViewModel entity)
        {
            if (entity == null)
            { 
                return false;
            }
            bool output = false;
            List<TBL_ACCREDITEDCONSULTANT_STATE> accreditedConsultantStates = new List<TBL_ACCREDITEDCONSULTANT_STATE>();
            if (entity.accreditedConsultantStates.Count > 0)
            {
                foreach (var ent in entity.accreditedConsultantStates)
                {
                    if (ent.accreditedConsultantStateCoveredID == 0)
                    {
                        var state = new TBL_ACCREDITEDCONSULTANT_STATE();
                        //state.AccreditedConsultantId = AccreditedConsultantId;
                        state.STATEID = ent.stateId;
                        context.TBL_ACCREDITEDCONSULTANT_STATE.Add(state);
                    }
                }
            }
                var consultant = new TBL_ACCREDITEDCONSULTANT
            {
                CITYID = entity.cityId,
                NAME = entity.name,
                FIRMNAME = entity.firmName,
                REGISTRATIONNUMBER = entity.registrationNumber,
                ACCREDITEDCONSULTANTTYPEID = entity.accreditedConsultantTypeId,
                SOLICITORBVN = entity.solicitorBVN,
                ACCOUNTNUMBER = entity.accountNumber,
                COUNTRYID = entity.countryId,
                EMAILADDRESS = entity.emailAddress,
                PHONENUMBER = entity.phoneNumber,
                ADDRESS = entity.address,
                CORECOMPETENCE = entity.coreCompetence,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                TBL_ACCREDITEDCONSULTANT_STATE = accreditedConsultantStates,
                DATEOFENGAGEMENT = entity.dateOfEngagement,
                CATEGORY = entity.category,
                STAFFCODE = entity.staffCode,
                AGENTCATEGORY = entity.agentCategory
                };
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralValuerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_AccreditedConsultant with Id: {entity.accreditedConsultantId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_ACCREDITEDCONSULTANT.Add(consultant);
                    auditTrail.AddAuditTrail(audit);
                    //AccreditedConsultantId = consultant.AccreditedConsultantId;
                    //if (entity.accreditedConsultantStates.Count > 0)
                    //{
                    //    AddUpdateAccreditedConsultantStates(entity.accreditedConsultantStates);
                    //}
                    output = await context.SaveChangesAsync() > 0;
                   
                    trans.Commit();
                }

                catch (Exception ex)
                {
                    trans.Rollback();

                    throw new SecureException(ex.Message);
                }
            }
            return output;
        }
        public async Task<bool> UpdateAccreditedConsultants(AccreditedConsultantsViewModel entity, int id)
        {
            bool output = false;
            var targetAccreditedConId = 0;


            var existingTempAccreditedConsultants = context.TBL_TEMP_ACCREDITEDCONSULTANT
      .FirstOrDefault(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.ACCREDITEDCONSULTANTID == entity.accreditedConsultantId);
            var unApprovedAccreditedConsultants = context.TBL_TEMP_ACCREDITEDCONSULTANT
                .Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                && x.ACCREDITEDCONSULTANTID == entity.accreditedConsultantId && x.ISCURRENT == true).ToList();
            TBL_TEMP_ACCREDITEDCONSULTANT tempAccreditedConsultants = new TBL_TEMP_ACCREDITEDCONSULTANT();
            TBL_TEMP_ACCREDITEDCONSULTANT_STATE tempAccreditedConsultantStates = new TBL_TEMP_ACCREDITEDCONSULTANT_STATE();

            var existingAccreditedConsultantStatesList = new List<TBL_TEMP_ACCREDITEDCONSULTANT_STATE>();
            List<TBL_TEMP_ACCREDITEDCONSULTANT_STATE> productCurrencies = new List<TBL_TEMP_ACCREDITEDCONSULTANT_STATE>();


            if (unApprovedAccreditedConsultants.Any())
            {
                throw new SecureException("Accredited Approval is already undergoing approval");
            }

            if (existingTempAccreditedConsultants != null)
            {
                var existingAccreditedConsultantStates = context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE
                    .Where(x => x.TEMPACCREDITEDCONSULTANTID ==
                        existingTempAccreditedConsultants.TEMPACCREDITEDCONSULTANTID).ToList();
                if (existingAccreditedConsultantStates.Count > 0)
                {
                    foreach (var curr in existingAccreditedConsultantStates)
                    {
                        context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Remove(curr);
                    }
                }

                var tempAccreditedConsultantsToUpdate = existingTempAccreditedConsultants;

                tempAccreditedConsultantsToUpdate.ACCREDITEDCONSULTANTID = (short)entity.accreditedConsultantId;
                tempAccreditedConsultantsToUpdate.CITYID = entity.cityId;
                tempAccreditedConsultantsToUpdate.NAME = entity.name;
                tempAccreditedConsultantsToUpdate.FIRMNAME = entity.firmName;
                tempAccreditedConsultantsToUpdate.REGISTRATIONNUMBER = entity.registrationNumber;
                tempAccreditedConsultantsToUpdate.ACCREDITEDCONSULTANTTYPEID = entity.accreditedConsultantTypeId;
                tempAccreditedConsultantsToUpdate.SOLICITORBVN = entity.solicitorBVN;
                tempAccreditedConsultantsToUpdate.ACCOUNTNUMBER = entity.accountNumber;
                tempAccreditedConsultantsToUpdate.COUNTRYID = entity.countryId;
                tempAccreditedConsultantsToUpdate.EMAILADDRESS = entity.emailAddress;
                tempAccreditedConsultantsToUpdate.PHONENUMBER = entity.phoneNumber;
                tempAccreditedConsultantsToUpdate.ADDRESS = entity.address;
                tempAccreditedConsultantsToUpdate.CORECOMPETENCE = entity.coreCompetence;
                tempAccreditedConsultantsToUpdate.COMPANYID = entity.companyId;
                tempAccreditedConsultantsToUpdate.ISCURRENT = true;
                tempAccreditedConsultantsToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempAccreditedConsultantsToUpdate.DELETED = false;
                //tempAccreditedConsultantsToUpdate.PRODUCT_BEHAVIOURID = productModel.productBehaviourId;
                tempAccreditedConsultantsToUpdate.OPERATION = "update";
                tempAccreditedConsultantsToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempAccreditedConsultantsToUpdate.DATEOFENGAGEMENT = entity.dateOfEngagement;
                tempAccreditedConsultantsToUpdate.CATEGORY = entity.category;
                tempAccreditedConsultantsToUpdate.STAFFCODE = entity.staffCode;
                tempAccreditedConsultantsToUpdate.AGENTCATEGORY = entity.agentCategory;

                context.TBL_TEMP_ACCREDITEDCONSULTANT.Add(tempAccreditedConsultantsToUpdate);
                context.SaveChanges();
                //Product Behaviour Update
                if (existingAccreditedConsultantStates != null)
                {
                    foreach (var item in entity.accreditedConsultantStates)
                    {
                        tempAccreditedConsultantStates = new TBL_TEMP_ACCREDITEDCONSULTANT_STATE
                        {
                            TEMPACCREDITEDCONSULTANTID= tempAccreditedConsultantsToUpdate.TEMPACCREDITEDCONSULTANTID,
                            STATEID = item.stateId,
                        };
                        context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Add(tempAccreditedConsultantStates);
                    }
                }



            }
            else
            {
                var targetAccreditedConsultant = context.TBL_ACCREDITEDCONSULTANT.Find(id);
                var targetAccreditedConsultantStates = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == targetAccreditedConsultant.ACCREDITEDCONSULTANTID).FirstOrDefault();


                tempAccreditedConsultants = new TBL_TEMP_ACCREDITEDCONSULTANT()
                {
                    ACCREDITEDCONSULTANTID=entity.accreditedConsultantId,
                    COMPANYID = entity.companyId,
                    CITYID = entity.cityId,
                    NAME = entity.name,
                    FIRMNAME = entity.firmName,
                    REGISTRATIONNUMBER = entity.registrationNumber,
                    ACCREDITEDCONSULTANTTYPEID = entity.accreditedConsultantTypeId,
                    SOLICITORBVN = entity.solicitorBVN,
                    ACCOUNTNUMBER = entity.accountNumber,
                    COUNTRYID = entity.countryId,
                    EMAILADDRESS = entity.emailAddress,
                    PHONENUMBER = entity.phoneNumber,
                    ADDRESS = entity.address,
                    CORECOMPETENCE = entity.coreCompetence,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    DELETED = false,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    OPERATION = "update",
                    DATEOFENGAGEMENT = entity.dateOfEngagement,
                    CATEGORY = entity.category,
                    STAFFCODE = entity.staffCode,
                    AGENTCATEGORY = entity.agentCategory
                };              

                context.TBL_TEMP_ACCREDITEDCONSULTANT.Add(tempAccreditedConsultants);
                foreach (var item in entity.accreditedConsultantStates)
                {
                    tempAccreditedConsultantStates = new TBL_TEMP_ACCREDITEDCONSULTANT_STATE()
                    {
                        STATEID = item.stateId,
                    };
                    context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Add(tempAccreditedConsultantStates);
                }
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralValuerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Accredited Consultant '{entity.accreditedConsultantName}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = id,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    output = await context.SaveChangesAsync() > 0;

                    targetAccreditedConId = existingTempAccreditedConsultants?.TEMPACCREDITEDCONSULTANTID ?? tempAccreditedConsultants.TEMPACCREDITEDCONSULTANTID;

                    var approval = new ApprovalViewModel
                    {
                        staffId = entity.createdBy,
                        companyId = entity.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = targetAccreditedConId,
                        operationId = (int)OperationsEnum.AccreditedConsultantCreated,
                        BranchId = entity.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(approval);

                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }





        public async Task<bool> UpdateAccreditedConsultants1(AccreditedConsultantsViewModel entity, int id)
        {
            var consultants = context.TBL_ACCREDITEDCONSULTANT.Find(id);
            if (consultants != null)
            {
                consultants.CITYID = entity.cityId;
                consultants.NAME = entity.name;
                consultants.FIRMNAME = entity.firmName;
                consultants.REGISTRATIONNUMBER = entity.registrationNumber;
                consultants.ACCREDITEDCONSULTANTTYPEID = entity.accreditedConsultantTypeId;
                consultants.SOLICITORBVN = entity.solicitorBVN;
                consultants.ACCOUNTNUMBER = entity.accountNumber;
                consultants.COUNTRYID = entity.countryId;
                consultants.EMAILADDRESS = entity.emailAddress;
                consultants.PHONENUMBER = entity.phoneNumber;
                consultants.ADDRESS = entity.address;
                consultants.CORECOMPETENCE = entity.coreCompetence;
                consultants.DATEOFENGAGEMENT = entity.dateOfEngagement;
                consultants.CATEGORY = entity.category;
                consultants.STAFFCODE = entity.staffCode;
                consultants.AGENTCATEGORY = entity.agentCategory;

            };
            AccreditedConsultantId = entity.accreditedConsultantId;
            if (entity.accreditedConsultantStates.Count > 0)
            {
                AddUpdateAccreditedConsultantStates(entity.accreditedConsultantStates);
            }
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Collateral_Solicitors with Id: {entity.accreditedConsultantId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }
        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedSolicitorsAwaitingApprovals(int staffId, int companyId, int accreditedConsultantId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AccreditedConsultantCreated).ToList();
            var charge = (from a in context.TBL_TEMP_ACCREDITEDCONSULTANT
                          join t in context.TBL_APPROVAL_TRAIL on a.TEMPACCREDITEDCONSULTANTID equals t.TARGETID
                          where (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                              && a.ISCURRENT == true
                              && t.RESPONSESTAFFID == null
                              && t.OPERATIONID == (int)OperationsEnum.AccreditedConsultantCreated
                              && a.ACCREDITEDCONSULTANTTYPEID == accreditedConsultantId
                          && ids.Contains((int)t.TOAPPROVALLEVELID)
                          select new AccreditedConsultantsViewModel
                          {
                              accreditedConsultantId = a.TEMPACCREDITEDCONSULTANTID,
                              registrationNumber = a.REGISTRATIONNUMBER,
                              name = a.NAME,
                              firmName = a.FIRMNAME,
                              accreditedConsultantTypeId = a.ACCREDITEDCONSULTANTTYPEID,
                              cityId = (short)a.CITYID,
                              accountNumber = a.ACCOUNTNUMBER,
                              solicitorBVN = a.SOLICITORBVN,
                              countryId = a.COUNTRYID,
                              emailAddress = a.EMAILADDRESS,
                              phoneNumber = a.PHONENUMBER,
                              address = a.ADDRESS,
                              dateOfEngagement = a.DATEOFENGAGEMENT.Value,
                              category = a.CATEGORY,
                              staffCode = a.STAFFCODE,
                              agentCategory = a.AGENTCATEGORY,
                              coreCompetence = a.CORECOMPETENCE,
                              accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(x => x.ACCREDITEDCONSULTANTID == a.ACCREDITEDCONSULTANTTYPEID).FirstOrDefault().NAME,
                              accreditedConsultantStates = context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Where(x => x.TEMPACCREDITEDCONSULTANTID == a.TEMPACCREDITEDCONSULTANTID).Select(k =>
                                 new AccreditedConsultantStateViewModel()
                                 {
                                     accreditedConsultantStateCoveredID = k.TEMP_CONSULT_STATE_COVREDID,
                                     stateId = k.STATEID,
                                     stateName = context.TBL_STATE.FirstOrDefault(x => x.STATEID == k.STATEID).STATENAME,
                                     accreditedConsultantId = k.TEMPACCREDITEDCONSULTANTID
                                 }).ToList()
                          });
            return charge.ToList();
        }

        public int GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.AccreditedConsultantCreated;
            //entity.externalInitialization = false;
            workFlow.StaffId = entity.staffId;
            workFlow.CompanyId = entity.companyId;
            workFlow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
            workFlow.TargetId = entity.targetId;
            workFlow.Comment = entity.comment;
            workFlow.OperationId = entity.operationId;
            workFlow.DeferredExecution = true;
            workFlow.ExternalInitialization = false;

            workFlow.LogActivity();

            int result = 0;
            if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
            {
                var tempAccreditedConsultant = context.TBL_TEMP_ACCREDITEDCONSULTANT.Find(entity.targetId);
                tempAccreditedConsultant.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                result = 2;
            }

            if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                ApproveAccreditedSolicitors(entity.targetId, (short)workFlow.StatusId, entity);
                result = 1;
            }
            context.SaveChanges();
            return result;
        }

        private void ApproveAccreditedSolicitors(int accreditedConsultantId, short approvalStatusId, UserInfo user)
        {
            var accreditedConsultantModel = context.TBL_TEMP_ACCREDITEDCONSULTANT.Find(accreditedConsultantId);
            var accreditedConsultantStateModel = context.TBL_TEMP_ACCREDITEDCONSULTANT_STATE.Where(x => x.TEMPACCREDITEDCONSULTANTID == accreditedConsultantModel.TEMPACCREDITEDCONSULTANTID).ToList();

            var accreditedConsultantToUpdate = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == accreditedConsultantModel.ACCREDITEDCONSULTANTID).FirstOrDefault();
            var accreditedConsultantStateToUpdate = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == accreditedConsultantModel.ACCREDITEDCONSULTANTID).ToList();
            var accreditedConsultantStateListToUpdate = new List<TBL_ACCREDITEDCONSULTANT_STATE>();
            TBL_ACCREDITEDCONSULTANT_STATE accreditedConsultantStates = new TBL_ACCREDITEDCONSULTANT_STATE();
            List<TBL_ACCREDITEDCONSULTANT_STATE> accreditedConsultantStatesList = new List<TBL_ACCREDITEDCONSULTANT_STATE>();

            if (accreditedConsultantToUpdate != null)
            {
                accreditedConsultantStateListToUpdate = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == accreditedConsultantToUpdate.ACCREDITEDCONSULTANTID).ToList();
                if (accreditedConsultantStateListToUpdate.Count() > 0)
                {
                    foreach (var curr in accreditedConsultantStateListToUpdate)
                    {
                        context.TBL_ACCREDITEDCONSULTANT_STATE.Remove(curr);
                    }
                }

                var existingaccreditedConsultant = accreditedConsultantToUpdate;
                if (accreditedConsultantModel != null)
                {
                    existingaccreditedConsultant.CITYID = accreditedConsultantModel.CITYID;
                    existingaccreditedConsultant.NAME = accreditedConsultantModel.NAME;
                    existingaccreditedConsultant.FIRMNAME = accreditedConsultantModel.FIRMNAME;
                    existingaccreditedConsultant.REGISTRATIONNUMBER = accreditedConsultantModel.REGISTRATIONNUMBER;
                    existingaccreditedConsultant.ACCREDITEDCONSULTANTTYPEID = accreditedConsultantModel.ACCREDITEDCONSULTANTTYPEID;
                    existingaccreditedConsultant.SOLICITORBVN = accreditedConsultantModel.SOLICITORBVN;
                    existingaccreditedConsultant.ACCOUNTNUMBER = accreditedConsultantModel.ACCOUNTNUMBER;
                    existingaccreditedConsultant.COUNTRYID = accreditedConsultantModel.COUNTRYID;
                    existingaccreditedConsultant.EMAILADDRESS = accreditedConsultantModel.EMAILADDRESS;
                    existingaccreditedConsultant.PHONENUMBER = accreditedConsultantModel.PHONENUMBER;
                    existingaccreditedConsultant.ADDRESS = accreditedConsultantModel.ADDRESS;
                    existingaccreditedConsultant.CORECOMPETENCE = accreditedConsultantModel.CORECOMPETENCE;
                    existingaccreditedConsultant.COMPANYID = accreditedConsultantModel.COMPANYID;
                    existingaccreditedConsultant.CREATEDBY = accreditedConsultantModel.CREATEDBY;
                    existingaccreditedConsultant.DATETIMECREATED = DateTime.Now;
                    existingaccreditedConsultant.DELETED = false;
                    existingaccreditedConsultant.DATEOFENGAGEMENT = accreditedConsultantModel.DATEOFENGAGEMENT;
                    existingaccreditedConsultant.CATEGORY = accreditedConsultantModel.CATEGORY;
                    existingaccreditedConsultant.STAFFCODE = accreditedConsultantModel.STAFFCODE;
                    existingaccreditedConsultant.AGENTCATEGORY = accreditedConsultantModel.AGENTCATEGORY;
                }

                accreditedConsultantModel.APPROVALSTATUSID = approvalStatusId;

                if (accreditedConsultantStateModel != null)
                {
                    foreach (var c in accreditedConsultantStateModel)
                    {
                        accreditedConsultantStatesList.Add(new TBL_ACCREDITEDCONSULTANT_STATE
                        {
                            ACCREDITEDCONSULTANTID = accreditedConsultantModel.ACCREDITEDCONSULTANTID,
                            STATEID = c.STATEID,
                        });
                    }
                }

                context.TBL_ACCREDITEDCONSULTANT_STATE.AddRange(accreditedConsultantStatesList);
                var test = accreditedConsultantStatesList.Count();
                var test2 = accreditedConsultantStatesList.ToList();
            }
            else
            {
                if (accreditedConsultantModel != null)
                {
                    var accreditedConsultant = new TBL_ACCREDITEDCONSULTANT()
                    {
                        CITYID = accreditedConsultantModel.CITYID,
                        NAME = accreditedConsultantModel.NAME,
                        FIRMNAME = accreditedConsultantModel.FIRMNAME,
                        REGISTRATIONNUMBER = accreditedConsultantModel.REGISTRATIONNUMBER,
                        ACCREDITEDCONSULTANTTYPEID = accreditedConsultantModel.ACCREDITEDCONSULTANTTYPEID,
                        SOLICITORBVN = accreditedConsultantModel.SOLICITORBVN,
                        ACCOUNTNUMBER = accreditedConsultantModel.ACCOUNTNUMBER,
                        COUNTRYID = accreditedConsultantModel.COUNTRYID,
                        EMAILADDRESS = accreditedConsultantModel.EMAILADDRESS,
                        PHONENUMBER = accreditedConsultantModel.PHONENUMBER,
                        ADDRESS = accreditedConsultantModel.ADDRESS,
                        CORECOMPETENCE = accreditedConsultantModel.CORECOMPETENCE,
                        COMPANYID = accreditedConsultantModel.COMPANYID,
                        CREATEDBY = accreditedConsultantModel.CREATEDBY,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false,
                        DATEOFENGAGEMENT = accreditedConsultantModel.DATEOFENGAGEMENT,
                        CATEGORY = accreditedConsultantModel.CATEGORY,
                        STAFFCODE = accreditedConsultantModel.STAFFCODE,
                        AGENTCATEGORY = accreditedConsultantModel.AGENTCATEGORY,

                    };
                    accreditedConsultantModel.APPROVALSTATUSID = approvalStatusId;
                    accreditedConsultantModel.ISCURRENT = false;
                    context.TBL_ACCREDITEDCONSULTANT.Add(accreditedConsultant);
                    context.SaveChanges();
                    if (accreditedConsultantStateModel != null)
                    {
                        foreach (var item in accreditedConsultantStateModel)
                        {
                            accreditedConsultantStates = new TBL_ACCREDITEDCONSULTANT_STATE
                            {
                                STATEID = item.STATEID,
                            };
                            accreditedConsultantStates.ACCREDITEDCONSULTANTID = accreditedConsultant.ACCREDITEDCONSULTANTID;
                            context.TBL_ACCREDITEDCONSULTANT_STATE.Add(accreditedConsultantStates);
                        }
                    }

                    accreditedConsultantModel.ACCREDITEDCONSULTANTID = accreditedConsultant.ACCREDITEDCONSULTANTID;
                    accreditedConsultantModel.APPROVALSTATUSID = approvalStatusId;
                }
            }
        }



        private void AddUpdateAccreditedConsultantStates(List<AccreditedConsultantStateViewModel> entity)
        {
            foreach (var ent in entity)
            {
                if (ent.accreditedConsultantStateCoveredID == 0)
                {
                    var state = new TBL_ACCREDITEDCONSULTANT_STATE();
                    state.ACCREDITEDCONSULTANTID = AccreditedConsultantId;
                    state.STATEID = ent.stateId;
                    context.TBL_ACCREDITEDCONSULTANT_STATE.Add(state);
                }
                else
                {
                    var state = context.TBL_ACCREDITEDCONSULTANT_STATE.Find(ent.accreditedConsultantStateCoveredID);
                    if (state != null)
                    {
                        state.STATEID = ent.stateId;
                    }
                }
            }
        }
        public async Task<bool> DeleteAccreditedConsultantStates(int id)
        {
            var itemToRemove = context.TBL_ACCREDITEDCONSULTANT_STATE.SingleOrDefault(x => x.CONSULT_STATE_COVREDID == id);
            if (itemToRemove != null)
            {
                context.TBL_ACCREDITEDCONSULTANT_STATE.Remove(itemToRemove);
                var response = await context.SaveChangesAsync() != 0;
                return response;
            }
            return false;
        }

        public IEnumerable<AccreditedConsultantTypeViewModel> GetAccreditedConsultantType()
        {
            var type = from a in context.TBL_ACCREDITEDCONSULTANT_TYPE
                       select new AccreditedConsultantTypeViewModel
                       {
                           accreditedConsultantTypeId = a.ACCREDITEDCONSULTANTID,
                           name = a.NAME
                       };
            return type;
        }
        #endregion
        #region Principals
        public IEnumerable<AccreditedPrincipalsViewModel> GetAccreditedPrincipals(int companyId)
        {
            return (from p in context.TBL_LOAN_PRINCIPAL
                    where p.COMPANYID == companyId orderby p.NAME ascending
                    select new AccreditedPrincipalsViewModel
                    {
                        principalsId = (short)p.PRINCIPALID,
                        principalsRegNumber = p.PRINCIPALSREGNUMBER,
                        accountNumber = p.ACCOUNTNUMBER,
                        //principalsBVN = p.PrincipalsBVN,
                        //cityId = p.CityId,
                        name = p.NAME,
                        //countryId = p.CountryId,
                        emailAddress = p.EMAILADDRESS,
                        phoneNumber = p.PHONENUMBER,
                        address = p.ADDRESS
                    });
        }
        public async Task<bool> AddAccreditedPrincipals(AccreditedPrincipalsViewModel entity)
        {
            var solicitors = new TBL_LOAN_PRINCIPAL
            {
                //CityId = entity.cityId,
                NAME = entity.name,
                //PrincipalsBVN = entity.principalsBVN,
                PRINCIPALSREGNUMBER = entity.principalsRegNumber,
                ACCOUNTNUMBER = entity.accountNumber,
                //CountryId = entity.countryId,
                EMAILADDRESS = entity.emailAddress,
                PHONENUMBER = entity.phoneNumber,
                ADDRESS = entity.address,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false
            };
            context.TBL_LOAN_PRINCIPAL.Add(solicitors);

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralValuerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_Collateral_Principals with Id: {entity.principalsId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }
        public async Task<bool> UpdateAccreditedPrincipals(AccreditedPrincipalsViewModel entity, int id)
        {
            var principal = context.TBL_LOAN_PRINCIPAL.Find(id);
            if (principal != null)
            {
                //principal.CityId = entity.cityId;
                principal.NAME = entity.name;
                principal.PRINCIPALSREGNUMBER = entity.principalsRegNumber;
                //principal.PrincipalsBVN = entity.principalsBVN;
                principal.ACCOUNTNUMBER = entity.accountNumber;
                //principal.CountryId = entity.countryId;
                principal.EMAILADDRESS = entity.emailAddress;
                principal.PHONENUMBER = entity.phoneNumber;
                principal.ADDRESS = entity.address;

            };

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Collateral_Principals with Id: {entity.principalsId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }
        #endregion
        #region Recovery Agents
        //public IEnumerable<AccreditedRecoveryAgentViewModel> GetAccreditedRecoveryAgent(int companyId)
        //{
        //    return (from r in context.tbl_Collateral_RecoveryAgents
        //            where r.CompanyId == companyId
        //            select new AccreditedRecoveryAgentViewModel
        //            {
        //                recoveryAgentsId = r.RecoveryAgentsId,
        //                recoveryAgentsLicenceNumber = r.RecoveryAgentsLicenceNumber,
        //                accountNumber = r.AccountNumber,
        //                agentBVN = r.AgentBVN,
        //                cityId = r.CityId,
        //                name = r.Name,
        //                countryId = r.CountryId,
        //                emailAddress = r.EmailAddress,
        //                phoneNumber = r.PhoneNumber,
        //                address = r.Address
        //            });
        //}
        //public async Task<bool> AddAccreditedRecoveryAgents(AccreditedRecoveryAgentViewModel entity)
        //{
        //    var agent = new tbl_Collateral_RecoveryAgents
        //    {
        //        CityId = entity.cityId,
        //        Name = entity.name,
        //        RecoveryAgentsLicenceNumber = entity.recoveryAgentsLicenceNumber,
        //        AgentBVN = entity.agentBVN,
        //        AccountNumber = entity.accountNumber,
        //        CountryId = entity.countryId,
        //        EmailAddress = entity.emailAddress,
        //        PhoneNumber = entity.phoneNumber,
        //        Address = entity.address,
        //        CompanyId = entity.companyId,
        //        CreatedBy = entity.createdBy,
        //        DateTimeCreated = DateTime.Now,
        //        Deleted = false
        //    };
        //    context.tbl_Collateral_RecoveryAgents.Add(agent);

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralValuerAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Added tbl_Collateral_RecoveryAgents with Id: {entity.recoveryAgentsId} ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now,
        //    };

        //    auditTrail.AddAuditTrail(audit);
        //    var response = await context.SaveChangesAsync() != 0;
        //    return response;
        //}
        //public async Task<bool> UpdateAccreditedRecoveryAgent(AccreditedRecoveryAgentViewModel entity, int id)
        //{
        //    var agents = context.tbl_Collateral_RecoveryAgents.Find(id);
        //    if (agents != null)
        //    {
        //        agents.CityId = entity.cityId;
        //        agents.Name = entity.name;
        //        agents.AgentBVN = entity.agentBVN;
        //        agents.RecoveryAgentsLicenceNumber = entity.recoveryAgentsLicenceNumber;
        //        agents.AccountNumber = entity.accountNumber;
        //        agents.CountryId = entity.countryId;
        //        agents.EmailAddress = entity.emailAddress;
        //        agents.PhoneNumber = entity.phoneNumber;
        //        agents.Address = entity.address;

        //    };

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralTypeAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Updated tbl_Collateral_RecoveryAgents with Id: {entity.recoveryAgentsId} ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now,
        //    };

        //    auditTrail.AddAuditTrail(audit);
        //    var response = await context.SaveChangesAsync() != 0;
        //    return response;
        //}
        #endregion
        #region Auditors
        //public IEnumerable<AccreditedAuditorsViewModel> GetAccreditedAuditors(int companyId)
        //{
        //    return (from a in context.tbl_Collateral_Auditors
        //            where a.CompanyId == companyId
        //            select new AccreditedAuditorsViewModel
        //            {
        //                auditorsId = a.AuditorsId,
        //                auditorsLicenceNumber = a.AuditorsLicenceNumber,
        //                cityId = a.CityId,
        //                name = a.Name,
        //                countryId = a.CountryId,
        //                emailAddress = a.EmailAddress,
        //                phoneNumber = a.PhoneNumber,
        //                address = a.Address
        //            });
        //}
        //public async Task<bool> AddAccreditedAuditors(AccreditedAuditorsViewModel entity)
        //{
        //    var auditor = new tbl_Collateral_Auditors
        //    {
        //        CityId = entity.cityId,
        //        Name = entity.name,
        //        AuditorsLicenceNumber = entity.auditorsLicenceNumber,
        //        CountryId = entity.countryId,
        //        EmailAddress = entity.emailAddress,
        //        PhoneNumber = entity.phoneNumber,
        //        Address = entity.address,
        //        CompanyId = entity.companyId,
        //        CreatedBy = entity.createdBy,
        //        DateTimeCreated = DateTime.Now,
        //        Deleted = false
        //    };
        //    context.tbl_Collateral_Auditors.Add(auditor);

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralValuerAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Added tbl_Collateral_Auditors with Id: {entity.auditorsId} ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now,
        //    };

        //    auditTrail.AddAuditTrail(audit);
        //    var response = await context.SaveChangesAsync() != 0;
        //    return response;
        //}
        //public async Task<bool> UpdateAccreditedAuditors(AccreditedAuditorsViewModel entity, int id)
        //{
        //    var auditors = context.tbl_Collateral_Auditors.Find(id);
        //    if (auditors != null)
        //    {
        //        auditors.CityId = entity.cityId;
        //        auditors.Name = entity.name;
        //        auditors.AuditorsLicenceNumber = entity.auditorsLicenceNumber;
        //        auditors.CountryId = entity.countryId;
        //        auditors.EmailAddress = entity.emailAddress;
        //        auditors.PhoneNumber = entity.phoneNumber;
        //        auditors.Address = entity.address;

        //    };

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralTypeAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Updated tbl_Collateral_Auditors with Id: {entity.auditorsId} ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now,
        //    };

        //    auditTrail.AddAuditTrail(audit);
        //    var response = await context.SaveChangesAsync() != 0;
        //    return response;
        //}
        #endregion

        #region Loan Consultants

        public List<LoanConsultantViewModel> GetLoanConsultant(int applicationId)
        {
            return context.TBL_LOAN_APPLICATION_DETL_CON.Where(x => x.DELETED == false && x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
            .Select(x => new LoanConsultantViewModel
            {
                id = x.LOANAPPLICATIONCONSULTANTID,
                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                accreditedConsultantId = x.ACCREDITEDCONSULTANTID,
                consultantName = context.TBL_ACCREDITEDCONSULTANT.FirstOrDefault(c => c.ACCREDITEDCONSULTANTID == x.ACCREDITEDCONSULTANTID).FIRMNAME,
                description = x.DESCRIPTION,
                productCustomerName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
            })
            .ToList();
        }

        public bool AddLoanConsultant(LoanConsultantViewModel model)
        {
            var data = new TBL_LOAN_APPLICATION_DETL_CON
            {
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                ACCREDITEDCONSULTANTID = model.accreditedConsultantId,
                DESCRIPTION = model.description,
                DATETIMECREATED = DateTime.Now,//genSetup.GetApplicationDate(),
                CREATEDBY = model.createdBy,
                DELETED = false,
            };

            context.TBL_LOAN_APPLICATION_DETL_CON.Add(data);

            //// Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanConsultantAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"Added Loan Consultant  ",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};
            //auditTrail.AddAuditTrail(audit);
            //// End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanConsultant(int id, LoanConsultantViewModel model)
        {
            var data = this.context.TBL_LOAN_APPLICATION_DETL_CON.Find(id);
            if (data == null) return false;

            data.ACCREDITEDCONSULTANTID = model.accreditedConsultantId;
            data.DESCRIPTION = model.description;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            //// Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanConsultantUpdated,
            //    STAFFID = model.lastUpdatedBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"Updated Loan Consultant' ",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};
            //auditTrail.AddAuditTrail(audit);
            //// End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveLoanConsultant(int id, UserInfo user)
        {
            var data = this.context.TBL_LOAN_APPLICATION_DETL_CON.Find(id);
            if (data == null) return false;

            var validateRecoveryCollection = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.ACCREDITEDCONSULTANT == id && x.DELETED == false).ToList();
            if (validateRecoveryCollection.Count() > 0 )
            {
                throw new SecureException("The agent is currently on recovery collection list. Kindly unassign before deleting");
            }

            data.DELETED = true;
            data.DATETIMEDELETED = DateTime.Now;
            data.DELETEDBY = user.createdBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Condition Precedent' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        #endregion Loan Consultants

    }
}

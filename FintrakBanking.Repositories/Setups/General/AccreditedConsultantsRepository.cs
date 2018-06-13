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

namespace FintrakBanking.Repositories.Setups.General
{
    public class AccreditedConsultantsRepository : IAccreditedConsultantsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private int AccreditedConsultantId;
        public AccreditedConsultantsRepository(FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
        }
        #region Solicitors
        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId)
        {
            return (from m in context.TBL_ACCREDITEDCONSULTANT
                    where m.COMPANYID == companyId
                    select new AccreditedConsultantsViewModel
                    {
                        accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                        registrationNumber = m.REGISTRATIONNUMBER,
                        name = m.NAME,
                        firmName = m.FIRMNAME,
                        accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                        cityId = (short) m.CITYID,
                        accountNumber = m.ACCOUNTNUMBER,
                        solicitorBVN = m.SOLICITORBVN,
                        countryId = m.COUNTRYID,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,
                        coreCompetence = m.CORECOMPETENCE,
                        accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(x => x.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).FirstOrDefault().NAME,
                        accreditedConsultantStates = context.TBL_ACCREDITEDCONSULTANT_STATE.Where(x => x.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID).Select(k =>
                           new AccreditedConsultantStateViewModel()
                           {
                               accreditedConsultantStateCoveredID = k.CONSULT_STATE_COVREDID,
                               stateId = k.STATEID,
                               stateName = context.TBL_STATE.FirstOrDefault(x=> x.STATEID == k.STATEID).STATENAME,
                               accreditedConsultantId = k.ACCREDITEDCONSULTANTID
                           }).ToList()
                    });
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultantsByStateId(int companyId, int stateId)
        {
            var data =   (from m in context.TBL_ACCREDITEDCONSULTANT
                    join c in context.TBL_ACCREDITEDCONSULTANT_STATE on m.ACCREDITEDCONSULTANTID equals c.ACCREDITEDCONSULTANTID
                    where m.COMPANYID == companyId && c.STATEID == stateId 
                    select new AccreditedConsultantsViewModel
                    {
                        accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                        registrationNumber = m.REGISTRATIONNUMBER,
                        name = m.NAME,
                        firmName = m.FIRMNAME,
                        accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                        cityId = (short) m.CITYID,
                        accountNumber = m.ACCOUNTNUMBER,
                        solicitorBVN = m.SOLICITORBVN,
                        countryId = m.COUNTRYID,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,
                        coreCompetence = m.CORECOMPETENCE,
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

        public async Task<bool> AddAccreditedConsultants(AccreditedConsultantsViewModel entity)
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
                TBL_ACCREDITEDCONSULTANT_STATE = accreditedConsultantStates
            };
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralValuerAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_AccreditedConsultant with Id: {entity.accreditedConsultantId} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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

                    throw new Exception(ex.Message);
                }
            }
            return output;
        }
        public async Task<bool> UpdateAccreditedConsultants(AccreditedConsultantsViewModel entity, int id)
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }
        private  void AddUpdateAccreditedConsultantStates(List<AccreditedConsultantStateViewModel> entity)
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
        //public IEnumerable<AccreditedPrincipalsViewModel> GetAccreditedPrincipals(int companyId)
        //{
        //    return (from p in context.tbl_Collateral_Principals
        //            where p.CompanyId == companyId
        //            select new AccreditedPrincipalsViewModel
        //            {
        //                principalsId = p.PrincipalsId,
        //                principalsRegNumber = p.PrincipalsRegNumber,
        //                accountNumber = p.AccountNumber,
        //                principalsBVN = p.PrincipalsBVN,
        //                cityId = p.CityId,
        //                name = p.Name,
        //                countryId = p.CountryId,
        //                emailAddress = p.EmailAddress,
        //                phoneNumber = p.PhoneNumber,
        //                address = p.Address
        //            });
        //}
        //public async Task<bool> AddAccreditedPrincipals(AccreditedPrincipalsViewModel entity)
        //{
        //    var solicitors = new tbl_Collateral_Principals
        //    {
        //        CityId = entity.cityId,
        //        Name = entity.name,
        //        PrincipalsBVN = entity.principalsBVN,
        //        PrincipalsRegNumber = entity.principalsRegNumber,
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
        //    context.tbl_Collateral_Principals.Add(solicitors);

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralValuerAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Added tbl_Collateral_Principals with Id: {entity.principalsId} ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now,
        //    };

        //    auditTrail.AddAuditTrail(audit);
        //    var response = await context.SaveChangesAsync() != 0;
        //    return response;
        //}
        //public async Task<bool> UpdateAccreditedPrincipals(AccreditedPrincipalsViewModel entity, int id)
        //{
        //    var principal = context.tbl_Collateral_Principals.Find(id);
        //    if (principal != null)
        //    {
        //        principal.CityId = entity.cityId;
        //        principal.Name = entity.name;
        //        principal.PrincipalsRegNumber = entity.principalsRegNumber;
        //        principal.PrincipalsBVN = entity.principalsBVN;
        //        principal.AccountNumber = entity.accountNumber;
        //        principal.CountryId = entity.countryId;
        //        principal.EmailAddress = entity.emailAddress;
        //        principal.PhoneNumber = entity.phoneNumber;
        //        principal.Address = entity.address;

        //    };

        //    // Audit Section ----------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralTypeAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Updated tbl_Collateral_Principals with Id: {entity.principalsId} ",
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
    }
}

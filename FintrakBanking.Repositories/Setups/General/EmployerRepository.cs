using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using System.Data;
using FintrakBanking.Common.Extensions;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Repositories.Setups.General
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;
        private IWorkflow _workflow;

        public EmployerRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail, IWorkflow workflow)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
            _workflow = workflow;
        }

        #region Employer functions
        public string addEmployer(EmployerViewModel employer)
        {
            if (employer != null)
            {
                var employerDb = new TBL_CUSTOMER_EMPLOYER
                {
                    EMPLOYER_NAME = employer.employerName,
                    ADDRESS = employer.address,
                    PHONENUMBER = employer.phoneNumber,
                    EMAILADDRESS = employer.emailAddress,
                    EMPLOYER_SUB_TYPEID = employer.employerSubTypeId,
                    CITYID  = employer.cityId,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    CREATEDBY = employer.createdBy,
                    COMPANYID = employer.companyId,
                    DELETED = false,
                    APPROVALSTATUSID = (short) ApprovalStatusEnum.Pending,
                    OPERATIONID = (int) OperationsEnum.RelatedEmployer,
                    ACTIVE = employer.active,
                    ESTABLISHMENTDATE = employer.establishmentDate,
                    STATEID = employer.stateId,
                    COUNTRYID = employer.countryId
                };

                _context.TBL_CUSTOMER_EMPLOYER.Add(employerDb);
              
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employer.staffId,
                    BRANCHID = (short)employer.userBranchId,
                    DETAIL = $"Loan employer with {employer.companyId} id is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been added successful";

            }
            return "The record has not been added";
        }

        public string deleteEmployer(int employerId, EmployerViewModel employer)
        {
          var employerDel =  _context.TBL_CUSTOMER_EMPLOYER.Find(employerId);
            if (employerDel!=null)
            {
                employerDel.DATETIMEDELETED = employer.dateTimeDeleted;
                employerDel.DELETED = true;
                employerDel.DELETEDBY = employer.staffId;
                
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employer.staffId,
                    BRANCHID = (short)employer.userBranchId,
                    DETAIL = $"Loan employer with {employer.companyId} id is deleted",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been deleted successful";

            }
            return "The record has not been deleted";
        }

        public IEnumerable<EmployerViewModel> getEmployer(int companyId)
        {
            var employerData = (from a in _context.TBL_CUSTOMER_EMPLOYER
                                join c in _context.TBL_CITY on a.CITYID equals c.CITYID
                                join sb in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB on a.EMPLOYER_SUB_TYPEID equals sb.EMPLOYER_SUB_TYPEID
                                where a.COMPANYID == companyId & a.DELETED == false
                                orderby a.EMPLOYER_NAME
                                select new EmployerViewModel {
                                    employerName = a.EMPLOYER_NAME,
                                    emailAddress = a.EMAILADDRESS,
                                    phoneNumber = a.PHONENUMBER,
                                    address = a.ADDRESS,
                                    cityName = c.CITYNAME,
                                    employerId = a.EMPLOYERID,
                                    createdBy = a.CREATEDBY,
                                    employerSubTypeName = sb.EMPLOYER_SUB_TYPE_NAME,
                                    employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                                    cityId = a.CITYID,
                                    companyId = companyId,
                                    stateId = c.TBL_LOCALGOVERNMENT.STATEID,
                                    
                                    employerTypeId = sb.EMPLOYER_TYPEID,
                                    //employerTypeName = sb.TBL_CUSTOMER_EMPLOYER_TYPE.EMPLOYER_TYPE_NAME
                                }).ToList();
            return employerData;
        }

        public IEnumerable<EmployerViewModel> getAllPendingEmployers(int companyId)
        {
            var employerData = (from a in _context.TBL_CUSTOMER_EMPLOYER
                                join c in _context.TBL_CITY on a.CITYID equals c.CITYID
                                join sb in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB on a.EMPLOYER_SUB_TYPEID equals sb.EMPLOYER_SUB_TYPEID
                                where a.COMPANYID == companyId && a.DELETED == false && a.APPROVALSTATUSID == (short) ApprovalStatusEnum.Pending
                                orderby a.EMPLOYER_NAME
                                select new EmployerViewModel
                                {
                                    employerName = a.EMPLOYER_NAME,
                                    emailAddress = a.EMAILADDRESS,
                                    phoneNumber = a.PHONENUMBER,
                                    address = a.ADDRESS,
                                    cityName = c.CITYNAME,
                                    employerId = a.EMPLOYERID,
                                    createdBy = a.CREATEDBY,
                                    employerSubTypeName = sb.EMPLOYER_SUB_TYPE_NAME,
                                    employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                                    cityId = a.CITYID,
                                    companyId = companyId,
                                    //stateId = c.TBL_LOCALGOVERNMENT.STATEID,

                                    stateId = a.STATEID,
                                    countryId = a.COUNTRYID,
                                    establishmentDate = a.ESTABLISHMENTDATE,
                                    active = a.ACTIVE,
                                    operationId = a.OPERATIONID,
                                    approvalStatusId = a.APPROVALSTATUSID,
                                    employerTypeId = sb.EMPLOYER_TYPEID,
                                    approvalStatus = _context.TBL_APPROVAL_STATUS.FirstOrDefault(b => b.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                    //employerTypeName = sb.TBL_CUSTOMER_EMPLOYER_TYPE.EMPLOYER_TYPE_NAME
                                }).ToList();
            return employerData;
        }

        public IEnumerable<EmployerViewModel> getAllApprovedEmployers(int companyId)
        {
            var employerData = (from a in _context.TBL_CUSTOMER_EMPLOYER
                                join c in _context.TBL_CITY on a.CITYID equals c.CITYID
                                join sb in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB on a.EMPLOYER_SUB_TYPEID equals sb.EMPLOYER_SUB_TYPEID
                                where a.COMPANYID == companyId && a.DELETED == false && a.APPROVALSTATUSID == (short) ApprovalStatusEnum.Approved
                                orderby a.EMPLOYER_NAME
                                select new EmployerViewModel
                                {
                                    employerName = a.EMPLOYER_NAME,
                                    emailAddress = a.EMAILADDRESS,
                                    phoneNumber = a.PHONENUMBER,
                                    address = a.ADDRESS,
                                    cityName = c.CITYNAME,
                                    employerId = a.EMPLOYERID,
                                    createdBy = a.CREATEDBY,
                                    employerSubTypeName = sb.EMPLOYER_SUB_TYPE_NAME,
                                    employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                                    cityId = a.CITYID,
                                    companyId = companyId,
                                    //stateId = c.TBL_LOCALGOVERNMENT.STATEID,

                                    stateId = a.STATEID,
                                    countryId = a.COUNTRYID,
                                    establishmentDate = a.ESTABLISHMENTDATE,
                                    active = a.ACTIVE,
                                    operationId = a.OPERATIONID,
                                    approvalStatusId = a.APPROVALSTATUSID,
                                    employerTypeId = sb.EMPLOYER_TYPEID,
                                    //employerTypeName = sb.TBL_CUSTOMER_EMPLOYER_TYPE.EMPLOYER_TYPE_NAME
                                }).ToList();
            return employerData;
        }

        public IEnumerable<EmployerViewModel> GetRelatedEmployersWaitingForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.RelatedEmployer;
            var levelIds = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var relatedEmployers = (from a in _context.TBL_CUSTOMER_EMPLOYER
                                    join c in _context.TBL_CITY on a.CITYID equals c.CITYID
                                    join sb in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB on a.EMPLOYER_SUB_TYPEID equals sb.EMPLOYER_SUB_TYPEID
                                    join t in _context.TBL_APPROVAL_TRAIL on a.EMPLOYERID equals t.TARGETID
                                    where (a.DELETED == false && t.OPERATIONID == (int) OperationsEnum.RelatedEmployer
                                    && a.APPROVALSTATUSID == (short) ApprovalStatusEnum.Processing
                                    && t.APPROVALSTATEID != (int)ApprovalState.Ended && t.RESPONSESTAFFID == null
                                    && (t.LOOPEDSTAFFID == null || t.LOOPEDSTAFFID == staffId)
                                    && ((levelIds.Contains((int)t.TOAPPROVALLEVELID) && t.LOOPEDSTAFFID == null) || (!levelIds.Contains((int)t.TOAPPROVALLEVELID) && t.LOOPEDSTAFFID == staffId))
                                    && (t.TOSTAFFID == null || t.TOSTAFFID == staffId))
                                    select new EmployerViewModel
                                    {
                                        employerName = a.EMPLOYER_NAME,
                                        emailAddress = a.EMAILADDRESS,
                                        phoneNumber = a.PHONENUMBER,
                                        address = a.ADDRESS,
                                        cityName = c.CITYNAME,
                                        employerId = a.EMPLOYERID,
                                        createdBy = a.CREATEDBY,
                                        employerSubTypeName = sb.EMPLOYER_SUB_TYPE_NAME,
                                        employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                                        cityId = a.CITYID,

                                        stateId = a.STATEID,
                                        countryId = a.COUNTRYID,
                                        establishmentDate = a.ESTABLISHMENTDATE.Value,
                                        active = a.ACTIVE,
                                        operationId = a.OPERATIONID,
                                        employerTypeId = sb.EMPLOYER_TYPEID,

                                        approvalStatusId = t.APPROVALSTATUSID,
                                        approvalTrailId = t.APPROVALTRAILID,
                                        currentApprovalLevelId = t.TOAPPROVALLEVELID,
                                        currentApprovalLevel = t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                        approvalStatus = _context.TBL_APPROVAL_STATUS.FirstOrDefault(a => a.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                        dateTimeCreated = a.DATETIMECREATED
                                    }).ToList();
            return relatedEmployers;
        }

        public WorkflowResponse ForwardRelatedEmployerForApproval(EmployerViewModel model)
        {
            int operationId = (int)OperationsEnum.RelatedEmployer;
            var employer = _context.TBL_CUSTOMER_EMPLOYER.Find(model.employerId);

            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved)
            {
                model.forwardAction = (int)ApprovalStatusEnum.Processing;
            }
            else
            {
                model.forwardAction = (int)ApprovalStatusEnum.Disapproved;
            }

            // workflow
            _workflow.OperationId = operationId;
            _workflow.StaffId = model.createdBy;
            _workflow.TargetId = model.employerId;
            _workflow.CompanyId = model.companyId;
            _workflow.Vote = model.vote;
            _workflow.NextLevelId = 0;
            _workflow.ToStaffId = null;
            _workflow.StatusId = (int) model.forwardAction;
            _workflow.Comment = model.comment;

            _workflow.DeferredExecution = true;
            _workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();

            // update new employer status
            if (employer != null) {
                if (employer.APPROVALSTATUSID == (short) ApprovalStatusEnum.Pending) {
                    employer.APPROVALSTATUSID = (short) ApprovalStatusEnum.Processing;
                }
                
            }

            if (_workflow.NewState == (int)ApprovalState.Ended)
            {
                if (_workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
                {
                    employer.APPROVALSTATUSID = (short) ApprovalStatusEnum.Approved;
                    _workflow.SetResponse = true;
                }
                else
                {
                    employer.APPROVALSTATUSID = (short) ApprovalStatusEnum.Disapproved;
                }
            }

            _context.SaveChanges();
            return _workflow.Response;
        }

        public String ResponseMessage(WorkflowResponse response, string itemHeading)
        {
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    return "The " + itemHeading + " request has been APPROVED successfully";
                }
                else
                {
                    return "The " + itemHeading + " request has been DISAPPROVED successfully";
                }
            }

        }

        public EmployerViewModel getEmployer(int employerId, int companyId)
        {
            var employerData = (from a in _context.TBL_CUSTOMER_EMPLOYER
                                join c in _context.TBL_CITY on a.CITYID equals c.CITYID
                                where a.COMPANYID == companyId & a.DELETED == false & a.EMPLOYERID==employerId
                                select new EmployerViewModel
                                {

                                    employerName = a.EMPLOYER_NAME,
                                    emailAddress = a.EMAILADDRESS,
                                    phoneNumber = a.PHONENUMBER,
                                    address = a.ADDRESS,
                                    cityName = c.CITYNAME,
                                    employerId = a.EMPLOYERID,
                                    createdBy = a.CREATEDBY

                                }).FirstOrDefault();

            return employerData;
        }

       

        public string updateEmployer(int employerId, EmployerViewModel employer)
        {
            var employerDel = _context.TBL_CUSTOMER_EMPLOYER.Find(employerId);
            if (employerDel != null)
            {
                employerDel.EMPLOYER_NAME = employer.employerName;
                employerDel.ADDRESS = employer.address;
                employerDel.PHONENUMBER = employer.phoneNumber;
                employerDel.EMAILADDRESS = employer.emailAddress;
                employerDel.EMPLOYER_SUB_TYPEID = employer.employerSubTypeId;
                employerDel.CITYID = employer.cityId;
                employerDel.DATETIMECREATED = _genSetup.GetApplicationDate();
                employerDel.DELETED = false;
                employerDel.DATETIMEUPDATED = DateTime.Now;
                employerDel.LASTUPDATEDBY = employer.staffId;
                

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employer.staffId,
                    BRANCHID = (short)employer.userBranchId,
                    DETAIL = $"Loan employer with {employer.companyId} id is updated",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been updated successful";

            }
            return "The record has not been updated";
        }


        public IEnumerable<EmployerSubType> getEmployerSunType(int emplyerTypeId)
        {
            var subType = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB
                          where a.EMPLOYER_TYPEID == emplyerTypeId
                          select new EmployerSubType
                          {
                              employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                              employerSubTypeName = a.EMPLOYER_SUB_TYPE_NAME
                          };
            return subType;
        }

        public IEnumerable<EmployerSubType> getAllEmployerSubTypes()
        {
            var subType = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB
                          select new EmployerSubType
                          {
                              employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                              employerSubTypeName = a.EMPLOYER_SUB_TYPE_NAME
                          };
            return subType;
        }

        public IEnumerable<EmployerType> getEmployerType()
        {
            var type = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE
                       select new EmployerType
                       {
                           employerTypeId = a.EMPLOYER_TYPEID,
                           employerTypeName = a.EMPLOYER_TYPE_NAME
                       };
            return type;
        }


        #endregion


        #region Employer Type
        public string addEmployerType(EmployerViewModel employerType)
        {
            if (employerType != null)
            {
                var employerDb = new TBL_CUSTOMER_EMPLOYER_TYPE
                {
                    EMPLOYER_TYPE_NAME = employerType.employerTypeName
                };
                _context.TBL_CUSTOMER_EMPLOYER_TYPE.Add(employerDb);


                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerType.staffId,
                    BRANCHID = (short)employerType.userBranchId,
                    DETAIL = $"Employer Type with {employerType.employerTypeName} name is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been added successful";

            }
            return "The record has not been added";
        }

        public string deleteEmployerType(int employerId, EmployerViewModel employerType)
        {
            var employerDel = _context.TBL_CUSTOMER_EMPLOYER_TYPE.Find(employerType.employerTypeId);
            if (employerDel != null)
            {
                _context.TBL_CUSTOMER_EMPLOYER_TYPE.Remove(employerDel);
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerType.staffId,
                    BRANCHID = (short)employerType.userBranchId,
                    DETAIL = $"Loan employer with {employerType.employerTypeName} name is deleted",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been deleted successful";

            }
            return "The record has not been deleted";
        }

        public string updateEmployerType(int employerTypeId, EmployerViewModel employerType)
        {
            var employerDel = _context.TBL_CUSTOMER_EMPLOYER_TYPE.Find(employerTypeId);
            if (employerDel != null)
            {
                employerDel.EMPLOYER_TYPE_NAME = employerType.employerTypeName;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerType.staffId,
                    BRANCHID = (short)employerType.userBranchId,
                    DETAIL = $"Loan employer with {employerType.companyId} id is updated",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been updated successful";

            }
            return "The record has not been updated";
        }

        public IEnumerable<EmployerType> getEmployerType(int employerTypeId)
        {
            var type = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE
                       where a.EMPLOYER_TYPEID == employerTypeId
                       select new EmployerType
                       {
                           employerTypeId = a.EMPLOYER_TYPEID,
                           employerTypeName = a.EMPLOYER_TYPE_NAME
                       };
            return type;
        }

        #endregion

        #region Employer Type
        public string addEmployerSubType(EmployerViewModel employerSubType)
        {
            if (employerSubType != null)
            {
                var employerDb = new TBL_CUSTOMER_EMPLOYER_TYPE_SUB
                {
                    EMPLOYER_TYPEID = employerSubType.employerTypeId,
                    EMPLOYER_SUB_TYPE_NAME = employerSubType.employerSubTypeName
                };
                _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB.Add(employerDb);


                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerSubType.staffId,
                    BRANCHID = (short)employerSubType.userBranchId,
                    DETAIL = $"Employer Sub Type with {employerSubType.employerSubTypeName} name is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerSubType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been added successful";

            }
            return "The record has not been added";
        }

        public string deleteEmployerSubType(int employerId, EmployerViewModel employerSubType)
        {
            var employerDel = _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB.Find(employerSubType.employerSubTypeId);
            if (employerDel != null)
            {
                _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB.Remove(employerDel);
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerSubType.staffId,
                    BRANCHID = (short)employerSubType.userBranchId,
                    DETAIL = $"Loan employer sub type with {employerSubType.employerSubTypeName} name is deleted",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerSubType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been deleted successful";

            }
            return "The record has not been deleted";
        }

        public string updateEmployerSubType(int employerTypeId, EmployerViewModel employerSubType)
        {
            var employerDel = _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB.Find(employerTypeId);
            if (employerDel != null)
            {
                employerDel.EMPLOYER_SUB_TYPE_NAME = employerSubType.employerSubTypeName;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employerSubType.staffId,
                    BRANCHID = (short)employerSubType.userBranchId,
                    DETAIL = $"Loan employer sub type with {employerSubType.companyId} id is updated",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = employerSubType.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this._auditTrail.AddAuditTrail(audit);
                _context.SaveChanges();

                return "The record has been updated successful";

            }
            return "The record has not been updated";
        }

        public IEnumerable<EmployerSubType> getEmployerSubType(int employerSubTypeId)
        {
            var type = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE_SUB
                       join b in _context.TBL_CUSTOMER_EMPLOYER_TYPE on a.EMPLOYER_TYPEID equals b.EMPLOYER_TYPEID
                       select new EmployerSubType
                       {
                           employerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                           employerSubTypeName = a.EMPLOYER_SUB_TYPE_NAME,
                           employerTypeName = b.EMPLOYER_TYPE_NAME,
                           employerTypeId = b.EMPLOYER_TYPEID
                       };
            return type;
        }

        #endregion
    }

}

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

namespace FintrakBanking.Repositories.Setups.General
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;

        public EmployerRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
        }

        public string addEmployer(EmployerViewModel employer)
        {
            if (employer!=null)
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
                    CREATEDBY =employer.createdBy,
                    COMPANYID=employer.companyId,
                    DELETED = false
                };
            _context.TBL_CUSTOMER_EMPLOYER.Add(employerDb);
              

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = employer.staffId,
                    BRANCHID = (short)employer.userBranchId,
                    DETAIL = $"Loan employer with {employer.companyId} id is added",
                    IPADDRESS = employer.userIPAddress,
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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
                    IPADDRESS = employer.userIPAddress,
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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
                                select new EmployerViewModel {

                                    employerName=a.EMPLOYER_NAME,
                                    emailAddress=a.EMAILADDRESS,
                                    phoneNumber=a.PHONENUMBER,
                                    address=a.ADDRESS,
                                    cityName = c.CITYNAME,
                                    employerId=a.EMPLOYERID,
                                    createdBy = a.CREATEDBY,
                                    employerSubTypeName = sb.EMPLOYER_SUB_TYPE_NAME,
                                    employerSubTypeId=a.EMPLOYER_SUB_TYPEID,
                                    cityId=a.CITYID,
                                    companyId=companyId,
                                    stateId=c.STATEID,
                                    employerTypeId = sb.EMPLOYER_TYPEID


                                }).ToList();

            return employerData;
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
                    IPADDRESS = employer.userIPAddress,
                    URL = employer.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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
                              EmployerSubTypeId = a.EMPLOYER_SUB_TYPEID,
                              EmployerSubTypeName = a.EMPLOYER_SUB_TYPE_NAME
                          };
            return subType;
        }

        public IEnumerable<EmployerType> getEmployerType()
        {
            var type = from a in _context.TBL_CUSTOMER_EMPLOYER_TYPE
                       select new EmployerType
                       {
                           EmployerTypeId = a.EMPLOYER_TYPEID,
                           EmployerTypeName = a.EMPLOYER_TYPE_NAME
                       };
            return type;
        }
    }
}

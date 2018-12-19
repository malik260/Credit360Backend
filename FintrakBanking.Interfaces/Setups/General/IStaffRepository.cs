using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IStaffRepository
    {
        bool UpdateStaff(int staffid, StaffInfoViewModel staffModel);

        bool AddTempStaff(StaffInfoViewModel staffModel);

        int GoForApproval(ApprovalViewModel entity);

        IEnumerable<StaffInfoViewModel> GetAllStaff();
        List<StaffSensitivityLevelViewModel> GetStaffSensitivityLevel();
        IEnumerable<StaffInfoViewModel> GetStaffAwaitingApprovals(int staffId, int companyId);

        IEnumerable<StaffViewModel> GetStaffName();

        IEnumerable<simpleStaffModel> GetStaffRelationshipManagerByStaffId(int staffId);

        IEnumerable<simpleStaffModel> GetStaffBusinessManagerByStaffId(int staffId);

        IEnumerable<simpleStaffModel> GetStaffByUnitId(int companyId, short departmentUnitId);

        bool DeleteStaff(int staffId, UserInfo user);

        bool IsStaffCodeAlreadyExist(string staffCode);

        bool IsTempStaffExist(string staffCode);

        StaffInfoViewModel GetStaffById(int staffId);

        StaffDetailsModel GetTempStaffDetail(int staffId);

        IEnumerable<StaffDetailsModel> GetStaffDetails(int companyId);

        StaffDetailsModel GetStaffDetail(string staffCode, int companyId);

        IEnumerable<simpleStaffModel> GetStaffNames(int companyId); 

        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();

        IQueryable<simpleStaffModel> SearchStaff(string searchString, int companyId); 

        IQueryable<simpleStaffModel> SearchStaffbyDepartmentId(string searchString, int companyId, int departmentId);

        bool AddStaffSignature(StaffDocumentViewModel model, byte[] file);

        bool UpdateStaffSignature(StaffDocumentViewModel model, int documentId);

        staffBulkFeedbackViewModel UploadStaffData(StaffDocumentViewModel model, byte[] file);

        IEnumerable<StaffDocumentViewModel> GetAllStaffSignatures(int companyId);

        StaffDocumentViewModel GetStaffSignatureByStaffCode(string staffCode, int companyId);

        bool UpdateSupervisor(SupervisorViewModel entity);

        bool GoForBulkApproval(List<ApprovalViewModel> model, UserInfo userInfo);
        // byte[] GetStaffSampleDocument();

        List<simpleStaffModel> StaffReportingLine(int staffId, string staffCode, int companyId);
        simpleStaffModel StaffReportingTo(int staffId, string staffCode, int companyId);
        simpleStaffModel StaffInformation(int staffId, string staffCode, int companyId);
        StaffMISDetailsModel StaffMIS(int staffId, string staffCode);
        IEnumerable<simpleStaffModel> GetSearchedStaff(string search);
    }
}
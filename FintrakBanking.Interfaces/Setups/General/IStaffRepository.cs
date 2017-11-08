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
        Task<bool> UpdateStaff(int staffid, StaffInfoViewModel staffModel);

        //bool AddStaff(StaffInfoViewModel staffModel);

        Task<bool> AddTempStaff(StaffInfoViewModel staffModel);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<StaffInfoViewModel> GetAllStaff();

        IEnumerable<StaffInfoViewModel> GetStaffAwaitingApprovals(int staffId, int companyId);

        IEnumerable<StaffViewModel> GetStaffName();

        bool DeleteStaff(int staffId, UserInfo user);

        bool IsStaffCodeAlreadyExist(string staffCode);

        bool IsTempStaffExist(string staffCode);

        StaffInfoViewModel GetStaffById(int staffId);

        StaffDetailsModel GetTempStaffDetail(int staffId);
        IEnumerable<StaffDetailsModel> GetStaffDetails(int companyId);
        StaffDetailsModel GetStaffDetail(string staffCode, int companyId);

        //IEnumerable<StaffDetailsModel> GetTempStaffDetails();
        

        IEnumerable<simpleStaffModel> GetStaffNames(); 
        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();

        IQueryable<simpleStaffModel> SearchStaff(string searchString, int companyId); 
        IQueryable<simpleStaffModel> SearchStaffbyDepartmentId(string searchString, int companyId, int departmentId);


        bool AddStaffSignature(StaffDocumentViewModel model, byte[] file);
        bool UpdateStaffSignature(StaffDocumentViewModel model, int documentId);
        IEnumerable<StaffDocumentViewModel> GetAllStaffSignatures();
        IEnumerable<StaffDocumentViewModel> GetStaffSignatureByStaffCode(string staffCode);
    }
}
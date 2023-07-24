using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IJobRequestRepository
    {
        IEnumerable<JobTypeHubViewModel> GetJobTypeHubStaff();
        IEnumerable<HubStaffViewModel> GetHubStaffByHubId(short jobTypeHubId);
        IEnumerable<HubStaffViewModel> GetHubStaffByHubTypeUnitId(short jobTypeUnitId);
        IEnumerable<JobTypeUnitViewModel> GetAllJobTypeUnit(short jobTypeId);
        IEnumerable<JobTypeHubViewModel> GetAllJobTypeHub(short jobTypeId);
        // bool ChargeCustomerJob(CollateralViewModel model, string actionName, string actionType, int loanApplicationDetailId);
        bool AddJobDocumentOnly(RequestDocumentViewModel model, byte[] file);
        IEnumerable<JobRequestViewModel> GetJobRequestByFilter(int staffId, int branchId, string filter, int? startNumber);
        bool UpdateInvoiceStatus(JobRequestInvoiceViewModel model);
        List<jobReasignment> GetJobReasignmentStaffById(int staffId, int companyId);
        //List<jobReasignment> GetJobTypeReasignmentAdminStaff(int companyId);
        IEnumerable<ApprovalStatusViewModel> GetJobRequestApprovaStatus();
        IEnumerable<JobRequestStatusFeedbackViewModel> GetJobRequestStatusFeedback(short statusId, short jobTypeId);
        JobRequestViewModel GetJobRequest(int jobRequestId);

        List<JobRequestViewModel> GetApplicationJobRequest(int targetId, int operationId, short jobSourceId);

        IEnumerable<ApplicationJobRequest> GetLoanApplicationJobsById(int loanApplicationId, int companyId);

        //IEnumerable<JobRequestViewModel> GetAllJobRequest();

        // IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId);

        //IEnumerable<JobRequestViewModel> GetJobRequestByDepartment(int staffId);
        IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId, int branchId);

        IEnumerable<JobRequestMessageViewModel> GetJobComments(int jobRequestId);

        // bool AddJobRequest(JobRequestViewModel model);
        string AddGlobalJobRequest(JobRequestViewModel model);
        bool AddJobComment(JobRequestMessageViewModel model);

        bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId);

        bool ReRouteJobRequest(JobRequestViewModel model);
        bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId);

        // Legal jobs
        bool saveCollateralJobsChargesSpecifiedByLegal(JobRequestCollateralSearchViewModel model);

        // job type
        IEnumerable<JobTypeViewModel> GetAllJobType();
        IEnumerable<JobTypeViewModel> GetJobSubType(short jobId);
        IEnumerable<JobSubTypeClassViewModel> GetJobSubTypeClass(short jobSubTypeId);

        bool AddJobType(JobTypeViewModel model);

        bool UpdateJobType(JobTypeViewModel model, short jobTypeId);

        // staff
        //IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId);
        List<JobRequestViewModel> GetJobRequestLegalJobDetail();
        List<JobRequestDetailViewModel> GetJobRequestDetailsById(int jobRequestId);
        string AddJobDocument(RequestDocumentViewModel model, JobRequestViewModel requestModel, byte[] file);
        bool AddJobReplyAndDocument(RequestDocumentViewModel model, byte[] file);
        bool UpdateJobDocument(RequestDocumentViewModel model, int documentId);
        IEnumerable<RequestDocumentViewModel> GetAllJobDocument();
        RequestDocumentViewModel GetJobDocument(int documentId);
        IEnumerable<RequestDocumentViewModel> GetJobRequestDocuments(string jobRequestCode);
        IEnumerable<RequestDocumentViewModel> GetJobRequestDocumentById(int documentId);

        bool AcknowledgeJob(JobRequestViewModel entity, int jobRequestId);

        bool ChargeCustomerForOnSearchJobs(JobRequestCollateralSearchViewModel model);
        bool ReverseChargeOnCustomerForCollateralSearch(JobRequestCollateralSearchViewModel model);
        List<JobRequestDetailViewModel> GetLegalJobRequestDetails();

        IEnumerable<JobRequestViewModel> GetAllGlobalJobRequestByFacilityRef(string facilityRef);
        IEnumerable<JobRequestViewModel> GetJobRequestBySearchString(int staffId, string searchString);

        bool AssignJobTypeToStaff(jobReasignment model);
        bool DeleteJobTypeForAStaff(jobReasignment model);
        bool UpdateAsignedJobTypeToStaff(jobReasignment model);
        List<jobReasignment> GetJobTypeReasignmentAdmin(int companyId);

        bool mapJobTypeHubStaff(JobTypeHubViewModel model);
        bool UpdatemappedJobTypeHubStaff(JobTypeHubViewModel model);


        bool deleteJobDocument(int documentId, int staffId);
        bool DeleteMappedJobTypeHubStaff(int hubStaffId, int staffId);

        jobRequestCountViewModel GetJobRequestStatusCount(int staffId, int branchId);
        IEnumerable<LMSOperationListViewModel> getLMSRApplicationDetail(int targetId);
        IEnumerable<LMSOperationListViewModel> getLMSROperation(int targetId);
        IEnumerable<LMSOperationListViewModel> getLOSOperationLoanData(int loanId, int operationId);

        #region Job Request Feedback
        IEnumerable<LookupViewModel> GetJobRequestStatus();
        IEnumerable<JobRequestStatusFeedbackViewModel> GetAllJobRequestStatusFeedback();
        bool AddUpdateJobRequestFeedBack(JobRequestStatusFeedbackViewModel feedback);
        bool ValidateJobRequestFeedBack(string feedback);
        #endregion
    }
}
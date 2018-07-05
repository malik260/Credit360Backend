using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IJobRequestRepository
    {
        bool ChargeCustomerJob(CollateralViewModel model, string actionName, string actionType, int loanApplicationDetailId);
        IEnumerable<JobRequestStatusFeedbackViewModel> GetJobRequestStatusFeedback(short statusId, short jobTypeId);
        JobRequestViewModel GetJobRequest(int jobRequestId);

        List<JobRequestViewModel> GetApplicationJobRequest(int applicationDetailId);

        IEnumerable<ApplicationJobRequest> GetLoanApplicationJobsById(int loanApplicationId, int companyId);

        IEnumerable<JobRequestViewModel> GetAllJobRequest();

        // IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId);

        IEnumerable<JobRequestViewModel> GetJobRequestByDepartment(int staffId);
        IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId, int branchId);

        IEnumerable<JobRequestMessageViewModel> GetJobComments(int jobRequestId);

        // bool AddJobRequest(JobRequestViewModel model);
        string AddGlobalJobRequest(JobRequestViewModel model);
        bool AddJobComment(JobRequestMessageViewModel model);

        bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId);

        bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId);

        // Legal jobs
        bool EffectLegaCollateralJobs(JobRequestCollateralSearchViewModel model);

        // job type
        IEnumerable<JobTypeViewModel> GetAllJobType();
        IEnumerable<JobTypeViewModel> GetJobSubType(short jobId);

        bool AddJobType(JobTypeViewModel model);

        bool UpdateJobType(JobTypeViewModel model, short jobTypeId);

        // staff
        //IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId);
        List<JobRequestViewModel> GetJobRequestLegalJobDetail();
        bool AddJobDocument(RequestDocumentViewModel model, JobRequestViewModel requestModel, byte[] file);
        bool AddJobReplyAndDocument(RequestDocumentViewModel model, byte[] file);
        bool UpdateJobDocument(RequestDocumentViewModel model, int documentId);
        IEnumerable<RequestDocumentViewModel> GetAllJobDocument();
        RequestDocumentViewModel GetJobDocument(int documentId);
        IEnumerable<RequestDocumentViewModel> GetJobRequestDocuments(string jobRequestCode);
        IEnumerable<RequestDocumentViewModel> GetJobRequestDocumentById(int documentId);

        bool AcknowledgeJob(JobRequestViewModel entity, int jobRequestId);

        #region Job Request Feedback
        IEnumerable<LookupViewModel> GetJobRequestStatus();
        IEnumerable<JobRequestStatusFeedbackViewModel> GetAllJobRequestStatusFeedback();
        bool AddUpdateJobRequestFeedBack(JobRequestStatusFeedbackViewModel feedback);
        bool ValidateJobRequestFeedBack(string feedback);
        #endregion
    }
}
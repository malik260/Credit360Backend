using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IJobRequestRepository
    {
        JobRequestViewModel GetJobRequest(int jobRequestId);

        IEnumerable<JobRequestViewModel> GetAllJobRequest();

        IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId);

        IEnumerable<JobRequestViewModel> GetJobRequestByDepartment(int staffId);
        IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId);

        IEnumerable<JobRequestMessageViewModel> GetJobComments(int jobRequestId);

        bool AddJobRequest(JobRequestViewModel model);
        string AddGlobalJobRequest(JobRequestViewModel model);
        bool AddJobComment(JobRequestMessageViewModel model);

        bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId);

        bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId);

        // job type
        IEnumerable<JobTypeViewModel> GetAllJobType();
        IEnumerable<JobTypeViewModel> GetJobSubType(short jobId);

        bool AddJobType(JobTypeViewModel model);

        bool UpdateJobType(JobTypeViewModel model, short jobTypeId);

        // staff
        IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId);

        bool AddJobDocument(RequestDocumentViewModel model, byte[] file);
        bool UpdateJobDocument(RequestDocumentViewModel model, int documentId);
        IEnumerable<RequestDocumentViewModel> GetAllJobDocument();
        RequestDocumentViewModel GetJobDocument(int documentId);
        IEnumerable<RequestDocumentViewModel> GetJobRequestDocument(string jobRequestCode);
    }
}
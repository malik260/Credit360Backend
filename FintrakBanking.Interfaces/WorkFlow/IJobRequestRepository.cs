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
        //IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int groupId);

        bool AddJobRequest(JobRequestViewModel model);

        bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId);

        bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId);

        // job type
        IEnumerable<JobTypeViewModel> GetAllJobType();

        bool AddJobType(JobTypeViewModel model);

        bool UpdateJobType(JobTypeViewModel model, short jobTypeId);

        // staff
        IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId);
    }
}
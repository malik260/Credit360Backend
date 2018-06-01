using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class JobRequestViewModel : GeneralEntity
    {
        public string senderDepartment;

        public string senderUnit { get; set; }
        public string jobTypeName { get; set; }
        public string senderRole { get; set; }
        public int loggedInStaffId { get; set; }
        public string operationName { get; set; }
        public short? jobStatusFeedBackId { get; set; }
        public string jobStatusFeedback { get; set; }
        public List<JobRequestMessageViewModel> msgExchangeTrail { get; set; }

        public int targetId { get; set; }
        public short departmentId { get; set; }
        public short departmentUnitId { get; set; }
        public int jobRequestId { get; set; }
        public string requestTitle { get; set; }
        public string jobRequestCode { get; set; }
        public short jobTypeId { get; set; }
        public int senderStaffId { get; set; }
        public int? receiverStaffId { get; set; }
        public int? reassignedTo { get; set; }
        public bool isReassigned { get; set; }
        public bool isAcknowledged { get; set; }
        public int operationsId { get; set; }
        public short requestStatusId { get; set; }
        public string senderComment { get; set; }
        public string responseComment { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public DateTime? reassignedDate { get; set; }
        public DateTime? systemReassignedDate { get; set; }
        public DateTime? responseDate { get; set; }
        public DateTime? systemResponseDate { get; set; }
        public DateTime? acknowledgementDate { get; set; }
        public DateTime? systemAcknowledgementDate { get; set; }
        public string from { get; set; }
        public string fromBranchName { get; set; }
        public string toBranchName { get; set; }
        
        public string to { get; set; }
        public string assignee { get; set; }
        public Array emailList { get; set; }
        public List<MessageLogViewModel> mailData { get; set; }
    }

    public  class JobRequestMessageViewModel : GeneralEntity
    {
        public int jobRequestMessageId { get; set; }

        public int jobRequestId { get; set; }

        public string message { get; set; }

        public string staffName { get; set; }
        public DateTime datetimeSent { get; set; }
    }

    public class JobRequestDetailViewModel : GeneralEntity
    {
        public string jobRequestCode { get; set; }

        public string operationsName { get; set; }

        public string customerName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int targetId { get; set; }
        public int operationsId { get; set; }

        public string accreditedConsultantName { get; set; }
        public object jobSubTypeName { get; set; }
        public short jobTypeId { get; set; }
        public string jobTypeName { get; set; }

        public int jobRequestDetailId { get; set; }
        public int jobRequestId { get; set; }
        public int accreditedConsultantId { get; set; }
        public short jobSubTypeId { get; set; }
        public string description { get; set; }
        public decimal? amount { get; set; }
        public string accountNumber { get; set; }
    }

    public class JobRequestCollateralSearchViewModel : GeneralEntity
    {
        public int casaAccountId { get; set; }

        public bool requireCharting { get; set; }
        public bool requireVerification { get; set; }
        public bool requireSearch { get; set; }
        public int jobRequestId { get; set; }
        public decimal? additionalCharge { get; set; }
        public string additionalChargeJustification { get; set; }
        public string accountNumber { get; set; }
        public short solicitorId { get; set; }
        public short collateralStateId { get; set; }
    }

    public class ApplicationJobRequest : LoanApplicationViewModel
    {
        public int? allProcessingJobsCount { get; set; }
        public int? allCancelledJobsCount { get; set; }
        public int? processedMiddleOfficeCount { get; set; }
        public int? allJobsCount { get; set; }
        public int? allPendingJobsCount { get; set; }
        public int? allApprovedJobsCount { get; set; }
        public int? allDisapproveJobsCount { get; set; }
        
        public List<LoanApplicationDetailInvoiceViewModel> invoiceDiscountDetail { get; set; }

        public List<EducationLoanViewModel> firstEducationtDetail { get; set; }

        public List<TraderLoanViewModel> firstTradderDetail { get; set; }

        public List<CollateralViewModel> loanCollateral { get; set; }
    }

    public class JobRequestStatusFeedbackViewModel : GeneralEntity
    {
        public short jobStatusFeedbackId { get; set; }

        public string jobStatusFeedbackName { get; set; }

        public short requestStatusId { get; set; }

        public short jobTypeId { get; set; }

        public string jobTypeName { get; set; }

        public string requestStatusName { get; set; }

    }

    public class JobTypeViewModel : GeneralEntity
    {
        public short jobTypeId { get; set; }
        public string jobTypeName { get; set; }
    }

    public class JobSubTypeViewModel : JobTypeViewModel
    {
        public short jobSubTypeId { get; set; }
        public string jobSubTypeName { get; set; }
    }

    public class OperationStaffViewModel : GeneralEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public int groupId { get; set; }
    }

    public class RequestDocumentViewModel : GeneralEntity
    {
        public string comment;

        public int documentId { get; set; }
        public int targetId { get; set; }
        public int operationId { get; set; }
        public string targetReferenceNumber { get; set; }
        public string jobRequestCode { get; set; }
        public string documentTitle { get; set; }
        public short documentTypeId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        public string physicalFileNumber { get; set; }
        public string physicalLocation { get; set; }

    }
}

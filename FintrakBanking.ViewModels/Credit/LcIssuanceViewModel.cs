using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class LcIssuanceViewModel : GeneralEntity
    {
        #region LCISSUANCE
        public int lcIssuanceId { get; set; }
        public int tempLcIssuanceId { get; set; }

        public string lcReferenceNumber { get; set; }
        public string EnhancementReferenceNumber { get; set; }

        public string beneficiaryName { get; set; }

        public decimal totalApprovedAmount { get; set; }

        public int letterOfCreditTypeId { get; set; }

        public bool? isDraftRequired { get; set; }

        public string beneficiaryAddress { get; set; }

        public string beneficiaryEmail { get; set; }

        public int customerId { get; set; }

        public string customerName { get; set; }

        public int fundSourceId { get; set; }

        public int fundSourceDetails { get; set; }

        public string formMNumber { get; set; }

        public string beneficiaryPhoneNumber { get; set; }

        public string beneficiaryBank { get; set; }

        public int currencyId { get; set; }

        public string proformaInvoiceId { get; set; }

        public decimal availableAmount { get; set; }
        public decimal availableAmountForRelease { get; set; }

        public decimal letterOfCreditAmount { get; set; }

        public DateTime letterOfcreditExpirydate { get; set; }

        public DateTime invoiceDate { get; set; }

        public DateTime invoiceDueDate { get; set; }

        public int? totalApprovedAmountCurrencyId { get; set; }

        public int? availableAmountCurrencyId { get; set; }

        public bool? cashBuildUpAvailable { get; set; }

        public string cashBuildUpReferenceType { get; set; }

        public string cashBuildUpReferenceNumber { get; set; }

        public decimal? percentageToCover { get; set; }

        public decimal? lcTolerancePercentage { get; set; }

        public decimal? lcToleranceValue { get; set; }
        public int transactionCycle { get; set; }

        public decimal releaseAmount { get; set; }

        public int? lcReleaseAmountId { get; set; }

        //ussance
        public int? lcUssanceId { get; set; }
        public decimal ussanceAmount { get; set; }
        public decimal ussanceRate { get; set; }
        public int ussanceTenor { get; set; }
        public DateTime lcEffectiveDate { get; set; }
        public DateTime lcMaturityDate { get; set; }
        public int? usanceAmountCurrencyId { get; set; }
        public int? releaseApplicationStatusId { get; set; }
        public int? releaseApprovalStatusId { get; set; }
        public int? usanceApplicationStatusId { get; set; }
        public int? usanceApprovalStatusId { get; set; }
        public decimal? totalUsanceAmount { get; set; }
        public string currentApprovalLevel { get; set; }
        public string requestApprovalLevel { get; set; }
        public string requestStaffName { get; set; }
        public string currentlyWith { get; set; }
        public DateTime arrivalDate { get; set; }
        public int? operationId { get; set; }
        public string approvalStatus { get; set; }
        public List<ApprovalTrailViewModel> comments { get; set; }

        #endregion LCISSUANCE
    }

    #region LCDOCUMENT
    public class LcDocumentViewModel : GeneralEntity
    {
        public int lcDocumentId { get; set; }

        public int lcIssuanceId { get; set; }

        public string documentTitle { get; set; }

        public bool isSentToIssuingBank { get; set; }

        public int numberOfCopies { get; set; }

        public bool isSentToApplicant { get; set; }

    }
    #endregion LCDOCUMENT

    #region SHIPPING
    public class LcShippingViewModel : GeneralEntity
    {
        public int lcShippingId { get; set; }

        public int lcIssuanceId { get; set; }

        public string partyName { get; set; }

        public string partyAddress { get; set; }

        public string portOfDischarge { get; set; }

        public string portOfShipment { get; set; }

        public DateTime latestShipmentDate { get; set; }

        public bool isPartShipmentAllowed { get; set; }

        public bool isTransShipmentAllowed { get; set; }

    }
    #endregion SHIPPING

    #region LCCONDITIONS
    public class LcConditionViewModel : GeneralEntity
    {
        public int lcConditionId { get; set; }

        public int lcIssuanceId { get; set; }

        public string condition { get; set; }

        public bool isSatisfied { get; set; }

        public bool isTransactionDynamics { get; set; }

    }
    #endregion LCCONDITIONS

    public class LcIssuanceApprovalViewModel : GeneralEntity
    {
        public int lcIssuanceId { get; set; }
        public string EnhancementReferenceNumber { get; set; }
        public int tempLcIssuanceId { get; set; }
        public int tempLcUsanceId { get; set; }

        public int? lcReleaseAmountId { get; set; }

        public string lcReferenceNumber { get; set; }

        public string beneficiaryName { get; set; }

        public decimal totalApprovedAmount { get; set; }

        public int letterOfCreditTypeId { get; set; }

        public bool? isDraftRequired { get; set; }

        public string beneficiaryAddress { get; set; }

        public string beneficiaryEmail { get; set; }

        public int customerId { get; set; }

        public int fundSourceId { get; set; }

        public int fundSourceDetails { get; set; }

        public string formMNumber { get; set; }

        public string beneficiaryPhoneNumber { get; set; }

        public string beneficiaryBank { get; set; }

        public int currencyId { get; set; }

        public string proformaInvoiceId { get; set; }

        public decimal availableAmount { get; set; }

        public decimal letterOfCreditAmount { get; set; }

        public DateTime letterOfcreditExpirydate { get; set; }

        public DateTime invoiceDate { get; set; }

        public DateTime invoiceDueDate { get; set; }

        public DateTime arrivalDate { get; set; }

        public string customerName { get; set; }

        public string customerCode { get; set; }

        public short? approvalStatusId { get; set; }

        public string approvalStatus { get; set; }

        public int? currentApprovalStateId { get; set; }

        public int? currentApprovalLevelId { get; set; }

        public string currentApprovalLevel { get; set; }
        public string currentlyWith { get; set; }
        public string requestApprovalLevel { get; set; }
        public string requestStaffName { get; set; }

        public string lastComment { get; set; }

        public int lcApprovalTrailId { get; set; }
        public int? releaseApprovalTrailId { get; set; }
        public int? usanceApprovalTrailId { get; set; }

        public int? toStaffId { get; set; }

        public string responsiblePerson { get; set; }

        public int? currentApprovalLevelTypeId { get; set; }

        public int? applicationStatusId { get; set; }

        public short lcIssuanceStatusId { get; set; }

        public string lcApplicationStatus { get; set; }
        public string usanceStatus { get; set; }
        public decimal? totalUsanceAmount { get; set; }//total amount available for usance

        public int? operationId { get; set; }
        public string usanceApprovalStatus { get; set; }
        public string UsanceCurrentApprovalLevel { get; set; }
        public string releaseCurrentApprovalLevel { get; set; }


        public int? totalApprovedAmountCurrencyId { get; set; }

        public int? availableAmountCurrencyId { get; set; }

        public bool? cashBuildUpAvailable { get; set; }

        public string cashBuildUpReferenceType { get; set; }

        public string cashBuildUpReferenceNumber { get; set; }

        public decimal? percentageToCover { get; set; }

        public decimal? lcTolerancePercentage { get; set; }

        public decimal? lcToleranceValue { get; set; }
        public decimal? totalUsanceAmountLocal { get; set; }//total amount used in usance
        public int transactionCycle { get; set; }

        public decimal? releaseAmount { get; set; }
        public decimal releasedAmount { get; set; }
        public decimal availableAmountForRelease { get; set; }
        public int? loopedStaffId { get; set; }
        public List<LcReleaseAmountViewModel> lcReleases { get; set; }
        public List<LcUssanceViewModel> lcUsances { get; set; }
        public List<LcIssuanceViewModel> lcEnhancements { get; set; }
        public List<LcIssuanceViewModel> lcExtensions { get; set; }
        public List<LcUssanceViewModel> lcUsanceExtensions { get; set; }


        //ussance
        public int? lcUssanceId { get; set; }
        public decimal ussanceAmount { get; set; }
        public double? ussanceRate { get; set; }
        public int ussanceTenor { get; set; }
        public DateTime lcEffectiveDate { get; set; }
        public DateTime lcMaturityDate { get; set; }
        public int? usanceAmountCurrencyId { get; set; }
        public string releaseApplicationStatus { get; set; }
        public string releaseApprovalStatus { get; set; }
        public int? usanceApplicationStatusId { get; set; }
        public int? usanceApprovalStatusId { get; set; }
    }

    public class LcUssanceViewModel : LcIssuanceViewModel
    {
        //ussance
        public int lcIssuanceId { get; set; }
        public int lcUssanceId { get; set; }
        public int tempLcUsanceId { get; set; }
        public decimal ussanceAmount { get; set; }
        public decimal ussanceAmountLocal { get; set; }
        public double? ussanceRate { get; set; }
        public int? ussanceTenor { get; set; }
        public int? oldUssanceTenor { get; set; }
        public DateTime? lcEffectiveDate { get; set; }
        public DateTime? lcMaturityDate { get; set; }
        public DateTime? oldLcMaturityDate { get; set; }
        public int? usanceAmountCurrencyId { get; set; }
        public int? usanceApplicationStatusId { get; set; }
        public int? usanceApprovalStatusId { get; set; }
        public string UsanceCurrentApprovalLevel { get; set; }
        public string usanceApprovalStatus { get; set; }
        public decimal? totalUsanceAmount { get; set; }
        public string usanceStatus { get; set; }
    }

    public class LcReleaseAmountViewModel : GeneralEntity
    {
        public int lcReleaseAmountId { get; set; }
        public int lcIssuanceId { get; set; }
        public decimal? releaseAmount { get; set; }
        public int? releaseApplicationStatusId { get; set; }
        public int? releaseApprovalStatusId { get; set; }
        public string releaseApplicationStatus { get; set; }
        public string releaseApprovalStatus { get; set; }
        public string releaseCurrentApprovalLevel { get; set; }
    }
}
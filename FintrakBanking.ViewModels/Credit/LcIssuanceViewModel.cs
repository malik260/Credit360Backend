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

        public string beneficiaryName { get; set; }

        public decimal totalApprovedAmount { get; set; }

        public int letterOfCreditTypeId { get; set; }

        public bool isDraftRequired { get; set; }

        public string beneficiaryAddress { get; set; }

        public string beneficiaryEmail { get; set; }

        public int customerId { get; set; }

        public int fundSourceId { get; set; }

        public int formNumber { get; set; }

        public int beneficiaryPhoneNumber { get; set; }

        public int beneficiaryBankId { get; set; }

        public int currencyId { get; set; }

        public int proformaInvoiceId { get; set; }

        public decimal availableAmount { get; set; }

        public decimal letterOfCreditAmount { get; set; }

        public DateTime letterOfcreditExpirydate { get; set; }

        public DateTime invoiceDate { get; set; }

        public DateTime invoiceDueDate { get; set; }

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

    }
    #endregion LCCONDITIONS
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class InvoiceDetailForCreation
    {
        public int? principalId { get; set; }
        public string principalName { get; set; }
        public DateTime contractStartDate { get; set; } 
        public DateTime contractEndDate { get; set; }
        public DateTime invoiceDate { get; set; }
        public string invoiceNo { get; set; } 
        public decimal invoiceValue { get; set; }
        public decimal invoiceAmount { get; set; }
        public string contractNo { get; set; }
        public string purchaseOrderNumber { get; set; }
        public string entrySheetNumber { get; set; }
        public string certificateNumber { get; set; }


        // PROPERTIES THAT WILL BE AUTO GENERATED 
        public short invoiceCurrencyId { get; set; } = 1;
        public double exchangeInvoiceRate { get; set; } = 1;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class AtcLodgmentViewModel : GeneralEntity
    {
        public int atcLodgmentId { get; set; }

        public int customerId { get; set; }

        public int atcTypeId { get; set; }

        public string description { get; set; }

        public string depot { get; set; }

        public decimal unitValue { get; set; }

        public int unitNumber { get; set; }

        public string certificateNumber { get; set; }

        public int statusId { get; set; }

        public int numberOfBags { get; set; }

        public int? branchId { get; set; }

        public int? currencyId { get; set; }
        public string currency { get; set; }

        public short approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string branchName { get; set; }
        public decimal totalValue { get {
                return unitNumber * unitValue * numberOfBags;
            } }

        public string atcTypeName { get; set; }
        public int atcReleaseId { get; set; }
        public string comment { get; set; }
        public int operationId { get; set; }
        public DateTime? dateCreated { get; set; }
        public string atcType { get; set; }
        public int unitToRelease { get; set; }
        public DateTime? dateApproved { get; set; }
        public int? loopedStaffId { get; set; }
        public int unitBalance { get; set; }

        public DateTime? dateReleased { get; set; }
        public double exchangeRate { get; set; }
        public decimal localEquivalent { get { return totalValue * (decimal) exchangeRate; } }
    }


    public class AtcReleaseViewModel
    {
        public int atcReleaseId { get; set; }
        public int unitNumber { get; set; }
        public int unitRelease { get; set; }
        public int unitBalance { get; set; }
        public int unitToRelease { get; set; }
        public int releaseBalance { get; set; }
        public int createdBy { get; set; }
        public int atcLodgmentId { get; set; }
        public short userBranchId { get; set; }
        public string userIPAddress { get; set; }
        public string applicationUrl { get; set; }
        public int companyId { get; set; }
        public DateTime dateCreated { get; set; }
        public int getBalance { get; set; }
        public short approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public string comment { get; set; }
        public string depot { get; set; }
        public string description { get; set; }
        public string branchName { get; set; }
        public string customerName { get; set; }
        public int? numberOfBags { get; set; }
        public string atcType { get; set; }
        public DateTime? dateApproved { get; set; }
        public decimal unitValue { get; set; }
    }
}
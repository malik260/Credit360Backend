using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class DailyInterestAccrualViewModel : GeneralEntity

    {
        public string referenceNumber { get; set; }

        public string baseReferenceNumber { get; set; }

        public short categoryId { get; set; }

        public byte transactionTypeId { get; set; }

        public short productId { get; set; }

        //public int companyId { get; set; }

        public short branchId { get; set; }

        public short currencyId { get; set; }

        public double exchangeRate { get; set; }

        public decimal mainAmount { get; set; }

        public double interestRate { get; set; }

        public DateTime date { get; set; }

        public short dayCountConventionId { get; set; }

        public decimal dailyAccuralAmount { get; set; }

        public decimal availableBalance { get; set; }

        public int daysInAYear { get; set; }




    }

}

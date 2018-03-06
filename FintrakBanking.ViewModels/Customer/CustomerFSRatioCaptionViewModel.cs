using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerFSRatioCaptionViewModel : GeneralEntity
    {

        public short ratioCaptionId { get; set; }
        public string ratioCaptionName { get; set; }
        public bool annualised { get; set; }
        public int position { get; set; }
        public int fsCaptionId { get; set; }
        public string fsCaptionName { get; set; }
    }

    public class CustomerFSRatioCaptionReportViewModel : GeneralEntity
    {

        public short ratioCaptionId { get; set; }
        public string ratioCaptionName { get; set; }
        public bool annualised { get; set; }
        public int position { get; set; }
        public DateTime fsDate1 { get; set; }
        public DateTime fsDate2 { get; set; }
        public DateTime fsDate3 { get; set; }
        public DateTime fsDate4 { get; set; }
        public string ratioValue1 { get; set; }
        public string ratioValue2 { get; set; }
        public string ratioValue3 { get; set; }
        public string ratioValue4 { get; set; }
        public string fsGroupCaption { get; set; }
    }

    public class CustomerFSRatioDetailViewModel : GeneralEntity
    {
        public int ratioDetailId { get; set; }
        public short ratioCaptionId { get; set; }
        public string ratioCaptionName { get; set; }
        public int? fscaptionId { get; set; }
        public string fsCaptionName { get; set; }
        public short? divisorTypeId { get; set; }
        public string divisorTypeName { get; set; }
        public double? multiplier { get; set; }
        public short? valueTypeId { get; set; }
        public string valueTypeName { get; set; }

    }
    public class CustomerFSRatioDivisorTypeViewModel : GeneralEntity
    {
        public short divisorTypeId { get; set; }
        public string divisorTypeName { get; set; }
    }

    public class CustomerFSRatioValueTypeViewModel : GeneralEntity
    {
        public short valueTypeId { get; set; }
        public string valueTypeName { get; set; }

    }
}

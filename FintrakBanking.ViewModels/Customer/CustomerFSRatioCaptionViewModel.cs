using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerFSRatioCaptionViewModel : GeneralEntity
    {

        public short ratioCaptionId { get; set; }
        public string ratioCaptionName { get; set; }
        public string companyName { get; set; }
        public bool annualised { get; set; }
        public int position { get; set; }
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

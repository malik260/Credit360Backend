using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{

    public class CustomerCreditBureauUploadViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public int customerCreditBureauId { get; set; }
        public string documentTitle { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileData { get; set; }
        public DateTime systemDateTime { get; set; }

    }



    public class CreditBureauSearchViewModel : GeneralEntity
    {
        public string productId;

        public string userName { get; set; }
        public string password { get; set; }

        public short creditBureauId { get; set; } //dxs / crc
        public int searchType { get; set; }       // consumer/ commercial

        public string enquiryReason { get; set; }
        public string customerName { get; set; }
        public string gender { get; set; }
        public string dateOfBirth { get; set; }
        public string identification { get; set; }
        public string accountOrRegistrationNumber { get; set; }
        public bool chargeBusiness  { get; set; }
}

    public class SearchInput : GeneralEntity
    {
        public string productId;
        public string userName { get; set; }
        public string password { get; set; }

        public short creditBureauId { get; set; } //dxs / crc
        public int searchType { get; set; }       // consumer/ commercial

        public int consumerID { get; set; }
        public List<string> mergeList { get; set; }
        public string subscriberEnquiryEngineID { get; set; }
        public int enquiryID { get; set; }
        public LoanCreditBureauViewModel customerCreditBureauUploadDetails { get; set; }
        public int casaAccountId { get; set; }
        public bool chargeBusiness { get; set; }
        public bool debitBusiness { get; set; }
    }

    public class XDSIndividualSearchViewModel
    {
        public string userName { get; set; }
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string ConsumerName { get; set; }
        public string DateOfBirth { get; set; }
        public string Identification { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

    public class XDSCommercialSearchViewModel
    {
        public string userName { get; set; }
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

    public class SearchFullResultViewModel
    {
        public string userName { get; set; }
        public string DataTicket { get; set; }
        public int ConsumerID { get; set; }
        public string MergeList { get; set; }
        public string SubscriberEnquiryEngineID { get; set; }
        public int EnquiryID { get; set; }
    }

    
    public class CRCRequestViewModel : GeneralEntity
    {
        public string productId { get; set; }
        public int casaAccountId { get; set; }
        public short searchType { get; set; }
        public int customerId { get; set; }
        public int? companyDirectorId { get; set; }

        //public int accountId { get; set; }

        public string userName { get; set; }
        public string password { get; set; }

        /// <summary>
        /// response type is the format you want your result to be. CRC - 1 --> XML , 2 --> PDF 
        /// </summary>
        public int responseType { get; set; }

        /// <summary>
        /// Credit bureau type, 1 --> CRMS (No endpoint), 2 --> XDS  , 3 --> CRC, 
        /// </summary>
        public short creditBureauId { get; set; } //dxs / crc

        /// <summary>
        /// this represent the kind of search the should be performed. 6110 for	INDIVIDUAL  6112 for CORPORATE  
        /// </summary>
        public int reportID { get; set; }       // consumer/ commercial

        /// <summary>
        /// Search Type Code is for identifing the kind of search that is to be performed 
        /// and it depend on the number of parameter that is being provided. use 0 for (name, gender, dob), 
        /// 4 (BVN (identity)), 5 (Telephone), 6 for all 5 parameters
        /// </summary>
        public int searchTypeCode { get; set; }

        /// <summary>
        /// Inquiry Reason for the search
        /// </summary>
        public string enquiryReason { get; set; }

        /// <summary>
        /// Customer search name. this could be Individual or Corporate (Business Name)
        /// </summary>
        public string customerName { get; set; }

        public string gender { get; set; }

        public string dateOfBirth { get; set; }

        /// <summary>
        /// BVN or National Identification number (NIMC)
        /// </summary>
        public string identification { get; set; }

        public string phoneNumber { get; set; }

        /// <summary>
        /// This is should be an NUBAN or Company Registration Number
        /// </summary>
        public string accountOrRegistrationNumber { get; set; }

        public string genderCode
        {
            get
            {
                return creditBureauId == 3 ?
                    gender == "f" ? "001" : "002" :
                    gender == "f" ? "Female" : "Male";
            }
        }

        //crc Application
        public string currencyCode { get; set; }
        public decimal amount { get; set; }
        public string number { get; set; }
        public string productCode { get; set; }
        public string branchCode { get; set; }
        public bool debitBusiness { get; set; }
    }

    public class MultiHitRequestViewModel : GeneralEntity
    {
        public int casaAccountId;
        public int searchType;

        public short creditBureauId { get; set; }
        public int customerId { get; set; }
        public int companyDirectorId { get; set; }
        public string userName { get; set; }
        public string password { get; set; }


        public string currencyCode { get; set; }
        public decimal amount { get; set; }
        public string number { get; set; }
        public string productCode { get; set; }


        public List<string> bureauID { get; set; }
        public int referenceNo { get; set; }

        /// <summary>
        /// Inquiry Reason for the search
        /// </summary>
        public string enquiryReason { get; set; }

        /// <summary>
        /// response type is the formate you want your result to be. CRC - 1 --> XML , 2 --> PDF 
        /// </summary>
        public int responseType { get; set; }
        public int reportID { get; set; }
        public bool debitBusiness { get; set; }
    }

    public class CRCSearchResult
    {
        public int SearchCompleted { get; set; }
        public string SearchResult { get; set; }
        public bool fileSaved { get; set; }
        public byte[] file { get; set; }
        public bool errorOccured { get; set; }
    }
    public class XDSSearchResult
    {
        public string errorMessage { get; set; }
        public string searchResult { get; set; }
        public bool errorOccured { get; set; }
        public bool fileSaved { get; set; }
        public byte[] file { get; set; }
        public int status { get; set; }
    }

    public class CreditCheckViewModel
    {
        public string bvn { get; set; }
        public string channel_code { get; set; }
        public int token { get; set; }
    }

    public class ResponseMessageCreditCheckViewModel
    {
        public string creditCheck { get; set; }
    }

    [Serializable()]
    [XmlRoot("CreditCheck")]
    public class CRMSCreditCheckViewModel
    {
        [XmlElementAttribute("Credit")]
        public CreditViewModel[] Credits { get; set; }

        [XmlElementAttribute("Summary")]
        public string Summary { get; set; }
    }

    [Serializable()]
    public class CreditViewModel
    {
        [XmlElement("CRMSRefNumber")]
        public string CRMSRefNumber { get; set; }

        [XmlElement("CreditType")]
        public string CreditType { get; set; }

        [XmlElement("CreditLimit")]
        public string CreditLimit { get; set; }

        [XmlElement("OutstandingAmount")]
        public string OutstandingAmount { get; set; }

        [XmlElement("EffectiveDate")]
        public string EffectiveDate { get; set; }

        [XmlElement("Tenor")]
        public string Tenor { get; set; }

        [XmlElement("ExpiryDate")]
        public string ExpiryDate { get; set; }

        [XmlElement("GrantingInstitution")]
        public string GrantingInstitution { get; set; }

        [XmlElement("PerformanceStatus")]
        public string PerformanceStatus { get; set; }


    }




//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.8670
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

//using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CreditCheck
    {

        private string summaryField;

        private CreditCheckCredit[] creditField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Summary
        {
            get
            {
                return this.summaryField;
            }
            set
            {
                this.summaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Credit", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CreditCheckCredit[] Credits
        {
            get
            {
                return this.creditField;
            }
            set
            {
                this.creditField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class CreditCheckCredit
    {

        private string cRMSRefNumberField;

        private string creditTypeField;

        private string creditLimitField;

        private string outstandingAmountField;

        private string effectiveDateField;

        private string tenorField;

        private string expiryDateField;

        private string grantingInstitutionField;

        private string performanceStatusField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CRMSRefNumber
        {
            get
            {
                return this.cRMSRefNumberField;
            }
            set
            {
                this.cRMSRefNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CreditType
        {
            get
            {
                return this.creditTypeField;
            }
            set
            {
                this.creditTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CreditLimit
        {
            get
            {
                return this.creditLimitField;
            }
            set
            {
                this.creditLimitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string OutstandingAmount
        {
            get
            {
                return this.outstandingAmountField;
            }
            set
            {
                this.outstandingAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EffectiveDate
        {
            get
            {
                return this.effectiveDateField;
            }
            set
            {
                this.effectiveDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Tenor
        {
            get
            {
                return this.tenorField;
            }
            set
            {
                this.tenorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string GrantingInstitution
        {
            get
            {
                return this.grantingInstitutionField;
            }
            set
            {
                this.grantingInstitutionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PerformanceStatus
        {
            get
            {
                return this.performanceStatusField;
            }
            set
            {
                this.performanceStatusField = value;
            }
        }
    }

    /// <remarks/>
    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    //public partial class NewDataSet
    //{

    //    private CreditCheck[] itemsField;

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlElementAttribute("CreditCheck")]
    //    public CreditCheck[] Items
    //    {
    //        get
    //        {
    //            return this.itemsField;
    //        }
    //        set
    //        {
    //            this.itemsField = value;
    //        }
    //    }
    //}


}

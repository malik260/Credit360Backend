using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class CompanyViewModel
    {
        public int companyId { get; set; }
        public string companyName { get; set; }
        public string address { get; set; }
        public string telephone { get; set; }
        public string email { get; set; }
        public DateTime? dateOfIncorporation { get; set; }
        public short? natureOfBusinessId { get; set; }
        public string nameOfScheme { get; set; }
        public string functionsRegistered { get; set; }
        public decimal? authorisedShareCapital { get; set; }
        public string nameOfRegistrar { get; set; }
        public string nameOfTrustees { get; set; }
        public string formerManagersTrustees { get; set; }
        public DateTime? dateOfRenewalOfRegistration { get; set; }
        public DateTime? dateOfCommencement { get; set; }
        public int? initialFloatation { get; set; }
        public int? initialSubscription { get; set; }
        public string registeredBy { get; set; }
        public string trusteesAddress { get; set; }
        public string investmentObjective { get; set; }
        public string website { get; set; }
        public string ebusinessCode { get; set; }
        public string eoyprofitAndLossGl { get; set; }
        public int countryId { get; set; }
        public string country { get; set; }
        public short currencyId { get; set; }
        public short companyClassId { get; set; }
        public short companyTypeId { get; set; }
        public short accountingStandardId { get; set; }
        public short managementTypeId { get; set; }
        public int parentId { get; set; }
        public short languageId { get; set; }
        public byte[] CompanyLogo { get; set; }
        public int createdBy { get; set; }
        public int? lastUpdatedBy { get; set; }
        public DateTime? dateTimeCreated { get; set; }
        public DateTime? dateTimeUpdated { get; set; }
        public string natureOfBusiness { get; set; }
        public decimal? shareHoldersFund { get; set; }
        public decimal? companyLimit { get; set; }
        public decimal? singleObligorLimit { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public string imagePath { get; set; }
        public short userBranchId { get; set; }
        public string userIPAddress { get; set; }
        public string applicationUrl { get; set; }
        public byte[] fileData { get; set; }
    }
    public class CompanyDirectorsViewModel : GeneralEntity
    {
        public int companyDirectorId { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string bvn { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public bool isActive { get; set; }
        public string directorName { get; set; }
        public int? shareHoldingPercentage { get; set; }
    }
        public class LanguageViewModel
    {
        public int languageId { get; set; }
        public string language { get; set; }
        public string languageCode { get; set; }
        public string languageWithCode { get { return (language + " - " + languageCode); } }
    }
    public class NatureOfBusinessViewModel
    {
        public int natureOfBusinessId { get; set; }
        public string natureOfBusiness { get; set; }
       
    }
}
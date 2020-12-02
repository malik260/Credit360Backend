using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerDocumentUploadViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public string documentTitle { get; set; }
        public short documentTypeId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        public string physicalFileNumber { get; set; }
        public string physicalLocation { get; set; }
    }
    public class CheckListDocumentUploadViewModel : GeneralEntity
    {
        public int checkListDefinitionId { get; set; }
        public int checkListStatusId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanDetailsId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        public string physicalFileNumber { get; set; }
        public string physicalLocation { get; set; }
        public bool overwrite { get; set; }
    }
    public class ConditionsPrecedentUploadViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public int conditionId { get; set; }
        public int loanApplicationId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        public string physicalFileNumber { get; set; }
        public string physicalLocation { get; set; }
        public string deletedByName { get; set; }
    }
    public class KYCDocumentTypeViewModel
    {
        public int documentTypeId { get; set; }
        public string documentTypeName { get; set; }
    }
}

using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrCommentViewModel : GeneralEntity
    {
        public int psrCommentId { get; set; }

        public string comment { get; set; }
        public int projectSiteReportId { get; set; }
    }

    public class PsrReportViewModel 
    {
        public string comment { get; set; }
        public string projectDetail { get; set; }
        public string facilityDetail { get; set; }
        public string performanceEvaluation { get; set; }
        public string observation { get; set; }
        public string recomendation { get; set; }
        public string taskForNextInspection { get; set; }
        public string apgExposure { get; set; }
    }

    public class PsrProjectViewModel : GeneralEntity
    {
        public IEnumerable< LoanApplicationViewModel> loanApplicationViewModel { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }


    public class PsrImagesViewModel : GeneralEntity
    {
        public string psrReportType;
        public bool overwrite;

        public int psrImageId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string imageCaption { get; set; }
        //public int createdBy { get; set; }
        //public DateTime dateTimeCreated { get; set; }
        public int projectSiteReportId { get; set; }
        public string fileExtension { get; set; }
        public int fileSize { get; set; }
        public string fileSizeUnit { get; set; }
    }

    public class PsrCommentImagesViewModel : GeneralEntity
    {
        public string psrReportType;
        public bool overwrite;

        public int psrCommentImageId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string imageCaption { get; set; }
        public int createdBy { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public int projectSiteReportId { get; set; }
        public string fileExtension { get; set; }
        public int fileSize { get; set; }
        public string fileSizeUnit { get; set; }
    }

}
using FintrakBanking.Entities.DocumentModels;
//using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.media;
using System;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Media;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Interfaces.Setups.General;
using ExcelDataReader;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Repositories.media
{
    public class MediaRepository : IMediaRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository genSetup;

        public MediaRepository(FinTrakBankingDocumentsContext _context, IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            genSetup = _genSetup;
        }
        public async Task<bool> AddFile(byte[] imgContent, string fileName, string extention)
        {
            var document = new TBL_MEDIA_COLLATERAL_DOCUMENTS()
            {
                FILENAME = fileName,
                FILEEXTENSION = extention,
                FILEDATA = imgContent,
                SYSTEMDATETIME = DateTime.Now
                
            };
            context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Add(document);
            var response = await context.SaveChangesAsync();
            return response > 0;
        }

        public DocumentViewModel GetDocumentById(int id)
        {
            return (from doc in context.TBL_MEDIA_COLLATERAL_DOCUMENTS
                    where doc.DOCUMENTID == id
                    select new DocumentViewModel()
                    {
                        documentId = doc.DOCUMENTID,
                        fileData = doc.FILEDATA,
                        fileExtension = doc.FILEEXTENSION,
                        fileName = doc.FILENAME
                    }).FirstOrDefault();

        }
        //    public string GetDocumentToViewById(int id)
        //    {
        //        var viewdoc = (from doc in context.tbl_Media_Loan_Documents
        //                where doc.DocumentId == id
        //                select new DocumentViewModel()
        //                {
        //                    documentId = doc.DocumentId,
        //                    fileData = doc.FileData,
        //                    fileExtension = doc.FileExtension,
        //                    fileName = doc.FileName
        //                }).FirstOrDefault();

        //        String HtmlContent = "";
        //        if (viewdoc != null)
        //        {
        //            ViewerConfig config = new ViewerConfig();
        //            Stream stream = new MemoryStream(viewdoc.fileData);
        //            HtmlOptions options = new HtmlOptions();
        //            options.IsResourcesEmbedded = true;
        //            ViewerHtmlHandler handler = new ViewerHtmlHandler(config);
        //            List<PageHtml> AllPages = handler.GetPages(stream, options);

        //            foreach (PageHtml html in AllPages)
        //            {
        //                HtmlContent += html.HtmlContent;
        //            }

        //            return HtmlContent;
        //        }
        //        return null;
        //    }


        public IEnumerable<CustomerAddressViewModels> ReadEntitiesFromFile(Stream stream)
        {
            var myEntities = new List<CustomerAddressViewModels>();
           // var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            using (IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                while (reader.Read())
                {
                    var myEntity = new CustomerAddressViewModels();
                    myEntity.address = reader.GetString(1);
                    myEntity.companyName = reader.GetString(2);
               //   var shemaa =  reader.GetSchemaTable();
                    myEntities.Add(myEntity);
                }
            }

            return myEntities;
        }

        public byte[] GetCompanyImage(int id)
        {
            var fileData = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.FirstOrDefault(x => x.COLLATERALCUSTOMERID == id);
            return fileData.FILEDATA;
        }
    }
}

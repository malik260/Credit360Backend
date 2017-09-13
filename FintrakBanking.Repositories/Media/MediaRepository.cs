using FintrakBanking.Entities.DocumentModels;
//using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.media;
using System;
using GroupDocs.Viewer.Config;
using GroupDocs.Viewer.Converter.Options;
using GroupDocs.Viewer.Domain.Html;
using GroupDocs.Viewer.Domain.Image;
using GroupDocs.Viewer.Handler;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Media;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace FintrakBanking.Repositories.media
{
    public class MediaRepository : IMediaRepository
    {
        private FinTrakBankingDocumentsContext context;
        public MediaRepository(FinTrakBankingDocumentsContext _context)
        {
            this.context = _context;
        }
        public async Task<bool> AddFile(byte[] imgContent, string fileName, string extention)
        {
            var document = new tbl_Media_Collateral_Documents()
            {
                FileName = fileName,
                FileExtension = extention,
                FileData = imgContent,
                SystemDateTime = DateTime.Now
            };
            context.tbl_Media_Collateral_Documents.Add(document);
            var response = await context.SaveChangesAsync();
            return response > 0;
        }

        public DocumentViewModel GetDocumentById(int id)
        {
            return (from doc in context.tbl_Media_Collateral_Documents
                    where doc.DocumentId == id
                    select new DocumentViewModel()
                    {
                        documentId = doc.DocumentId,
                        fileData = doc.FileData,
                        fileExtension = doc.FileExtension,
                        fileName = doc.FileName
                    }).FirstOrDefault();

        }
        public string GetDocumentToViewById(int id)
        {
            var viewdoc = (from doc in context.tbl_Media_Loan_Documents
                    where doc.DocumentId == id
                    select new DocumentViewModel()
                    {
                        documentId = doc.DocumentId,
                        fileData = doc.FileData,
                        fileExtension = doc.FileExtension,
                        fileName = doc.FileName
                    }).FirstOrDefault();

            String HtmlContent = "";
            if (viewdoc != null)
            {
                ViewerConfig config = new ViewerConfig();
                Stream stream = new MemoryStream(viewdoc.fileData);
                HtmlOptions options = new HtmlOptions();
                options.IsResourcesEmbedded = true;
                ViewerHtmlHandler handler = new ViewerHtmlHandler(config);
                List<PageHtml> AllPages = handler.GetPages(stream, options);

                foreach (PageHtml html in AllPages)
                {
                    HtmlContent += html.HtmlContent;
                }

                return HtmlContent;
            }
            return null;
        }
    }
}

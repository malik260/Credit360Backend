using FintrakBanking.Entities.DocumentModels;
//using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Media;
using System.Linq;

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
    }
}

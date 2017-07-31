using FintrakBanking.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.media
{
    public interface IMediaRepository
    {
        Task<bool> AddFile(byte[] imgContent, string fileName, string extention);

        DocumentViewModel GetDocumentById(int id);
    }
}

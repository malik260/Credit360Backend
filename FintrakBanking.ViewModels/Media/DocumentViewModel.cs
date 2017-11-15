using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Media
{
    public class DocumentViewModel: GeneralEntity
    {
        public int documentId { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileData { get; set; }
        public string base64String { get { return Convert.ToBase64String(this.fileData); } }
    }
}

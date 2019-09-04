using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DocumentTypeViewModel : GeneralEntity
    {
        public int documentTypeId { get; set; }
        public int documentCategoryId { get; set; }

        public string documentTypeName { get; set; }
        public string documentCategoryName { get; set; }

    }
}
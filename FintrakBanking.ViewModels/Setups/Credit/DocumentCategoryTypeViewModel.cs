using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DocumentCategoryTypeViewModel : GeneralEntity
    {
        public int documentCategoryTypeId { get; set; }

        public int documentTypeId { get; set; }

        public int documentCategoryId { get; set; }

    }
}
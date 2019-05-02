using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Media;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentArchiveRepository
    {
        DocumentArchiveViewModel GetDocumentsByCustomerCode(string code);

        DocumentArchiveViewModel GetDocumentsByCustomerCodeAndCategory(string code, int categoryId);


    }
}

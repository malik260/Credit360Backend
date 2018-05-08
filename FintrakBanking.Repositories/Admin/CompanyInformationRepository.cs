using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Admin
{
   public class CompanyInformationRepository : ICompanyInformationRepository
    {
        private FinTrakBankingDocumentsContext documentContext;
        public CompanyInformationRepository(FinTrakBankingDocumentsContext _documentContext)
        {
            documentContext = _documentContext;
        }

        public byte[] GetCompanyImage()
        {
            var fileData = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.FirstOrDefault(x => x.COLLATERALCUSTOMERID == 2019);
            return fileData.FILEDATA;
        }

    }
}

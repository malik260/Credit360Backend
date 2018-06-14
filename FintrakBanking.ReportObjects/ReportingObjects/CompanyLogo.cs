using FintrakBanking.Entities.DocumentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
   public class CompanyLogo
    {
         FinTrakBankingDocumentsContext documentContext = new FinTrakBankingDocumentsContext();
        public byte[] GetCompanyLogoArray(int companyId)
        {
            var fileData = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(x => x.DOCUMENTID == 1).FirstOrDefault().FILEDATA;
            return fileData;
        }
    }
}

using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICreditBureauProcess
    {
        string XDSSearchCreditBureau(CreditBureauSearchViewModel searchInfo);
        List<dynamic> GetApprovedSearchReasons();
        string GetFullSearchResult(SearchInput searchInput);
    }
}
